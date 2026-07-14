using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Item History 화면 — MES Item 계보 트리와 선택 Item의 이력·웨이퍼 조회.
    ///
    /// 영역 구성 (계약 룰 5 — 레이아웃은 WinForms 담당):
    /// - 상단: 조회 카드 (Type 다중 필터[체크콤보] + Item ID 검색[필수, 자동완성 — ITEM만])
    /// - 중앙 좌측: Item 계보 트리(Scrap Item은 빨간 텍스트) + 선택 Item의 웨이퍼 목록
    /// - 중앙 우측: 선택 Item 상세 카드(주요 컬럼 표) + 이력 그리드
    ///   (MES_ITEM_HIS 전체 컬럼, 최신순, 하단 상태바 — 페이징 없음)
    ///
    /// 화면 오픈 시 자동 조회는 하지 않는다 — Item ID(필수)를 넣고 Search로 조회한다.
    /// 트리/자동완성은 ITEM만 다룬다. 트리에서 Item을 선택하면 그 Item의 이력(ITEM_HIS)과
    /// 웨이퍼 목록(UNIT_MAS)을 서버에서 함께 불러온다(로딩 팝업 표시).
    /// 검색어와 ITEM_ID가 정확히 일치하면 그 Item을, 없으면 첫 Item을 자동 선택한다.
    ///
    /// 서버 호출은 아래 "서버 조회 (★ 회사 환경 교체 지점)" 영역의 private
    /// 메서드 4개(tree/ids/history/units)에만 있다 — 회사 환경에서는 그 메서드들의
    /// 본문만 사내 인터페이스(전문/미들웨어) 호출로 바꾸면 된다. 반환 타입이
    /// DataTable / string[]이라 화면 코드는 손대지 않는다.
    /// 모든 조회는 백그라운드 스레드 + UI Invoke 패턴(계약 규칙 3)으로 수행한다.
    /// </summary>
    public partial class ItemHistoryForm : Form
    {

        // 마지막 트리 조회 결과 — 상세 카드/웨이퍼 목록의 원천.
        private DataTable treeData;

        // 조회조건 초기화가 끝나기 전의 변경 이벤트발 재조회를 막는다.
        private bool searchReady;

        // 검색 직후 코드로 SelectedValue를 넣을 때 이벤트 중복 처리를 막는다.
        private bool suppressTreeEvent;

        // Item ID 자동완성 typeahead: 입력이 잠시 멈추면 서버에서 후보를 가져온다.
        private System.Windows.Forms.Timer autoCompleteTimer;

        // 자동완성 요청 버전 — 오래된 응답이 최신 후보를 덮어쓰지 않게 한다.
        private int autoCompleteVersion;

        // 트리 선택(이력/웨이퍼 조회) 버전 — 빠른 재선택 시 오래된 응답을 버린다.
        private int selectionVersion;

        // Unit 이력 조회 버전 — 빠른 재선택 시 오래된 응답을 버린다.
        private int unitHistoryVersion;

        // Unit History 탭의 이력 그리드 (탭 구성과 함께 코드에서 생성).
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridUnitHistory;

        // Scrap 상태 노드의 트리 텍스트 색 — 어두운 테마에서는 밝은 빨강이어야 보인다.
        private static string ScrapForeColor
        {
            get { return Modern.Lab.Theming.ModernTheme.IsDarkBased ? "#FF99A4" : "#C42B1C"; }
        }

        // 그리드 행 배경색 (Win11 시맨틱 면 색과 동일 계열) — 어두운 테마는 어두운 톤.
        private static string ScrapRowColor
        {
            get { return Modern.Lab.Theming.ModernTheme.IsDarkBased ? "#4C2B2C" : "#FDE7E9"; }
        }

        private static string DoneRowColor
        {
            get { return Modern.Lab.Theming.ModernTheme.IsDarkBased ? "#39412A" : "#DFF6DD"; }
        }

        // 노드 종류/상태 배지 색.
        private static readonly Dictionary<string, string> typeBadgeColors = new Dictionary<string, string>
        {
            { "Main", "#DBEAFE" },
            { "PartA", "#FEF3C7" },
            { "PartB", "#E0E7FF" }
        };

        private static readonly Dictionary<string, string> statBadgeColors = new Dictionary<string, string>
        {
            { "Release", "#DCFCE7" },
            { "Scrap", "#FEE2E2" },
            { "Create", "" }
        };

        public ItemHistoryForm()
        {
            this.InitializeComponent();
        }

        // ===== 서버 조회 (★ 회사 환경 교체 지점) =====
        //
        // 아래 3개 메서드의 본문만 사내 서버 호출로 바꾸면 화면이 그대로 동작한다.
        // 홈 환경에서는 Spring Boot REST(modernlab-api)를 호출한다.

        /// <summary>홈 환경 API 주소 — 회사 적용 시 함께 제거한다.</summary>
        private const string apiBaseUrl = "http://localhost:8080";

        /// <summary>
        /// API 호출 제한 시간(ms). WebClient 기본값은 100초라, 서버가 응답 없이
        /// 매달리면(예: 백엔드는 죽고 포트만 살아 있는 경우) 로딩 팝업이 그만큼
        /// 계속 떠 "실행 중"처럼 보인다 — 짧게 잘라 토스트 오류로 빠지게 한다.
        /// </summary>
        private const int apiTimeoutMs = 5000;

        /// <summary>제한 시간을 적용한 WebClient (홈 환경 전용 헬퍼).</summary>
        private sealed class TimedWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest request = base.GetWebRequest(address);
                request.Timeout = apiTimeoutMs;
                return request;
            }
        }

        // Item/Unit 통합 트리. 검색 결과의 조상(최상위 Item까지)과 자손이 함께
        // 오고 IS_MATCH(Y/N)로 직접 매칭 노드가 표시되어야 한다.
        private DataTable RequestItemTree(string keyword, string[] subProdTypes)
        {
            StringBuilder query = new StringBuilder();
            query.Append("/api/items/tree?keyword=").Append(Uri.EscapeDataString(keyword ?? string.Empty));

            // 다중 Type은 같은 이름의 파라미터를 반복해서 보낸다 (Spring List 바인딩).
            if (subProdTypes != null)
            {
                foreach (string subProdType in subProdTypes)
                {
                    if (!string.IsNullOrEmpty(subProdType))
                    {
                        query.Append("&subTyp=").Append(Uri.EscapeDataString(subProdType));
                    }
                }
            }

            return this.DownloadTable(query.ToString());
        }

        // Item ID 자동완성 후보 — 부분 일치하는 ITEM/WF ID 상위 일부.
        // 입력 중 typeahead로 호출되므로 가볍고 빠르게 유지한다.
        private string[] RequestIdCandidates(string keyword)
        {
            string query = "/api/items/ids?keyword=" + Uri.EscapeDataString(keyword ?? string.Empty);

            using (WebClient client = new TimedWebClient())
            {
                client.Encoding = Encoding.UTF8;
                string json = client.DownloadString(apiBaseUrl + query);

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string[] candidates = serializer.Deserialize<string[]>(json);
                return candidates != null ? candidates : new string[0];
            }
        }

        // 선택 Item의 이력 (MES_ITEM_HIS 전체 컬럼, 최신순).
        private DataTable RequestItemHistory(string itemId)
        {
            string query = "/api/items/history?itemId=" + Uri.EscapeDataString(itemId ?? string.Empty);
            return this.DownloadTable(query);
        }

        // 선택 Item에 속한 Unit 목록 (MES_UNIT_MAS 현재 상태).
        private DataTable RequestUnits(string itemId)
        {
            string query = "/api/items/units?itemId=" + Uri.EscapeDataString(itemId ?? string.Empty);
            return this.DownloadTable(query);
        }

        // 선택 Unit의 이력 (UNIT_HIS 전체 컬럼, 최신순).
        private DataTable RequestUnitHistory(string unitId)
        {
            string query = "/api/items/unit-history?unitId=" + Uri.EscapeDataString(unitId ?? string.Empty);
            return this.DownloadTable(query);
        }

        // REST 공통: JSON 배열 응답을 DataTable로 변환한다 (홈 환경 전용 헬퍼).
        private DataTable DownloadTable(string pathAndQuery)
        {
            using (WebClient client = new TimedWebClient())
            {
                client.Encoding = Encoding.UTF8;
                string json = client.DownloadString(apiBaseUrl + pathAndQuery);
                return JsonTableConverter.ToDataTable(json);
            }
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            // Type 필터 체크콤보: 미체크 = 전체 (플레이스홀더 "All Types").
            DataTable typeTable = new DataTable();
            typeTable.Columns.Add("TYPE_CODE", typeof(string));
            typeTable.Columns.Add("TYPE_NAME", typeof(string));
            typeTable.Rows.Add("Main", "Main");
            typeTable.Rows.Add("PartA", "PartA");
            typeTable.Rows.Add("PartB", "PartB");

            this.cboType.DisplayMember = "TYPE_NAME";
            this.cboType.ValueMember = "TYPE_CODE";
            this.cboType.DataSource = typeTable;

            // Item ID 자동완성: 입력이 300ms 멈추면 서버 typeahead로 후보를 갱신한다.
            this.txtItemId.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.txtItemId.AutoCompleteSource = AutoCompleteSource.CustomSource;

            this.autoCompleteTimer = new System.Windows.Forms.Timer();
            this.autoCompleteTimer.Interval = 300;
            this.autoCompleteTimer.Tick += this.OnAutoCompleteTimerTick;

            // 트리: ITEM 계보만 표시한다(Unit 제외). ITEM_ID/PARENT_ITEM_ID로 계보 구성.
            // Scrap Item은 클라이언트에서 채운 NODE_COLOR 컬럼으로 빨간 텍스트가 된다.
            this.treeItemUnit.IdMember = "ITEM_ID";
            this.treeItemUnit.ParentIdMember = "PARENT_ITEM_ID";
            this.treeItemUnit.DisplayMember = "ITEM_ID";
            this.treeItemUnit.ForeColorMember = "NODE_COLOR";

            // 공정 진행 단계 표시: 이력 이벤트를 LABEL/STATE로 만들어 넘긴다.
            this.stepIndicator.DisplayMember = "LABEL";
            this.stepIndicator.StateMember = "STATE";

            // 선택 Item에 속한 Unit 목록 (MES_UNIT_MAS 현재 상태).
            this.gridUnits.ConfigureColumns(
                new ModernDataGridColumn("UNIT_ID", "Unit ID", 150),
                new ModernDataGridColumn("SUB_TYP", "Type", 70) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STAT_TYP", "Status", 66) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("EVENT_CD", "Event", 90),
                new ModernDataGridColumn("OPER_ID", "Oper", 90),
                new ModernDataGridColumn("STATION_ID", "Eqp", 90));

            // Scrap 웨이퍼 행은 옅은 빨강 배경(트리 텍스트 빨강과 짝).
            this.gridUnits.RowColorMember = "ROW_COLOR";

            // 이력 그리드: MES_ITEM_HIS 전체 컬럼 + 파생(DURATION). 실제 컬럼명 그대로 바인딩.
            this.gridHistory.ConfigureColumns(
                new ModernDataGridColumn("EVENT_TM", "Event Time", 150) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("DURATION", "Duration", 84) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("EVENT_CD", "Event", 96) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STAT_TYP", "Status", 84) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("OPER_ID", "Operation", 96) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STATION_ID", "Equipment", 96) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("BOX_ID", "Carrier", 92) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STORE_ID", "Stocker", 92) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("FLOW_ID", "Flow", 130),
                new ModernDataGridColumn("MODEL_ID", "Product", 150),
                new ModernDataGridColumn("ITEM_TYP", "Prod Type", 90) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("SUB_TYP", "Sub Type", 90) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("ORG_ITEM_ID", "Org Item", 120),
                new ModernDataGridColumn("PARENT_ITEM_ID", "Parent Item", 120),
                new ModernDataGridColumn("ITEM_ID", "Item", 120),
                new ModernDataGridColumn("TIMEKEY", "Time Key", 150));

            // 이력 행 상태별 색: Scrapped 빨강, JobEnd(완료) 초록.
            this.gridHistory.RowColorMember = "ROW_COLOR";

            // 하단 이력 영역을 탭으로 구성: Item History(기존 그리드) + Unit History.
            // Units 목록에서 Unit을 클릭하면 Unit History 탭의 "데이터만" 갱신한다 —
            // 탭 자동 전환은 하지 않는다 (사용자가 보던 탭 유지).
            this.gridUnitHistory = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.gridUnitHistory.AutoFitColumns = true;
            this.gridUnitHistory.ShowStatusBar = true;
            this.gridUnitHistory.StatusCountFormat = "{0:N0} events";
            this.gridUnitHistory.ConfigureColumns(
                new ModernDataGridColumn("EVENT_TM", "Event Time", 150) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("DURATION", "Duration", 84) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("EVENT_CD", "Event", 96) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STAT_TYP", "Status", 84) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("OPER_ID", "Operation", 96) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STATION_ID", "Equipment", 96) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("BOX_ID", "Carrier", 92) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STORE_ID", "Stocker", 92) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("FLOW_ID", "Flow", 130),
                new ModernDataGridColumn("MODEL_ID", "Product", 150),
                new ModernDataGridColumn("SUB_TYP", "Sub Type", 90) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("ORG_UNIT_ID", "Org Unit", 130),
                new ModernDataGridColumn("PARENT_UNIT_ID", "Parent Unit", 130),
                new ModernDataGridColumn("UNIT_ID", "Unit", 130),
                new ModernDataGridColumn("ITEM_ID", "Item", 120),
                new ModernDataGridColumn("TIMEKEY", "Time Key", 150));
            this.gridUnitHistory.RowColorMember = "ROW_COLOR";

            this.tabHistory.AddTab("Item History", this.gridHistory);
            this.tabHistory.AddTab("Unit History", this.gridUnitHistory);
            this.gridUnits.SelectionChanged += this.OnUnitSelectionChanged;

            // 화면 오픈 시 자동 조회 없음 — Item ID(필수) 입력 후 Search로 조회한다.
            this.ClearSelection();
            this.searchReady = true;
        }

        // ===== 트리 조회 =====

        private void OnSearchClick(object sender, EventArgs e)
        {
            this.ExecuteSearch();
        }

        // Reset: 조건과 결과를 모두 비운다. Item ID가 필수라 빈 조건 재조회는 없다.
        private void OnResetClick(object sender, EventArgs e)
        {
            this.txtItemId.Text = string.Empty;
            this.cboType.CheckedValues = null;
            this.treeData = null;
            this.treeItemUnit.DataSource = null;
            this.ClearSelection();
        }

        // 체크된 Type 코드들을 문자열 배열로 변환한다 (미체크 = null = 전체).
        private string[] GetCheckedTypes()
        {
            object[] checkedValues = this.cboType.CheckedValues;

            if (checkedValues == null || checkedValues.Length == 0)
            {
                return null;
            }

            string[] types = new string[checkedValues.Length];

            for (int index = 0; index < checkedValues.Length; index++)
            {
                types[index] = checkedValues[index] as string;
            }

            return types;
        }

        // 백그라운드에서 서버를 호출하고 UI 스레드로 복귀해 반영한다.
        // 반영 순서: 트리 바인딩 → 자동완성 후보 누적 →
        // 매칭 노드 자동 선택(조상 자동 펼침) → 선택 결과 화면 반영.
        private void ExecuteSearch()
        {
            if (!this.searchReady)
            {
                return;
            }

            string keyword = this.txtItemId.Text.Trim();

            // Item ID는 필수 입력이다.
            if (keyword.Length == 0)
            {
                this.toastMain.Show("Item ID is required.", ToastKind.Warning);
                return;
            }

            string[] subProdTypes = this.GetCheckedTypes();

            this.busyMain.Busy = true;

            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                try
                {
                    DataTable tree = this.RequestItemTree(keyword, subProdTypes);

                    this.Invoke(new MethodInvoker(delegate
                    {
                        // 트리가 참조하는 컬럼이 (해당 값이 전부 null이면 JSON에서 키가
                        // 생략돼) 누락될 수 있으므로 미리 보장한다.
                        EnsureColumns(tree, "ITEM_ID", "PARENT_ITEM_ID", "ORG_ITEM_ID",
                            "SUB_TYP", "ITEM_TYP", "STAT_TYP", "EVENT_CD", "MODEL_ID",
                            "FLOW_ID", "OPER_ID", "STATION_ID", "BOX_ID", "STORE_ID", "EVENT_TM");

                        ApplyScrapColor(tree);
                        this.treeData = tree;

                        this.suppressTreeEvent = true;

                        try
                        {
                            this.treeItemUnit.DataSource = tree;
                            this.treeItemUnit.SelectedValue = FindAutoSelectId(tree, keyword);
                        }
                        finally
                        {
                            this.suppressTreeEvent = false;
                        }

                        this.busyMain.Busy = false;
                        this.ApplyTreeSelection();
                    }));
                }
                catch (Exception ex)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        this.busyMain.Busy = false;
                        this.toastMain.Show("Server call failed: " + ex.Message, ToastKind.Error);
                    }));
                }
            });
        }

        // Scrap 상태 Item에 트리 텍스트 색 컬럼(NODE_COLOR)을 채운다.
        private static void ApplyScrapColor(DataTable tree)
        {
            if (!tree.Columns.Contains("NODE_COLOR"))
            {
                tree.Columns.Add("NODE_COLOR", typeof(string));
            }

            foreach (DataRow row in tree.Rows)
            {
                if (ToText(row, "STAT_TYP") == "Scrap")
                {
                    row["NODE_COLOR"] = ScrapForeColor;   // 트리 텍스트 빨강
                }
            }
        }

        // ===== Item ID 자동완성 (서버 typeahead) =====

        // 입력이 바뀔 때마다 디바운스 타이머를 재시작한다 — 타이핑이 멈추면 조회.
        private void OnItemIdTextChanged(object sender, EventArgs e)
        {
            if (!this.searchReady)
            {
                return;
            }

            // 자동완성 목록에서 항목을 "선택"하면 그 값이 그대로 TextBox에 들어와
            // TextChanged가 다시 발생한다. 이때 값이 이미 후보 목록에 있는 완전 일치이면
            // 재조회/재표시를 건너뛴다 — 선택 직후 같은 값으로 목록이 다시 뜨는 것을 막는다.
            if (this.IsExistingCandidate(this.txtItemId.Text.Trim()))
            {
                this.autoCompleteTimer.Stop();
                return;
            }

            this.autoCompleteTimer.Stop();
            this.autoCompleteTimer.Start();
        }

        // 현재 자동완성 후보 목록에 주어진 텍스트와 대소문자 무시 완전 일치가 있는지.
        private bool IsExistingCandidate(string text)
        {
            AutoCompleteStringCollection source = this.txtItemId.AutoCompleteCustomSource;

            if (source == null || text.Length == 0)
            {
                return false;
            }

            foreach (string candidate in source)
            {
                if (string.Equals(candidate, text, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnAutoCompleteTimerTick(object sender, EventArgs e)
        {
            this.autoCompleteTimer.Stop();

            string keyword = this.txtItemId.Text.Trim();

            if (keyword.Length == 0)
            {
                this.txtItemId.AutoCompleteCustomSource = null;
                return;
            }

            this.autoCompleteVersion = this.autoCompleteVersion + 1;
            int version = this.autoCompleteVersion;

            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                try
                {
                    string[] candidates = this.RequestIdCandidates(keyword);

                    this.Invoke(new MethodInvoker(delegate
                    {
                        // 그 사이 새 요청이 나갔으면 이 응답은 버린다.
                        if (version != this.autoCompleteVersion)
                        {
                            return;
                        }

                        AutoCompleteStringCollection collection = new AutoCompleteStringCollection();
                        collection.AddRange(candidates);
                        this.txtItemId.AutoCompleteCustomSource = collection;
                    }));
                }
                catch (Exception)
                {
                    // 자동완성 실패는 조용히 무시한다 — 검색 자체에는 영향 없음.
                }
            });
        }

        // 자동 선택 대상: 검색어와 ITEM_ID가 정확히 일치하는 Item이 있으면 그 Item,
        // 없으면 트리의 첫 Item(정렬상 최상단).
        private static string FindAutoSelectId(DataTable tree, string keyword)
        {
            foreach (DataRow row in tree.Rows)
            {
                string itemId = ToText(row, "ITEM_ID");

                if (string.Equals(itemId, keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return itemId;
                }
            }

            return tree.Rows.Count > 0 ? ToText(tree.Rows[0], "ITEM_ID") : null;
        }

        // ===== 트리 선택 → 상세 + 웨이퍼 목록 + 이력 =====

        private void OnTreeSelectionChanged(object sender, EventArgs e)
        {
            if (!this.searchReady || this.suppressTreeEvent)
            {
                return;
            }

            this.ApplyTreeSelection();
        }

        // 현재 트리 선택(ITEM) 상태를 상세 카드에 즉시 반영하고, 이력/웨이퍼는
        // 서버에서 백그라운드로 불러온다(로딩 팝업 표시).
        private void ApplyTreeSelection()
        {
            DataRowView selected = this.treeItemUnit.SelectedItem as DataRowView;

            if (selected == null)
            {
                this.ClearSelection();
                return;
            }

            this.FillDetail(selected);
            this.LoadItemDetails(GetString(selected, "ITEM_ID"));
        }

        private void FillDetail(DataRowView row)
        {
            string subType = GetString(row, "SUB_TYP");
            string stat = GetString(row, "STAT_TYP");

            // 카드 제목에 선택 Item ID를 표시한다.
            string itemId = GetString(row, "ITEM_ID");
            this.detailCard.Text = itemId.Length > 0 ? itemId : "Selection";
            this.badgeType.Text = subType.Length > 0 ? subType : "-";
            this.badgeType.Color = typeBadgeColors.ContainsKey(subType) ? typeBadgeColors[subType] : string.Empty;
            this.badgeStat.Text = stat.Length > 0 ? stat : "-";
            this.badgeStat.Color = statBadgeColors.ContainsKey(stat) ? statBadgeColors[stat] : string.Empty;

            // Selection: Item의 주요 컬럼들 (MES_ITEM_MAS 현재 상태).
            this.valProduct.Text = GetString(row, "MODEL_ID");
            this.valProdTyp.Text = GetString(row, "ITEM_TYP");
            this.valEvent.Text = GetString(row, "EVENT_CD");
            this.valFlow.Text = GetString(row, "FLOW_ID");
            this.valOper.Text = GetString(row, "OPER_ID");
            this.valEqp.Text = GetString(row, "STATION_ID");
            this.valCarrier.Text = GetString(row, "BOX_ID");
            this.valStk.Text = GetString(row, "STORE_ID");
            this.valEventTm.Text = GetString(row, "EVENT_TM");
        }

        // 선택 Item의 이력(MES_ITEM_HIS)과 웨이퍼 목록(MES_UNIT_MAS)을 백그라운드에서
        // 함께 불러온다. 두 조회가 끝날 때까지 로딩 팝업이 계속 표시된다.
        private void LoadItemDetails(string itemId)
        {
            this.busyMain.Busy = true;
            this.selectionVersion = this.selectionVersion + 1;
            int version = this.selectionVersion;

            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                try
                {
                    DataTable history = this.RequestItemHistory(itemId);
                    DataTable units = this.RequestUnits(itemId);

                    this.Invoke(new MethodInvoker(delegate
                    {
                        // 그 사이 다른 Item이 선택됐으면 이 응답은 버린다.
                        if (version != this.selectionVersion)
                        {
                            return;
                        }

                        // ---- 이력 그리드 (ITEM_HIS 전체 컬럼) ----
                        EnsureColumns(history, "TIMEKEY", "ITEM_ID", "EVENT_CD", "EVENT_TM",
                            "MODEL_ID", "ITEM_TYP", "SUB_TYP", "ORG_ITEM_ID", "PARENT_ITEM_ID",
                            "BOX_ID", "FLOW_ID", "OPER_ID", "STATION_ID", "STORE_ID", "STAT_TYP");
                        AddDurationColumn(history);
                        AddRowColor(history);
                        this.gridHistory.DataSource = history;
                        this.gridHistory.StatusText = itemId + CycleTimeSuffix(history);
                        this.stepIndicator.DataSource = BuildStepTable(history);

                        // ---- 웨이퍼 목록 ----
                        EnsureColumns(units, "UNIT_ID", "SUB_TYP", "STAT_TYP",
                            "EVENT_CD", "OPER_ID", "STATION_ID");
                        AddUnitRowColor(units);
                        this.gridUnits.DataSource = units;
                        this.unitCard.Text = "Units — " + itemId;

                        this.busyMain.Busy = false;
                    }));
                }
                catch (Exception ex)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        this.busyMain.Busy = false;
                        this.toastMain.Show("Server call failed: " + ex.Message, ToastKind.Error);
                    }));
                }
            });
        }

        // ===== Unit 이력 (하단 탭) =====

        // Units 목록에서 Unit 선택 → Unit History 탭 데이터를 갱신한다.
        // 탭을 자동으로 전환하지는 않는다 (사용자가 보던 탭 유지).
        private void OnUnitSelectionChanged(object sender, EventArgs e)
        {
            DataRowView row = this.gridUnits.SelectedItem as DataRowView;
            if (row == null)
            {
                return;
            }

            string unitId = ToText(row.Row, "UNIT_ID");
            if (string.IsNullOrEmpty(unitId))
            {
                return;
            }

            this.LoadUnitHistory(unitId);
        }

        // 선택 Unit의 이력을 백그라운드에서 불러와 Unit History 탭 그리드에 채운다.
        private void LoadUnitHistory(string unitId)
        {
            this.unitHistoryVersion = this.unitHistoryVersion + 1;
            int version = this.unitHistoryVersion;

            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                try
                {
                    DataTable history = this.RequestUnitHistory(unitId);

                    this.Invoke(new MethodInvoker(delegate
                    {
                        // 그 사이 다른 Unit이 선택됐으면 이 응답은 버린다.
                        if (version != this.unitHistoryVersion)
                        {
                            return;
                        }

                        EnsureColumns(history, "TIMEKEY", "UNIT_ID", "ITEM_ID", "EVENT_CD", "EVENT_TM",
                            "MODEL_ID", "ITEM_TYP", "SUB_TYP", "ORG_UNIT_ID", "PARENT_UNIT_ID",
                            "BOX_ID", "FLOW_ID", "OPER_ID", "STATION_ID", "STORE_ID", "STAT_TYP");
                        AddDurationColumn(history);
                        AddRowColor(history);
                        this.gridUnitHistory.DataSource = history;
                        this.gridUnitHistory.StatusText = unitId + CycleTimeSuffix(history);
                        this.tabHistory.SetTabTitle(1, "Unit History — " + unitId);
                    }));
                }
                catch (Exception ex)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        this.toastMain.Show("Server call failed: " + ex.Message, ToastKind.Error);
                    }));
                }
            });
        }

        // 웨이퍼 목록의 Scrap 행 배경(옅은 빨강) 컬럼을 채운다.
        private static void AddUnitRowColor(DataTable units)
        {
            if (units == null)
            {
                return;
            }

            if (!units.Columns.Contains("ROW_COLOR"))
            {
                units.Columns.Add("ROW_COLOR", typeof(string));
            }

            foreach (DataRow row in units.Rows)
            {
                if (ToText(row, "STAT_TYP") == "Scrap")
                {
                    row["ROW_COLOR"] = ScrapRowColor;
                }
            }
        }

        // Selection 상세 표의 셀 배경/괘선을 그린다.
        // 캡션 열(0,2,4)은 그리드 헤더처럼 옅은 톤(SurfaceAlt)으로 칠하고,
        // 모든 셀에 BorderSubtle 괘선을 둘러 "표" 형태로 보이게 한다.
        // (TableLayoutPanel 기본 CellBorderStyle은 진회색 클래식 선이라 쓰지 않는다.)
        // 색은 하드코딩하지 않고 ModernTheme 팔레트에서 읽는다 — 커스텀 페인트는
        // ModernThemeWinForms.Apply의 속성 치환이 닿지 않으므로 이렇게 해야
        // 라이트/다크 모두에서 맞는 색이 나온다.
        private void OnDetailCellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            bool captionColumn = e.Column == 0 || e.Column == 2 || e.Column == 4;

            if (captionColumn)
            {
                using (SolidBrush headerBrush = new SolidBrush(Modern.Lab.Theming.ModernTheme.SurfaceAlt))
                {
                    e.Graphics.FillRectangle(headerBrush, e.CellBounds);
                }
            }

            using (Pen linePen = new Pen(Modern.Lab.Theming.ModernTheme.BorderSubtle))
            {
                Rectangle cell = e.CellBounds;

                // 오른쪽·아래 선은 모든 셀에, 왼쪽·위 선은 가장자리 셀에만 그려
                // 이웃 셀과 선이 겹치지 않게 한다.
                e.Graphics.DrawLine(linePen, cell.Right - 1, cell.Top, cell.Right - 1, cell.Bottom - 1);
                e.Graphics.DrawLine(linePen, cell.Left, cell.Bottom - 1, cell.Right - 1, cell.Bottom - 1);

                if (e.Column == 0)
                {
                    e.Graphics.DrawLine(linePen, cell.Left, cell.Top, cell.Left, cell.Bottom - 1);
                }

                if (e.Row == 0)
                {
                    e.Graphics.DrawLine(linePen, cell.Left, cell.Top, cell.Right - 1, cell.Top);
                }
            }
        }

        private void ClearSelection()
        {
            this.detailCard.Text = "Selection";
            this.badgeType.Text = "-";
            this.badgeType.Color = string.Empty;
            this.badgeStat.Text = "-";
            this.badgeStat.Color = string.Empty;
            this.valProduct.Text = "-";
            this.valProdTyp.Text = "-";
            this.valEvent.Text = "-";
            this.valFlow.Text = "-";
            this.valOper.Text = "-";
            this.valEqp.Text = "-";
            this.valCarrier.Text = "-";
            this.valStk.Text = "-";
            this.valEventTm.Text = "-";
            this.gridUnits.DataSource = null;
            this.unitCard.Text = "Units";
            this.gridHistory.DataSource = null;
            this.gridHistory.StatusText = string.Empty;
            this.stepIndicator.DataSource = null;

            // Unit History 탭도 함께 비운다 (탭이 아직 구성 전이면 건너뜀).
            if (this.gridUnitHistory != null)
            {
                this.unitHistoryVersion = this.unitHistoryVersion + 1;
                this.gridUnitHistory.DataSource = null;
                this.gridUnitHistory.StatusText = string.Empty;
                this.tabHistory.SetTabTitle(1, "Unit History");
            }
        }

        // ===== 이력 파생 계산 (소요시간 · 사이클타임 · 진행 단계) =====

        // 이력은 최신순(TIMEKEY DESC). 각 행의 DURATION = 이 이벤트 시각 − 바로 이전
        // (더 오래된) 이벤트 시각. 가장 오래된 행(맨 아래)은 이전이 없어 빈칸.
        private static void AddDurationColumn(DataTable history)
        {
            if (history == null)
            {
                return;
            }

            if (!history.Columns.Contains("DURATION"))
            {
                history.Columns.Add("DURATION", typeof(string));
            }

            for (int index = 0; index < history.Rows.Count; index++)
            {
                DateTime current;
                DateTime older = DateTime.MinValue;

                bool hasCurrent = TryParseEventTime(history.Rows[index], out current);
                bool hasOlder = index + 1 < history.Rows.Count
                    && TryParseEventTime(history.Rows[index + 1], out older);

                if (hasCurrent && hasOlder)
                {
                    history.Rows[index]["DURATION"] = FormatDuration(current - older);
                }
                else
                {
                    history.Rows[index]["DURATION"] = string.Empty;
                }
            }
        }

        // 이력 행 배경색 컬럼(ROW_COLOR)을 이벤트로 채운다:
        // Scrapped=빨강, JobEnd(완료)=초록, 그 외는 빈칸(기본 교차색 유지).
        private static void AddRowColor(DataTable history)
        {
            if (history == null)
            {
                return;
            }

            if (!history.Columns.Contains("ROW_COLOR"))
            {
                history.Columns.Add("ROW_COLOR", typeof(string));
            }

            foreach (DataRow row in history.Rows)
            {
                string eventCd = ToText(row, "EVENT_CD");

                if (eventCd == "Scrapped")
                {
                    row["ROW_COLOR"] = ScrapRowColor;
                }
                else if (eventCd == "JobEnd")
                {
                    row["ROW_COLOR"] = DoneRowColor;
                }
            }
        }

        // 상태바 우측에 붙일 총 사이클타임(가장 오래된 → 가장 최근 이벤트) 접미어.
        private static string CycleTimeSuffix(DataTable history)
        {
            if (history == null || history.Rows.Count < 2)
            {
                return string.Empty;
            }

            DateTime newest;
            DateTime oldest;

            // 최신순이므로 첫 행이 최신, 마지막 행이 최초.
            if (TryParseEventTime(history.Rows[0], out newest)
                && TryParseEventTime(history.Rows[history.Rows.Count - 1], out oldest))
            {
                return "  ·  Cycle " + FormatDuration(newest - oldest);
            }

            return string.Empty;
        }

        // 이력 이벤트를 시간순(왼→오른쪽) 단계로 만든다. 마지막(최신) 이벤트가
        // 현재 단계이며, 그 이벤트가 Scrapped면 Failed(빨강)로 표시한다.
        private static DataTable BuildStepTable(DataTable history)
        {
            DataTable steps = new DataTable();
            steps.Columns.Add("LABEL", typeof(string));
            steps.Columns.Add("STATE", typeof(string));

            if (history == null || history.Rows.Count == 0)
            {
                return steps;
            }

            // 최신순 → 시간순으로 뒤집어 왼쪽부터 진행 순서가 되게 한다.
            int lastIndex = history.Rows.Count - 1;

            for (int index = lastIndex; index >= 0; index--)
            {
                string eventCd = ToText(history.Rows[index], "EVENT_CD");
                bool isCurrent = index == 0; // 최신 = 현재 단계
                string state;

                if (isCurrent)
                {
                    state = (eventCd == "Scrapped") ? "Failed" : "Current";
                }
                else
                {
                    state = "Completed";
                }

                steps.Rows.Add(eventCd, state);
            }

            return steps;
        }

        private static bool TryParseEventTime(DataRow row, out DateTime value)
        {
            value = DateTime.MinValue;
            string text = ToText(row, "EVENT_TM");

            if (text.Length == 0)
            {
                return false;
            }

            return DateTime.TryParse(text, out value);
        }

        // TimeSpan을 사람이 읽는 짧은 문자열로: "1d 4h" / "4h 12m" / "12m" / "45s".
        private static string FormatDuration(TimeSpan span)
        {
            if (span.Ticks < 0)
            {
                span = span.Negate();
            }

            if (span.TotalDays >= 1d)
            {
                return (int)span.TotalDays + "d " + span.Hours + "h";
            }

            if (span.TotalHours >= 1d)
            {
                return (int)span.TotalHours + "h " + span.Minutes + "m";
            }

            if (span.TotalMinutes >= 1d)
            {
                return span.Minutes + "m";
            }

            return span.Seconds + "s";
        }

        // ===== 공통 헬퍼 =====

        // 서버 응답에 컬럼 자체가 없거나(null 키 생략) DBNull인 경우를 모두
        // 빈 문자열로 처리한다.
        // 그리드/바인딩이 참조하는 컬럼이 DataTable에 없으면 문자열 빈 컬럼으로
        // 추가한다. JsonTableConverter는 값이 전부 null인 컬럼을 만들지 않으므로
        // (서버가 null 키를 생략), 바인딩 오류를 막으려면 표시 직전에 보장해야 한다.
        private static void EnsureColumns(DataTable table, params string[] columnNames)
        {
            if (table == null)
            {
                return;
            }

            foreach (string name in columnNames)
            {
                if (!table.Columns.Contains(name))
                {
                    table.Columns.Add(name, typeof(string));
                }
            }
        }

        private static string GetString(DataRowView row, string columnName)
        {
            if (!row.Row.Table.Columns.Contains(columnName))
            {
                return string.Empty;
            }

            object value = row[columnName];
            return value == DBNull.Value || value == null ? string.Empty : value.ToString();
        }

        private static string ToText(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
            {
                return string.Empty;
            }

            object value = row[columnName];
            return value == DBNull.Value || value == null ? string.Empty : value.ToString();
        }
    }
}
