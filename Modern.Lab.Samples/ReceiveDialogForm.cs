using System;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Receive 다이얼로그 — 처리 방식을 선택해 수신 처리한다:
    ///
    /// - Checked Items: 현황판에서 체크한 Receive 대상(Arrived) 목록을 보여주고
    ///   확인 후 일괄 수신 처리한다. 처리 전에 대상을 눈으로 확인하는 단계다.
    /// - Manual Entry: 조회해도 현황판에 나오지 않는(어떤 인터페이스에도
    ///   올라오지 않은) 아이템을 Item ID + 발송 공장 입력으로 **강제 수신
    ///   처리**한다. 전문은 단일 호출(서버가 체크+처리를 한 번에)이고, 실패
    ///   사유는 다이얼로그 안 배지에 남는다 — 입력을 교정하는 동안 계속 볼
    ///   수 있어야 하기 때문이다. 입력이 바뀌면 이전 결과는 지운다.
    ///
    /// 성공하면 다이얼로그를 닫고 부모 폼이 재조회한다. 체크된 대상이 없으면
    /// Manual Entry 모드로 시작한다.
    ///
    /// ★ 회사 환경 교체 지점 — 서버 호출은 전부 PendingApiClient에 모여 있다.
    ///   그 클래스의 Receive/ManualReceive/GetSendFacilities 본문을 회사
    ///   인터페이스(ITEM 생성 전문 · 수신 전문 · 공장 코드 조회)로 바꾼다.
    /// </summary>
    public partial class ReceiveDialogForm : Form
    {
        // 결과 배지 색 — 실패(빨강 틴트)/입력 안내(호박 틴트).
        private const string errorColor = "#FEE2E2";
        private const string warningColor = "#FEF3C7";

        // 처리 방식 라디오 값.
        private const string modeChecked = "CHECKED";
        private const string modeManual = "MANUAL";

        // 체크된 Receive 대상 목록 (부모 폼이 현황판에서 추려서 준다).
        private readonly DataTable checkedItems;

        /// <summary>처리된 건수 (DialogResult가 OK일 때만 유효).</summary>
        public int ProcessedCount { get; private set; }

        /// <summary>매뉴얼 강제 수신된 Item ID — 체크 목록 처리였으면 null.</summary>
        public string ManualItemId { get; private set; }

        public ReceiveDialogForm(DataTable checkedItems)
        {
            this.checkedItems = checkedItems;
            this.InitializeComponent();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            int checkedCount = this.checkedItems != null ? this.checkedItems.Rows.Count : 0;

            // 처리 방식 라디오: 체크 목록 처리 / 매뉴얼 강제 처리.
            DataTable modeTable = new DataTable();
            modeTable.Columns.Add("VALUE", typeof(string));
            modeTable.Columns.Add("LABEL", typeof(string));
            modeTable.Rows.Add(modeChecked, "Checked Items (" + checkedCount.ToString("N0") + ")");
            modeTable.Rows.Add(modeManual, "Manual Entry");

            this.radioMode.DisplayMember = "LABEL";
            this.radioMode.ValueMember = "VALUE";
            this.radioMode.DataSource = modeTable;

            // 체크 목록: 처리 전에 대상을 확인하는 용도라 식별 정보만 보여준다.
            this.gridItems.ConfigureColumns(
                new ModernDataGridColumn("ITEM_ID"),
                new ModernDataGridColumn("SEND_FAC") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("BOX_ID") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("ARRIVE_TM") { TextAlignment = GridTextAlignment.Center });
            this.gridItems.DataSource = this.checkedItems;

            // 발송 공장 콤보 — 서버에서 공장 코드 목록을 받아 채운다.
            try
            {
                this.cboSendFac.DataSource = PendingApiClient.GetSendFacilities();
            }
            catch (Exception)
            {
                // 공장 코드 조회 실패 시 빈 콤보로 둔다 — 수신 시 서버가 다시 검증한다.
                this.cboSendFac.DataSource = new string[0];
            }

            // 체크된 대상이 없으면 매뉴얼 모드로 시작한다.
            this.radioMode.SelectedValue = checkedCount > 0 ? modeChecked : modeManual;
            this.ApplyMode();
            this.HideResult();
        }

        // 현재 선택된 처리 방식 (라디오 미선택이면 매뉴얼로 본다).
        private string GetMode()
        {
            string value = this.radioMode.SelectedValue as string;
            return value ?? modeManual;
        }

        private void OnModeChanged(object sender, EventArgs e)
        {
            this.ApplyMode();
            this.HideResult();
        }

        // 모드에 맞는 입력 영역만 보여주고, 체크 목록이 비었으면 그 모드의
        // Receive를 막는다.
        private void ApplyMode()
        {
            bool checkedMode = this.GetMode() == modeChecked;

            this.pnlChecked.Visible = checkedMode;
            this.pnlManual.Visible = !checkedMode;
            this.btnReceive.Enabled = !checkedMode
                    || (this.checkedItems != null && this.checkedItems.Rows.Count > 0);

            if (!checkedMode)
            {
                this.txtItemId.Focus();
            }
        }

        // Receive: 체크 목록 모드는 대상 전부를 일괄 수신 처리하고, 매뉴얼
        // 모드는 강제 수신 단일 전문을 호출한다. 실패 사유는 배지에 남기고
        // 다이얼로그를 유지한다(교정 후 재시도).
        private void OnReceiveClick(object sender, EventArgs e)
        {
            if (this.GetMode() == modeChecked)
            {
                if (this.checkedItems == null || this.checkedItems.Rows.Count == 0)
                {
                    this.ShowResult("No checked item is waiting for receive.", warningColor);
                    return;
                }

                // 체크된 대상을 서버에 순차 수신 처리하고 성공 건만 집계한다.
                int processed = 0;

                try
                {
                    foreach (DataRow row in this.checkedItems.Rows)
                    {
                        PendingApiClient.ActionResult itemResult = PendingApiClient.Receive(
                                PendingTablePresenter.CellText(row, "ITEM_ID"));

                        if (itemResult.Success)
                        {
                            processed = processed + 1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.ShowResult("Server call failed: " + ex.Message, errorColor);
                    return;
                }

                this.ProcessedCount = processed;
                this.ManualItemId = null;
                this.DialogResult = DialogResult.OK;
                return;
            }

            string itemId = this.txtItemId.Text.Trim();
            string sendFac = this.cboSendFac.SelectedValue as string;

            if (itemId.Length == 0)
            {
                this.ShowResult("Enter an item ID.", warningColor);
                this.txtItemId.Focus();
                return;
            }

            // 강제 수신 단일 전문 — 서버가 중복 체크 + 처리를 한 번에 한다.
            PendingApiClient.ActionResult result;

            try
            {
                result = PendingApiClient.ManualReceive(itemId, sendFac);
            }
            catch (Exception ex)
            {
                this.ShowResult("Server call failed: " + ex.Message, errorColor);
                this.txtItemId.Focus();
                return;
            }

            if (!result.Success)
            {
                this.ShowResult(result.Message, errorColor);
                this.txtItemId.Focus();
                return;
            }

            this.ProcessedCount = 1;
            this.ManualItemId = itemId.ToUpperInvariant();
            this.DialogResult = DialogResult.OK;
        }

        // 입력이 바뀌면 이전 실패 사유를 지운다 — 낡은 메시지가 새 입력에
        // 대한 결과처럼 보이지 않게 한다.
        private void OnInputChanged(object sender, EventArgs e)
        {
            this.HideResult();
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        // 다이얼로그 관례 키: Enter = Receive, Esc = 닫기.
        // (ModernButton은 IButtonControl이 아니라 폼의 AcceptButton/CancelButton에
        // 지정할 수 없어 키를 직접 처리한다.)
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                this.OnReceiveClick(this.btnReceive, EventArgs.Empty);
                return true;
            }

            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        private void ShowResult(string message, string color)
        {
            this.badgeResult.Text = message;
            this.badgeResult.Color = color;
            this.badgeResult.Visible = true;
        }

        private void HideResult()
        {
            this.badgeResult.Visible = false;
        }
    }
}
