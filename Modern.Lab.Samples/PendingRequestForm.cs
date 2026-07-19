using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Pending Requests — 공장 간 물류 인수 파이프라인의 조회 + 수동 처리 화면.
    ///
    /// 업무 흐름 (2026-07-17 FAC_SEND_MAS 기반 재정의):
    /// 1) 보내는 쪽이 발송 통보를 FAC_SEND_MAS(ITEM_ID/BOX_ID/SEND_TM/SEND_FAC/
    ///    RECV_YN='N')에 적재한다.
    /// 2) 물류가 실제 도착하면 의뢰 인터페이스(가제 IF_REQ_MAS)에 ITEM_ID와
    ///    물류도착시간이 적재된다.
    /// 3) Receive 처리로 전산에 ITEM(Created → Released)이 생성되고 RECV_YN/RECV_TM이
    ///    채워진다.
    /// 4) 의뢰서가 있으면 의뢰내용(REQ_NO/SAMPLE_NM …)이 연결되고, 연결되면 다음
    ///    단계(Create 처리 — PROC_YN/PROC_TM)로 자동 진행한다.
    /// 이 과정은 원래 전부 자동이다. 이 화면은 **자동이 일부/전부 실패한 건의 수동
    /// 처리**(Receive/Create)와, 도착했지만 의뢰서가 없는 목록 + 전체 현황을
    /// 현업에게 보여주기 위해 존재한다.
    ///
    /// 조회 기준은 FAC_SEND_MAS와 의뢰 인터페이스를 합친 **단일 현황판**
    /// (FULL OUTER JOIN 상당)이다 — 발송 통보 없이 도착한 미확인 물류도 의뢰
    /// 인터페이스는 타므로 동일 레벨의 행으로 취급하고, FAC_SEND_MAS에 있냐
    /// 없냐만 Sent 배지(SEND_YN — Y 초록 / N 빨강 틴트)와 필터로 구분한다.
    ///
    /// 영역 구성 (계약 룰 5 — 레이아웃은 WinForms 담당, 배치는 .Designer.cs):
    /// - 상단: 조회 카드 (Item ID 부분일치 + 상태 필터 + Sent 필터 + 경과일 필터)
    /// - 중단 좌측: 현황판 그리드 (체크박스 · Sent 배지 · 상태 배지 · Days 배지 ·
    ///   행 단위 Receive/Create 버튼 · 페이지 바)
    /// - 중단 우측: 선택 Item의 Unit 리스트 (MES_UNIT_MAS 현재 상태) + 선택 행
    ///   파이프라인 스텝 인디케이터 (Units는 보통 6개 이하라 남는 하단 공간 활용)
    /// - 하단 좌측: 상태별 KPI 배지 + 미확인 건수 + 미연결 경과 통계(Avg/Oldest)
    /// - 하단 우측 실행 카드: Excel / Receive(다이얼로그 — 체크 목록 확인 후
    ///   일괄 처리 또는 매뉴얼 강제 수신) / Create(다이얼로그 — 체크 목록 확인
    ///   후 일괄 처리)
    ///
    /// 상태 규칙 (파생·필터·집계는 전부 PendingTablePresenter):
    ///   Sent(발송 통보) → Arrived(→ Receive) → Request Unlinked(경과일 강조) →
    ///   Request Linked(→ Create) → Completed. 상태는 "아직 안 된 첫 단계"라 자동이
    ///   어디서 멈췄든 다음 할 일이 하나로 정해진다. 경과일은 운송 중(발송
    ///   시각 기준 — 운송 지연)과 도착 후 의뢰서 미연결(도착 시각 기준 —
    ///   방치 기간) 행에 표기한다. 화면 표기는 전부 영어.
    ///
    /// 서버 호출은 "서버 조회 (★ 회사 환경 교체 지점)" 영역의 private 메서드
    /// 2개(도착 목록/Unit 목록)에만 있다. 홈 환경은 도착 목록 API 하나뿐이라
    /// FAC_SEND_MAS/의뢰 인터페이스는 PendingInterfaceSimulator가 결정적으로
    /// 시뮬레이션한다 — 회사 적용 시 시뮬레이터를 지우고 조인 현황판을 서버에서
    /// 직접 내려받는다.
    ///
    /// Receive/Create 처리 흐름: 서버 처리(데모는 시뮬레이터의 서버측 저장소,
    /// 회사는 ITEM 생성 전문/의뢰 처리 갱신 인터페이스) → 성공 시 **재조회**
    /// (조회 조건·페이지·행 포커스 유지). 처리 시각(RECV_TM/PROC_TM)은 서버가
    /// 적재하는 값이므로 화면이 만들지 않고 재조회 결과를 그대로 보여준다.
    /// </summary>
    public partial class PendingRequestForm : Form
    {
        // ===== 홈 환경 API (★ 회사 환경 교체 지점) =====

        /// <summary>홈 환경 API 주소 — 회사 적용 시 함께 제거한다.</summary>
        private const string apiBaseUrl = "http://localhost:8080";

        /// <summary>API 호출 제한 시간(ms).</summary>
        private const int apiTimeoutMs = 5000;

        // ===== 상태 필드 =====

        // 마지막 조회의 현황판 필터 결과 전체 (KPI/엑셀의 원천).
        private DataTable resultData;

        // resultData에 그리드 컬럼 필터(헤더 깔때기)를 적용한 표시 행 목록 —
        // 페이지 슬라이스와 포커스 복원은 이 목록 기준이다. 원본 행을 그대로
        // 참조하므로 체크 상태 동기화가 유지된다.
        private List<DataRow> viewRows;

        // 코드로 CurrentPage를 되돌릴 때 PageChanged 재진입을 막는다.
        private bool suppressPageEvent;

        // 조회 버전 — 빠른 재조회 시 오래된 응답을 버린다.
        private int searchVersion;

        // Unit 조회 버전 — 빠른 재선택 시 오래된 응답을 버린다.
        private int unitsVersion;

        public PendingRequestForm()
        {
            this.InitializeComponent();

            // 로딩 커버 한 줄 — 폼 스스로 오픈 시 깜빡임(WPF 콘텐츠 생성·컬럼
            // AutoFit 중간 레이아웃)을 가린다. 메인 프레임(여는 쪽) 수정 불필요.
            Modern.Lab.WinForms.Controls.Hosting.ModernLoadCover.Attach(this);
        }

        // 백그라운드 작업의 UI 반영 공통 경로. 폼이 닫히는 중이면 Invoke를
        // 시도하지 않고, 확인 직후 닫히는 경쟁 상태도 예외 없이 무시한다.
        private void PostToUi(MethodInvoker action)
        {
            if (this.IsDisposed || this.Disposing || !this.IsHandleCreated)
            {
                return;
            }

            try
            {
                this.BeginInvoke(action);
            }
            catch (ObjectDisposedException)
            {
                // Dispose와 동시에 끝난 백그라운드 요청은 버린다.
            }
            catch (InvalidOperationException)
            {
                // 핸들이 해제된 직후의 반영 요청은 버린다.
            }
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

        // ===== 서버 조회 (★ 회사 환경 교체 지점) =====

        // 물류 도착 목록 — 홈 환경에서 유일하게 실재하는 원천이다. 회사 적용 시
        // 이 메서드를 FAC_SEND_MAS FULL OUTER JOIN IF_REQ_MAS 현황판 조회
        // (SEND_YN 포함)로 바꾼다. 상태 파생/필터/집계는 그대로 클라이언트
        // (PendingTablePresenter)가 처리한다.
        private DataTable RequestArrivals(string keyword)
        {
            StringBuilder query = new StringBuilder();
            query.Append("/api/items/pending-requests");

            if (!string.IsNullOrEmpty(keyword))
            {
                query.Append("?keyword=").Append(Uri.EscapeDataString(keyword));
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
            // 상태 필터: All + 파이프라인 5단계 — 값은 STATUS 컬럼 문자열 그대로.
            DataTable statusTable = new DataTable();
            statusTable.Columns.Add("VALUE", typeof(string));
            statusTable.Columns.Add("LABEL", typeof(string));
            statusTable.Rows.Add("", "All");

            foreach (string statusName in PendingTablePresenter.StatusNames)
            {
                statusTable.Rows.Add(statusName, statusName);
            }

            this.cboStatus.DisplayMember = "LABEL";
            this.cboStatus.ValueMember = "VALUE";
            this.cboStatus.DataSource = statusTable;
            this.cboStatus.SelectedValue = "";

            // 발송 통보 필터: All / Notified(FAC_SEND_MAS 있음) / Unmatched(미확인).
            DataTable sendTable = new DataTable();
            sendTable.Columns.Add("VALUE", typeof(string));
            sendTable.Columns.Add("LABEL", typeof(string));
            sendTable.Rows.Add("", "All");
            sendTable.Rows.Add("Y", "Notified");
            sendTable.Rows.Add("N", "Unmatched");

            this.cboSend.DisplayMember = "LABEL";
            this.cboSend.ValueMember = "VALUE";
            this.cboSend.DataSource = sendTable;
            this.cboSend.SelectedValue = "";

            // 경과일 필터: All(0) / 1+ / 3+ / 7+ / 14+ 일 — 미연결 건의 방치 기간.
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

            // Item ID 자동완성 콤보(검색형): 후보 목록은 전체 조회 결과로 채운다.
            this.cboItemId.DisplayMember = "ITEM_ID";
            this.cboItemId.ValueMember = "ITEM_ID";

            // 컬럼 정의만 코드에서 구성한다 (디자이너 직렬화 대상이 아님).
            // 캡션은 용어사전(GridCaptionDictionary) 기본값을 쓴다 — 버튼 컬럼처럼
            // 이 화면 전용 필드만 캡션을 명시한다. 이 정의는 엑셀 내보내기의
            // 원천이기도 하다 (체크박스/버튼 컬럼은 내보내기가 알아서 제외).
            //
            // 현황판: 체크박스(벌크 대상) + Sent 배지(발송 통보 여부) + 상태
            // 배지 + Days 배지(미연결만 색) + 행 단위 Receive/Create 버튼(해당
            // 단계 행만 활성) + 발송/도착/수신/의뢰 정보.
            this.gridBoard.ConfigureColumns(
                new ModernDataGridColumn("CHK", "", GridWidths.Check)
                {
                    Kind = GridColumnKind.CheckBox,
                    // 헤더 체크박스 = 현재 페이지 전체 선택/해제.
                    HeaderCheckBox = true
                },
                new ModernDataGridColumn("ITEM_ID"),
                new ModernDataGridColumn("SEND_YN")
                {
                    Kind = GridColumnKind.Badge,
                    BadgeColorMember = "SEND_COLOR",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("STATUS")
                {
                    Kind = GridColumnKind.Badge,
                    BadgeColorMember = "STATUS_COLOR",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("ELAPSED_DAYS")
                {
                    Kind = GridColumnKind.Badge,
                    BadgeColorMember = "DAYS_COLOR",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("RECV_ACTION", "Receive")
                {
                    Kind = GridColumnKind.Button,
                    ButtonText = "Receive",
                    ButtonEnabledMember = "RECV_CAN",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("CRT_ACTION", "Create")
                {
                    Kind = GridColumnKind.Button,
                    ButtonText = "Create",
                    ButtonEnabledMember = "CRT_CAN",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("SEND_FAC") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("SEND_TM") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("BOX_ID") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("ARRIVE_TM") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("RECV_TM") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("ITEM_STAT") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("REQ_NO"),
                new ModernDataGridColumn("SAMPLE_NM"),
                new ModernDataGridColumn("PROC_TM") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("RECV_DESC"));

            // Unit 리스트: 좁은 패널에 맞는 최소 컬럼.
            this.gridUnits.ConfigureColumns(
                new ModernDataGridColumn("UNIT_ID"),
                new ModernDataGridColumn("SUB_TYP", "Type") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STAT_TYP") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("EVENT_TM", "Arrived At") { TextAlignment = GridTextAlignment.Center });

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
            this.cboStatus.SelectedValue = "";
            this.cboSend.SelectedValue = "";
            this.cboElapsed.SelectedValue = "0";
            this.ExecuteSearch();
        }

        // 현재 선택된 상태 필터 값 ("" = 전체, 그 외 = StatusNames 문자열).
        private string GetStatusFilter()
        {
            string value = this.cboStatus.SelectedValue as string;
            return value ?? string.Empty;
        }

        // 현재 선택된 발송 통보 필터 값 ("" = 전체, "Y" = 통보 있음, "N" = 미확인).
        private string GetSendFilter()
        {
            string value = this.cboSend.SelectedValue as string;
            return value ?? string.Empty;
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

        // 백그라운드에서 서버를 호출하고 UI 스레드로 복귀해 반영한다.
        // 반영 순서: 도착 목록 → 현황판 구성(★ 시뮬레이터) → 상태·경과 파생 →
        // 필터(상태+통보+경과일) → 페이지 1 바인딩 → KPI 갱신.
        private void ExecuteSearch()
        {
            this.ExecuteSearch(false, null, -1);
        }

        // keepPage: 수동 Receive/Create 직후 재조회처럼 보던 페이지를 유지하고
        // 싶을 때 true — 결과 건수가 줄어 페이지가 사라졌으면 마지막 페이지로
        // 보정한다. 조회 버튼/초기 조회는 false(1페이지부터).
        // focusItemId/focusIndex: 재조회 후 되돌릴 행 포커스 — 처리 전에 보던
        // 행(Item ID)을 다시 선택하고, 필터로 사라졌으면 같은 위치(focusIndex)의
        // 행을 대신 선택한다. null/-1이면 기본 동작(첫 행 선택).
        private void ExecuteSearch(bool keepPage, string focusItemId, int focusIndex)
        {
            string keyword = this.cboItemId.Text.Trim();
            string statusFilter = this.GetStatusFilter();
            string sendFilter = this.GetSendFilter();
            int minDays = this.GetMinDays();
            int requestedPage = keepPage ? this.pagination.CurrentPage : 1;

            this.busyMain.Busy = true;
            this.searchVersion = this.searchVersion + 1;
            int version = this.searchVersion;

            // 새 검색부터는 이전 선택 행의 Unit 조회도 현재 화면과 무관하다.
            // 늦게 도착한 응답이 새 현황판 우측 패널을 채우지 않게 무효화한다.
            this.unitsVersion = this.unitsVersion + 1;
            this.gridUnits.DataSource = null;
            this.unitCard.Text = "Units";
            this.stepPipeline.DataSource = null;

            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                try
                {
                    DataTable arrivals = this.RequestArrivals(keyword);

                    this.PostToUi(new MethodInvoker(delegate
                    {
                        // 그 사이 새 조회가 나갔으면 이 응답은 버린다.
                        if (version != this.searchVersion)
                        {
                            return;
                        }

                        // 경과일(Days)은 이 시각의 클라이언트 날짜 기준으로 계산되므로,
                        // 계산 기준 일시를 현황판 타이틀 오른쪽에 표기한다.
                        DateTime daysBasis = DateTime.Now;

                        // ★ 회사 환경 교체 지점 — 조인 현황판을 서버에서 직접
                        // 내려받으면 이 한 줄이 서버 호출 결과로 바뀐다.
                        DataTable board = PendingInterfaceSimulator.Build(arrivals);

                        PendingTablePresenter.ApplyWorkflowColumns(board);
                        this.resultData = PendingTablePresenter.Filter(
                                board, statusFilter, sendFilter, minDays);

                        // 그리드 컬럼 필터: 값 체크리스트는 페이지 조각이 아니라
                        // 조회 결과 전체에서 모으고, 표시 행 목록에 필터를 반영한다.
                        this.gridBoard.FilterValueSource = this.resultData;
                        this.RefreshViewRows();

                        this.boardCard.TitleRightText =
                                "Days as of " + daysBasis.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                        // 페이지 바: 페이지 크기는 그리드 높이에 맞춰(화면 크기 자동 계산),
                        // 전체 건수 반영 후 요청 페이지(기본 1)로 — 재조회로 결과가
                        // 줄었으면 마지막 페이지로 보정한다.
                        this.suppressPageEvent = true;

                        try
                        {
                            this.pagination.PageSize = this.gridBoard.VisibleRowCapacity;
                            this.pagination.TotalCount = this.viewRows.Count;

                            int pageSize = Math.Max(1, this.pagination.PageSize);
                            int pageCount = ((this.viewRows.Count - 1) / pageSize) + 1;

                            if (pageCount < 1)
                            {
                                pageCount = 1;
                            }

                            this.pagination.CurrentPage =
                                    Math.Max(1, Math.Min(requestedPage, pageCount));
                        }
                        finally
                        {
                            this.suppressPageEvent = false;
                        }

                        this.BindCurrentPage();
                        this.RefreshSummary();

                        // 전체 조회(조건 없음)일 때 그 결과로 Item ID 자동완성
                        // 후보를 갱신한다 — DataSource 할당의 첫 행 자동 선택은
                        // 텍스트를 비워 되돌린다 (조건이 채워진 채 남지 않게).
                        // (빈 결과는 ITEM_ID 컬럼 자체가 없을 수 있어 건너뛴다.)
                        if (keyword.Length == 0 && statusFilter.Length == 0
                                && sendFilter.Length == 0 && minDays == 0
                                && arrivals.Columns.Contains("ITEM_ID"))
                        {
                            this.cboItemId.DataSource = arrivals.DefaultView.ToTable(false, "ITEM_ID");
                            this.cboItemId.Text = string.Empty;
                        }

                        this.gridUnits.DataSource = null;
                        this.unitCard.Text = "Units";

                        // 수동 처리 직후 재조회면 처리 전에 보던 행으로 포커스를
                        // 되돌린다 — 선택 변경 이벤트가 Units 패널도 그 행 기준으로
                        // 다시 채운다.
                        if (keepPage)
                        {
                            this.FocusBoardRow(focusItemId, focusIndex);
                        }

                        this.busyMain.Busy = false;
                    }));
                }
                catch (Exception ex)
                {
                    this.PostToUi(new MethodInvoker(delegate
                    {
                        // 이전 조회의 실패는 최신 조회의 로딩/오류 상태를 바꾸지 않는다.
                        if (version != this.searchVersion)
                        {
                            return;
                        }

                        this.busyMain.Busy = false;
                        this.toastMain.Show("Server call failed: " + ex.Message, ToastKind.Error);
                    }));
                }
            });
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

        // 그리드 높이 변화(창 크기 조절, 스플리터 이동)로 표시 가능 행 수가 바뀌면
        // 페이지 크기를 다시 맞춘다 — 페이지는 고정 건수가 아니라 화면 크기에 따라
        // 자동 계산된다. 보고 있던 첫 행이 계속 보이도록 페이지 번호를 환산한다.
        private void OnGridCapacityChanged(object sender, EventArgs e)
        {
            int capacity = this.gridBoard.VisibleRowCapacity;

            if (capacity == this.pagination.PageSize)
            {
                return;
            }

            int firstIndex = (this.pagination.CurrentPage - 1) * this.pagination.PageSize;

            this.suppressPageEvent = true;

            try
            {
                this.pagination.PageSize = capacity;
                this.pagination.CurrentPage = (firstIndex / capacity) + 1;
            }
            finally
            {
                this.suppressPageEvent = false;
            }

            this.BindCurrentPage();
        }

        // 그리드 컬럼 필터를 통과한 표시 행 목록을 다시 만든다 — 원본 행을
        // 그대로 참조해 체크 토글이 원본과 어긋나지 않는다.
        private void RefreshViewRows()
        {
            this.viewRows = new List<DataRow>();

            if (this.resultData == null)
            {
                return;
            }

            foreach (DataRow row in this.resultData.Rows)
            {
                if (this.gridBoard.MatchesColumnFilters(row))
                {
                    this.viewRows.Add(row);
                }
            }
        }

        // 컬럼 필터(헤더 깔때기)가 바뀌면 표시 행 목록과 페이지를 재계산한다 —
        // 필터는 현재 페이지 조각이 아니라 조회 결과 전체에 적용된다.
        private void OnBoardColumnFiltersChanged(object sender, EventArgs e)
        {
            this.RefreshViewRows();

            this.suppressPageEvent = true;

            try
            {
                this.pagination.TotalCount = this.viewRows.Count;

                int pageSize = Math.Max(1, this.pagination.PageSize);
                int pageCount = ((this.viewRows.Count - 1) / pageSize) + 1;
                this.pagination.CurrentPage =
                        Math.Max(1, Math.Min(this.pagination.CurrentPage, pageCount));
            }
            finally
            {
                this.suppressPageEvent = false;
            }

            this.BindCurrentPage();
        }

        // 표시 행 목록에서 현재 페이지 구간만 잘라 그리드에 바인딩한다 (로컬 슬라이스).
        private void BindCurrentPage()
        {
            if (this.resultData == null || this.viewRows == null)
            {
                this.gridBoard.DataSource = null;
                return;
            }

            int pageSize = this.pagination.PageSize;
            DataTable page = this.resultData.Clone();
            int start = (this.pagination.CurrentPage - 1) * pageSize;
            int end = Math.Min(start + pageSize, this.viewRows.Count);

            for (int index = start; index < end; index++)
            {
                page.ImportRow(this.viewRows[index]);
            }

            // 페이지 조각은 원본의 복사본이라, 체크박스 토글을 원본(resultData)에
            // 되돌려야 페이지를 오가도 체크 상태가 유지된다.
            page.ColumnChanged += this.OnPageColumnChanged;

            this.gridBoard.DataSource = page;
        }

        // 페이지 조각에서 바뀐 체크 상태를 조회 결과 원본에 반영한다.
        private void OnPageColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Column.ColumnName != "CHK")
            {
                return;
            }

            DataRow source = this.FindResultRow(PendingTablePresenter.CellText(e.Row, "ITEM_ID"));

            if (source != null)
            {
                source["CHK"] = e.ProposedValue;
            }
        }

        // 현황판에서 현재 포커스된 행의 Item ID ("" = 선택 없음).
        private string GetFocusedItemId()
        {
            DataRowView row = this.gridBoard.SelectedItem as DataRowView;
            return row == null ? string.Empty : PendingTablePresenter.CellText(row.Row, "ITEM_ID");
        }

        // 재조회 후 행 포커스를 복원한다 — Item ID를 표시 행 목록 전체에서 찾아
        // 다른 페이지에 있으면 그 페이지로 이동해 선택한다 (매뉴얼 Receive로
        // 새로 올라온 행도 이걸로 찾아간다). 필터로 사라졌으면 현재 페이지의
        // 같은 위치(fallbackIndex) 행을 대신 선택한다.
        private void FocusBoardRow(string itemId, int fallbackIndex)
        {
            if (!string.IsNullOrEmpty(itemId) && this.viewRows != null)
            {
                int pageSize = Math.Max(1, this.pagination.PageSize);

                for (int index = 0; index < this.viewRows.Count; index++)
                {
                    if (PendingTablePresenter.CellText(this.viewRows[index], "ITEM_ID") != itemId)
                    {
                        continue;
                    }

                    int targetPage = (index / pageSize) + 1;

                    if (targetPage != this.pagination.CurrentPage)
                    {
                        this.suppressPageEvent = true;

                        try
                        {
                            this.pagination.CurrentPage = targetPage;
                        }
                        finally
                        {
                            this.suppressPageEvent = false;
                        }

                        this.BindCurrentPage();
                    }

                    this.gridBoard.SelectedIndex = index % pageSize;
                    return;
                }
            }

            DataTable page = this.gridBoard.DataSource as DataTable;

            if (page == null || page.Rows.Count == 0)
            {
                return;
            }

            this.gridBoard.SelectedIndex =
                    Math.Min(Math.Max(fallbackIndex, 0), page.Rows.Count - 1);
        }

        // 조회 결과 원본에서 Item ID로 행을 찾는다 (현황판의 Item ID는 유일).
        private DataRow FindResultRow(string itemId)
        {
            if (this.resultData == null || itemId.Length == 0)
            {
                return null;
            }

            foreach (DataRow row in this.resultData.Rows)
            {
                if (PendingTablePresenter.CellText(row, "ITEM_ID") == itemId)
                {
                    return row;
                }
            }

            return null;
        }

        // ===== KPI =====

        // 상태별 건수(전체 현황) + 미확인 물류 건수 + 미연결 경과 통계를 KPI
        // 배지에 표기한다. 조회(수동 처리 후 재조회 포함) 직후에 다시 계산한다.
        private void RefreshSummary()
        {
            PendingTablePresenter.PendingSummary summary =
                    PendingTablePresenter.Aggregate(this.resultData);

            this.badgeTransit.Text = "Sent "
                    + summary.StatusCounts[PendingTablePresenter.StatusTransit].ToString("N0");
            this.badgeArrived.Text = "Arrived "
                    + summary.StatusCounts[PendingTablePresenter.StatusArrived].ToString("N0");
            this.badgeNoReq.Text = "Unlinked "
                    + summary.StatusCounts[PendingTablePresenter.StatusNoRequest].ToString("N0");
            this.badgeLinked.Text = "Linked "
                    + summary.StatusCounts[PendingTablePresenter.StatusLinked].ToString("N0");
            this.badgeDone.Text = "Completed "
                    + summary.StatusCounts[PendingTablePresenter.StatusCompleted].ToString("N0");
            this.badgeUnmatched.Text = "Unmatched " + summary.UnmatchedCount.ToString("N0");

            this.badgeAvg.Text = summary.NoLinkCount > 0
                    ? "Avg " + summary.DaysAverage.ToString("0.0", CultureInfo.InvariantCulture) + " d"
                    : "Avg -";
            this.badgeOldest.Text = summary.NoLinkCount > 0
                    ? "Oldest " + summary.DaysMax.ToString("N0") + " d"
                    : "Oldest -";
        }

        // ===== Item 선택 → Unit 리스트 =====

        private void OnBoardSelectionChanged(object sender, EventArgs e)
        {
            DataRowView row = this.gridBoard.SelectedItem as DataRowView;

            if (row == null)
            {
                this.stepPipeline.DataSource = null;
                return;
            }

            this.UpdatePipeline(row.Row);

            string itemId = PendingTablePresenter.CellText(row.Row, "ITEM_ID");

            if (string.IsNullOrEmpty(itemId))
            {
                return;
            }

            this.LoadUnits(itemId);
        }

        // 선택 행의 파이프라인 위치를 우측 하단 Pipeline 카드의 스텝
        // 인디케이터에 보여준다. 상태(STATUS)가 "아직 안 된 첫 단계"이므로 그
        // 앞 단계는 완료, 그 단계가 현재다. Completed 상태는 전 단계 완료로
        // 표기한다.
        private void UpdatePipeline(DataRow row)
        {
            int status = Array.IndexOf(
                    PendingTablePresenter.StatusNames, PendingTablePresenter.CellText(row, "STATUS"));

            if (status < 0)
            {
                this.stepPipeline.DataSource = null;
                return;
            }

            DataTable steps = new DataTable();
            steps.Columns.Add("LABEL", typeof(string));
            steps.Columns.Add("STATE", typeof(string));

            for (int index = 0; index < PendingTablePresenter.StatusNames.Length; index++)
            {
                string state;

                if (index < status || status == PendingTablePresenter.StatusCompleted)
                {
                    state = "Completed";
                }
                else if (index == status)
                {
                    state = "Current";
                }
                else
                {
                    state = "Pending";
                }

                // 두 단어 상태("Request Unlinked" 등)는 좁은 셀에서 잘리지 않게
                // 스텝 레이블에서만 2줄로 표시한다 (배지/필터는 한 줄 그대로).
                steps.Rows.Add(
                        PendingTablePresenter.StatusNames[index].Replace(' ', '\n'), state);
            }

            this.stepPipeline.DataSource = steps;
        }

        // 선택 Item의 Unit 목록을 백그라운드에서 불러온다 (보조 정보라 로딩 팝업 없음).
        // 도착 전(운송 중) Item은 전산에 Unit이 아직 없어 빈 목록이 나온다.
        private void LoadUnits(string itemId)
        {
            this.unitsVersion = this.unitsVersion + 1;
            int version = this.unitsVersion;

            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                try
                {
                    DataTable units = this.RequestUnits(itemId);

                    this.PostToUi(new MethodInvoker(delegate
                    {
                        // 그 사이 다른 Item이 선택됐으면 이 응답은 버린다.
                        if (version != this.unitsVersion)
                        {
                            return;
                        }

                        this.gridUnits.DataSource = units;
                        this.unitCard.Text = "Units — " + itemId;
                    }));
                }
                catch (Exception ex)
                {
                    this.PostToUi(new MethodInvoker(delegate
                    {
                        // 이전 Item 선택의 오류는 현재 선택에 표시하지 않는다.
                        if (version != this.unitsVersion)
                        {
                            return;
                        }

                        this.toastMain.Show("Server call failed: " + ex.Message, ToastKind.Error);
                    }));
                }
            });
        }

        // ===== 수동 처리 =====

        // 체크된 행 중 지정 단계 대상만 담은 다이얼로그용 목록 — Receive는
        // RECV_CAN(Arrived), Create는 CRT_CAN(Request Linked) 행이 대상이다.
        // 페이지를 오가며 체크한 것 전부(조회 결과 원본 기준)를 훑는다.
        private DataTable BuildCheckedList(string actionFlag)
        {
            DataTable list = this.resultData != null ? this.resultData.Clone() : new DataTable();

            if (this.resultData != null)
            {
                foreach (DataRow row in this.resultData.Rows)
                {
                    if (PendingTablePresenter.FlagSet(row, "CHK")
                            && PendingTablePresenter.FlagSet(row, actionFlag))
                    {
                        list.ImportRow(row);
                    }
                }
            }

            return list;
        }

        // Receive: 다이얼로그에서 처리 방식을 고른다 — 체크된 아이템 목록을
        // 확인 후 일괄 수신하거나, 조회에 안 나오는 아이템을 Item ID + 발송
        // 공장 입력으로 강제 수신(매뉴얼)한다. 처리 시각(RECV_TM) 등은 서버가
        // 적재하므로 성공 후 **재조회**(조건·페이지 유지)로 반영하고, 매뉴얼
        // 수신이면 새로 올라온 그 행으로 포커스를 옮긴다.
        // (★ 회사 환경 교체 지점은 ReceiveDialogForm 안의 인터페이스 호출부다.)
        private void OnReceiveClick(object sender, EventArgs e)
        {
            using (ReceiveDialogForm dialog = new ReceiveDialogForm(this.BuildCheckedList("RECV_CAN")))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                string message = dialog.ManualItemId != null
                        ? "Item " + dialog.ManualItemId + " received."
                        : dialog.ProcessedCount.ToString("N0") + " item(s) received.";
                this.toastMain.Show(message, ToastKind.Success);

                string focusItemId = dialog.ManualItemId ?? this.GetFocusedItemId();
                this.ExecuteSearch(true, focusItemId, this.gridBoard.SelectedIndex);
            }
        }

        // Create: 다이얼로그에서 체크된 Create 대상 목록을 확인한 뒤 일괄
        // 처리한다 — 의뢰 인터페이스에 처리여부/처리시간이 기록되는 단계다.
        // 처리 시각(PROC_TM)은 서버가 적재하므로 성공 후 **재조회**(조건·페이지
        // 유지) → 보던 행 포커스 복원으로 반영한다. Receive와 달리 매뉴얼(강제)
        // 케이스가 없어 대상이 없으면 다이얼로그 없이 안내만 한다.
        // (★ 회사 환경 교체 지점은 CreateDialogForm 안의 인터페이스 호출부다.)
        private void OnCreateClick(object sender, EventArgs e)
        {
            DataTable createList = this.BuildCheckedList("CRT_CAN");

            if (createList.Rows.Count == 0)
            {
                this.toastMain.Show("Check items to create first.", ToastKind.Warning);
                return;
            }

            using (CreateDialogForm dialog = new CreateDialogForm(createList))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                this.toastMain.Show(
                        dialog.ProcessedCount.ToString("N0") + " item(s) created.", ToastKind.Success);
                this.ExecuteSearch(true, this.GetFocusedItemId(), this.gridBoard.SelectedIndex);
            }
        }

        // 행 단위 Receive/Create: 그리드 버튼 컬럼 (해당 단계 행만 활성).
        // 벌크 버튼과 동일하게 서버 처리 성공 후 재조회(조건·페이지 유지)로
        // 반영한다 — 서버가 적재한 RECV_TM/PROC_TM·상태가 그대로 내려온다.
        // ★ 회사 환경 교체 지점 — 벌크 버튼과 동일한 인터페이스로 교체한다.
        private void OnGridCellButtonClick(object sender, GridButtonClickEventArgs e)
        {
            if (e.DataPropertyName != "RECV_ACTION" && e.DataPropertyName != "CRT_ACTION")
            {
                return;
            }

            DataRowView row = e.Item as DataRowView;

            if (row == null)
            {
                return;
            }

            string itemId = PendingTablePresenter.CellText(row.Row, "ITEM_ID");

            if (e.DataPropertyName == "RECV_ACTION")
            {
                PendingInterfaceSimulator.ProcessReceive(itemId);
                this.toastMain.Show("Item " + itemId + " received.", ToastKind.Success);
            }
            else
            {
                PendingInterfaceSimulator.ProcessCreate(itemId);
                this.toastMain.Show("Item " + itemId + " created.", ToastKind.Success);
            }

            // 행 버튼으로 처리한 행이 처리 후에도 포커스를 유지한다.
            this.ExecuteSearch(true, itemId, this.gridBoard.SelectedIndex);
        }

        // ===== 내보내기 =====

        // Excel: 조회 결과 전체(현재 페이지가 아니라)를 진짜 Excel 파일
        // (.xlsx, Open XML)로 저장한다 — 그리드 ExportXlsx(외부 라이브러리 없음).
        private void OnExportClick(object sender, EventArgs e)
        {
            if (this.resultData == null || this.resultData.Rows.Count == 0)
            {
                this.toastMain.Show("Nothing to export. Search first.", ToastKind.Warning);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Excel Workbook|*.xlsx";
                dialog.FileName = "PendingRequests_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    // 그리드 컬럼 정의(화면 표시와 동일한 순서·캡션·형식)를 단일
                    // 원천으로 저장한다 — 체크박스/버튼 컬럼은 내보내기가 알아서
                    // 제외하므로 내보내기용 컬럼 목록을 따로 관리하지 않는다.
                    this.gridBoard.ExportXlsx(dialog.FileName, "Pending Requests", this.resultData);
                    this.toastMain.Show(
                            this.resultData.Rows.Count.ToString("N0") + " items exported.", ToastKind.Success);
                }
                catch (Exception ex)
                {
                    this.toastMain.Show("Export failed: " + ex.Message, ToastKind.Error);
                }
            }
        }
    }
}
