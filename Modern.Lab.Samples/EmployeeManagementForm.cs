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

        // 마지막 조회 결과(필터+정렬 적용, 전체 페이지). 페이지 이동 시
        // 재조회 없이 여기서 현재 페이지만 잘라 그리드에 바인딩한다.
        // 실제 서버 페이징 폼이라면 페이지 이동마다 해당 페이지를 요청한다.
        private DataTable lastResult;

        // 조직도 (평면 자기참조 테이블) — 트리 선택 시 하위 부서 코드 수집에 사용.
        private DataTable orgTable;

        // 조회조건 초기화(LoadSearchCodes)가 끝나기 전에는 변경 이벤트발 재조회를
        // 막는다 — 코드 테이블 할당만으로 이벤트가 발생하기 때문.
        private bool searchReady;

        public EmployeeManagementForm()
        {
            this.InitializeComponent();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            this.employeeMaster = CreateEmployeeMaster();
            this.nextEmployeeNumber = 1021;

            this.LoadSearchCodes();

            // 페이지 크기를 그리드 표시 가능 행 수에 맞춘다. 아직 WPF 배치 전이면
            // 첫 렌더링 후 VisibleRowCapacityChanged가 다시 맞춰 준다.
            int initialCapacity = this.gridEmployee.VisibleRowCapacity;

            if (initialCapacity > 1)
            {
                this.pagerEmployee.PageSize = initialCapacity;
            }

            this.searchReady = true;
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

            this.ConfigureGridColumns();

            // 조직도 트리: 서버 조직 테이블(키/부모키/명칭 평면 구조)을 그대로 할당.
            // 부모 키가 없는 행(모던랩)이 루트가 된다.
            this.orgTable = new DataTable();
            this.orgTable.Columns.Add("ORG_CODE", typeof(string));
            this.orgTable.Columns.Add("PARENT_CODE", typeof(string));
            this.orgTable.Columns.Add("ORG_NAME", typeof(string));
            this.orgTable.Rows.Add("C0", null, "모던랩");
            this.orgTable.Rows.Add("H1", "C0", "경영본부");
            this.orgTable.Rows.Add("D1", "H1", "경영지원팀");
            this.orgTable.Rows.Add("H2", "C0", "개발본부");
            this.orgTable.Rows.Add("D2", "H2", "개발1팀");
            this.orgTable.Rows.Add("D3", "H2", "개발2팀");
            this.orgTable.Rows.Add("H3", "C0", "품질본부");
            this.orgTable.Rows.Add("D4", "H3", "품질보증팀");

            this.treeOrg.IdMember = "ORG_CODE";
            this.treeOrg.ParentIdMember = "PARENT_CODE";
            this.treeOrg.DisplayMember = "ORG_NAME";
            this.treeOrg.DataSource = this.orgTable;

            // 엑셀 드롭다운 메뉴: 코드/명칭 구조 그대로.
            DataTable exportTable = new DataTable();
            exportTable.Columns.Add("EXPORT_CODE", typeof(string));
            exportTable.Columns.Add("EXPORT_NAME", typeof(string));
            exportTable.Rows.Add("PAGE", "현재 페이지 내보내기");
            exportTable.Rows.Add("ALL", "전체 결과 내보내기");
            exportTable.Rows.Add("CSV", "CSV로 내보내기");

            this.ddExcel.DisplayMember = "EXPORT_NAME";
            this.ddExcel.ValueMember = "EXPORT_CODE";
            this.ddExcel.DataSource = exportTable;

            // 정렬 기준 라디오 그룹: 코드/명칭 구조 그대로 (SORT_CODE = 정렬 컬럼명).
            DataTable sortTable = new DataTable();
            sortTable.Columns.Add("SORT_CODE", typeof(string));
            sortTable.Columns.Add("SORT_NAME", typeof(string));
            sortTable.Rows.Add("EMP_NO", "사번순");
            sortTable.Rows.Add("HIRE_DATE", "입사일순");
            sortTable.Rows.Add("SALARY", "연봉순");

            this.radioSort.DisplayMember = "SORT_NAME";
            this.radioSort.ValueMember = "SORT_CODE";
            this.radioSort.DataSource = sortTable;
            this.radioSort.SelectedValue = "EMP_NO";

            this.listDeptCount.DisplayMember = "CATEGORY";
            this.listDeptCount.ValueMember = "CNT";
            this.listDeptCount.ColorMember = "COLOR";
            this.listRankCount.DisplayMember = "CATEGORY";
            this.listRankCount.ValueMember = "CNT";
            this.listRankCount.ColorMember = "COLOR";

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

        // 그리드 컬럼 구성 — 이메일 컬럼은 토글(설정성 켬/끔)에 따라 포함된다.
        private void ConfigureGridColumns()
        {
            List<ModernDataGridColumn> columns = new List<ModernDataGridColumn>();
            columns.Add(new ModernDataGridColumn("EMP_NO", "사번", 90));
            columns.Add(new ModernDataGridColumn("EMP_NAME", "이름", 110));
            columns.Add(new ModernDataGridColumn("DEPT_NAME", "부서", 130));
            columns.Add(new ModernDataGridColumn("POSITION", "직급", 90) { TextAlignment = GridTextAlignment.Center });
            columns.Add(new ModernDataGridColumn("HIRE_DATE", "입사일", 110) { TextAlignment = GridTextAlignment.Center });

            if (this.tglShowEmail.Checked)
            {
                columns.Add(new ModernDataGridColumn("SALARY", "연봉(만원)", 100) { TextAlignment = GridTextAlignment.Right, Format = "N0" });
                columns.Add(new ModernDataGridColumn("EMAIL", "이메일"));
            }
            else
            {
                // 이메일 숨김 시 연봉이 남은 공간을 채운다(마지막 컬럼 star).
                columns.Add(new ModernDataGridColumn("SALARY", "연봉(만원)") { TextAlignment = GridTextAlignment.Right, Format = "N0" });
            }

            this.gridEmployee.ConfigureColumns(columns.ToArray());
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

            // 입사일 구간: 미선택(null)은 조건에서 제외. HIRE_DATE가 ISO 형식
            // (yyyy-MM-dd) 문자열이라 문자열 비교가 곧 날짜 비교가 된다.
            DateTime? hireFrom = this.dtHireFrom.Value;
            DateTime? hireTo = this.dtHireTo.Value;

            if (hireFrom.HasValue)
            {
                conditions.Add("HIRE_DATE >= '" + hireFrom.Value.ToString("yyyy-MM-dd") + "'");
            }

            if (hireTo.HasValue)
            {
                conditions.Add("HIRE_DATE <= '" + hireTo.Value.ToString("yyyy-MM-dd") + "'");
            }

            // 연봉 구간: 미입력(null)은 조건에서 제외. SALARY가 int 컬럼이라
            // 숫자 비교 그대로 동작한다.
            decimal? salaryFrom = this.numSalaryFrom.Value;
            decimal? salaryTo = this.numSalaryTo.Value;

            if (salaryFrom.HasValue)
            {
                conditions.Add("SALARY >= " + salaryFrom.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            if (salaryTo.HasValue)
            {
                conditions.Add("SALARY <= " + salaryTo.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            // 체크박스 필터: 켜져 있을 때만 조건 추가.
            if (this.chkRecentOnly.Checked)
            {
                conditions.Add("HIRE_DATE >= '2020-01-01'");
            }

            // 조직도 필터: 선택 조직의 하위 부서 코드들로 제한. 미선택(null)은 전체.
            string orgCode = this.treeOrg.SelectedValue as string;

            if (!string.IsNullOrEmpty(orgCode))
            {
                List<string> deptCodes = this.CollectDeptCodes(orgCode);

                if (deptCodes.Count > 0)
                {
                    conditions.Add("DEPT_CODE IN ('" + string.Join("', '", deptCodes.ToArray()) + "')");
                }
            }

            // 입사월 필터: 해당 년월(yyyy-MM)에 입사한 직원만. 미선택(null)은 제외.
            DateTime? hireMonth = this.monthHire.Value;

            if (hireMonth.HasValue)
            {
                conditions.Add("HIRE_DATE LIKE '" + hireMonth.Value.ToString("yyyy-MM") + "%'");
            }

            DataView view = new DataView(this.employeeMaster);
            view.RowFilter = string.Join(" AND ", conditions.ToArray());

            // 정렬 기준: 라디오 선택 값(정렬 컬럼명). 미선택이면 사번순.
            string sortColumn = this.radioSort.SelectedValue as string;
            view.Sort = (sortColumn ?? "EMP_NO") + " ASC";

            DataTable result = view.ToTable();

            // 통계(건수·칩)는 전체 결과 기준, 그리드에는 현재 페이지만 표시.
            this.lastResult = result;
            this.pagerEmployee.TotalCount = result.Rows.Count;
            this.pagerEmployee.CurrentPage = 1;
            this.BindCurrentPage();

            this.cardCount.Value = result.Rows.Count.ToString();
            this.listDeptCount.DataSource = GroupCount(result, "DEPT_NAME", deptChipColors);
            this.listRankCount.DataSource = GroupCount(result, "POSITION", rankChipColors);
        }

        // 부서별/직급별 칩 배경색 — 서버 코드 테이블의 색 컬럼을 흉내 내는 로컬 매핑.
        // 매핑에 없는 분류는 기본색으로 폴백된다.
        private static readonly Dictionary<string, string> deptChipColors = new Dictionary<string, string>
        {
            { "경영지원팀", "#DBEAFE" },
            { "개발1팀", "#DCFCE7" },
            { "개발2팀", "#FEF3C7" },
            { "품질보증팀", "#FCE7F3" }
        };

        private static readonly Dictionary<string, string> rankChipColors = new Dictionary<string, string>
        {
            { "부장", "#E0E7FF" },
            { "과장", "#CCFBF1" },
            { "대리", "#FFEDD5" },
            { "사원", "#F3E8FF" }
        };

        // 서버 측 GROUP BY를 대신하는 로컬 집계. 처음 나타난 순서를 유지한다.
        // colorMap이 있으면 분류 → 색 문자열을 COLOR 컬럼으로 함께 내린다.
        private static DataTable GroupCount(DataTable source, string columnName, IDictionary<string, string> colorMap)
        {
            DataTable table = new DataTable();
            table.Columns.Add("CATEGORY", typeof(string));
            table.Columns.Add("CNT", typeof(int));

            if (colorMap != null)
            {
                table.Columns.Add("COLOR", typeof(string));
            }

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
                if (colorMap != null && colorMap.ContainsKey(key))
                {
                    table.Rows.Add(key, counts[key], colorMap[key]);
                }
                else
                {
                    table.Rows.Add(key, counts[key]);
                }
            }

            return table;
        }

        private void OnSearchClick(object sender, EventArgs e)
        {
            // 조회 버튼은 서버 왕복을 흉내 내어 로딩 오버레이 사용법을 보여준다.
            // 실제 폼에서는 백그라운드 조회 시작 시 Busy = true,
            // UI 스레드 Invoke로 결과를 반영한 뒤 Busy = false로 되돌린다.
            this.busyOverlay.Busy = true;

            Timer delay = new Timer();
            delay.Interval = 600;
            delay.Tick += delegate(object timerSender, EventArgs timerArgs)
            {
                delay.Stop();
                delay.Dispose();
                this.ExecuteSearch();
                this.busyOverlay.Busy = false;
            };
            delay.Start();
        }

        // 체크 즉시 재조회 — CheckedChanged 이벤트 배선 예시.
        private void OnRecentOnlyCheckedChanged(object sender, EventArgs e)
        {
            if (this.searchReady)
            {
                this.ExecuteSearch();
            }
        }

        // 정렬 라디오 선택 즉시 재조회 — SelectedValueChanged 이벤트 배선 예시.
        private void OnSortChanged(object sender, EventArgs e)
        {
            if (this.searchReady)
            {
                this.ExecuteSearch();
            }
        }

        // 마지막 조회 결과에서 현재 페이지 구간만 잘라 그리드에 바인딩한다.
        private void BindCurrentPage()
        {
            if (this.lastResult == null)
            {
                return;
            }

            DataTable pageTable = this.lastResult.Clone();
            int start = (this.pagerEmployee.CurrentPage - 1) * this.pagerEmployee.PageSize;
            int end = Math.Min(start + this.pagerEmployee.PageSize, this.lastResult.Rows.Count);

            for (int i = start; i < end; i++)
            {
                pageTable.ImportRow(this.lastResult.Rows[i]);
            }

            this.gridEmployee.DataSource = pageTable;
        }

        // 페이지 이동 — 저장해 둔 결과에서 해당 페이지를 다시 바인딩한다.
        private void OnPagerPageChanged(object sender, EventArgs e)
        {
            if (this.searchReady)
            {
                this.BindCurrentPage();
            }
        }

        // 조직도에서 조직을 클릭하면 즉시 재조회한다.
        private void OnOrgTreeSelectionChanged(object sender, EventArgs e)
        {
            if (this.searchReady)
            {
                this.ExecuteSearch();
            }
        }

        // 선택 조직부터 하위 전체를 훑어 부서 코드(D*)만 모은다.
        // 실제 폼에서는 서버가 하위 부서 목록을 내려주는 경우가 많다.
        private List<string> CollectDeptCodes(string orgCode)
        {
            List<string> result = new List<string>();
            Queue<string> pending = new Queue<string>();
            pending.Enqueue(orgCode);

            while (pending.Count > 0)
            {
                string current = pending.Dequeue();

                if (current.StartsWith("D"))
                {
                    result.Add(current);
                }

                foreach (DataRow row in this.orgTable.Rows)
                {
                    if (string.Equals(row["PARENT_CODE"] as string, current, StringComparison.Ordinal))
                    {
                        pending.Enqueue(row["ORG_CODE"].ToString());
                    }
                }
            }

            return result;
        }

        // 그리드 높이가 바뀌면(폼 리사이즈) 페이지 크기를 표시 가능 행 수에 맞춘다.
        // 서버 페이징 폼이라면 리사이즈 종료(Form.ResizeEnd) 시점에만 재요청할 것.
        private void OnGridCapacityChanged(object sender, EventArgs e)
        {
            if (!this.searchReady)
            {
                return;
            }

            int capacity = this.gridEmployee.VisibleRowCapacity;

            if (this.pagerEmployee.PageSize != capacity)
            {
                this.pagerEmployee.PageSize = capacity;   // CurrentPage는 자동 보정
                this.BindCurrentPage();
            }
        }

        // 이메일 표시 토글 — 그리드 컬럼을 재구성하고 다시 바인딩한다.
        private void OnShowEmailToggled(object sender, EventArgs e)
        {
            if (this.searchReady)
            {
                this.ConfigureGridColumns();
                this.ExecuteSearch();
            }
        }

        private void OnResetClick(object sender, EventArgs e)
        {
            this.txtName.Text = string.Empty;
            this.cboDept.SelectedIndex = -1;
            this.cboRank.CheckedValues = null;
            this.dtHireFrom.Value = null;
            this.dtHireTo.Value = null;
            this.numSalaryFrom.Value = null;
            this.numSalaryTo.Value = null;
            this.chkRecentOnly.Checked = false;
            this.radioSort.SelectedValue = "EMP_NO";
            this.monthHire.Value = null;
            this.treeOrg.SelectedValue = null;
            this.ExecuteSearch();
        }

        private void OnExecuteClick(object sender, EventArgs e)
        {
            DataRowView selected = this.gridEmployee.SelectedItem as DataRowView;

            if (selected == null)
            {
                this.toastMain.Show("실행할 직원을 먼저 선택하세요.", Modern.Lab.Controls.Wpf.Display.ToastKind.Warning);
                return;
            }

            // 연동 지점: 선택 직원 대상 업무 동작(배치 실행, 승인 요청 등)을
            // 여기서 수행한다.
            this.toastMain.Show(
                selected["EMP_NAME"] + " (" + selected["EMP_NO"] + ") 대상 작업을 실행했습니다.",
                Modern.Lab.Controls.Wpf.Display.ToastKind.Info);
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
                employeeNo.ToLowerInvariant() + "@modernlab.co.kr",
                4000);

            this.ExecuteSearch();
            this.toastMain.Show("신규 직원 " + employeeNo + " 이(가) 추가되었습니다.", Modern.Lab.Controls.Wpf.Display.ToastKind.Success);
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            // 연동 지점: 현재 마스터를 서버로 전송한다.
            this.toastMain.Show(
                "직원 " + this.employeeMaster.Rows.Count + "건을 저장했습니다.",
                Modern.Lab.Controls.Wpf.Display.ToastKind.Success);
        }

        private void OnDeleteClick(object sender, EventArgs e)
        {
            DataRowView selected = this.gridEmployee.SelectedItem as DataRowView;

            if (selected == null)
            {
                this.toastMain.Show("삭제할 직원을 먼저 선택하세요.", Modern.Lab.Controls.Wpf.Display.ToastKind.Warning);
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
            this.toastMain.Show(employeeName + " (" + employeeNo + ") 직원을 삭제했습니다.", Modern.Lab.Controls.Wpf.Display.ToastKind.Success);
        }

        // 엑셀 드롭다운 메뉴 선택 — 코드(EXPORT_CODE)에 따라 분기한다.
        private void OnExcelItemClicked(object sender, Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs e)
        {
            string code = e.Value as string;
            int count = string.Equals(code, "PAGE", StringComparison.Ordinal)
                ? this.gridEmployee.RowCount
                : (this.lastResult != null ? this.lastResult.Rows.Count : 0);

            // 연동 지점: 선택된 방식으로 내보내기를 수행한다.
            this.toastMain.Show(
                e.DisplayText + " — " + count + "건을 내보냈습니다.",
                Modern.Lab.Controls.Wpf.Display.ToastKind.Info);
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
            table.Columns.Add("SALARY", typeof(int));

            table.Rows.Add("E1001", "김민수", "D1", "경영지원팀", "R1", "부장", "2012-03-02", "minsu.kim@modernlab.co.kr", 9500);
            table.Rows.Add("E1002", "이서연", "D2", "개발1팀", "R2", "과장", "2015-07-13", "seoyeon.lee@modernlab.co.kr", 7200);
            table.Rows.Add("E1003", "박지훈", "D2", "개발1팀", "R3", "대리", "2018-01-22", "jihun.park@modernlab.co.kr", 5600);
            table.Rows.Add("E1004", "최유진", "D3", "개발2팀", "R2", "과장", "2014-11-03", "yujin.choi@modernlab.co.kr", 7500);
            table.Rows.Add("E1005", "정다은", "D3", "개발2팀", "R4", "사원", "2021-05-17", "daeun.jung@modernlab.co.kr", 4200);
            table.Rows.Add("E1006", "한상우", "D4", "품질보증팀", "R3", "대리", "2019-09-09", "sangwoo.han@modernlab.co.kr", 5400);
            table.Rows.Add("E1007", "오세라", "D4", "품질보증팀", "R4", "사원", "2022-02-28", "sera.oh@modernlab.co.kr", 4000);
            table.Rows.Add("E1008", "장현우", "D3", "개발2팀", "R3", "대리", "2017-06-01", "hyunwoo.jang@modernlab.co.kr", 5800);
            table.Rows.Add("E1009", "김하늘", "D1", "경영지원팀", "R4", "사원", "2023-01-09", "haneul.kim@modernlab.co.kr", 3900);
            table.Rows.Add("E1010", "이준호", "D2", "개발1팀", "R1", "부장", "2010-04-19", "junho.lee@modernlab.co.kr", 10200);
            table.Rows.Add("E1011", "송미래", "D2", "개발1팀", "R4", "사원", "2022-08-16", "mirae.song@modernlab.co.kr", 4100);
            table.Rows.Add("E1012", "황도윤", "D3", "개발2팀", "R2", "과장", "2013-10-28", "doyun.hwang@modernlab.co.kr", 7800);
            table.Rows.Add("E1013", "임채원", "D4", "품질보증팀", "R2", "과장", "2016-02-15", "chaewon.lim@modernlab.co.kr", 7000);
            table.Rows.Add("E1014", "강태민", "D1", "경영지원팀", "R3", "대리", "2019-12-02", "taemin.kang@modernlab.co.kr", 5500);
            table.Rows.Add("E1015", "윤소희", "D3", "개발2팀", "R4", "사원", "2023-06-26", "sohee.yun@modernlab.co.kr", 3800);
            table.Rows.Add("E1016", "조은우", "D2", "개발1팀", "R3", "대리", "2018-05-08", "eunwoo.cho@modernlab.co.kr", 5700);
            table.Rows.Add("E1017", "신예린", "D4", "품질보증팀", "R4", "사원", "2024-03-04", "yerin.shin@modernlab.co.kr", 3700);
            table.Rows.Add("E1018", "배성준", "D3", "개발2팀", "R1", "부장", "2011-08-22", "seongjun.bae@modernlab.co.kr", 9800);
            table.Rows.Add("E1019", "문지아", "D1", "경영지원팀", "R2", "과장", "2015-09-14", "jia.moon@modernlab.co.kr", 7300);
            table.Rows.Add("E1020", "서동혁", "D2", "개발1팀", "R4", "사원", "2021-11-29", "donghyuk.seo@modernlab.co.kr", 4300);

            return table;
        }
    }
}
