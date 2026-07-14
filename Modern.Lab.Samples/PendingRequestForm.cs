using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Controls.Wpf.Input;
using Modern.Lab.Samples.Services;
using Modern.Lab.Theming;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.WinForms.Controls.Input;
using Modern.Lab.WinForms.Controls.Layout;
using Modern.Lab.WinForms.Controls.Selection;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Pending Requests — 다른 곳에서 도착(입고)했지만 의뢰서가 아직 작성되지 않은
    /// Item들의 워크리스트 화면. 경과일(도착 후 며칠 지났는지)을 Days 배지 색으로
    /// 강조하고(행 배경색은 쓰지 않는다), 체크박스로 고른 행을 반송(Return)/
    /// 물류처리(Logistics)한다.
    ///
    /// 영역 구성 (계약 룰 5 — 레이아웃은 WinForms 담당):
    /// - 상단: 조회 카드 (Item ID 부분일치 + 경과일 필터 + 물류처리 라디오)
    /// - 중단: 미의뢰 Item 리스트 (체크박스 · Days 배지 · 행 단위 물류처리 버튼 ·
    ///         페이지 바) + 우측 선택 Item의 Unit 리스트
    /// - 하단 좌측 카드: KPI 배지 4개 + 경과일 구간별 분포 배지
    /// - 하단 우측 실행 카드: Export Excel / Return / Logistics 버튼
    ///   (Return·Logistics는 체크된 행 대상, 물류처리는 미처리 건만)
    ///
    /// 경과일 강조 규칙(회사 정책에 맞게 조정 지점):
    ///   Days 배지색 0-2일 파랑 · 3-6일 호박 · 7-13일 주황 · 14일+ 빨강 틴트,
    ///   Priority는 Normal/Watch/Warning/Critical. 화면 표기는 전부 영어.
    ///
    /// 서버 호출은 "서버 조회 (★ 회사 환경 교체 지점)" 영역의 private 메서드
    /// 2개(pending/units)에만 있다. Return/Logistics 동작(반송 전문, 물류 시스템
    /// 인터페이스)도 회사 인터페이스로 교체하는 지점이다 — 데모는 알림만 띄우고
    /// 화면 상태(LOGIS_YN)만 바꾼다.
    /// </summary>
    public class PendingRequestForm : Form
    {
        // ===== 홈 환경 API (★ 회사 환경 교체 지점) =====

        /// <summary>홈 환경 API 주소 — 회사 적용 시 함께 제거한다.</summary>
        private const string apiBaseUrl = "http://localhost:8080";

        /// <summary>API 호출 제한 시간(ms).</summary>
        private const int apiTimeoutMs = 5000;

        /// <summary>Item 리스트 페이지당 건수.</summary>
        private const int pageSize = 15;

        // ===== 레이아웃 필드 =====

        private ModernComboBox cboItemId;
        private ModernComboBox cboElapsed;
        private ModernRadioGroup rdoLogistics;
        private ModernButton btnSearch;
        private ModernButton btnReset;

        private ModernGroupBox itemCard;
        private ModernDataGrid gridItems;
        private ModernPagination pagination;
        private ModernGroupBox unitCard;
        private ModernDataGrid gridUnits;

        // 하단 슬림 스트립 — KPI/경과일 분포 모두 배지 스타일로 표기한다.
        private ModernStatusBadge badgePending;
        private ModernStatusBadge badgeUnits;
        private ModernStatusBadge badgeAvg;
        private ModernStatusBadge badgeOldest;
        private ModernStatusBadge[] badgeAging;

        private ModernButton btnExport;
        private ModernButton btnReturn;
        private ModernButton btnLogistics;

        private ModernBusyOverlay busyMain;
        private ModernToast toastMain;

        // ===== 상태 필드 =====

        // 마지막 조회 결과 전체 (페이지 슬라이스/KPI/분포/엑셀의 원천).
        private DataTable resultData;

        // 경과일 구간별 건수 (0-2 / 3-6 / 7-13 / 14+) — 하단 중앙 분포 막대의 원천.
        private readonly int[] agingCounts = new int[4];

        // 코드로 CurrentPage를 되돌릴 때 PageChanged 재진입을 막는다.
        private bool suppressPageEvent;

        // 조회 버전 — 빠른 재조회 시 오래된 응답을 버린다.
        private int searchVersion;

        // Unit 조회 버전 — 빠른 재선택 시 오래된 응답을 버린다.
        private int unitsVersion;

        // 경과일 구간 라벨 (분포 배지와 Priority 계산이 같은 경계를 쓴다).
        private static readonly string[] agingLabels = { "0-2 d", "3-6 d", "7-13 d", "14+ d" };

        // 경과일 구간별 배지 배경색 (구간이 심해질수록 파랑 → 호박 → 주황 → 빨강 틴트).
        // 하단 분포 배지와 그리드 Days 배지가 같은 색을 쓴다.
        private static readonly string[] agingBadgeColors = { "#DBEAFE", "#FEF3C7", "#FFE0CC", "#FEE2E2" };

        public PendingRequestForm()
        {
            this.InitializeLayout();
            this.Load += this.OnFormLoad;
        }

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

        // ===== 레이아웃 구성 =====

        private void InitializeLayout()
        {
            this.Text = "Pending Requests";
            this.ClientSize = new Size(1540, 800);
            this.MinimumSize = new Size(1240, 660);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9f);
            this.Padding = new Padding(12);
            this.BackColor = ModernTheme.Background;

            // Dock은 마지막에 추가한 것이 바깥쪽 — Fill(중단)을 먼저,
            // Bottom(하단 밴드) → Top(조회/타이틀) 순으로 추가한다.
            Panel midPanel = new Panel();
            midPanel.Dock = DockStyle.Fill;
            midPanel.BackColor = ModernTheme.Background;
            this.BuildMidZone(midPanel);

            Panel gapBottom = new Panel();
            gapBottom.Dock = DockStyle.Bottom;
            gapBottom.Height = 8;
            gapBottom.BackColor = ModernTheme.Background;

            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 56;
            bottomPanel.BackColor = ModernTheme.Background;
            this.BuildBottomStrip(bottomPanel);

            Panel gapSearch = new Panel();
            gapSearch.Dock = DockStyle.Top;
            gapSearch.Height = 8;
            gapSearch.BackColor = ModernTheme.Background;

            ModernCardPanel searchCard = this.BuildSearchCard();

            Panel spTitle = new Panel();
            spTitle.Dock = DockStyle.Top;
            spTitle.Height = 8;
            spTitle.BackColor = ModernTheme.Background;

            Panel titlePanel = new Panel();
            titlePanel.Dock = DockStyle.Top;
            titlePanel.Height = 28;
            titlePanel.BackColor = ModernTheme.Background;

            ModernLabel lblTitle = new ModernLabel();
            lblTitle.Kind = LabelKind.Title;
            lblTitle.TitleBar = true;
            lblTitle.Text = "Pending Requests";
            lblTitle.Dock = DockStyle.Fill;
            titlePanel.Controls.Add(lblTitle);

            ModernStatusBadge badgeEnv = new ModernStatusBadge();
            badgeEnv.Color = "#DBEAFE";
            badgeEnv.Dock = DockStyle.Right;
            badgeEnv.Width = 60;
            badgeEnv.Text = "MES";
            titlePanel.Controls.Add(badgeEnv);

            this.toastMain = new ModernToast();
            this.busyMain = new ModernBusyOverlay();
            this.busyMain.Message = "Loading...";
            this.busyMain.SubMessage = "Fetching pending items";

            this.Controls.Add(this.toastMain);
            this.Controls.Add(this.busyMain);
            this.Controls.Add(midPanel);
            this.Controls.Add(gapBottom);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(gapSearch);
            this.Controls.Add(searchCard);
            this.Controls.Add(spTitle);
            this.Controls.Add(titlePanel);
        }

        /// <summary>상단 조회 카드: Item ID(부분일치) + 경과일 필터 + 물류처리 라디오 + Search/Reset.</summary>
        private ModernCardPanel BuildSearchCard()
        {
            ModernCardPanel searchCard = new ModernCardPanel();
            searchCard.BackColor = ModernTheme.Surface;
            searchCard.Dock = DockStyle.Top;
            searchCard.Size = new Size(1516, 56);
            searchCard.Padding = new Padding(12, 8, 12, 8);

            ModernLabel lblItemId = new ModernLabel();
            lblItemId.Kind = LabelKind.Label;
            lblItemId.Text = "Item ID";
            lblItemId.SetBounds(12, 12, 56, 32);
            searchCard.Controls.Add(lblItemId);

            // 검색형 콤보(기본 DropDown = 편집 가능): 입력하면 후보 목록이 필터링된다.
            // 후보는 전체 조회 결과의 Item ID로 채운다 (ExecuteSearch 참고).
            this.cboItemId = new ModernComboBox();
            this.cboItemId.PlaceholderText = "All / type to filter";
            this.cboItemId.SetBounds(72, 12, 200, 32);
            searchCard.Controls.Add(this.cboItemId);

            ModernLabel lblElapsed = new ModernLabel();
            lblElapsed.Kind = LabelKind.Label;
            lblElapsed.Text = "Elapsed";
            lblElapsed.SetBounds(296, 12, 56, 32);
            searchCard.Controls.Add(lblElapsed);

            this.cboElapsed = new ModernComboBox();
            this.cboElapsed.SetBounds(356, 12, 140, 32);
            searchCard.Controls.Add(this.cboElapsed);

            ModernLabel lblLogistics = new ModernLabel();
            lblLogistics.Kind = LabelKind.Label;
            lblLogistics.Text = "Logistics";
            lblLogistics.SetBounds(520, 12, 64, 32);
            searchCard.Controls.Add(lblLogistics);

            // 물류처리 필터: All / Pending(미처리) / Done(처리 완료).
            this.rdoLogistics = new ModernRadioGroup();
            this.rdoLogistics.SetBounds(588, 12, 280, 32);
            searchCard.Controls.Add(this.rdoLogistics);

            this.btnSearch = new ModernButton();
            this.btnSearch.Kind = ButtonKind.Primary;
            this.btnSearch.Text = "Search";
            this.btnSearch.SetBounds(1336, 12, 80, 32);
            this.btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnSearch.Click += this.OnSearchClick;
            searchCard.Controls.Add(this.btnSearch);

            this.btnReset = new ModernButton();
            this.btnReset.Kind = ButtonKind.Subtle;
            this.btnReset.Text = "Reset";
            this.btnReset.SetBounds(1424, 12, 80, 32);
            this.btnReset.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnReset.Click += this.OnResetClick;
            searchCard.Controls.Add(this.btnReset);

            return searchCard;
        }

        /// <summary>중단: 미의뢰 Item 리스트(페이지 바) + 우측 선택 Item의 Unit 리스트.</summary>
        private void BuildMidZone(Panel host)
        {
            ModernSplitContainer splitMid = new ModernSplitContainer();
            splitMid.Dock = DockStyle.Fill;
            splitMid.Orientation = Orientation.Vertical;
            splitMid.FixedPanel = FixedPanel.Panel2;
            splitMid.Size = new Size(1516, 420);
            splitMid.Panel1MinSize = 600;
            splitMid.Panel2MinSize = 260;
            splitMid.SplitterDistance = 1130;
            host.Controls.Add(splitMid);

            // Item 리스트 — 하단에 페이지 바 (전체 건수 + 페이지 이동).
            this.itemCard = new ModernGroupBox();
            this.itemCard.BackColor = ModernTheme.Surface;
            this.itemCard.Dock = DockStyle.Fill;
            this.itemCard.Padding = new Padding(8, 40, 8, 8);
            this.itemCard.Text = "Pending Items";
            this.itemCard.TitleAccent = true;
            splitMid.Panel1.Controls.Add(this.itemCard);

            // 행 배경색(RowColorMember)은 쓰지 않는다 — 경과일 강조는 Days 배지가 담당.
            this.gridItems = new ModernDataGrid();
            this.gridItems.AutoFitColumns = true;
            this.gridItems.Dock = DockStyle.Fill;
            this.gridItems.SelectionChanged += this.OnItemSelectionChanged;
            this.gridItems.CellButtonClick += this.OnGridCellButtonClick;
            this.itemCard.Controls.Add(this.gridItems);

            this.pagination = new ModernPagination();
            this.pagination.Dock = DockStyle.Bottom;
            this.pagination.Height = 36;
            this.pagination.PageSize = pageSize;
            this.pagination.TotalCountFormat = "{0:N0} items";
            this.pagination.PageChanged += this.OnPageChanged;
            this.itemCard.Controls.Add(this.pagination);

            // Unit 리스트 — 보통 3~4줄만 보이면 되는 보조 정보라 좁은 우측 패널.
            splitMid.Panel2.Padding = new Padding(8, 0, 0, 0);

            this.unitCard = new ModernGroupBox();
            this.unitCard.BackColor = ModernTheme.Surface;
            this.unitCard.Dock = DockStyle.Fill;
            this.unitCard.Padding = new Padding(8, 40, 8, 8);
            this.unitCard.Text = "Units";
            splitMid.Panel2.Controls.Add(this.unitCard);

            this.gridUnits = new ModernDataGrid();
            this.gridUnits.AutoFitColumns = true;
            this.gridUnits.Dock = DockStyle.Fill;
            this.gridUnits.ShowStatusBar = true;
            this.gridUnits.StatusCountFormat = "{0:N0} units";
            this.unitCard.Controls.Add(this.gridUnits);
        }

        /// <summary>
        /// 하단 밴드: 좌측 지표 카드(KPI 배지 4개 + 경과일 분포 배지 4개) ·
        /// 우측 실행 카드(Export Excel / Return / Logistics).
        /// Return·Logistics는 그리드에서 체크한 행을 대상으로 한다.
        /// </summary>
        private void BuildBottomStrip(Panel host)
        {
            ModernCardPanel strip = new ModernCardPanel();
            strip.BackColor = ModernTheme.Surface;
            strip.Dock = DockStyle.Fill;
            strip.Size = new Size(1164, 56);

            // KPI 배지 (무색) — 조회 결과 전체(페이지가 아니라)를 집계한다.
            this.badgePending = AddBadge(strip, 12, 116, string.Empty);
            this.badgeUnits = AddBadge(strip, 136, 100, string.Empty);
            this.badgeAvg = AddBadge(strip, 244, 108, string.Empty);
            this.badgeOldest = AddBadge(strip, 360, 116, string.Empty);

            ModernLabel lblAging = new ModernLabel();
            lblAging.Kind = LabelKind.Label;
            lblAging.Text = "Aging";
            lblAging.SetBounds(508, 16, 44, 24);
            strip.Controls.Add(lblAging);

            // 경과일 구간 배지 (구간별 틴트색) — 분포 통계.
            this.badgeAging = new ModernStatusBadge[agingLabels.Length];

            for (int index = 0; index < agingLabels.Length; index++)
            {
                this.badgeAging[index] = AddBadge(strip, 556 + index * 100, 92, agingBadgeColors[index]);
            }

            // 우측 실행 카드 — 체크된 행에 대한 벌크 액션 + 엑셀 내보내기.
            ModernCardPanel actionCard = new ModernCardPanel();
            actionCard.BackColor = ModernTheme.Surface;
            actionCard.Dock = DockStyle.Right;
            actionCard.Width = 344;

            this.btnExport = new ModernButton();
            this.btnExport.Kind = ButtonKind.Subtle;
            this.btnExport.Text = "Export Excel";
            this.btnExport.SetBounds(12, 12, 110, 32);
            this.btnExport.Click += this.OnExportClick;
            actionCard.Controls.Add(this.btnExport);

            this.btnReturn = new ModernButton();
            this.btnReturn.Kind = ButtonKind.Danger;
            this.btnReturn.Text = "Return";
            this.btnReturn.SetBounds(130, 12, 90, 32);
            this.btnReturn.Click += this.OnReturnClick;
            actionCard.Controls.Add(this.btnReturn);

            this.btnLogistics = new ModernButton();
            this.btnLogistics.Kind = ButtonKind.Execute;
            this.btnLogistics.Text = "Logistics";
            this.btnLogistics.SetBounds(228, 12, 104, 32);
            this.btnLogistics.Click += this.OnLogisticsClick;
            actionCard.Controls.Add(this.btnLogistics);

            Panel gap = new Panel();
            gap.Dock = DockStyle.Right;
            gap.Width = 8;
            gap.BackColor = ModernTheme.Background;

            // Dock은 나중에 추가한 것이 먼저 배치된다 — Fill을 먼저 추가해야
            // 우측 카드/간격이 자리를 잡은 뒤 남은 폭을 채운다.
            host.Controls.Add(strip);
            host.Controls.Add(gap);
            host.Controls.Add(actionCard);
        }

        private static ModernStatusBadge AddBadge(Control parent, int x, int width, string color)
        {
            ModernStatusBadge badge = new ModernStatusBadge();
            badge.Text = "-";
            badge.Color = color;
            badge.SetBounds(x, 16, width, 24);
            parent.Controls.Add(badge);
            return badge;
        }

        // ===== 서버 조회 (★ 회사 환경 교체 지점) =====

        // 도착 후 의뢰서 미작성 Item 목록. 서버가 경과일(ELAPSED_DAYS)과
        // Unit 수(UNIT_CNT)를 계산해 경과일 오래된 순으로 준다.
        private DataTable RequestPendingItems(string keyword, int minDays)
        {
            StringBuilder query = new StringBuilder();
            query.Append("/api/items/pending-requests?minDays=").Append(minDays);

            if (!string.IsNullOrEmpty(keyword))
            {
                query.Append("&keyword=").Append(Uri.EscapeDataString(keyword));
            }

            return this.DownloadTable(query.ToString());
        }

        // 선택 Item에 속한 Unit 목록 (MES_UNIT_MAS 현재 상태).
        private DataTable RequestUnits(string itemId)
        {
            string query = "/api/items/units?itemId=" + Uri.EscapeDataString(itemId ?? string.Empty);
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

        // ===== 초기 구성 + 자동 조회 =====

        private void OnFormLoad(object sender, EventArgs e)
        {
            // 경과일 필터: All(0) / 1+ / 3+ / 7+ / 14+ 일.
            DataTable elapsedTable = new DataTable();
            elapsedTable.Columns.Add("DAYS", typeof(string));
            elapsedTable.Columns.Add("LABEL", typeof(string));
            elapsedTable.Rows.Add("0", "All");
            elapsedTable.Rows.Add("1", "1+ days");
            elapsedTable.Rows.Add("3", "3+ days");
            elapsedTable.Rows.Add("7", "7+ days");
            elapsedTable.Rows.Add("14", "14+ days");

            this.cboElapsed.DisplayMember = "LABEL";
            this.cboElapsed.ValueMember = "DAYS";
            this.cboElapsed.DataSource = elapsedTable;
            this.cboElapsed.SelectedValue = "0";

            // 물류처리 필터: All(전체) / Pending(미처리) / Done(처리 완료).
            DataTable logisticsTable = new DataTable();
            logisticsTable.Columns.Add("VALUE", typeof(string));
            logisticsTable.Columns.Add("LABEL", typeof(string));
            logisticsTable.Rows.Add("ALL", "All");
            logisticsTable.Rows.Add("N", "Pending");
            logisticsTable.Rows.Add("Y", "Done");

            this.rdoLogistics.DisplayMember = "LABEL";
            this.rdoLogistics.ValueMember = "VALUE";
            this.rdoLogistics.DataSource = logisticsTable;
            this.rdoLogistics.SelectedValue = "ALL";

            // Item ID 자동완성 콤보: 후보 목록은 전체 조회 결과로 채운다.
            this.cboItemId.DisplayMember = "ITEM_ID";
            this.cboItemId.ValueMember = "ITEM_ID";

            // Item 리스트: 체크박스(벌크 대상) + 도착 정보 + Days 배지 + Priority +
            // 행 단위 물류처리 버튼(미처리 건만 활성). 실제 컬럼명 그대로 바인딩.
            this.gridItems.ConfigureColumns(
                new ModernDataGridColumn("CHK", "", 44) { Kind = GridColumnKind.CheckBox },
                new ModernDataGridColumn("ITEM_ID", "Item ID", 140),
                new ModernDataGridColumn("ELAPSED_DAYS", "Days", 70)
                {
                    Kind = GridColumnKind.Badge,
                    BadgeColorMember = "DAYS_COLOR",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("PRIORITY", "Priority", 80) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("UNIT_CNT", "Units", 60) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("LOGIS_YN", "Logistics", 100)
                {
                    Kind = GridColumnKind.Button,
                    ButtonText = "Process",
                    ButtonEnabledMember = "LOGIS_CAN",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("EVENT_TM", "Arrived At", 150) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("MODEL_ID", "Product", 140),
                new ModernDataGridColumn("SUB_TYP", "Type", 80) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("ORG_ITEM_ID", "Org Item", 120),
                new ModernDataGridColumn("BOX_ID", "Carrier", 90) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STORE_ID", "Stocker", 90) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("DESCRIPTION", "Description"));

            // Unit 리스트: 좁은 패널에 맞는 최소 컬럼.
            this.gridUnits.ConfigureColumns(
                new ModernDataGridColumn("UNIT_ID", "Unit ID", 130),
                new ModernDataGridColumn("SUB_TYP", "Type", 70) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STAT_TYP", "Status", 70) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("EVENT_TM", "Arrived At", 140) { TextAlignment = GridTextAlignment.Center });

            // 워크리스트 화면이라 열자마자 자동 조회한다 (필수 조건 없음).
            this.ExecuteSearch();
        }

        // ===== 조회 =====

        private void OnSearchClick(object sender, EventArgs e)
        {
            this.ExecuteSearch();
        }

        private void OnResetClick(object sender, EventArgs e)
        {
            this.cboItemId.Text = string.Empty;
            this.cboElapsed.SelectedValue = "0";
            this.rdoLogistics.SelectedValue = "ALL";
            this.ExecuteSearch();
        }

        // 현재 선택된 경과일 필터 값 (일 수, 0 = 전체).
        private int GetMinDays()
        {
            string value = this.cboElapsed.SelectedValue as string;
            int days;

            if (value != null && int.TryParse(value, out days))
            {
                return days;
            }

            return 0;
        }

        // 현재 선택된 물류처리 필터 값 ("" = 전체, "N" = 미처리, "Y" = 처리 완료).
        private string GetLogisticsFilter()
        {
            string value = this.rdoLogistics.SelectedValue as string;

            if (value == "N" || value == "Y")
            {
                return value;
            }

            return string.Empty;
        }

        // 백그라운드에서 서버를 호출하고 UI 스레드로 복귀해 반영한다.
        // 반영 순서: 파생 컬럼(Priority/Days 배지색/체크/물류) → 물류처리 필터 →
        // 페이지 1 바인딩 → KPI/분포 갱신.
        private void ExecuteSearch()
        {
            string keyword = this.cboItemId.Text.Trim();
            int minDays = this.GetMinDays();
            string logisticsFilter = this.GetLogisticsFilter();

            this.busyMain.Busy = true;
            this.searchVersion = this.searchVersion + 1;
            int version = this.searchVersion;

            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                try
                {
                    DataTable pending = this.RequestPendingItems(keyword, minDays);

                    this.Invoke(new MethodInvoker(delegate
                    {
                        // 그 사이 새 조회가 나갔으면 이 응답은 버린다.
                        if (version != this.searchVersion)
                        {
                            return;
                        }

                        EnsureColumns(pending, "ITEM_ID", "ORG_ITEM_ID", "MODEL_ID", "ITEM_TYP",
                            "SUB_TYP", "EVENT_CD", "EVENT_TM", "BOX_ID", "STORE_ID",
                            "STAT_TYP", "DESCRIPTION", "ELAPSED_DAYS", "UNIT_CNT", "LOGIS_YN");
                        ApplyDerivedColumns(pending);
                        this.resultData = ApplyLogisticsFilter(pending, logisticsFilter);

                        // 페이지 바: 전체 건수 반영 후 1페이지부터.
                        this.suppressPageEvent = true;

                        try
                        {
                            this.pagination.TotalCount = this.resultData.Rows.Count;
                            this.pagination.CurrentPage = 1;
                        }
                        finally
                        {
                            this.suppressPageEvent = false;
                        }

                        this.BindCurrentPage();
                        this.UpdateKpis();
                        this.UpdateAgingCounts();

                        // 전체 조회(조건 없음)일 때 그 결과로 Item ID 자동완성
                        // 후보를 갱신한다 — DataSource 할당의 첫 행 자동 선택은
                        // 텍스트를 비워 되돌린다 (조건이 채워진 채 남지 않게).
                        if (keyword.Length == 0 && minDays == 0 && logisticsFilter.Length == 0)
                        {
                            this.cboItemId.DataSource = pending.DefaultView.ToTable(false, "ITEM_ID");
                            this.cboItemId.Text = string.Empty;
                        }

                        this.gridUnits.DataSource = null;
                        this.unitCard.Text = "Units";

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

        // 파생 컬럼: PRIORITY(Normal/Watch/Warning/Critical), DAYS_COLOR(Days 배지
        // 배경색 — 경과일 구간 틴트), CHK(벌크 대상 체크박스), LOGIS_CAN(행 단위
        // 물류처리 버튼 활성 여부 = 아직 미처리)을 채운다.
        private static void ApplyDerivedColumns(DataTable pending)
        {
            if (!pending.Columns.Contains("PRIORITY"))
            {
                pending.Columns.Add("PRIORITY", typeof(string));
            }

            if (!pending.Columns.Contains("DAYS_COLOR"))
            {
                pending.Columns.Add("DAYS_COLOR", typeof(string));
            }

            if (!pending.Columns.Contains("CHK"))
            {
                pending.Columns.Add("CHK", typeof(bool));
            }

            if (!pending.Columns.Contains("LOGIS_CAN"))
            {
                pending.Columns.Add("LOGIS_CAN", typeof(bool));
            }

            foreach (DataRow row in pending.Rows)
            {
                int days = ParseDays(ToText(row, "ELAPSED_DAYS"));
                int band;

                if (days >= 14)
                {
                    row["PRIORITY"] = "Critical";
                    band = 3;
                }
                else if (days >= 7)
                {
                    row["PRIORITY"] = "Warning";
                    band = 2;
                }
                else if (days >= 3)
                {
                    row["PRIORITY"] = "Watch";
                    band = 1;
                }
                else
                {
                    row["PRIORITY"] = "Normal";
                    band = 0;
                }

                row["DAYS_COLOR"] = agingBadgeColors[band];

                // 기존 컬럼에 행이 이미 있던 경우 DBNull로 남으므로 명시적으로 채운다.
                if (row.IsNull("CHK"))
                {
                    row["CHK"] = false;
                }

                row["LOGIS_CAN"] = !IsLogisticsDone(row);
            }
        }

        // 물류처리 완료 여부 — LOGIS_YN이 "Y"면 완료로 본다.
        private static bool IsLogisticsDone(DataRow row)
        {
            return "Y".Equals(ToText(row, "LOGIS_YN").Trim(), StringComparison.OrdinalIgnoreCase);
        }

        // 물류처리 라디오(All/Pending/Done)에 맞춰 조회 결과를 잘라낸다.
        // ★ 회사 환경 교체 지점 — 서버가 LOGIS_YN 필터를 지원하면 조회 조건으로
        // 넘기고 이 클라이언트 필터는 제거한다.
        private static DataTable ApplyLogisticsFilter(DataTable pending, string logisticsFilter)
        {
            if (logisticsFilter.Length == 0)
            {
                return pending;
            }

            bool wantDone = logisticsFilter == "Y";
            DataTable filtered = pending.Clone();

            foreach (DataRow row in pending.Rows)
            {
                if (IsLogisticsDone(row) == wantDone)
                {
                    filtered.ImportRow(row);
                }
            }

            return filtered;
        }

        // ===== 페이지 =====

        private void OnPageChanged(object sender, EventArgs e)
        {
            if (this.suppressPageEvent)
            {
                return;
            }

            this.BindCurrentPage();
        }

        // 조회 결과에서 현재 페이지 구간만 잘라 그리드에 바인딩한다 (로컬 슬라이스).
        private void BindCurrentPage()
        {
            if (this.resultData == null)
            {
                this.gridItems.DataSource = null;
                return;
            }

            DataTable page = this.resultData.Clone();
            int start = (this.pagination.CurrentPage - 1) * pageSize;
            int end = Math.Min(start + pageSize, this.resultData.Rows.Count);

            for (int index = start; index < end; index++)
            {
                page.ImportRow(this.resultData.Rows[index]);
            }

            // 페이지 조각은 원본의 복사본이라, 체크박스 토글을 원본(resultData)에
            // 되돌려야 페이지를 오가도 체크 상태가 유지된다.
            page.ColumnChanged += this.OnPageColumnChanged;

            this.gridItems.DataSource = page;
        }

        // 페이지 조각에서 바뀐 체크 상태를 조회 결과 원본에 반영한다.
        private void OnPageColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Column.ColumnName != "CHK")
            {
                return;
            }

            DataRow source = this.FindResultRow(ToText(e.Row, "ITEM_ID"));

            if (source != null)
            {
                source["CHK"] = e.ProposedValue;
            }
        }

        // 조회 결과 원본에서 Item ID로 행을 찾는다 (워크리스트라 Item ID는 유일).
        private DataRow FindResultRow(string itemId)
        {
            if (this.resultData == null || itemId.Length == 0)
            {
                return null;
            }

            foreach (DataRow row in this.resultData.Rows)
            {
                if (ToText(row, "ITEM_ID") == itemId)
                {
                    return row;
                }
            }

            return null;
        }

        // ===== KPI + 경과일 분포 =====

        private void UpdateKpis()
        {
            int itemCount = this.resultData.Rows.Count;
            int unitTotal = 0;
            int daysSum = 0;
            int daysMax = 0;

            foreach (DataRow row in this.resultData.Rows)
            {
                unitTotal += ParseDays(ToText(row, "UNIT_CNT"));

                int days = ParseDays(ToText(row, "ELAPSED_DAYS"));
                daysSum += days;

                if (days > daysMax)
                {
                    daysMax = days;
                }
            }

            this.badgePending.Text = "Pending " + itemCount.ToString("N0");
            this.badgeUnits.Text = "Units " + unitTotal.ToString("N0");
            this.badgeAvg.Text = itemCount > 0
                    ? "Avg " + ((double)daysSum / itemCount).ToString("0.0", CultureInfo.InvariantCulture) + " d"
                    : "Avg -";
            this.badgeOldest.Text = itemCount > 0 ? "Oldest " + daysMax.ToString("N0") + " d" : "Oldest -";
        }

        // 경과일 구간(0-2 / 3-6 / 7-13 / 14+)별 건수를 집계해 분포 배지에 표기한다.
        private void UpdateAgingCounts()
        {
            for (int index = 0; index < this.agingCounts.Length; index++)
            {
                this.agingCounts[index] = 0;
            }

            foreach (DataRow row in this.resultData.Rows)
            {
                int days = ParseDays(ToText(row, "ELAPSED_DAYS"));

                if (days >= 14)
                {
                    this.agingCounts[3] = this.agingCounts[3] + 1;
                }
                else if (days >= 7)
                {
                    this.agingCounts[2] = this.agingCounts[2] + 1;
                }
                else if (days >= 3)
                {
                    this.agingCounts[1] = this.agingCounts[1] + 1;
                }
                else
                {
                    this.agingCounts[0] = this.agingCounts[0] + 1;
                }
            }

            for (int index = 0; index < this.agingCounts.Length; index++)
            {
                this.badgeAging[index].Text = agingLabels[index] + " · " + this.agingCounts[index].ToString("N0");
            }
        }

        // ===== Item 선택 → Unit 리스트 =====

        private void OnItemSelectionChanged(object sender, EventArgs e)
        {
            DataRowView row = this.gridItems.SelectedItem as DataRowView;
            if (row == null)
            {
                return;
            }

            string itemId = ToText(row.Row, "ITEM_ID");
            if (string.IsNullOrEmpty(itemId))
            {
                return;
            }

            this.LoadUnits(itemId);
        }

        // 선택 Item의 Unit 목록을 백그라운드에서 불러온다 (보조 정보라 로딩 팝업 없음).
        private void LoadUnits(string itemId)
        {
            this.unitsVersion = this.unitsVersion + 1;
            int version = this.unitsVersion;

            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                try
                {
                    DataTable units = this.RequestUnits(itemId);

                    this.Invoke(new MethodInvoker(delegate
                    {
                        // 그 사이 다른 Item이 선택됐으면 이 응답은 버린다.
                        if (version != this.unitsVersion)
                        {
                            return;
                        }

                        EnsureColumns(units, "UNIT_ID", "SUB_TYP", "STAT_TYP", "EVENT_TM");
                        this.gridUnits.DataSource = units;
                        this.unitCard.Text = "Units — " + itemId;
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

        // ===== 실행 패널 =====

        // Export Excel: 조회 결과 전체(현재 페이지가 아니라)를 CSV(Excel 호환)로 저장.
        // UTF-8 BOM을 붙여 Excel이 인코딩을 바로 인식하게 한다.
        private void OnExportClick(object sender, EventArgs e)
        {
            if (this.resultData == null || this.resultData.Rows.Count == 0)
            {
                this.toastMain.Show("Nothing to export. Search first.", ToastKind.Warning);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV (Excel compatible)|*.csv";
                dialog.FileName = "PendingRequests_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    this.WriteCsv(dialog.FileName);
                    this.toastMain.Show(
                            this.resultData.Rows.Count.ToString("N0") + " items exported.", ToastKind.Success);
                }
                catch (Exception ex)
                {
                    this.toastMain.Show("Export failed: " + ex.Message, ToastKind.Error);
                }
            }
        }

        // 내보낼 컬럼(그리드 표시 순서와 동일)과 헤더.
        private static readonly string[] exportColumns =
        {
            "ITEM_ID", "ELAPSED_DAYS", "PRIORITY", "UNIT_CNT", "LOGIS_YN", "EVENT_TM",
            "MODEL_ID", "SUB_TYP", "ORG_ITEM_ID", "BOX_ID", "STORE_ID", "DESCRIPTION"
        };

        private static readonly string[] exportHeaders =
        {
            "Item ID", "Days", "Priority", "Units", "Logistics", "Arrived At",
            "Product", "Type", "Org Item", "Carrier", "Stocker", "Description"
        };

        private void WriteCsv(string path)
        {
            StringBuilder csv = new StringBuilder();
            csv.AppendLine(string.Join(",", exportHeaders));

            foreach (DataRow row in this.resultData.Rows)
            {
                string[] cells = new string[exportColumns.Length];

                for (int index = 0; index < exportColumns.Length; index++)
                {
                    cells[index] = EscapeCsv(ToText(row, exportColumns[index]));
                }

                csv.AppendLine(string.Join(",", cells));
            }

            // BOM 포함 UTF-8 — Excel이 더블클릭만으로 인코딩을 인식한다.
            File.WriteAllText(path, csv.ToString(), new UTF8Encoding(true));
        }

        private static string EscapeCsv(string value)
        {
            if (value.IndexOfAny(new char[] { ',', '"', '\n', '\r' }) < 0)
            {
                return value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        // 체크된 행 목록 (조회 결과 원본 기준 — 페이지를 오가며 체크한 것 전부).
        private List<DataRow> GetCheckedRows()
        {
            List<DataRow> checkedRows = new List<DataRow>();

            if (this.resultData == null)
            {
                return checkedRows;
            }

            foreach (DataRow row in this.resultData.Rows)
            {
                object value = row["CHK"];

                if (value != DBNull.Value && (bool)value)
                {
                    checkedRows.Add(row);
                }
            }

            return checkedRows;
        }

        // Return: 체크된 행을 반송 처리한다.
        // ★ 회사 환경 교체 지점 — 홈 환경에는 대상 시스템이 없어 데모 알림만 띄운다.
        //   회사 적용 시 이 본문을 사내 반송 인터페이스(전문 등) 호출로 바꾼다.
        private void OnReturnClick(object sender, EventArgs e)
        {
            List<DataRow> checkedRows = this.GetCheckedRows();

            if (checkedRows.Count == 0)
            {
                this.toastMain.Show("Check items to return first.", ToastKind.Warning);
                return;
            }

            foreach (DataRow row in checkedRows)
            {
                row["CHK"] = false;
            }

            this.BindCurrentPage();
            this.toastMain.Show(checkedRows.Count.ToString("N0") + " item(s) returned. (demo)", ToastKind.Success);
        }

        // Logistics: 체크된 행 중 아직 물류처리되지 않은 건만 처리한다.
        // ★ 회사 환경 교체 지점 — 데모는 LOGIS_YN만 "Y"로 바꾸고 알림을 띄운다.
        //   회사 적용 시 이 본문을 물류 시스템 인터페이스 호출로 바꾼다.
        private void OnLogisticsClick(object sender, EventArgs e)
        {
            List<DataRow> checkedRows = this.GetCheckedRows();

            if (checkedRows.Count == 0)
            {
                this.toastMain.Show("Check items to process first.", ToastKind.Warning);
                return;
            }

            int processed = 0;

            foreach (DataRow row in checkedRows)
            {
                if (!IsLogisticsDone(row))
                {
                    MarkLogisticsDone(row);
                    processed = processed + 1;
                }

                row["CHK"] = false;
            }

            this.BindCurrentPage();

            if (processed == 0)
            {
                this.toastMain.Show("All checked items are already processed.", ToastKind.Warning);
                return;
            }

            this.toastMain.Show(processed.ToString("N0") + " item(s) sent to logistics. (demo)", ToastKind.Success);
        }

        // 행 단위 물류처리: 그리드 Logistics 컬럼의 Process 버튼 (미처리 행만 활성).
        // ★ 회사 환경 교체 지점 — OnLogisticsClick과 동일한 인터페이스로 교체한다.
        private void OnGridCellButtonClick(object sender, GridButtonClickEventArgs e)
        {
            if (e.DataPropertyName != "LOGIS_YN")
            {
                return;
            }

            DataRowView row = e.Item as DataRowView;

            if (row == null)
            {
                return;
            }

            string itemId = ToText(row.Row, "ITEM_ID");

            // 페이지 조각(화면)과 조회 결과 원본을 함께 갱신한다 — 버튼은
            // LOGIS_CAN 바인딩으로 즉시 비활성화된다.
            MarkLogisticsDone(row.Row);
            DataRow source = this.FindResultRow(itemId);

            if (source != null)
            {
                MarkLogisticsDone(source);
            }

            this.toastMain.Show("Logistics processed for " + itemId + ". (demo)", ToastKind.Success);
        }

        // 물류처리 완료 상태로 표시한다 (LOGIS_YN=Y, 행 버튼 비활성).
        private static void MarkLogisticsDone(DataRow row)
        {
            row["LOGIS_YN"] = "Y";
            row["LOGIS_CAN"] = false;
        }

        // ===== 공통 헬퍼 =====

        // 서버 숫자 컬럼(JSON number)을 관용적으로 파싱한다 — 빈 값/소수 표기 모두 허용.
        private static int ParseDays(string text)
        {
            double value;

            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                return (int)value;
            }

            return 0;
        }

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
