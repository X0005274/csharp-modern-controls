namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>ModernDataGridColumn의 셀 표시 종류.</summary>
    public enum GridColumnKind
    {
        /// <summary>일반 텍스트 셀 (기본).</summary>
        Text,

        /// <summary>
        /// 체크박스 셀 — bool 컬럼에 양방향 바인딩된다.
        /// 행 다중 선택(벌크 작업 대상 지정) 용도. 그리드가 읽기 전용이어도
        /// 체크박스는 클릭으로 즉시 토글되며 원본 행 값이 바로 갱신된다.
        /// </summary>
        CheckBox,

        /// <summary>
        /// 배지(알약) 셀 — 값 텍스트를 BadgeColorMember 컬럼의 배경색으로
        /// 감싸 표시한다. 글자색은 배경색에서 자동 유도된다(칩/배지 공통 규칙).
        /// </summary>
        Badge,

        /// <summary>
        /// 버튼 셀 — ButtonText 캡션의 행 단위 액션 버튼. 클릭하면 그리드의
        /// CellButtonClick 이벤트가 발생한다. ButtonEnabledMember 컬럼 값으로
        /// 행별 활성/비활성을 제어할 수 있다.
        /// </summary>
        Button
    }
}
