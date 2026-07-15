namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// ModernDataGrid / ModernDataGridControl이 사용하는 컬럼 정의.
    /// WinForms 폼이 WPF DataGrid 타입을 직접 다루지 않고도 컬럼을 정의할 수 있도록
    /// 하는 단순 데이터 홀더이다.
    /// </summary>
    public class ModernDataGridColumn
    {
        /// <summary>빈 정의를 만든다(스타 너비, 왼쪽 정렬, 형식 없음, 텍스트 셀).</summary>
        public ModernDataGridColumn()
        {
            this.DataPropertyName = string.Empty;
            this.HeaderText = string.Empty;
            this.Width = -1d;
            this.TextAlignment = GridTextAlignment.Left;
            this.Format = string.Empty;
            this.Kind = GridColumnKind.Text;
            this.TextColor = string.Empty;
            this.TextSemiBold = false;
            this.BadgeColorMember = string.Empty;
            this.ButtonText = string.Empty;
            this.ButtonEnabledMember = string.Empty;
        }

        /// <summary>지정한 컬럼/속성에 바인딩되는 스타 너비 컬럼을 만든다.</summary>
        public ModernDataGridColumn(string dataPropertyName, string headerText)
            : this()
        {
            this.DataPropertyName = dataPropertyName;
            this.HeaderText = headerText;
        }

        /// <summary>지정한 컬럼/속성에 바인딩되는 고정 너비 컬럼을 만든다.</summary>
        public ModernDataGridColumn(string dataPropertyName, string headerText, double width)
            : this(dataPropertyName, headerText)
        {
            this.Width = width;
        }

        /// <summary>원본 컬럼/속성 이름(DataTable 컬럼 또는 객체 속성).</summary>
        public string DataPropertyName { get; set; }

        /// <summary>헤더 캡션.</summary>
        public string HeaderText { get; set; }

        /// <summary>픽셀 너비. 0 이하이면 스타 크기 조정(남은 공간 채우기)을 의미한다.</summary>
        public double Width { get; set; }

        /// <summary>셀 텍스트의 가로 정렬.</summary>
        public GridTextAlignment TextAlignment { get; set; }

        /// <summary>
        /// 표시 형식 문자열 (예: 숫자 "N0"/"N2", 날짜 "yyyy-MM-dd").
        /// 원본 컬럼이 숫자/날짜 **타입**일 때만 적용된다 — 문자열 컬럼은 그대로 표시.
        /// 비어 있으면 값의 기본 문자열 표현을 쓴다. 정렬(sort)은 형식과 무관하게
        /// 원본 타입 값 기준으로 동작한다.
        /// </summary>
        public string Format { get; set; }

        /// <summary>셀 표시 종류 (기본 Text). CheckBox/Badge/Button 셀로 전환한다.</summary>
        public GridColumnKind Kind { get; set; }

        /// <summary>
        /// Text 전용: 컬럼 전체 텍스트에 적용할 글자색 — 파생 지표(Duration 등)를
        /// 강조할 때 쓴다. 값은 "#0078D4" 같은 색 문자열이며 비어 있거나 해석
        /// 불가하면 기본 글자색을 유지한다. 선택 행에서도 이 색이 유지되므로
        /// 선택 배경과 대비가 좋은 색을 고른다.
        /// </summary>
        public string TextColor { get; set; }

        /// <summary>
        /// Text 전용: 컬럼 전체 텍스트를 SemiBold로 강조한다 (기본 false).
        /// TextColor와 조합해 파생 지표를 색+굵기로 강조할 때 쓴다.
        /// AutoFitColumns 측정도 SemiBold 폭 기준으로 계산된다.
        /// </summary>
        public bool TextSemiBold { get; set; }

        /// <summary>
        /// Badge 전용: 배지 배경색으로 쓸 컬럼/속성 이름. 값은 "#FEE2E2" 같은
        /// 색 문자열. 비었거나 해석 불가한 행은 배경 없는 일반 텍스트로 표시된다.
        /// </summary>
        public string BadgeColorMember { get; set; }

        /// <summary>Button 전용: 버튼 캡션.</summary>
        public string ButtonText { get; set; }

        /// <summary>
        /// Button 전용: 행별 버튼 활성화 여부로 쓸 컬럼/속성 이름 (선택 사항).
        /// bool 컬럼 또는 "Y"/"true"/"1" 계열 문자열을 참으로 해석한다.
        /// 비워 두면 모든 행에서 버튼이 활성화된다.
        /// </summary>
        public string ButtonEnabledMember { get; set; }
    }
}
