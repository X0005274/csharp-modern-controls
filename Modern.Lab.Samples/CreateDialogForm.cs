using System;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Create 확인 다이얼로그 — 현황판에서 체크한 Create 대상(Request Linked)
    /// 목록을 눈으로 확인하는 단계다. **처리는 하지 않는다** — 확인(OK)하면
    /// 부모 폼이 그리드 행 버튼(한 건)과 같은 공용 메서드(ProcessBoardItems)로
    /// 일괄 처리하고 재조회한다. Create는 의뢰서가 연결된 건만 처리 가능하므로
    /// 매뉴얼(강제) 입력 모드는 없다.
    /// </summary>
    public partial class CreateDialogForm : Form
    {
        // 체크된 Create 대상 목록 (부모 폼이 현황판에서 추려서 준다).
        private readonly DataTable checkedItems;

        public CreateDialogForm(DataTable checkedItems)
        {
            this.checkedItems = checkedItems;
            this.InitializeComponent();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            // 체크 목록: 어떤 의뢰가 처리되는지 확인하는 용도라 의뢰 정보 위주.
            this.gridItems.ConfigureColumns(
                new ModernDataGridColumn("ITEM_ID"),
                new ModernDataGridColumn("REQ_NO"),
                new ModernDataGridColumn("SAMPLE_NM"),
                new ModernDataGridColumn("RECV_TM") { TextAlignment = GridTextAlignment.Center });
            this.gridItems.DataSource = this.checkedItems;

            this.btnCreate.Enabled =
                    this.checkedItems != null && this.checkedItems.Rows.Count > 0;
        }

        // 확인 — 처리는 부모 폼이 한다.
        private void OnCreateClick(object sender, EventArgs e)
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

        // 다이얼로그 관례 키: Enter = Create(확인), Esc = 닫기.
        // (ModernButton은 IButtonControl이 아니라 폼의 AcceptButton/CancelButton에
        // 지정할 수 없어 키를 직접 처리한다.)
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                this.OnCreateClick(this.btnCreate, EventArgs.Empty);
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
