using System.Data;
using Modern.Lab.Data;

namespace Modern.Lab.Samples.Services
{
    /// <summary>
    /// Carrier Editor 화면의 순수 DataTable 헬퍼 — 수납 현황(폼이 조회 후
    /// NormalizeUnits로 정규화한 풀 맵)의 집계를 담당한다.
    /// 시각 표현(채움 색/선택/미리보기)은 슬롯 맵 컨트롤(ModernSlotMap)이
    /// 자체 처리하므로 파생 컬럼은 필요 없다.
    /// </summary>
    internal static class CarrierEditPresenter
    {
        /// <summary>채워진 자리 수 — 카드 타이틀의 "N / 용량 filled" 원천.</summary>
        internal static int CountFilled(DataTable units)
        {
            int count = 0;

            if (units == null)
            {
                return count;
            }

            foreach (DataRow row in units.Rows)
            {
                if (TableHelper.CellText(row, "UNIT_ID").Trim().Length > 0)
                {
                    count = count + 1;
                }
            }

            return count;
        }
    }
}
