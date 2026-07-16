using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Item History 화면 — MES Item 계보 트리와 선택 Item의 이력·웨이퍼 조회.
    ///
    /// 영역 구성 (계약 룰 5 — 레이아웃은 WinForms 담당, 배치는 .Designer.cs):
    /// - 상단: 조회 카드 (Type 다중 필터[체크콤보] + Item ID 검색[필수, 자동완성 — ITEM만])
    /// - 중앙 좌측: Item 계보 트리(Scrap Item은 빨간 텍스트) + 선택 Item의 웨이퍼 목록
    /// - 중앙 우측: 선택 Item 상세 카드(주요 컬럼 표) + 이력 탭
    ///   (Item History / Unit History — 페이지 구성은 .Designer.cs의 ModernTabPage)
    ///
    /// 화면 오픈 시 자동 조회는 하지 않는다 — Item ID(필수)를 넣고 Search로 조회한다.
    /// 트리/자동완성은 ITEM만 다룬다. 트리에서 Item을 선택하면 그 Item의 이력(ITEM_HIS)과
    /// 웨이퍼 목록(UNIT_MAS)을 서버에서 함께 불러온다(로딩 팝업 표시).
    /// 검색어와 ITEM_ID가 정확히 일치하면 그 Item을, 없으면 첫 Item을 자동 선택한다.
    ///
    /// 서버 호출은 아래 "서버 조회 (★ 회사 환경 교체 지점)" 영역의 private
    /// 메서드들에만 있다 — 회사 환경에서는 그 메서드들의 본문만 사내 인터페이스
    /// (전문/미들웨어) 호출로 바꾸면 된다. 반환 타입이 DataTable / string[]이라
    /// 화면 코드는 손대지 않는다.
    ///
    /// 모든 조회는 async/await(Task.Run) + 버전 가드 패턴이다: await 이후는
    /// UI 스레드로 복귀하므로(WinForms 동기화 컨텍스트) Invoke가 필요 없고,
    /// 빠른 재조회 시 버전 비교로 오래된 응답을 버린다. 파생 컬럼/표시 계산은
    /// HistoryTablePresenter(순수 DataTable 로직)에 있다.
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

        // 상세 표의 (값 라벨 ↔ 응답 컬럼) 매핑 — 채우기/비우기를 루프 하나로 처리한다.
        // 항목을 추가하려면 .Designer.cs에 라벨을 놓고 여기 한 줄만 더하면 된다.
        private readonly KeyValuePair<ModernLabel, string>[] detailBindings;

        // Duration(파생 지표) 강조색 — 액센트/상태색(빨강·초록)과 겹치지 않는
        // 틸 계열. 어두운 테마에서는 밝은 톤이어야 보인다.
        private static string DurationColor
        {
            get { return Modern.Lab.Theming.ModernTheme.IsDarkBased ? "#4DD0C2" : "#0F7B6C"; }
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

            this.detailBindings = new KeyValuePair<ModernLabel, string>[]
            {
                new KeyValuePair<ModernLabel, string>(this.valEvent, "EVENT_CD"),
                new KeyValuePair<ModernLabel, string>(this.valEventTm, "EVENT_TM"),
                new KeyValuePair<ModernLabel, string>(this.valCarrier, "BOX_ID"),
                new KeyValuePair<ModernLabel, string>(this.valStk, "STORE_ID"),
                new KeyValuePair<ModernLabel, string>(this.valProduct, "MODEL_ID"),
                new KeyValuePair<ModernLabel, string>(this.valFlow, "FLOW_ID"),
                new KeyValuePair<ModernLabel, string>(this.valOper, "OPER_ID"),
                new KeyValuePair<ModernLabel, string>(this.valEqp, "STATION_ID"),
                new KeyValuePair<ModernLabel, string>(this.valDescription, "DESCRIPTION")
            };
        }

        // ===== 서버 조회 (★ 회사 환경 교체 지점) =====
        //
        // 아래 메서드들의 본문만 사내 서버 호출로 바꾸면 화면이 그대로 동작한다.
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

        // ===== 초기 데이터/컬럼 구성 =====

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

            // 컬럼 정의만 코드에서 구성한다 (디자이너 직렬화 대상이 아님).
            // 트리/그리드의 멤버 지정(IdMember, RowColorMember 등)과 탭 페이지 구성은
            // 전부 .Designer.cs에 있다. 서버 응답에 없는 컬럼은 각 컨트롤이
            // DataSource 할당 시 자기 컬럼 정의로 자동 보장한다.
            //
            // 캡션은 용어사전(GridCaptionDictionary) 기본값을 쓴다 — 캡션 인자가
            // 없는 컬럼이 사전 참조이고, 이 화면 문맥의 재정의(좁은 패널 축약,
            // 이력 그리드의 Item/Unit 축약)만 명시한다.

            // 선택 Item에 속한 Unit 목록 (MES_UNIT_MAS 현재 상태).
            // AutoFitColumns 그리드라 폭은 생략한다 — 헤더+데이터 실측으로 계산된다.
            this.gridUnits.ConfigureColumns(
                new ModernDataGridColumn("UNIT_ID"),
                new ModernDataGridColumn("SUB_TYP", "Type") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STAT_TYP") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("EVENT_CD"),
                new ModernDataGridColumn("OPER_ID", "Oper"),
                new ModernDataGridColumn("STATION_ID", "Eqp"));

            // 이력 그리드: MES_ITEM_HIS 전체 컬럼 + 파생(DURATION). 실제 컬럼명 그대로 바인딩.
            // AutoFitColumns 그리드라 폭은 생략한다.
            this.gridHistory.ConfigureColumns(
                new ModernDataGridColumn("EVENT_TM") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("DURATION")
                {
                    TextAlignment = GridTextAlignment.Center,
                    TextColor = DurationColor,
                    TextSemiBold = true
                },
                new ModernDataGridColumn("EVENT_CD") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STAT_TYP") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("OPER_ID") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STATION_ID") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("BOX_ID") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STORE_ID") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("FLOW_ID"),
                new ModernDataGridColumn("MODEL_ID"),
                new ModernDataGridColumn("ITEM_TYP") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("SUB_TYP") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("ORG_ITEM_ID"),
                new ModernDataGridColumn("PARENT_ITEM_ID"),
                new ModernDataGridColumn("ITEM_ID", "Item"),
                new ModernDataGridColumn("TIMEKEY"));

            // Unit History 탭 그리드 (UNIT_HIS 전체 컬럼). Units 목록에서 Unit을
            // 클릭하면 이 탭의 "데이터만" 갱신한다 — 탭 자동 전환은 하지 않는다
            // (사용자가 보던 탭 유지).
            this.gridUnitHistory.ConfigureColumns(
                new ModernDataGridColumn("EVENT_TM") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("DURATION")
                {
                    TextAlignment = GridTextAlignment.Center,
                    TextColor = DurationColor,
                    TextSemiBold = true
                },
                new ModernDataGridColumn("EVENT_CD") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STAT_TYP") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("OPER_ID") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STATION_ID") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("BOX_ID") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STORE_ID") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("FLOW_ID"),
                new ModernDataGridColumn("MODEL_ID"),
                new ModernDataGridColumn("SUB_TYP") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("ORG_UNIT_ID"),
                new ModernDataGridColumn("PARENT_UNIT_ID"),
                new ModernDataGridColumn("UNIT_ID", "Unit"),
                new ModernDataGridColumn("ITEM_ID", "Item"),
                new ModernDataGridColumn("TIMEKEY"));

            // 화면 오픈 시 자동 조회 없음 — Item ID(필수) 입력 후 Search로 조회한다.
            this.ClearSelection();
            this.searchReady = true;
        }

        // ===== 트리 조회 =====

        private void OnSearchClick(object sender, EventArgs e)
        {
            this.ExecuteSearchAsync();
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

        // 백그라운드에서 서버를 호출하고(await 복귀 = UI 스레드) 반영한다.
        // 반영 순서: 트리 바인딩 → 매칭 노드 자동 선택 → 선택 결과 화면 반영.
        private async void ExecuteSearchAsync()
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

            // 검색이 시작되면 진행 중인 자동완성은 의미가 없다 — 디바운스 타이머와
            // 대기 중 응답(버전 무효화)을 취소하고, 열려 있는 추천 목록도 닫는다.
            // 안 그러면 검색 직후 뒤늦게 도착한 후보가 목록을 다시 열어 화면에 남는다.
            this.autoCompleteTimer.Stop();
            this.autoCompleteVersion = this.autoCompleteVersion + 1;
            this.txtItemId.CloseSuggestions();

            string[] subProdTypes = this.GetCheckedTypes();

            this.busyMain.Busy = true;

            try
            {
                DataTable tree = await Task.Run(() => this.RequestItemTree(keyword, subProdTypes));

                if (this.IsDisposed)
                {
                    return;
                }

                // 트리 멤버 컬럼(ITEM_ID 등)은 DataSource 할당 시 트리가 스스로
                // 보장하고, 상세 카드는 CellText가 누락 컬럼을 빈 값으로 읽는다.
                HistoryTablePresenter.ApplyScrapColor(tree);
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
            }
            catch (Exception ex)
            {
                if (this.IsDisposed)
                {
                    return;
                }

                this.busyMain.Busy = false;
                this.toastMain.Show("Server call failed: " + ex.Message, ToastKind.Error);
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

        private async void OnAutoCompleteTimerTick(object sender, EventArgs e)
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

            try
            {
                string[] candidates = await Task.Run(() => this.RequestIdCandidates(keyword));

                // 그 사이 새 요청이 나갔거나 폼이 닫혔으면 이 응답은 버린다.
                if (this.IsDisposed || version != this.autoCompleteVersion)
                {
                    return;
                }

                AutoCompleteStringCollection collection = new AutoCompleteStringCollection();
                collection.AddRange(candidates);
                this.txtItemId.AutoCompleteCustomSource = collection;
            }
            catch (Exception)
            {
                // 자동완성 실패는 조용히 무시한다 — 검색 자체에는 영향 없음.
            }
        }

        // 자동 선택 대상: 검색어와 ITEM_ID가 정확히 일치하는 Item이 있으면 그 Item,
        // 없으면 트리의 첫 Item(정렬상 최상단).
        private static string FindAutoSelectId(DataTable tree, string keyword)
        {
            foreach (DataRow row in tree.Rows)
            {
                string itemId = HistoryTablePresenter.CellText(row, "ITEM_ID");

                if (string.Equals(itemId, keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return itemId;
                }
            }

            return tree.Rows.Count > 0 ? HistoryTablePresenter.CellText(tree.Rows[0], "ITEM_ID") : null;
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
            this.LoadItemDetailsAsync(HistoryTablePresenter.CellText(selected, "ITEM_ID"));
        }

        private void FillDetail(DataRowView row)
        {
            string prodType = HistoryTablePresenter.CellText(row, "ITEM_TYP");
            string subType = HistoryTablePresenter.CellText(row, "SUB_TYP");
            string stat = HistoryTablePresenter.CellText(row, "STAT_TYP");

            // 카드 제목에 선택 Item ID를 표시한다.
            string itemId = HistoryTablePresenter.CellText(row, "ITEM_ID");
            this.detailCard.Text = itemId.Length > 0 ? itemId : "Selection";

            // 타입 배지: "Prod Type - Sub Type" 결합 표기 (색은 Sub Type 기준).
            string typeText = prodType.Length > 0 && subType.Length > 0
                    ? prodType + " - " + subType
                    : (subType.Length > 0 ? subType : prodType);
            this.badgeType.Text = typeText.Length > 0 ? typeText : "-";
            this.badgeType.Color = typeBadgeColors.ContainsKey(subType) ? typeBadgeColors[subType] : string.Empty;
            this.badgeStat.Text = stat.Length > 0 ? stat : "-";
            this.badgeStat.Color = statBadgeColors.ContainsKey(stat) ? statBadgeColors[stat] : string.Empty;

            // 상세 표: (값 라벨 ↔ 컬럼) 매핑을 돌며 채운다 (MES_ITEM_MAS 현재 상태).
            foreach (KeyValuePair<ModernLabel, string> binding in this.detailBindings)
            {
                binding.Key.Text = HistoryTablePresenter.CellText(row, binding.Value);
            }
        }

        // 선택 Item의 이력(MES_ITEM_HIS)과 웨이퍼 목록(MES_UNIT_MAS)을 백그라운드에서
        // 함께 불러온다. 두 조회가 끝날 때까지 로딩 팝업이 계속 표시된다.
        private async void LoadItemDetailsAsync(string itemId)
        {
            this.busyMain.Busy = true;
            this.selectionVersion = this.selectionVersion + 1;
            int version = this.selectionVersion;

            try
            {
                DataTable[] results = await Task.Run(() => new DataTable[]
                {
                    this.RequestItemHistory(itemId),
                    this.RequestUnits(itemId)
                });

                // 그 사이 다른 Item이 선택됐으면 버린다 — 로딩 팝업은 최신 요청이 정리한다.
                if (this.IsDisposed || version != this.selectionVersion)
                {
                    return;
                }

                DataTable history = results[0];
                DataTable units = results[1];

                // ---- 이력 그리드 (ITEM_HIS 전체 컬럼; 누락 컬럼은 그리드가 보장) ----
                HistoryTablePresenter.AddDurationColumn(history);
                HistoryTablePresenter.AddRowColor(history);
                this.gridHistory.DataSource = history;
                this.gridHistory.StatusText = itemId + HistoryTablePresenter.CycleTimeSuffix(history);
                this.stepIndicator.DataSource = HistoryTablePresenter.BuildStepTable(history);

                // ---- 웨이퍼 목록 ----
                HistoryTablePresenter.AddUnitRowColor(units);
                this.gridUnits.DataSource = units;
                this.unitCard.Text = "Units — " + itemId;

                this.busyMain.Busy = false;
            }
            catch (Exception ex)
            {
                if (this.IsDisposed)
                {
                    return;
                }

                this.busyMain.Busy = false;
                this.toastMain.Show("Server call failed: " + ex.Message, ToastKind.Error);
            }
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

            string unitId = HistoryTablePresenter.CellText(row, "UNIT_ID");

            if (unitId.Length == 0)
            {
                return;
            }

            this.LoadUnitHistoryAsync(unitId);
        }

        // 선택 Unit의 이력을 백그라운드에서 불러와 Unit History 탭 그리드에 채운다.
        private async void LoadUnitHistoryAsync(string unitId)
        {
            this.unitHistoryVersion = this.unitHistoryVersion + 1;
            int version = this.unitHistoryVersion;

            try
            {
                DataTable history = await Task.Run(() => this.RequestUnitHistory(unitId));

                // 그 사이 다른 Unit이 선택됐거나 폼이 닫혔으면 이 응답은 버린다.
                if (this.IsDisposed || version != this.unitHistoryVersion)
                {
                    return;
                }

                HistoryTablePresenter.AddDurationColumn(history);
                HistoryTablePresenter.AddRowColor(history);
                this.gridUnitHistory.DataSource = history;
                this.gridUnitHistory.StatusText = unitId + HistoryTablePresenter.CycleTimeSuffix(history);
                this.tabHistory.SetTabTitle(1, "Unit History — " + unitId);
            }
            catch (Exception ex)
            {
                if (this.IsDisposed)
                {
                    return;
                }

                this.toastMain.Show("Server call failed: " + ex.Message, ToastKind.Error);
            }
        }

        private void ClearSelection()
        {
            this.detailCard.Text = "Selection";
            this.badgeType.Text = "-";
            this.badgeType.Color = string.Empty;
            this.badgeStat.Text = "-";
            this.badgeStat.Color = string.Empty;

            foreach (KeyValuePair<ModernLabel, string> binding in this.detailBindings)
            {
                binding.Key.Text = "-";
            }

            this.gridUnits.DataSource = null;
            this.unitCard.Text = "Units";
            this.gridHistory.DataSource = null;
            this.gridHistory.StatusText = string.Empty;
            this.stepIndicator.DataSource = null;

            // Unit History 탭도 함께 비운다.
            this.unitHistoryVersion = this.unitHistoryVersion + 1;
            this.gridUnitHistory.DataSource = null;
            this.gridUnitHistory.StatusText = string.Empty;
            this.tabHistory.SetTabTitle(1, "Unit History");
        }
    }
}
