using System;
using System.Collections.Generic;
using System.Data;
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
    /// Lot History 화면 — MES Lot/Wafer 계보 트리와 선택 노드의 이력 조회.
    ///
    /// 영역 구성 (계약 룰 5 — 레이아웃은 WinForms 담당):
    /// - 상단: 조회 카드 (Type 다중 필터[체크콤보] + Lot ID 검색[필수, 자동완성])
    /// - 중앙 좌측: Lot/Wafer 계보 트리(Scrap Lot은 빨간 텍스트) + 선택 Lot의 웨이퍼 목록
    /// - 중앙 우측: 선택 노드 상세 카드 + 이력 그리드 (최신순, 하단 상태바 —
    ///   이력은 한 번에 봐야 하므로 페이징하지 않는다)
    ///
    /// 화면 오픈 시 자동 조회는 하지 않는다 — Lot ID(필수)를 넣고 Search로 조회한다.
    ///
    /// 서버는 검색 결과의 조상(최상위 Lot까지)과 자손을 함께 내려주고
    /// IS_MATCH로 직접 매칭 노드를 표시한다 — 클라이언트는 그 값으로
    /// 조회된 Lot/Wafer를 자동 선택한다.
    ///
    /// 서버 호출은 아래 "서버 조회 (★ 회사 환경 교체 지점)" 영역의 private
    /// 메서드 3개에만 있다 — 회사 환경에서는 그 메서드들의 본문만 사내
    /// 인터페이스(전문/미들웨어) 호출로 바꾸면 된다. 반환 타입이 DataTable /
    /// string[]이라 화면 코드는 손대지 않는다.
    /// 모든 조회는 백그라운드 스레드 + UI Invoke 패턴(계약 규칙 3)으로 수행한다.
    /// </summary>
    public partial class LotHistoryForm : Form
    {

        // 마지막 트리 조회 결과 — 상세 카드/웨이퍼 목록의 원천.
        private DataTable treeData;

        // 조회조건 초기화가 끝나기 전의 변경 이벤트발 재조회를 막는다.
        private bool searchReady;

        // 검색 직후 코드로 SelectedValue를 넣을 때 이벤트 중복 처리를 막는다.
        private bool suppressTreeEvent;

        // Lot ID 자동완성 typeahead: 입력이 잠시 멈추면 서버에서 후보를 가져온다.
        private System.Windows.Forms.Timer autoCompleteTimer;

        // 자동완성 요청 버전 — 오래된 응답이 최신 후보를 덮어쓰지 않게 한다.
        private int autoCompleteVersion;

        // Scrap 상태 노드의 트리 텍스트 색.
        private const string scrapForeColor = "#DC2626";

        // 노드 종류/상태 배지 색.
        private static readonly Dictionary<string, string> typeBadgeColors = new Dictionary<string, string>
        {
            { "Wafer", "#DBEAFE" },
            { "Chip", "#FEF3C7" },
            { "Lamella", "#E0E7FF" }
        };

        private static readonly Dictionary<string, string> statBadgeColors = new Dictionary<string, string>
        {
            { "Release", "#DCFCE7" },
            { "Scrap", "#FEE2E2" },
            { "Create", "" }
        };

        public LotHistoryForm()
        {
            this.InitializeComponent();
        }

        // ===== 서버 조회 (★ 회사 환경 교체 지점) =====
        //
        // 아래 3개 메서드의 본문만 사내 서버 호출로 바꾸면 화면이 그대로 동작한다.
        // 홈 환경에서는 Spring Boot REST(modernlab-api)를 호출한다.

        /// <summary>홈 환경 API 주소 — 회사 적용 시 함께 제거한다.</summary>
        private const string apiBaseUrl = "http://localhost:8080";

        // Lot/Wafer 통합 트리. 검색 결과의 조상(최상위 Lot까지)과 자손이 함께
        // 오고 IS_MATCH(Y/N)로 직접 매칭 노드가 표시되어야 한다.
        private DataTable RequestLotTree(string keyword, string[] subProdTypes)
        {
            StringBuilder query = new StringBuilder();
            query.Append("/api/mes/tree?keyword=").Append(Uri.EscapeDataString(keyword ?? string.Empty));

            // 다중 Type은 같은 이름의 파라미터를 반복해서 보낸다 (Spring List 바인딩).
            if (subProdTypes != null)
            {
                foreach (string subProdType in subProdTypes)
                {
                    if (!string.IsNullOrEmpty(subProdType))
                    {
                        query.Append("&subProdTyp=").Append(Uri.EscapeDataString(subProdType));
                    }
                }
            }

            return this.DownloadTable(query.ToString());
        }

        // Lot ID 자동완성 후보 — 부분 일치하는 LOT/WF ID 상위 일부.
        // 입력 중 typeahead로 호출되므로 가볍고 빠르게 유지한다.
        private string[] RequestIdCandidates(string keyword)
        {
            string query = "/api/mes/ids?keyword=" + Uri.EscapeDataString(keyword ?? string.Empty);

            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                string json = client.DownloadString(apiBaseUrl + query);

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string[] candidates = serializer.Deserialize<string[]>(json);
                return candidates != null ? candidates : new string[0];
            }
        }

        // 선택 노드의 이력 (최신순).
        private DataTable RequestHistory(string nodeId, string nodeKind)
        {
            string query =
                "/api/mes/history?id=" + Uri.EscapeDataString(nodeId ?? string.Empty) +
                "&kind=" + Uri.EscapeDataString(nodeKind ?? "LOT");

            return this.DownloadTable(query);
        }

        // REST 공통: JSON 배열 응답을 DataTable로 변환한다 (홈 환경 전용 헬퍼).
        private DataTable DownloadTable(string pathAndQuery)
        {
            using (WebClient client = new WebClient())
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
            typeTable.Rows.Add("Wafer", "Wafer");
            typeTable.Rows.Add("Chip", "Chip");
            typeTable.Rows.Add("Lamella", "Lamella");

            this.cboType.DisplayMember = "TYPE_NAME";
            this.cboType.ValueMember = "TYPE_CODE";
            this.cboType.DataSource = typeTable;

            // Lot ID 자동완성: 입력이 300ms 멈추면 서버 typeahead로 후보를 갱신한다.
            this.txtLotId.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.txtLotId.AutoCompleteSource = AutoCompleteSource.CustomSource;

            this.autoCompleteTimer = new System.Windows.Forms.Timer();
            this.autoCompleteTimer.Interval = 300;
            this.autoCompleteTimer.Tick += this.OnAutoCompleteTimerTick;

            // 트리: 서버 통합 트리 응답(ID/PARENT_ID)을 그대로 계보로 사용.
            // Scrap 노드는 클라이언트에서 채운 NODE_COLOR 컬럼으로 빨간 텍스트가 된다.
            this.treeLotWf.IdMember = "ID";
            this.treeLotWf.ParentIdMember = "PARENT_ID";
            this.treeLotWf.DisplayMember = "ID";
            this.treeLotWf.ForeColorMember = "NODE_COLOR";

            // 선택 Lot의 웨이퍼 목록 (Lot과 중복되는 정보는 제외한 웨이퍼 고유 항목).
            this.gridWafers.ConfigureColumns(
                new ModernDataGridColumn("ID", "Wafer ID", 150),
                new ModernDataGridColumn("LOT_STAT_TYP", "Status", 62) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("EVENT_CD", "Event"));

            this.gridHistory.ConfigureColumns(
                new ModernDataGridColumn("EVENT_TM", "Event Time", 160) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("EVENT_CD", "Event", 100) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("OPER_ID", "Operation", 110) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("EQP_ID", "Equipment", 110) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("CARRIER_ID", "Carrier", 100) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STK_ID", "Stocker", 100) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("LOT_STAT_TYP", "Status", 90) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("FLOW_ID", "Flow", 150),
                new ModernDataGridColumn("TIMEKEY", "Time Key"));

            // 화면 오픈 시 자동 조회 없음 — Lot ID(필수) 입력 후 Search로 조회한다.
            this.ClearSelection();
            this.searchReady = true;
        }

        // ===== 트리 조회 =====

        private void OnSearchClick(object sender, EventArgs e)
        {
            this.ExecuteSearch();
        }

        // Reset: 조건과 결과를 모두 비운다. Lot ID가 필수라 빈 조건 재조회는 없다.
        private void OnResetClick(object sender, EventArgs e)
        {
            this.txtLotId.Text = string.Empty;
            this.cboType.CheckedValues = null;
            this.treeData = null;
            this.treeLotWf.DataSource = null;
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

            string keyword = this.txtLotId.Text.Trim();

            // Lot ID는 필수 입력이다.
            if (keyword.Length == 0)
            {
                this.toastMain.Show("Lot ID is required.", ToastKind.Warning);
                return;
            }

            string[] subProdTypes = this.GetCheckedTypes();

            this.busyMain.Busy = true;

            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                try
                {
                    DataTable tree = this.RequestLotTree(keyword, subProdTypes);

                    this.Invoke(new MethodInvoker(delegate
                    {
                        ApplyScrapColor(tree);
                        this.treeData = tree;

                        this.suppressTreeEvent = true;

                        try
                        {
                            this.treeLotWf.DataSource = tree;
                            this.treeLotWf.SelectedValue = FindAutoSelectId(tree, keyword);
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

        // Scrap 상태 노드에 트리 텍스트 색 컬럼(NODE_COLOR)을 채운다.
        private static void ApplyScrapColor(DataTable tree)
        {
            if (!tree.Columns.Contains("NODE_COLOR"))
            {
                tree.Columns.Add("NODE_COLOR", typeof(string));
            }

            foreach (DataRow row in tree.Rows)
            {
                if (ToText(row, "LOT_STAT_TYP") == "Scrap")
                {
                    row["NODE_COLOR"] = scrapForeColor;
                }
            }
        }

        // ===== Lot ID 자동완성 (서버 typeahead) =====

        // 입력이 바뀔 때마다 디바운스 타이머를 재시작한다 — 타이핑이 멈추면 조회.
        private void OnLotIdTextChanged(object sender, EventArgs e)
        {
            if (!this.searchReady)
            {
                return;
            }

            this.autoCompleteTimer.Stop();
            this.autoCompleteTimer.Start();
        }

        private void OnAutoCompleteTimerTick(object sender, EventArgs e)
        {
            this.autoCompleteTimer.Stop();

            string keyword = this.txtLotId.Text.Trim();

            if (keyword.Length == 0)
            {
                this.txtLotId.AutoCompleteCustomSource = null;
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
                        this.txtLotId.AutoCompleteCustomSource = collection;
                    }));
                }
                catch (Exception)
                {
                    // 자동완성 실패는 조용히 무시한다 — 검색 자체에는 영향 없음.
                }
            });
        }

        // 자동 선택 대상: 검색어와 ID가 정확히 일치하는 노드가 있으면 그 노드,
        // 없으면 서버가 IS_MATCH='Y'로 표시한 첫 노드.
        private static string FindAutoSelectId(DataTable tree, string keyword)
        {
            string firstMatch = null;

            foreach (DataRow row in tree.Rows)
            {
                string id = ToText(row, "ID");

                if (string.Equals(id, keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return id;
                }

                if (firstMatch == null && ToText(row, "IS_MATCH") == "Y")
                {
                    firstMatch = id;
                }
            }

            return firstMatch;
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

        // 현재 트리 선택 상태를 상세 카드/웨이퍼 목록/이력 그리드에 반영한다.
        private void ApplyTreeSelection()
        {
            DataRowView selected = this.treeLotWf.SelectedItem as DataRowView;

            if (selected == null)
            {
                this.ClearSelection();
                return;
            }

            this.FillDetail(selected);
            this.FillWaferList(selected);
            this.LoadHistory(GetString(selected, "ID"), GetString(selected, "NODE_KIND"));
        }

        private void FillDetail(DataRowView row)
        {
            string subType = GetString(row, "SUB_PROD_TYP");
            string stat = GetString(row, "LOT_STAT_TYP");

            this.lblSelId.Text = GetString(row, "ID");
            this.badgeType.Text = subType.Length > 0 ? subType : "-";
            this.badgeType.Color = typeBadgeColors.ContainsKey(subType) ? typeBadgeColors[subType] : string.Empty;
            this.badgeStat.Text = stat.Length > 0 ? stat : "-";
            this.badgeStat.Color = statBadgeColors.ContainsKey(stat) ? statBadgeColors[stat] : string.Empty;

            this.valProduct.Text = GetString(row, "PROD_ID");
            this.valFlow.Text = GetString(row, "FLOW_ID");
            this.valOper.Text = GetString(row, "OPER_ID");
            this.valEqp.Text = GetString(row, "EQP_ID");
            this.valCarrier.Text = GetString(row, "CARRIER_ID");
            this.valEventTm.Text = GetString(row, "EVENT_TM");
        }

        // 선택 노드가 Lot이면 그 Lot의 웨이퍼, Wafer이면 같은 Lot의 웨이퍼
        // (형제)를 좌측 하단 목록에 보여준다.
        private void FillWaferList(DataRowView row)
        {
            string kind = GetString(row, "NODE_KIND");
            string lotId = kind == "LOT" ? GetString(row, "ID") : GetString(row, "PARENT_ID");

            if (this.treeData == null || lotId.Length == 0 ||
                !this.treeData.Columns.Contains("NODE_KIND") ||
                !this.treeData.Columns.Contains("PARENT_ID"))
            {
                this.gridWafers.DataSource = null;
                this.waferCard.Text = "Wafers";
                return;
            }

            string filter = "NODE_KIND = 'WF' AND PARENT_ID = '" + lotId.Replace("'", "''") + "'";
            DataView wafers = new DataView(this.treeData, filter, "ID", DataViewRowState.CurrentRows);

            this.gridWafers.DataSource = wafers;
            this.waferCard.Text = "Wafers — " + lotId;
        }

        private void LoadHistory(string nodeId, string nodeKind)
        {
            this.busyMain.Busy = true;

            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                try
                {
                    DataTable history = this.RequestHistory(nodeId, nodeKind);

                    this.Invoke(new MethodInvoker(delegate
                    {
                        // 이력은 한 번에 본다 — 페이징 없이 전체 바인딩,
                        // 건수와 조회 대상은 그리드 상태바에 표시.
                        this.gridHistory.DataSource = history;
                        this.gridHistory.StatusText = nodeId + " · " + nodeKind;
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

        private void ClearSelection()
        {
            this.lblSelId.Text = "-";
            this.badgeType.Text = "-";
            this.badgeType.Color = string.Empty;
            this.badgeStat.Text = "-";
            this.badgeStat.Color = string.Empty;
            this.valProduct.Text = "-";
            this.valFlow.Text = "-";
            this.valOper.Text = "-";
            this.valEqp.Text = "-";
            this.valCarrier.Text = "-";
            this.valEventTm.Text = "-";
            this.gridWafers.DataSource = null;
            this.waferCard.Text = "Wafers";
            this.gridHistory.DataSource = null;
            this.gridHistory.StatusText = string.Empty;
        }

        // ===== 공통 헬퍼 =====

        // 서버 응답에 컬럼 자체가 없거나(null 키 생략) DBNull인 경우를 모두
        // 빈 문자열로 처리한다.
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
