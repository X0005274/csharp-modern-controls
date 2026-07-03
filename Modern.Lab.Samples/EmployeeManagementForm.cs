using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Controls.Wpf.Input;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.WinForms.Controls.Input;
using Modern.Lab.WinForms.Controls.Layout;
using Modern.Lab.WinForms.Controls.Selection;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Employee management sample screen assembled per the control design
    /// contract (docs/design-notes.md section 6-1):
    /// - Area layout is pure WinForms (TableLayoutPanel/FlowLayoutPanel);
    ///   modern controls are leaf widgets only.
    /// - Top: search conditions (name / department / rank + buttons).
    /// - Middle: employee grid.
    /// - Bottom left: result count KPI + per-department / per-rank summaries.
    /// - Bottom right: action buttons.
    /// The in-memory master DataTable stands in for the server; every query
    /// runs through the same DataSource assignment path a real form would use.
    /// </summary>
    public class EmployeeManagementForm : Form
    {
        private DataTable employeeMaster;
        private int nextEmployeeNumber;

        private ModernTextBox nameTextBox;
        private ModernComboBox deptComboBox;
        private ModernComboBox rankComboBox;
        private ModernButton searchButton;
        private ModernButton resetButton;

        private ModernDataGrid employeeGrid;

        private ModernKpiCard countCard;
        private ModernSummaryList deptSummary;
        private ModernSummaryList rankSummary;

        private ModernButton newButton;
        private ModernButton saveButton;
        private ModernButton deleteButton;
        private ModernButton excelButton;

        public EmployeeManagementForm()
        {
            this.employeeMaster = CreateEmployeeMaster();
            this.nextEmployeeNumber = 1021;

            this.InitializeLayout();
            this.LoadSearchCodes();
            this.ExecuteSearch();
        }

        private void InitializeLayout()
        {
            this.Text = "직원관리";
            this.BackColor = Color.FromArgb(247, 248, 250);
            this.ClientSize = new Size(920, 640);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 3;
            root.Padding = new Padding(16, 12, 16, 12);
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 100f));
            this.Controls.Add(root);

            root.Controls.Add(this.BuildSearchArea(), 0, 0);
            root.Controls.Add(this.BuildGridArea(), 0, 1);
            root.Controls.Add(this.BuildBottomArea(), 0, 2);
        }

        // Top area: search conditions in a single flow row on a shared card panel.
        private Control BuildSearchArea()
        {
            ModernCardPanel searchCard = new ModernCardPanel();
            searchCard.Dock = DockStyle.Fill;
            searchCard.Margin = new Padding(0);
            searchCard.Padding = new Padding(12, 9, 12, 9);

            FlowLayoutPanel searchRow = new FlowLayoutPanel();
            searchRow.Dock = DockStyle.Fill;
            searchRow.FlowDirection = FlowDirection.LeftToRight;
            searchRow.WrapContents = false;
            searchRow.Margin = new Padding(0);

            searchRow.Controls.Add(CreateFieldLabel("이름"));

            this.nameTextBox = new ModernTextBox();
            this.nameTextBox.PlaceholderText = "이름 검색";
            this.nameTextBox.Size = new Size(160, 32);
            this.nameTextBox.Margin = new Padding(0, 4, 16, 4);
            this.nameTextBox.EnterPressed += this.OnSearchClick;
            searchRow.Controls.Add(this.nameTextBox);

            searchRow.Controls.Add(CreateFieldLabel("부서"));

            this.deptComboBox = new ModernComboBox();
            this.deptComboBox.Size = new Size(140, 32);
            this.deptComboBox.Margin = new Padding(0, 4, 16, 4);
            searchRow.Controls.Add(this.deptComboBox);

            searchRow.Controls.Add(CreateFieldLabel("직급"));

            this.rankComboBox = new ModernComboBox();
            this.rankComboBox.Size = new Size(120, 32);
            this.rankComboBox.Margin = new Padding(0, 4, 24, 4);
            searchRow.Controls.Add(this.rankComboBox);

            this.searchButton = new ModernButton();
            this.searchButton.Text = "조회";
            this.searchButton.Kind = ButtonKind.Primary;
            this.searchButton.Size = new Size(80, 32);
            this.searchButton.Margin = new Padding(0, 4, 8, 4);
            this.searchButton.Click += this.OnSearchClick;
            searchRow.Controls.Add(this.searchButton);

            this.resetButton = new ModernButton();
            this.resetButton.Text = "초기화";
            this.resetButton.Kind = ButtonKind.Subtle;
            this.resetButton.Size = new Size(80, 32);
            this.resetButton.Margin = new Padding(0, 4, 0, 4);
            this.resetButton.Click += this.OnResetClick;
            searchRow.Controls.Add(this.resetButton);

            searchCard.Controls.Add(searchRow);

            return searchCard;
        }

        private static ModernLabel CreateFieldLabel(string text)
        {
            ModernLabel label = new ModernLabel();
            label.Text = text;
            label.Kind = LabelKind.Label;
            label.Size = new Size(40, 32);
            label.Margin = new Padding(0, 4, 4, 4);
            return label;
        }

        // Middle area: the employee grid fills all remaining space.
        private Control BuildGridArea()
        {
            this.employeeGrid = new ModernDataGrid();
            this.employeeGrid.Dock = DockStyle.Fill;
            this.employeeGrid.Margin = new Padding(0, 8, 0, 8);
            this.employeeGrid.ConfigureColumns(
                new ModernDataGridColumn("EMP_NO", "사번", 90),
                new ModernDataGridColumn("EMP_NAME", "이름", 110),
                new ModernDataGridColumn("DEPT_NAME", "부서", 130),
                new ModernDataGridColumn("POSITION", "직급", 90) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("HIRE_DATE", "입사일", 110) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("EMAIL", "이메일"));

            return this.employeeGrid;
        }

        // Bottom area: one shared card panel — flat statistics on the left
        // (KPI + stacked per-department / per-rank chip rows), action buttons
        // on the right. Button tiers: execute = Secondary, delete = Danger
        // (outlined red), export = Subtle.
        private Control BuildBottomArea()
        {
            ModernCardPanel bottomCard = new ModernCardPanel();
            bottomCard.Dock = DockStyle.Fill;
            bottomCard.Margin = new Padding(0);
            bottomCard.Padding = new Padding(12, 6, 12, 6);

            TableLayoutPanel bottom = new TableLayoutPanel();
            bottom.Dock = DockStyle.Fill;
            bottom.Margin = new Padding(0);
            bottom.ColumnCount = 3;
            bottom.RowCount = 1;
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            this.countCard = new ModernKpiCard();
            this.countCard.Title = "조회 건수";
            this.countCard.Value = "0";
            this.countCard.Flat = true;
            this.countCard.Dock = DockStyle.Fill;
            this.countCard.Margin = new Padding(0, 0, 8, 0);

            TableLayoutPanel statsRows = new TableLayoutPanel();
            statsRows.Dock = DockStyle.Fill;
            statsRows.Margin = new Padding(0);
            statsRows.ColumnCount = 1;
            statsRows.RowCount = 2;
            statsRows.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            statsRows.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            this.deptSummary = new ModernSummaryList();
            this.deptSummary.Title = "부서별";
            this.deptSummary.DisplayMember = "CATEGORY";
            this.deptSummary.ValueMember = "CNT";
            this.deptSummary.Flat = true;
            this.deptSummary.Dock = DockStyle.Fill;
            this.deptSummary.Margin = new Padding(0);

            this.rankSummary = new ModernSummaryList();
            this.rankSummary.Title = "직급별";
            this.rankSummary.DisplayMember = "CATEGORY";
            this.rankSummary.ValueMember = "CNT";
            this.rankSummary.Flat = true;
            this.rankSummary.Dock = DockStyle.Fill;
            this.rankSummary.Margin = new Padding(0);

            statsRows.Controls.Add(this.deptSummary, 0, 0);
            statsRows.Controls.Add(this.rankSummary, 0, 1);

            FlowLayoutPanel buttonRow = new FlowLayoutPanel();
            buttonRow.FlowDirection = FlowDirection.LeftToRight;
            buttonRow.WrapContents = false;
            buttonRow.AutoSize = true;
            buttonRow.Anchor = AnchorStyles.Right;
            buttonRow.Margin = new Padding(0);

            this.newButton = CreateActionButton("신규", ButtonKind.Secondary, this.OnNewClick);
            buttonRow.Controls.Add(this.newButton);

            this.saveButton = CreateActionButton("저장", ButtonKind.Secondary, this.OnSaveClick);
            buttonRow.Controls.Add(this.saveButton);

            this.deleteButton = CreateActionButton("삭제", ButtonKind.Danger, this.OnDeleteClick);
            buttonRow.Controls.Add(this.deleteButton);

            this.excelButton = CreateActionButton("엑셀", ButtonKind.Subtle, this.OnExcelClick);
            this.excelButton.Margin = new Padding(0, 0, 0, 0);
            buttonRow.Controls.Add(this.excelButton);

            bottom.Controls.Add(this.countCard, 0, 0);
            bottom.Controls.Add(statsRows, 1, 0);
            bottom.Controls.Add(buttonRow, 2, 0);

            bottomCard.Controls.Add(bottom);

            return bottomCard;
        }

        private static ModernButton CreateActionButton(string text, ButtonKind kind, EventHandler onClick)
        {
            ModernButton button = new ModernButton();
            button.Text = text;
            button.Kind = kind;
            button.Size = new Size(80, 32);
            button.Margin = new Padding(0, 0, 8, 0);
            button.Click += onClick;
            return button;
        }

        // Loads the search-condition code tables. In a real form these come from
        // a server request/reply; the control contract keeps this code identical.
        private void LoadSearchCodes()
        {
            DataTable deptTable = new DataTable();
            deptTable.Columns.Add("DEPT_CODE", typeof(string));
            deptTable.Columns.Add("DEPT_NAME", typeof(string));
            deptTable.Rows.Add("", "전체");
            deptTable.Rows.Add("D1", "경영지원팀");
            deptTable.Rows.Add("D2", "개발1팀");
            deptTable.Rows.Add("D3", "개발2팀");
            deptTable.Rows.Add("D4", "품질보증팀");

            this.deptComboBox.DisplayMember = "DEPT_NAME";
            this.deptComboBox.ValueMember = "DEPT_CODE";
            this.deptComboBox.DataSource = deptTable;

            DataTable rankTable = new DataTable();
            rankTable.Columns.Add("RANK_CODE", typeof(string));
            rankTable.Columns.Add("RANK_NAME", typeof(string));
            rankTable.Rows.Add("", "전체");
            rankTable.Rows.Add("부장", "부장");
            rankTable.Rows.Add("과장", "과장");
            rankTable.Rows.Add("대리", "대리");
            rankTable.Rows.Add("사원", "사원");

            this.rankComboBox.DisplayMember = "RANK_NAME";
            this.rankComboBox.ValueMember = "RANK_CODE";
            this.rankComboBox.DataSource = rankTable;
        }

        // Runs the query against the in-memory master and pushes the result into
        // the grid and the statistics area — the search → grid → stats flow.
        private void ExecuteSearch()
        {
            string nameFilter = this.nameTextBox.Text.Trim();
            string deptCode = this.deptComboBox.SelectedValue as string;
            string rankCode = this.rankComboBox.SelectedValue as string;

            List<string> conditions = new List<string>();

            if (nameFilter.Length > 0)
            {
                conditions.Add("EMP_NAME LIKE '%" + nameFilter.Replace("'", "''") + "%'");
            }

            if (!string.IsNullOrEmpty(deptCode))
            {
                conditions.Add("DEPT_CODE = '" + deptCode + "'");
            }

            if (!string.IsNullOrEmpty(rankCode))
            {
                conditions.Add("POSITION = '" + rankCode + "'");
            }

            DataView view = new DataView(this.employeeMaster);
            view.RowFilter = string.Join(" AND ", conditions.ToArray());
            view.Sort = "EMP_NO ASC";

            DataTable result = view.ToTable();

            this.employeeGrid.DataSource = result;
            this.countCard.Value = result.Rows.Count.ToString();
            this.deptSummary.DataSource = GroupCount(result, "DEPT_NAME");
            this.rankSummary.DataSource = GroupCount(result, "POSITION");
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
            this.nameTextBox.Text = string.Empty;
            this.deptComboBox.SelectedIndex = 0;
            this.rankComboBox.SelectedIndex = 0;
            this.ExecuteSearch();
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
            DataRowView selected = this.employeeGrid.SelectedItem as DataRowView;

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
                "조회 결과 " + this.employeeGrid.RowCount + "건을 엑셀로 내보냅니다. (샘플: 내보내기 지점)",
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
