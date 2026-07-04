using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// 직원관리 샘플 — 마이그레이션된 기존 폼과 동일한 방식으로 작성:
    /// 모든 컨트롤은 EmployeeManagementForm.Designer.cs(InitializeComponent)에
    /// 선언·배치되어 VS 폼 디자이너에서 열고 편집할 수 있다. 여기에는 데이터
    /// 배선만 존재한다 — DataSource 계열 멤버는 계약상 Browsable(false)라
    /// 디자이너 직렬화에 나타나지 않으며, 실제 마이그레이션 폼과 동일하다.
    ///
    /// 영역 구성 (계약 룰 5 — 레이아웃은 WinForms 담당):
    /// - 상단 카드: 조회조건 (이름 / 부서 / 직급 + 조회·초기화)
    /// - 중간: 직원 그리드 (4방향 Anchor)
    /// - 하단 카드: 좌측 조회 건수 KPI + 부서별/직급별 칩,
    ///   우측 실행 버튼 (실행=Execute, 신규·저장=Secondary, 삭제=Danger, 엑셀=Subtle)
    ///
    /// 코드/명칭 구조: 직급 테이블은 (RANK_CODE, RANK_NAME)으로 구성된다.
    /// 화면에는 명칭(DisplayMember)이 보이고, 조회 조건에는 코드(ValueMember,
    /// CheckedValues)가 사용된다 — 실제 서버 호출 시 코드를 그대로 전송하는 구조.
    /// 메모리 내 직원 마스터 DataTable이 서버를 대신하며, 모든 조회는 실제 폼과
    /// 동일한 DataSource 할당 경로를 거친다.
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

        // 조회조건 코드 테이블을 적재한다. 실제 폼에서는 서버 request/reply로
        // 받아오며, 컨트롤 계약 덕분에 이 코드는 동일하게 유지된다.
        private void LoadSearchCodes()
        {
            // "전체" 더미 행 없음: 미선택(SelectedIndex = -1) = 전체를 의미하고
            // 대신 플레이스홀더가 표시된다.
            DataTable deptTable = new DataTable();
            deptTable.Columns.Add("DEPT_CODE", typeof(string));
            deptTable.Columns.Add("DEPT_NAME", typeof(string));
            deptTable.Rows.Add("D1", "경영지원팀");
            deptTable.Rows.Add("D2", "개발1팀");
            deptTable.Rows.Add("D3", "개발2팀");
            deptTable.Rows.Add("D4", "품질보증팀");

            // 부서 콤보는 멀티컬럼 드롭다운: 코드+부서명이 헤더와 함께 표시되고,
            // 검색 시 코드("D3")로도 명칭("개발")으로도 필터링된다.
            this.cboDept.ConfigureDropDownColumns(
                new ModernDataGridColumn("DEPT_CODE", "코드", 60),
                new ModernDataGridColumn("DEPT_NAME", "부서명", 110));
            this.cboDept.DisplayMember = "DEPT_NAME";
            this.cboDept.ValueMember = "DEPT_CODE";
            this.cboDept.DataSource = deptTable;
            this.cboDept.SelectedIndex = -1;

            // 직급 필터는 체크콤보: 서버 직급 테이블 구조(코드, 명칭) 그대로.
            // 화면에는 명칭(RANK_NAME)이 보이고, CheckedValues는 코드(RANK_CODE)
            // 배열을 반환하므로 서버 호출에 코드를 그대로 쓸 수 있다.
            // 아무것도 체크하지 않으면 "전체"(플레이스홀더 표시).
            DataTable rankTable = new DataTable();
            rankTable.Columns.Add("RANK_CODE", typeof(string));
            rankTable.Columns.Add("RANK_NAME", typeof(string));
            rankTable.Rows.Add("R1", "부장");
            rankTable.Rows.Add("R2", "과장");
            rankTable.Rows.Add("R3", "대리");
            rankTable.Rows.Add("R4", "사원");

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

            // 이름 필터의 검색창형 자동완성 — 기존 WinForms TextBox 멤버와
            // 완전히 동일한 배선.
            AutoCompleteStringCollection nameCandidates = new AutoCompleteStringCollection();

            foreach (DataRow row in this.employeeMaster.Rows)
            {
                nameCandidates.Add(row["EMP_NAME"].ToString());
            }

            this.txtName.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.txtName.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.txtName.AutoCompleteCustomSource = nameCandidates;
        }

        // 메모리 내 마스터를 대상으로 조회를 실행하고 결과를 그리드와 통계
        // 영역에 반영한다 — 조회 → 그리드 → 통계 흐름.
        private void ExecuteSearch()
        {
            string nameFilter = this.txtName.Text.Trim();
            string deptCode = this.cboDept.SelectedValue as string;

            // 서버 호출 시나리오: 화면 표시는 명칭이지만 조건에는 코드가 실린다.
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

                conditions.Add("POSITION_CODE IN (" + string.Join(", ", quoted.ToArray()) + ")");
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

        // 서버 측 GROUP BY를 대신하는 로컬 집계. 처음 나타난 순서를 유지한다.
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

            // 연동 지점: 선택 직원 대상 업무 동작(배치 실행, 승인 요청 등)을
            // 여기서 수행한다.
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
                "D2", "개발1팀", "R4", "사원",
                DateTime.Today.ToString("yyyy-MM-dd"),
                employeeNo.ToLowerInvariant() + "@modernlab.co.kr");

            this.ExecuteSearch();
            MessageBox.Show(this, "신규 직원 " + employeeNo + " 이(가) 추가되었습니다.", "직원관리");
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            // 연동 지점: 현재 마스터를 서버로 전송한다.
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
            // 연동 지점: 현재 그리드 결과를 엑셀로 내보낸다.
            MessageBox.Show(
                this,
                "조회 결과 " + this.gridEmployee.RowCount + "건을 엑셀로 내보냅니다. (샘플: 내보내기 지점)",
                "직원관리");
        }

        // 서버 데이터베이스를 대신하는 메모리 내 직원 마스터 (계약 룰 2).
        // POSITION_CODE(서버 코드)와 POSITION(표시 명칭)을 모두 가진다.
        private static DataTable CreateEmployeeMaster()
        {
            DataTable table = new DataTable();
            table.Columns.Add("EMP_NO", typeof(string));
            table.Columns.Add("EMP_NAME", typeof(string));
            table.Columns.Add("DEPT_CODE", typeof(string));
            table.Columns.Add("DEPT_NAME", typeof(string));
            table.Columns.Add("POSITION_CODE", typeof(string));
            table.Columns.Add("POSITION", typeof(string));
            table.Columns.Add("HIRE_DATE", typeof(string));
            table.Columns.Add("EMAIL", typeof(string));

            table.Rows.Add("E1001", "김민수", "D1", "경영지원팀", "R1", "부장", "2012-03-02", "minsu.kim@modernlab.co.kr");
            table.Rows.Add("E1002", "이서연", "D2", "개발1팀", "R2", "과장", "2015-07-13", "seoyeon.lee@modernlab.co.kr");
            table.Rows.Add("E1003", "박지훈", "D2", "개발1팀", "R3", "대리", "2018-01-22", "jihun.park@modernlab.co.kr");
            table.Rows.Add("E1004", "최유진", "D3", "개발2팀", "R2", "과장", "2014-11-03", "yujin.choi@modernlab.co.kr");
            table.Rows.Add("E1005", "정다은", "D3", "개발2팀", "R4", "사원", "2021-05-17", "daeun.jung@modernlab.co.kr");
            table.Rows.Add("E1006", "한상우", "D4", "품질보증팀", "R3", "대리", "2019-09-09", "sangwoo.han@modernlab.co.kr");
            table.Rows.Add("E1007", "오세라", "D4", "품질보증팀", "R4", "사원", "2022-02-28", "sera.oh@modernlab.co.kr");
            table.Rows.Add("E1008", "장현우", "D3", "개발2팀", "R3", "대리", "2017-06-01", "hyunwoo.jang@modernlab.co.kr");
            table.Rows.Add("E1009", "김하늘", "D1", "경영지원팀", "R4", "사원", "2023-01-09", "haneul.kim@modernlab.co.kr");
            table.Rows.Add("E1010", "이준호", "D2", "개발1팀", "R1", "부장", "2010-04-19", "junho.lee@modernlab.co.kr");
            table.Rows.Add("E1011", "송미래", "D2", "개발1팀", "R4", "사원", "2022-08-16", "mirae.song@modernlab.co.kr");
            table.Rows.Add("E1012", "황도윤", "D3", "개발2팀", "R2", "과장", "2013-10-28", "doyun.hwang@modernlab.co.kr");
            table.Rows.Add("E1013", "임채원", "D4", "품질보증팀", "R2", "과장", "2016-02-15", "chaewon.lim@modernlab.co.kr");
            table.Rows.Add("E1014", "강태민", "D1", "경영지원팀", "R3", "대리", "2019-12-02", "taemin.kang@modernlab.co.kr");
            table.Rows.Add("E1015", "윤소희", "D3", "개발2팀", "R4", "사원", "2023-06-26", "sohee.yun@modernlab.co.kr");
            table.Rows.Add("E1016", "조은우", "D2", "개발1팀", "R3", "대리", "2018-05-08", "eunwoo.cho@modernlab.co.kr");
            table.Rows.Add("E1017", "신예린", "D4", "품질보증팀", "R4", "사원", "2024-03-04", "yerin.shin@modernlab.co.kr");
            table.Rows.Add("E1018", "배성준", "D3", "개발2팀", "R1", "부장", "2011-08-22", "seongjun.bae@modernlab.co.kr");
            table.Rows.Add("E1019", "문지아", "D1", "경영지원팀", "R2", "과장", "2015-09-14", "jia.moon@modernlab.co.kr");
            table.Rows.Add("E1020", "서동혁", "D2", "개발1팀", "R4", "사원", "2021-11-29", "donghyuk.seo@modernlab.co.kr");

            return table;
        }
    }
}
