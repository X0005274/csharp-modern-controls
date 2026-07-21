using System;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Create 확인 다이얼로그 — 현황판에서 체크한 Create 대상(Request Linked)
    /// 목록을 보여주고, 확인 후 일괄 Create 처리한다. Receive 다이얼로그와
    /// 같은 "처리 전에 대상을 눈으로 확인" 단계다. Create는 의뢰서가 연결된
    /// 건만 처리 가능하므로 매뉴얼(강제) 입력 모드는 없다.
    ///
    /// 성공하면 다이얼로그를 닫고 부모 폼이 재조회한다.
    ///
    /// ★ 회사 환경 교체 지점 — 서버 호출은 PendingApiClient.Create에 모여 있다.
    ///   그 메서드 본문을 회사 의뢰 처리 인터페이스 호출로 바꾼다.
    /// </summary>
    public partial class CreateDialogForm : Form
    {
        // 체크된 Create 대상 목록 (부모 폼이 현황판에서 추려서 준다).
        private readonly DataTable checkedItems;

        /// <summary>처리된 건수 (DialogResult가 OK일 때만 유효).</summary>
        public int ProcessedCount { get; private set; }

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

        private void OnCreateClick(object sender, EventArgs e)
        {
            if (this.checkedItems == null || this.checkedItems.Rows.Count == 0)
            {
                return;
            }

            // 체크된 대상을 서버에 순차 Create 처리하고 성공 건만 집계한다.
            int processed = 0;

            try
            {
                foreach (DataRow row in this.checkedItems.Rows)
                {
                    PendingApiClient.ActionResult result = PendingApiClient.Create(
                            PendingTablePresenter.CellText(row, "ITEM_ID"));

                    if (result.Success)
                    {
                        processed = processed + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Server call failed: " + ex.Message,
                        "Create", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.ProcessedCount = processed;
            this.DialogResult = DialogResult.OK;
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        // 다이얼로그 관례 키: Enter = Create, Esc = 닫기.
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
