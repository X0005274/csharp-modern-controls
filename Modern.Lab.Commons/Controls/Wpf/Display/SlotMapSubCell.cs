namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// ModernSlotMap 복합 셀의 하위 자리 한 칸 (예: TRAY LCC의 핑거 A~E).
    /// LamellaPost 표현에서는 포스트로 그려지고, Marker(Top/Left/Right)가
    /// 라멜라 칩이 포스트에 붙는 위치가 된다. Generic 표현에서는 이름
    /// 도트로 그려진다.
    /// </summary>
    public class SlotMapSubCell
    {
        /// <summary>빈 하위 자리를 만든다.</summary>
        public SlotMapSubCell()
        {
            this.Name = string.Empty;
            this.UnitId = string.Empty;
            this.Marker = string.Empty;
            this.Color = string.Empty;
            this.Detail = string.Empty;
        }

        /// <summary>자리 이름 — 핑거 눈금 글자 (예: "A").</summary>
        public string Name { get; set; }

        /// <summary>수납된 유닛 ID (빈 자리는 빈 문자열).</summary>
        public string UnitId { get; set; }

        /// <summary>삽입 위치 — "Top"/"Left"/"Right" 중 하나 (없으면 빈
        /// 문자열). LamellaPost 표현에서 라멜라가 포스트에 붙는 위치이며,
        /// 채워진 자리에서만 그려진다.</summary>
        public string Marker { get; set; }

        /// <summary>채움 색 재정의 (선택) — 비우면 셀 색(Cell.Color) → 토큰
        /// 기본색 순으로 폴백한다. 하위 자리마다 소속(아이템)이 다를 수
        /// 있어 셀 색과 별도로 둔다.</summary>
        public string Color { get; set; }

        /// <summary>툴팁에 덧붙일 부가 정보 (선택) — 소속 아이템 ID 등.
        /// "A: CH-C01-01A (Top) — IT-C01"처럼 뒤에 붙는다.</summary>
        public string Detail { get; set; }
    }
}
