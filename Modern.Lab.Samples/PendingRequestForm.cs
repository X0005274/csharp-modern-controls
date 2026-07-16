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
using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Pending Requests — 다른 곳에서 도착(입고)했지만 의뢰서가 아직 작성되지 않은
    /// Item들의 워크리스트 화면. 경과일(도착 후 며칠 지났는지)을 Days 배지 색으로
    /// 강조하고(행 배경색은 쓰지 않는다), 체크박스로 고른 행을 반송(Return)/
    /// 물류처리(Logistics)한다.
    ///
    /// 영역 구성 (계약 룰 5 — 레이아웃은 WinForms 담당, 배치는 .Designer.cs):
    /// - 상단: 조회 카드 (Item ID 부분일치 + 경과일 필터 + 물류처리 라디오)
    /// - 중단: 미의뢰 Item 리스트 (체크박스 · Days 배지 · 행 단위 물류처리 버튼 ·
    ///         페이지 바) + 우측 선택 Item의 Unit 리스트
    /// - 하단 좌측 카드: KPI 배지 4개 + 경과일 구간별 분포 배지
    /// - 하단 우측 실행 카드: Export Excel / Return / Logistics 버튼
    ///   (Return·Logistics는 체크된 행 대상, 물류처리는 미처리 건만)
    ///
    /// 상태 규칙 (2026-07-15 확정):
    ///   - 경과일 체크(Days 배지색 + Priority + 분포/Avg/Oldest 집계)는
    ///     **물류처리 완료 건만** 대상 — 미처리 행은 무색 배지 + Priority "-".
    ///   - 물류처리(행 Process 버튼·하단 Logistics 벌크)는 **미처리 건만** 가능.
    ///   - 반송(Return)은 처리 여부와 무관하게 **어느 행이든** 가능.
    ///   배지색: 0-2일 파랑 · 3-6일 호박 · 7-13일 주황 · 14일+ 빨강 틴트.
    ///   화면 표기는 전부 영어.
    ///
    /// 역할 분담 (ItemHistory와 동일 구조):
    /// - 서버: 원본 행(도착 정보)과 서버만 계산 가능한 집계(UNIT_CNT)만 준다.
    /// - PendingTablePresenter: 표시 파생(ELAPSED_DAYS/PRIORITY/DAYS_COLOR/CHK/
    ///   LOGIS_CAN)·필터(경과일/물류)·KPI 집계 — 순수 DataTable 로직.
    /// - 폼: 서버 호출, 컨트롤 바인딩, 이벤트 배선만.
    ///
    /// 서버 호출은 "서버 조회 (★ 회사 환경 교체 지점)" 영역의 private 메서드
    /// 2개(pending/units)에만 있다. Return/Logistics 동작(반송 전문, 물류 시스템
    /// 인터페이스)과 LOGIS_YN 데모 시뮬레이션도 회사 인터페이스로 교체하는
    /// 지점이다 — 데모는 알림만 띄우고 화면 상태(LOGIS_YN)만 바꾼다.
    /// Export Excel은 그리드 컬럼 정의를 단일 원천으로 grid.ExportXlsx가
    /// 저장한다 — 내보내기 컬럼을 따로 관리하지 않는다.
    /// </summary>
    public partial class PendingRequestForm : Form
    {
        // ===== 홈 환경 API (★ 회사 환경 교체 지점) =====

        /// <summary>홈 환경 API 주소 — 회사 적용 시 함께 제거한다.</summary>
        private const string apiBaseUrl = "http://localhost:8080";

        /// <summary>API 호출 제한 시간(ms).</summary>
        private const int apiTimeoutMs = 5000;

        /// <summary>Item 리스트 페이지당 건수 (디자이너의 pagination.PageSize와 동일 값).</summary>
        private const int pageSize = 15;

        // ===== 상태 필드 =====

        // 경과일 분포 배지 4개를 구간 인덱스로 접근하기 위한 배열 뷰
        // (.Designer.cs의 badgeAging0~3 — 디자이너는 배열을 직렬화하지 못한다).
        private readonly ModernStatusBadge[] badgeAging;

        // 마지막 조회 결과 전체 (페이지 슬라이스/KPI/분포/엑셀의 원천).
        private DataTable resultData;

        // 코드로 CurrentPage를 되돌릴 때 PageChanged 재진입을 막는다.
        private bool suppressPageEvent;

        // 조회 버전 — 빠른 재조회 시 오래된 응답을 버린다.
        private int searchVersion;

        // Unit 조회 버전 — 빠른 재선택 시 오래된 응답을 버린다.
        private int unitsVersion;

        public PendingRequestForm()
        {
            this.InitializeComponent();

            this.badgeAging = new ModernStatusBadge[]
            {
                this.badgeAging0, this.badgeAging1, this.badgeAging2, this.badgeAging3
            };
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

        // 도착 후 의뢰서 미작성 Item 목록. 서버는 원본 행(도착 오래된 순)과
        // 서버만 계산 가능한 집계(UNIT_CNT)만 준다 — 경과일(ELAPSED_DAYS)과
        // 필터는 클라이언트(PendingTablePresenter)가 처리한다.
        private DataTable RequestPendingItems(string keyword)
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

            // Item ID 자동완성 콤보(검색형): 후보 목록은 전체 조회 결과로 채운다.
            this.cboItemId.DisplayMember = "ITEM_ID";
            this.cboItemId.ValueMember = "ITEM_ID";

            // 컬럼 정의만 코드에서 구성한다 (디자이너 직렬화 대상이 아님).
            // 서버 응답에 없는 컬럼은 그리드가 DataSource 할당 시 자동 보장한다.
            // AutoFitColumns 그리드라 폭은 생략한다 — 예외는 내용 측정 대상이
            // 아닌 CheckBox 컬럼뿐이며 시맨틱 프리셋(GridWidths)으로 지정한다.
            //
            // 캡션은 용어사전(GridCaptionDictionary) 기본값을 쓴다 — 캡션 인자가
            // 없는 컬럼이 사전 참조이고, 이 화면 문맥의 재정의(EVENT_TM=도착 시각,
            // SUB_TYP 축약)만 명시한다. 이 정의는 엑셀 내보내기의 원천이기도 하다.
            //
            // Item 리스트: 체크박스(벌크 대상) + 도착 정보 + Days 배지 + Priority +
            // 행 단위 물류처리 버튼(미처리 건만 활성). 실제 컬럼명 그대로 바인딩.
            this.gridItems.ConfigureColumns(
                new ModernDataGridColumn("CHK", "", GridWidths.Check) { Kind = GridColumnKind.CheckBox },
                new ModernDataGridColumn("ITEM_ID"),
                new ModernDataGridColumn("ELAPSED_DAYS")
                {
                    Kind = GridColumnKind.Badge,
                    BadgeColorMember = "DAYS_COLOR",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("PRIORITY") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("UNIT_CNT") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("LOGIS_YN")
                {
                    Kind = GridColumnKind.Button,
                    ButtonText = "Process",
                    ButtonEnabledMember = "LOGIS_CAN",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("EVENT_TM", "Arrived At") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("MODEL_ID"),
                new ModernDataGridColumn("SUB_TYP", "Type") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("ORG_ITEM_ID"),
                new ModernDataGridColumn("BOX_ID") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STORE_ID") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("DESCRIPTION"));

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
        // 반영 순서: 경과일 파생 → LOGIS_YN 데모 시뮬레이션(★) → 워크리스트
        // 파생(Priority/배지색/체크/버튼) → 필터(경과일+물류) → 페이지 1 바인딩 →
        // KPI/분포 갱신. 파생·필터·집계는 전부 PendingTablePresenter가 계산한다.
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
                    DataTable pending = this.RequestPendingItems(keyword);

                    this.Invoke(new MethodInvoker(delegate
                    {
                        // 그 사이 새 조회가 나갔으면 이 응답은 버린다.
                        if (version != this.searchVersion)
                        {
                            return;
                        }

                        // 그리드 표시 컬럼은 그리드가 DataSource 할당 시 스스로
                        // 보장한다 — 여기서는 파생 컬럼만 채운다.
                        PendingTablePresenter.AddElapsedDays(pending);
                        this.SimulateLogisticsState(pending);
                        PendingTablePresenter.ApplyWorkflowColumns(pending);
                        this.resultData = PendingTablePresenter.Filter(pending, minDays, logisticsFilter);

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
                        this.ApplySummary(PendingTablePresenter.Aggregate(this.resultData));

                        // 전체 조회(조건 없음)일 때 그 결과로 Item ID 자동완성
                        // 후보를 갱신한다 — DataSource 할당의 첫 행 자동 선택은
                        // 텍스트를 비워 되돌린다 (조건이 채워진 채 남지 않게).
                        // (빈 결과는 ITEM_ID 컬럼 자체가 없을 수 있어 건너뛴다.)
                        if (keyword.Length == 0 && minDays == 0 && logisticsFilter.Length == 0
                                && pending.Columns.Contains("ITEM_ID"))
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

        // ★ 회사 환경 교체 지점 — 홈 API는 물류처리 상태(LOGIS_YN)를 내려주지
        // 않으므로 데모용으로 결정적 시뮬레이션한다(경과일 3의 배수가 아니면 완료).
        // 서버가 실제 물류처리 상태를 내려주면 이 메서드를 통째로 제거한다.
        private void SimulateLogisticsState(DataTable pending)
        {
            if (!pending.Columns.Contains("LOGIS_YN"))
            {
                pending.Columns.Add("LOGIS_YN", typeof(string));
            }

            foreach (DataRow row in pending.Rows)
            {
                if (PendingTablePresenter.CellText(row, "LOGIS_YN").Trim().Length == 0)
                {
                    int days = PendingTablePresenter.ParseDays(
                            PendingTablePresenter.CellText(row, "ELAPSED_DAYS"));
                    row["LOGIS_YN"] = days % 3 != 0 ? "Y" : "N";
                }
            }
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

            DataRow source = this.FindResultRow(PendingTablePresenter.CellText(e.Row, "ITEM_ID"));

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
                if (PendingTablePresenter.CellText(row, "ITEM_ID") == itemId)
                {
                    return row;
                }
            }

            return null;
        }

        // ===== KPI + 경과일 분포 =====

        // 집계(Presenter 계산 결과)를 KPI 배지 4개와 분포 배지 4개에 표기한다.
        private void ApplySummary(PendingTablePresenter.PendingSummary summary)
        {
            this.badgePending.Text = "Pending " + summary.ItemCount.ToString("N0");
            this.badgeUnits.Text = "Units " + summary.UnitTotal.ToString("N0");
            this.badgeAvg.Text = summary.AgedCount > 0
                    ? "Avg " + summary.DaysAverage.ToString("0.0", CultureInfo.InvariantCulture) + " d"
                    : "Avg -";
            this.badgeOldest.Text = summary.AgedCount > 0
                    ? "Oldest " + summary.DaysMax.ToString("N0") + " d"
                    : "Oldest -";

            for (int index = 0; index < this.badgeAging.Length; index++)
            {
                this.badgeAging[index].Text = PendingTablePresenter.AgingLabels[index]
                        + " · " + summary.AgingCounts[index].ToString("N0");
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

            string itemId = PendingTablePresenter.CellText(row.Row, "ITEM_ID");
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

        // Export Excel: 조회 결과 전체(현재 페이지가 아니라)를 진짜 Excel 파일
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
                    this.WriteExcel(dialog.FileName);
                    this.toastMain.Show(
                            this.resultData.Rows.Count.ToString("N0") + " items exported.", ToastKind.Success);
                }
                catch (Exception ex)
                {
                    this.toastMain.Show("Export failed: " + ex.Message, ToastKind.Error);
                }
            }
        }

        // 그리드 컬럼 정의(화면 표시와 동일한 순서·캡션·형식)를 단일 원천으로
        // 전체 조회 결과를 저장한다 — 체크박스/버튼 컬럼은 내보내기가 알아서
        // 제외하므로 내보내기용 컬럼 목록을 따로 관리하지 않는다.
        private void WriteExcel(string path)
        {
            this.gridItems.ExportXlsx(path, "Pending Requests", this.resultData);
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
                if (!PendingTablePresenter.IsLogisticsDone(row))
                {
                    PendingTablePresenter.MarkLogisticsDone(row);
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

            string itemId = PendingTablePresenter.CellText(row.Row, "ITEM_ID");

            // 페이지 조각(화면)과 조회 결과 원본을 함께 갱신한다 — 버튼은
            // LOGIS_CAN 바인딩으로 즉시 비활성화된다.
            PendingTablePresenter.MarkLogisticsDone(row.Row);
            DataRow source = this.FindResultRow(itemId);

            if (source != null)
            {
                PendingTablePresenter.MarkLogisticsDone(source);
            }

            this.toastMain.Show("Logistics processed for " + itemId + ". (demo)", ToastKind.Success);
        }
    }
}
