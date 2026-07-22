using System.Collections.Generic;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// ModernSlotMap의 한 구획 — 같은 종류 자리들의 묶음이다 (예: FOUP의
    /// "Slots" 하나, TRAY의 "STUB"과 "LCC" 두 구획). 구획마다 제목·채움
    /// 집계가 붙고, Columns 열의 격자로 셀이 배치된다 (1 = 세로 사다리).
    /// </summary>
    public class SlotMapSection
    {
        /// <summary>빈 구획을 만든다 (세로 사다리, 제목 없음, 기본 상자 표현).</summary>
        public SlotMapSection()
        {
            this.Title = string.Empty;
            this.Columns = 1;
            this.CellFontSize = 0d;
            this.Kind = SlotMapSectionKind.Generic;
            this.Cells = new List<SlotMapCell>();
        }

        /// <summary>실물 표현 종류 — WaferEdge(FOUP 웨이퍼 에지 뷰) /
        /// PinStub(핀 스텁 탑 뷰) / LamellaPost(포스트+라멜라). 기본
        /// Generic(상자 표현)은 기존 화면과 호환된다.</summary>
        public SlotMapSectionKind Kind { get; set; }

        /// <summary>구획 제목 (예: "Slots", "STUB", "LCC").</summary>
        public string Title { get; set; }

        /// <summary>셀 격자의 열 수 — 1이면 세로 사다리(FOUP 슬롯 스택),
        /// 5면 5열 격자(TRAY LCC) 식으로 실물 배치를 흉내 낸다.</summary>
        public int Columns { get; set; }

        /// <summary>이 구획 셀의 유닛 ID 글자 크기(px) 재정의 — 0이면 기본값.
        /// 열이 적어 셀이 넓은 구획(예: STUB 3열)을 크게 보이게 할 때 쓴다.
        /// 번호 칩은 이 값보다 한 단계 작게 따라간다.</summary>
        public double CellFontSize { get; set; }

        /// <summary>자리(셀) 목록 — 표시/미리보기 배정 순서 그대로 쓴다.</summary>
        public List<SlotMapCell> Cells { get; private set; }
    }
}
