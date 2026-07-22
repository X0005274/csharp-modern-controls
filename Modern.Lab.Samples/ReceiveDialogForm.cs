using System;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Receive 확인 다이얼로그 — 현황판에서 체크한 Receive 대상(발송 통보
    /// 있음 + 미수신) 목록을 눈으로 확인하는 단계다. **처리는 하지 않는다** —
    /// 확인(OK)하면 부모 폼이 그리드 행 버튼(한 건)과 같은 공용 메서드
    /// (ProcessBoardItems)로 일괄 수신하고 재조회한다. 조회에 없는 아이템의
    /// 강제 수신은 Manual Receive(전용 다이얼로그)가 담당한다.
    /// </summary>
    public partial class ReceiveDialogForm : Form
    {
        // 체크된 Receive 대상 목록 (부모 폼이 현황판에서 추려서 준다).
        private readonly DataTable checkedItems;

        public ReceiveDialogForm(DataTable checkedItems)
        {
            this.checkedItems = checkedItems;
            this.InitializeComponent();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            // 체크 목록: 처리 전에 대상을 확인하는 용도라 식별 정보만 보여준다.
            // Receive 대상은 미수신 건이므로 발송 정보(SEND_TM)를 보여준다.
            this.gridItems.ConfigureColumns(
                new ModernDataGridColumn("ITEM_ID"),
                new ModernDataGridColumn("SEND_FAC") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("BOX_ID") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("SEND_TM") { TextAlignment = GridTextAlignment.Center });
            this.gridItems.DataSource = this.checkedItems;

            this.btnReceive.Enabled =
                    this.checkedItems != null && this.checkedItems.Rows.Count > 0;
        }

        // 확인 — 처리는 부모 폼이 한다.
        private void OnReceiveClick(object sender, EventArgs e)
        {
            if (this.checkedItems == null || this.checkedItems.Rows.Count == 0)
            {
                return;
            }

            this.DialogResult = DialogResult.OK;
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        // 다이얼로그 관례 키: Enter = Receive(확인), Esc = 닫기.
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
    }
}
