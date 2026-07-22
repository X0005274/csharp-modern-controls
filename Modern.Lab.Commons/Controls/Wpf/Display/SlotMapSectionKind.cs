namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// ModernSlotMap 구획의 실물 표현 종류 — 셀을 어떤 실물 단면으로 그릴지
    /// 정한다. 셀/키/이벤트/미리보기 계약은 종류와 무관하게 동일하다.
    /// </summary>
    public enum SlotMapSectionKind
    {
        /// <summary>기본 상자 표현 — 번호 칩 + 유닛 토큰(단일) / 핑거 도트
        /// 미니 행(복합). 종류를 지정하지 않은 기존 화면과의 호환용.</summary>
        Generic = 0,

        /// <summary>웨이퍼 에지 뷰 — 카세트를 옆에서 본 단면. 좌우 레일 사이에
        /// 얇은 가로 바(웨이퍼)가 층층이 쌓이고, 빈 슬롯은 레일 홈만 남는다.
        /// FOUP 슬롯 스택용 (단일 수납 셀 + Columns = 1).</summary>
        WaferEdge = 1,

        /// <summary>핀 스텁 탑 뷰 — 원형 금속 스텁 위에 사각 칩을 올린 모습.
        /// 빈 스텁은 가운데 핀 자국만 있는 맨 원판이다. TRAY STUB용
        /// (단일 수납 셀).</summary>
        PinStub = 2,

        /// <summary>포스트 + 라멜라 — 베이스에서 솟은 포스트(핑거 A~E)에
        /// 라멜라 칩이 붙은 모습. 삽입 위치(Top/Left/Right)는 라멜라가 붙는
        /// **위치 자체**로 표현된다. TRAY LCC용 (복합 수납 셀).</summary>
        LamellaPost = 3
    }
}
