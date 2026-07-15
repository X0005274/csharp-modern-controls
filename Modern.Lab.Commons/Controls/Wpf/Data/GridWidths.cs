namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// 그리드 컬럼 폭의 시맨틱 프리셋 — 픽셀 숫자 대신 의도를 읽을 수 있는 이름.
    ///
    /// 사용 지침:
    /// - <b>AutoFitColumns를 켠 그리드는 폭을 아예 생략하는 것이 우선이다</b> —
    ///   텍스트/배지/버튼 컬럼의 폭은 헤더+데이터 실측으로 재계산되어 어차피
    ///   무시된다. 숫자를 적어 두면 죽은 값이 되어 유지보수만 흐린다.
    /// - 폭이 실제로 쓰이는 곳에서만 프리셋을 쓴다: AutoFit을 끈 그리드,
    ///   그리고 AutoFit 그리드의 CheckBox 컬럼(내용 측정 대상이 아님).
    /// - 새 폭이 반복해서 필요해지면 숫자를 늘어놓지 말고 여기에 이름을 추가한다
    ///   (토큰 원칙 — 값이 되풀이되면 토큰으로).
    /// </summary>
    public static class GridWidths
    {
        /// <summary>체크박스 컬럼 (벌크 선택 표시).</summary>
        public const double Check = 44d;

        /// <summary>짧은 상태/구분 코드 (Status, Type 등).</summary>
        public const double Status = 84d;

        /// <summary>업무 코드 (Event, Operation, Equipment 등).</summary>
        public const double Code = 96d;

        /// <summary>식별자 (Item/Unit/Lot ID 등).</summary>
        public const double Id = 130d;

        /// <summary>명칭/제품명.</summary>
        public const double Name = 150d;

        /// <summary>일시 (yyyy-MM-dd HH:mm:ss).</summary>
        public const double DateTime = 150d;
    }
}
