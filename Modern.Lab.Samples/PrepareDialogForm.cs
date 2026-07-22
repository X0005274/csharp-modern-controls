using System;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Data;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Job Prepare 다이얼로그 — 작업준비(투입)는 **인포트·아웃포트·아웃
    /// 캐리어를 먼저 지정**하고 시작한다. 인포트에는 Lot이 자기 캐리어째
    /// 장착되고(별도 선택 불필요), 아웃포트는 이 작업에 예약되며, 작업종료
    /// 시 Lot이 **배정한 아웃 캐리어**에 담겨 그 포트로 나간다.
    ///
    /// 콤보에는 지금 쓸 수 있는 것만 올라온다 — 인포트는 빈 것, 아웃포트는
    /// 비어 있고 다른 작업에 예약되지 않은 것, 캐리어는 빈 캐리어 풀. 대상
    /// 장비/Lot은 부모 폼이 정해서 넘긴다 (Prepare = 최우선 Lot, Assign =
    /// 지정 Lot).
    /// </summary>
    public partial class PrepareDialogForm : Form
    {
        // 부모가 넘긴 포트 상세 (EquipmentLotPresenter.BuildPortRows 결과).
        private readonly DataTable ports;

        // 부모가 넘긴 빈 캐리어 풀 (CARRIER_ID 컬럼).
        private readonly DataTable carriers;

        /// <summary>선택한 인포트 번호(1-기준) — DialogResult가 OK일 때만 유효.</summary>
        public int SelectedInPort { get; private set; }

        /// <summary>선택한 아웃포트 번호(1-기준) — DialogResult가 OK일 때만 유효.</summary>
        public int SelectedOutPort { get; private set; }

        /// <summary>선택한 아웃 캐리어 — DialogResult가 OK일 때만 유효.</summary>
        public string SelectedCarrier { get; private set; }

        // 미리 선택할 인포트(타입 내 1-기준 번호, 0 = 없음) — 포트 행에서
        // Load로 진입하면 그 포트가 선택된 채 열린다 (변경은 가능).
        private readonly int preferredInPort;

        public PrepareDialogForm(
                string eqpId, string lotId, DataTable ports, DataTable carriers,
                int preferredInPort = 0)
        {
            this.ports = ports;
            this.carriers = carriers;
            this.preferredInPort = preferredInPort;
            this.InitializeComponent();
            this.lblEqpValue.Text = eqpId;
            this.lblLotValue.Text = lotId;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            // 쓸 수 있는 포트만 콤보에 올린다 — 인포트는 Empty, 아웃포트는
            // Empty(예약된 포트는 State가 Reserved라 자연히 제외된다). 표시는
            // 포트 카드와 같은 연속 포트 번호를 쓴다.
            this.cboInPort.DisplayMember = "LABEL";
            this.cboInPort.ValueMember = "NO";
            this.cboInPort.DataSource = this.BuildPortOptions("In");

            // 포트 행에서 Load로 진입한 경우 그 인포트를 미리 선택한다.
            if (this.preferredInPort > 0)
            {
                this.cboInPort.SelectedValue = this.preferredInPort;
            }

            this.cboOutPort.DisplayMember = "LABEL";
            this.cboOutPort.ValueMember = "NO";
            this.cboOutPort.DataSource = this.BuildPortOptions("Out");

            // 아웃 캐리어: 빈 캐리어 풀에서 고른다 — In은 Lot이 자기 캐리어째
            // 들어오므로 선택이 없다.
            this.cboCarrier.DisplayMember = "CARRIER_ID";
            this.cboCarrier.ValueMember = "CARRIER_ID";
            this.cboCarrier.DataSource = this.carriers;
        }

        // 포트 상세에서 지정 구분(In/Out)의 Empty 포트만 골라 콤보 목록을
        // 만든다 — 값(NO)은 처리 호출용 타입 내 번호, 라벨은 연속 포트 번호.
        private DataTable BuildPortOptions(string portType)
        {
            DataTable options = new DataTable();
            options.Columns.Add("NO", typeof(int));
            options.Columns.Add("LABEL", typeof(string));

            if (this.ports == null)
            {
                return options;
            }

            foreach (DataRow row in this.ports.Rows)
            {
                if (TableHelper.CellText(row, "PORT_TYPE") != portType
                        || TableHelper.CellText(row, "PORT_STAT") != "Empty")
                {
                    continue;
                }

                options.Rows.Add(
                        TableHelper.ParseInt(
                                TableHelper.CellText(row, "PORT_IDX")),
                        "Port " + TableHelper.CellText(row, "PORT_NO"));
            }

            return options;
        }

        private void OnPrepareClick(object sender, EventArgs e)
        {
            object inPort = this.cboInPort.SelectedValue;
            object outPort = this.cboOutPort.SelectedValue;
            string carrier = this.cboCarrier.SelectedValue as string;

            if (!(inPort is int) || !(outPort is int) || string.IsNullOrEmpty(carrier))
            {
                return;
            }

            this.SelectedInPort = (int)inPort;
            this.SelectedOutPort = (int)outPort;
            this.SelectedCarrier = carrier;
            this.DialogResult = DialogResult.OK;
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        // 다이얼로그 관례 키: Enter = Prepare, Esc = 닫기.
        // (ModernButton은 IButtonControl이 아니라 폼의 AcceptButton/CancelButton에
        // 지정할 수 없어 키를 직접 처리한다.)
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                this.OnPrepareClick(this.btnPrepare, EventArgs.Empty);
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
