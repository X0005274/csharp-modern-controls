using System;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Designer-pattern sample: this form is authored the way a MIGRATED legacy
    /// form looks — all controls are declared and laid out in
    /// DesignerMigrationForm.Designer.cs (InitializeComponent), exactly what the
    /// VS form designer reads and writes. Open this form in the designer to
    /// verify the wrapper controls render as snapshot previews on the design
    /// surface (docs/design-notes.md sections 2, 4, 5).
    ///
    /// Only data wiring lives here, as in a real migrated form: the designer
    /// never serializes DataSource (Browsable(false) by contract).
    /// </summary>
    public partial class DesignerMigrationForm : Form
    {
        public DesignerMigrationForm()
        {
            this.InitializeComponent();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            this.cboDept.DisplayMember = "DEPT_NAME";
            this.cboDept.ValueMember = "DEPT_CODE";
            this.cboDept.DataSource = CreateDepartmentTable();

            this.gridEmployee.ConfigureColumns(
                new ModernDataGridColumn("EMP_NO", "사번", 90),
                new ModernDataGridColumn("EMP_NAME", "이름", 110),
                new ModernDataGridColumn("DEPT_NAME", "부서", 130),
                new ModernDataGridColumn("POSITION", "직급", 90) { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("HIRE_DATE", "입사일") { TextAlignment = GridTextAlignment.Center });

            this.ExecuteSearch();
        }

        private void OnSearchClick(object sender, EventArgs e)
        {
            this.ExecuteSearch();
        }

        private void ExecuteSearch()
        {
            string nameFilter = this.txtName.Text.Trim();
            string deptCode = this.cboDept.SelectedValue as string;

            DataView view = new DataView(CreateEmployeeTable());
            string filter = string.Empty;

            if (nameFilter.Length > 0)
            {
                filter = "EMP_NAME LIKE '%" + nameFilter.Replace("'", "''") + "%'";
            }

            if (!string.IsNullOrEmpty(deptCode))
            {
                if (filter.Length > 0)
                {
                    filter = filter + " AND ";
                }

                filter = filter + "DEPT_CODE = '" + deptCode + "'";
            }

            view.RowFilter = filter;
            this.gridEmployee.DataSource = view.ToTable();
        }

        // Stand-ins for a server request/reply result (contract rule 2).
        private static DataTable CreateDepartmentTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("DEPT_CODE", typeof(string));
            table.Columns.Add("DEPT_NAME", typeof(string));
            table.Rows.Add("", "전체");
            table.Rows.Add("D1", "경영지원팀");
            table.Rows.Add("D2", "개발1팀");
            table.Rows.Add("D3", "개발2팀");
            return table;
        }

        private static DataTable CreateEmployeeTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("EMP_NO", typeof(string));
            table.Columns.Add("EMP_NAME", typeof(string));
            table.Columns.Add("DEPT_CODE", typeof(string));
            table.Columns.Add("DEPT_NAME", typeof(string));
            table.Columns.Add("POSITION", typeof(string));
            table.Columns.Add("HIRE_DATE", typeof(string));
            table.Rows.Add("E1001", "김민수", "D1", "경영지원팀", "부장", "2012-03-02");
            table.Rows.Add("E1002", "이서연", "D2", "개발1팀", "과장", "2015-07-13");
            table.Rows.Add("E1003", "박지훈", "D2", "개발1팀", "대리", "2018-01-22");
            table.Rows.Add("E1004", "최유진", "D3", "개발2팀", "과장", "2014-11-03");
            table.Rows.Add("E1005", "정다은", "D3", "개발2팀", "사원", "2021-05-17");
            return table;
        }
    }
}
