using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Employee management sample, authored the way a migrated legacy form
    /// looks: every control is declared and laid out in
    /// EmployeeManagementForm.Designer.cs (InitializeComponent) so the VS form
    /// designer can open and edit this form. Only data wiring lives here —
    /// DataSource members are Browsable(false) by contract and never appear in
    /// designer serialization, exactly as in a real migrated form.
    ///
    /// Areas (contract rule 5 — layout stays WinForms):
    /// - Top card: search conditions (name / department / rank + query/reset).
    /// - Middle: employee grid (anchored to all four edges).
    /// - Bottom card: result-count KPI + per-department / per-rank chips on the
    ///   left, action buttons (new/save = Secondary, delete = Danger,
    ///   excel = Subtle) on the right.
    /// The in-memory master DataTable stands in for the server; every query
    /// runs through the same DataSource assignment path a real form would use.
    /// </summary>
    public partial class EmployeeManagementForm : Form
    {
        private DataTable employeeMaster;
        private int nextEmployeeNumber;

        public EmployeeManagementForm()
        {
            this.InitializeComponent();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            this.employeeMaster = CreateEmployeeMaster();
            this.nextEmployeeNumber = 1021;

            this.LoadSearchCodes();
            this.ExecuteSearch();
        }

        // Loads the search-condition code tables. In a real form these come from
        // a server request/reply; the control contract keeps this code identical.
        private void LoadSearchCodes()
        {
            // No "전체" row: an unselected combo (SelectedIndex = -1) means "all"
            // and shows the placeholder instead.
            DataTable deptTable = new DataTable();
            deptTable.Columns.Add("DEPT_CODE", typeof(string));
            deptTable.Columns.Add("DEPT_NAME", typeof(string));
            deptTable.Rows.Add("D1", "경영지원팀");
            deptTable.Rows.Add("D2", "개발1팀");
            deptTable.Rows.Add("D3", "개발2팀");
            deptTable.Rows.Add("D4", "품질보증팀");

            this.cboDept.DisplayMember = "DEPT_NAME";
            this.cboDept.ValueMember = "DEPT_CODE";
            this.cboDept.DataSource = deptTable;
            this.cboDept.SelectedIndex = -1;

            // Rank filter is a check-combo: multiple ranks may be checked at
            // once; nothing checked means "all" (placeholder shows).
            DataTable rankTable = new DataTable();
            rankTable.Columns.Add("RANK_CODE", typeof(string));
            rankTable.Columns.Add("RANK_NAME", typeof(string));
            rankTable.Rows.Add("부장", "부장");
            rankTable.Rows.Add("과장", "과장");
            rankTable.Rows.Add("대리", "대리");
            rankTable.Rows.Add("사원", "사원");

            this.cboRank.DisplayMember = "RANK_NAME";
            this.cboRank.ValueMember = "RANK_CODE";
            this.cboRank.DataSource = rankTable;

            this.gridEmployee.ConfigureColumns(
                new ModernDataGridColumn("EMP_NO", "사번", 90),
                new ModernDataGridColumn("EMP_NAME", "이름", 110),
                new ModernDataGridColumn("DEPT_NAME", "부서", 130),
                new ModernDataGridColumn("POSITION", "직급", 90) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("HIRE_DATE", "입사일", 110) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("EMAIL", "이메일"));

            this.listDeptCount.DisplayMember = "CATEGORY";
            this.listDeptCount.ValueMember = "CNT";
            this.listRankCount.DisplayMember = "CATEGORY";
            this.listRankCount.ValueMember = "CNT";

            // Search-box style autocomplete on the name filter, wired exactly
            // like the legacy WinForms TextBox members.
            AutoCompleteStringCollection nameCandidates = new AutoCompleteStringCollection();

            foreach (DataRow row in this.employeeMaster.Rows)
            {
                nameCandidates.Add(row["EMP_NAME"].ToString());
            }

            this.txtName.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.txtName.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.txtName.AutoCompleteCustomSource = nameCandidates;
        }

        // Runs the query against the in-memory master and pushes the result into
        // the grid and the statistics area — the search → grid → stats flow.
        private void ExecuteSearch()
        {
            string nameFilter = this.txtName.Text.Trim();
            string deptCode = this.cboDept.SelectedValue as string;
            object[] rankCodes = this.cboRank.CheckedValues;

            List<string> conditions = new List<string>();

            if (nameFilter.Length > 0)
            {
                conditions.Add("EMP_NAME LIKE '%" + nameFilter.Replace("'", "''") + "%'");
            }

            if (!string.IsNullOrEmpty(deptCode))
            {
                conditions.Add("DEPT_CODE = '" + deptCode + "'");
            }

            if (rankCodes != null && rankCodes.Length > 0)
            {
                List<string> quoted = new List<string>();

                foreach (object code in rankCodes)
                {
                    quoted.Add("'" + code.ToString().Replace("'", "''") + "'");
                }

                conditions.Add("POSITION IN (" + string.Join(", ", quoted.ToArray()) + ")");
            }

            DataView view = new DataView(this.employeeMaster);
            view.RowFilter = string.Join(" AND ", conditions.ToArray());
            view.Sort = "EMP_NO ASC";

            DataTable result = view.ToTable();

            this.gridEmployee.DataSource = result;
            this.cardCount.Value = result.Rows.Count.ToString();
            this.listDeptCount.DataSource = GroupCount(result, "DEPT_NAME");
            this.listRankCount.DataSource = GroupCount(result, "POSITION");
        }

        // Local stand-in for a server-side GROUP BY, preserving first-seen order.
        private static DataTable GroupCount(DataTable source, string columnName)
        {
            DataTable table = new DataTable();
            table.Columns.Add("CATEGORY", typeof(string));
            table.Columns.Add("CNT", typeof(int));

            Dictionary<string, int> counts = new Dictionary<string, int>();
            List<string> order = new List<string>();

            foreach (DataRow row in source.Rows)
            {
                string key = row[columnName].ToString();

                if (!counts.ContainsKey(key))
                {
                    counts[key] = 0;
                    order.Add(key);
                }

                counts[key] = counts[key] + 1;
            }

            foreach (string key in order)
            {
                table.Rows.Add(key, counts[key]);
            }

            return table;
        }

        private void OnSearchClick(object sender, EventArgs e)
        {
            this.ExecuteSearch();
        }

        private void OnResetClick(object sender, EventArgs e)
        {
            this.txtName.Text = string.Empty;
            this.cboDept.SelectedIndex = -1;
            this.cboRank.CheckedValues = null;
            this.ExecuteSearch();
        }

        private void OnExecuteClick(object sender, EventArgs e)
        {
            DataRowView selected = this.gridEmployee.SelectedItem as DataRowView;

            if (selected == null)
            {
                MessageBox.Show(this, "실행할 직원을 먼저 선택하세요.", "직원관리");
                return;
            }

            // Integration point: run the business action for the selected
            // employee here (e.g. batch job, approval request).
            MessageBox.Show(
                this,
                selected["EMP_NAME"] + " (" + selected["EMP_NO"] + ") 대상 작업을 실행합니다. (샘플: 실행 지점)",
                "직원관리");
        }

        private void OnNewClick(object sender, EventArgs e)
        {
            string employeeNo = "E" + this.nextEmployeeNumber.ToString();
            this.nextEmployeeNumber = this.nextEmployeeNumber + 1;

            this.employeeMaster.Rows.Add(
                employeeNo,
                "신규직원" + employeeNo.Substring(2),
                "D2", "개발1팀", "사원",
                DateTime.Today.ToString("yyyy-MM-dd"),
                employeeNo.ToLowerInvariant() + "@modernlab.co.kr");

            this.ExecuteSearch();
            MessageBox.Show(this, "신규 직원 " + employeeNo + " 이(가) 추가되었습니다.", "직원관리");
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            // Integration point: send the current master to the server here.
            MessageBox.Show(
                this,
                "현재 직원 " + this.employeeMaster.Rows.Count + "건을 저장했습니다. (샘플: 서버 전송 지점)",
                "직원관리");
        }

        private void OnDeleteClick(object sender, EventArgs e)
        {
            DataRowView selected = this.gridEmployee.SelectedItem as DataRowView;

            if (selected == null)
            {
                MessageBox.Show(this, "삭제할 직원을 먼저 선택하세요.", "직원관리");
                return;
            }

            string employeeNo = selected["EMP_NO"].ToString();
            string employeeName = selected["EMP_NAME"].ToString();

            DialogResult answer = MessageBox.Show(
                this,
                employeeName + " (" + employeeNo + ") 직원을 삭제하시겠습니까?",
                "직원관리",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            DataRow[] matches = this.employeeMaster.Select("EMP_NO = '" + employeeNo.Replace("'", "''") + "'");

            foreach (DataRow match in matches)
            {
                this.employeeMaster.Rows.Remove(match);
            }

            this.ExecuteSearch();
        }

        private void OnExcelClick(object sender, EventArgs e)
        {
            // Integration point: export the current grid result to Excel here.
            MessageBox.Show(
                this,
                "조회 결과 " + this.gridEmployee.RowCount + "건을 엑셀로 내보냅니다. (샘플: 내보내기 지점)",
                "직원관리");
        }

        // In-memory employee master standing in for the server database (rule 2).
        private static DataTable CreateEmployeeMaster()
        {
            DataTable table = new DataTable();
            table.Columns.Add("EMP_NO", typeof(string));
            table.Columns.Add("EMP_NAME", typeof(string));
            table.Columns.Add("DEPT_CODE", typeof(string));
            table.Columns.Add("DEPT_NAME", typeof(string));
            table.Columns.Add("POSITION", typeof(string));
            table.Columns.Add("HIRE_DATE", typeof(string));
            table.Columns.Add("EMAIL", typeof(string));

            table.Rows.Add("E1001", "김민수", "D1", "경영지원팀", "부장", "2012-03-02", "minsu.kim@modernlab.co.kr");
            table.Rows.Add("E1002", "이서연", "D2", "개발1팀", "과장", "2015-07-13", "seoyeon.lee@modernlab.co.kr");
            table.Rows.Add("E1003", "박지훈", "D2", "개발1팀", "대리", "2018-01-22", "jihun.park@modernlab.co.kr");
            table.Rows.Add("E1004", "최유진", "D3", "개발2팀", "과장", "2014-11-03", "yujin.choi@modernlab.co.kr");
            table.Rows.Add("E1005", "정다은", "D3", "개발2팀", "사원", "2021-05-17", "daeun.jung@modernlab.co.kr");
            table.Rows.Add("E1006", "한상우", "D4", "품질보증팀", "대리", "2019-09-09", "sangwoo.han@modernlab.co.kr");
            table.Rows.Add("E1007", "오세라", "D4", "품질보증팀", "사원", "2022-02-28", "sera.oh@modernlab.co.kr");
            table.Rows.Add("E1008", "장현우", "D3", "개발2팀", "대리", "2017-06-01", "hyunwoo.jang@modernlab.co.kr");
            table.Rows.Add("E1009", "김하늘", "D1", "경영지원팀", "사원", "2023-01-09", "haneul.kim@modernlab.co.kr");
            table.Rows.Add("E1010", "이준호", "D2", "개발1팀", "부장", "2010-04-19", "junho.lee@modernlab.co.kr");
            table.Rows.Add("E1011", "송미래", "D2", "개발1팀", "사원", "2022-08-16", "mirae.song@modernlab.co.kr");
            table.Rows.Add("E1012", "황도윤", "D3", "개발2팀", "과장", "2013-10-28", "doyun.hwang@modernlab.co.kr");
            table.Rows.Add("E1013", "임채원", "D4", "품질보증팀", "과장", "2016-02-15", "chaewon.lim@modernlab.co.kr");
            table.Rows.Add("E1014", "강태민", "D1", "경영지원팀", "대리", "2019-12-02", "taemin.kang@modernlab.co.kr");
            table.Rows.Add("E1015", "윤소희", "D3", "개발2팀", "사원", "2023-06-26", "sohee.yun@modernlab.co.kr");
            table.Rows.Add("E1016", "조은우", "D2", "개발1팀", "대리", "2018-05-08", "eunwoo.cho@modernlab.co.kr");
            table.Rows.Add("E1017", "신예린", "D4", "품질보증팀", "사원", "2024-03-04", "yerin.shin@modernlab.co.kr");
            table.Rows.Add("E1018", "배성준", "D3", "개발2팀", "부장", "2011-08-22", "seongjun.bae@modernlab.co.kr");
            table.Rows.Add("E1019", "문지아", "D1", "경영지원팀", "과장", "2015-09-14", "jia.moon@modernlab.co.kr");
            table.Rows.Add("E1020", "서동혁", "D2", "개발1팀", "사원", "2021-11-29", "donghyuk.seo@modernlab.co.kr");

            return table;
        }
    }
}
