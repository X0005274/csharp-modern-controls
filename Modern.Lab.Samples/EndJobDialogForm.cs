using System;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Data;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Job End 다이얼로그 — 작업종료 전에 캐리어(듀러블) 슬롯별 판정을
    /// 입력받는다. 슬롯 현황(GetEndJobSlots 결과)을 그리드로 보여주고,
    /// JUDGE_RSLT만 셀 콤보(SUCC/FAIL)로 입력한다 — **웨이퍼(WF_ID)가 있는
    /// 행만** 입력할 수 있고(빈 슬롯은 콤보가 회색 잠금), 웨이퍼 행 전부에
    /// 판정이 있어야 End가 확정된다.
    ///
    /// 확정 시 JudgeResults(입력이 반영된 슬롯 테이블)를 부모가 작업종료
    /// 전문에 실어 보낸다 — 서버(시뮬레이터)도 같은 규칙을 재검증한다.
    /// </summary>
    public partial class EndJobDialogForm : Form
    {
        // 부모가 넘긴 슬롯 현황 — CAN_JUDGE(입력 가능) 파생 후 그리드에 바인딩.
        private readonly DataTable slots;

        /// <summary>판정이 **입력(변경)된 행만** 담은 테이블 — DialogResult가
        /// OK일 때만 유효. 바인딩 직전 AcceptChanges로 기준을 잡아 두므로
        /// GetChanges가 사용자가 콤보를 만진 행만 돌려준다 — 서버에는 이
        /// 변경분만 보내고, 전체 대비 완결성 검증은 서버가 한다.</summary>
        public DataTable JudgeResults
        {
            get { return this.slots.GetChanges(); }
        }

        public EndJobDialogForm(string eqpId, string lotId, DataTable slots)
        {
            this.slots = slots;
            this.InitializeComponent();
            this.lblEqpValue.Text = eqpId;
            this.lblLotValue.Text = lotId;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            // 입력 가능 파생 — 웨이퍼가 있는 슬롯만 판정 콤보가 활성이 된다.
            if (!this.slots.Columns.Contains("CAN_JUDGE"))
            {
                this.slots.Columns.Add("CAN_JUDGE", typeof(bool));
            }

            foreach (DataRow row in this.slots.Rows)
            {
                row["CAN_JUDGE"] =
                        TableHelper.CellText(row, "WF_ID").Trim().Length > 0;
            }

            // 여기까지가 "조회 원본" — 이후 콤보 입력만 변경으로 잡혀
            // JudgeResults(GetChanges)가 입력된 행만 돌려준다.
            this.slots.AcceptChanges();

            // 슬롯 그리드 — JUDGE_RSLT만 셀 콤보 입력(SUCC/FAIL), 나머지는 표시 전용.
            this.gridSlots.ConfigureColumns(
                new ModernDataGridColumn("DURABLE_ID", "Durable"),
                new ModernDataGridColumn("DURABLE_TYP", "Type") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("SUB_DURABLE_TYP", "Sub Type") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("SLOT_NO", "Slot") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("FINGER_ID", "Finger") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("WF_ID", "Wafer"),
                new ModernDataGridColumn("JUDGE_RSLT", "Judge", 150d)
                {
                    Kind = GridColumnKind.Combo,
                    ComboItems = new string[] { "SUCC", "FAIL" },
                    ComboItemColors = new string[] { "#DCFCE7", "#FEE2E2" },
                    ComboEnabledMember = "CAN_JUDGE"
                });

            this.gridSlots.DataSource = this.slots;
        }

        private void OnEndClick(object sender, EventArgs e)
        {
            // 웨이퍼가 있는 슬롯 전부에 판정이 있어야 확정된다.
            foreach (DataRow row in this.slots.Rows)
            {
                if (TableHelper.CellText(row, "WF_ID").Trim().Length == 0)
                {
                    continue;
                }

                string judge = TableHelper.CellText(row, "JUDGE_RSLT").Trim();

                if (judge != "SUCC" && judge != "FAIL")
                {
                    this.badgeWarn.Text = "Select SUCC / FAIL for every wafer slot";
                    this.badgeWarn.Visible = true;
                    return;
                }
            }

            this.DialogResult = DialogResult.OK;
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        // 다이얼로그 관례 키: Enter = End, Esc = 닫기.
        // (ModernButton은 IButtonControl이 아니라 폼의 AcceptButton/CancelButton에
        // 지정할 수 없어 키를 직접 처리한다.)
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                this.OnEndClick(this.btnEnd, EventArgs.Empty);
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
