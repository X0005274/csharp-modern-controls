using System;
using System.Data;
using System.Globalization;

namespace Modern.Lab.Samples.Services
{
    /// <summary>
    /// Pending Requests 화면의 파생 컬럼·필터·집계 모음 — 화면(폼)과 분리된
    /// 순수 DataTable 로직이다 (HistoryTablePresenter와 같은 역할 분담).
    ///
    /// 서버는 원본 행(도착 정보)과 서버만 알 수 있는 집계(UNIT_CNT)만 내려주고,
    /// 표시용 파생은 전부 여기서 계산한다:
    /// - ELAPSED_DAYS: 오늘 − 도착일(EVENT_TM). 쿼리의 SYSDATE 계산을 대체한다.
    /// - PRIORITY / DAYS_COLOR: 경과일 구간(0-2/3-6/7-13/14+) 강조 —
    ///   **물류처리 완료 건만** 대상, 미처리 행은 무색 배지 + "-".
    /// - CHK / LOGIS_CAN: 벌크 체크박스, 행 단위 물류처리 버튼 활성(미처리만).
    /// - Filter: 경과일 최소값 + 물류처리 상태 필터 (둘 다 클라이언트에서 일관 처리).
    /// - Aggregate: KPI(건수/Unit 합/평균/최대)와 경과일 구간 분포.
    /// </summary>
    internal static class PendingTablePresenter
    {
        /// <summary>경과일 구간 라벨 (분포 배지 표기와 PRIORITY 계산이 같은 경계를 쓴다).</summary>
        internal static readonly string[] AgingLabels = { "0-2 d", "3-6 d", "7-13 d", "14+ d" };

        // 경과일 구간별 배지 배경색 (구간이 심해질수록 파랑 → 호박 → 주황 → 빨강 틴트).
        // 하단 분포 배지(.Designer.cs의 Color)와 그리드 Days 배지가 같은 색을 쓴다.
        private static readonly string[] agingBadgeColors = { "#DBEAFE", "#FEF3C7", "#FFE0CC", "#FEE2E2" };

        /// <summary>집계 결과 — 폼은 이 값을 KPI/분포 배지에 그대로 표기만 한다.</summary>
        internal sealed class PendingSummary
        {
            /// <summary>조회 결과 행 수.</summary>
            internal int ItemCount;

            /// <summary>UNIT_CNT 합계.</summary>
            internal int UnitTotal;

            /// <summary>경과일 집계 대상(물류처리 완료) 건수. 0이면 Avg/Oldest는 무의미.</summary>
            internal int AgedCount;

            /// <summary>경과일 평균 (AgedCount가 0이면 0).</summary>
            internal double DaysAverage;

            /// <summary>경과일 최대 (AgedCount가 0이면 0).</summary>
            internal int DaysMax;

            /// <summary>경과일 구간(0-2/3-6/7-13/14+)별 건수 — AgingLabels와 같은 순서.</summary>
            internal readonly int[] AgingCounts = new int[4];
        }

        /// <summary>
        /// 경과일 파생 컬럼(ELAPSED_DAYS = 오늘 − EVENT_TM 날짜)을 채운다.
        /// EVENT_TM이 없거나 해석 불가한 행은 0일로 둔다. 일 단위 경과라
        /// 클라이언트 시계 기준으로 충분하다 (ItemHistory의 DURATION과 동일 철학).
        /// </summary>
        internal static void AddElapsedDays(DataTable pending)
        {
            if (pending == null)
            {
                return;
            }

            if (!pending.Columns.Contains("ELAPSED_DAYS"))
            {
                pending.Columns.Add("ELAPSED_DAYS", typeof(int));
            }

            DateTime today = DateTime.Today;

            foreach (DataRow row in pending.Rows)
            {
                DateTime arrived;

                if (DateTime.TryParse(CellText(row, "EVENT_TM"), out arrived))
                {
                    int days = (int)(today - arrived.Date).TotalDays;
                    row["ELAPSED_DAYS"] = days > 0 ? days : 0;
                }
                else
                {
                    row["ELAPSED_DAYS"] = 0;
                }
            }
        }

        /// <summary>
        /// 워크리스트 파생 컬럼을 채운다: PRIORITY/DAYS_COLOR(경과일 강조 —
        /// 물류처리 완료 건만), CHK(벌크 체크박스), LOGIS_CAN(행 버튼 활성 = 미처리).
        /// LOGIS_YN은 호출 전에 채워져 있어야 한다 (서버 값 또는 폼의 데모 시뮬레이션).
        /// </summary>
        internal static void ApplyWorkflowColumns(DataTable pending)
        {
            if (pending == null)
            {
                return;
            }

            if (!pending.Columns.Contains("PRIORITY"))
            {
                pending.Columns.Add("PRIORITY", typeof(string));
            }

            if (!pending.Columns.Contains("DAYS_COLOR"))
            {
                pending.Columns.Add("DAYS_COLOR", typeof(string));
            }

            if (!pending.Columns.Contains("CHK"))
            {
                pending.Columns.Add("CHK", typeof(bool));
            }

            if (!pending.Columns.Contains("LOGIS_CAN"))
            {
                pending.Columns.Add("LOGIS_CAN", typeof(bool));
            }

            foreach (DataRow row in pending.Rows)
            {
                ApplyAging(row);

                // 기존 컬럼에 행이 이미 있던 경우 DBNull로 남으므로 명시적으로 채운다.
                if (row.IsNull("CHK"))
                {
                    row["CHK"] = false;
                }

                row["LOGIS_CAN"] = !IsLogisticsDone(row);
            }
        }

        /// <summary>
        /// 경과일 강조(PRIORITY + Days 배지색)를 한 행에 적용한다.
        /// 경과일 체크는 **물류처리가 완료된 건만** 대상 — 미처리 행은
        /// 무색 배지 + Priority "-" 로 표시한다.
        /// </summary>
        internal static void ApplyAging(DataRow row)
        {
            if (!IsLogisticsDone(row))
            {
                row["PRIORITY"] = "-";
                row["DAYS_COLOR"] = string.Empty;
                return;
            }

            int days = ParseDays(CellText(row, "ELAPSED_DAYS"));
            int band = AgingBand(days);

            switch (band)
            {
                case 3:
                    row["PRIORITY"] = "Critical";
                    break;
                case 2:
                    row["PRIORITY"] = "Warning";
                    break;
                case 1:
                    row["PRIORITY"] = "Watch";
                    break;
                default:
                    row["PRIORITY"] = "Normal";
                    break;
            }

            row["DAYS_COLOR"] = agingBadgeColors[band];
        }

        /// <summary>물류처리 완료 여부 — LOGIS_YN이 "Y"면 완료로 본다.</summary>
        internal static bool IsLogisticsDone(DataRow row)
        {
            return "Y".Equals(CellText(row, "LOGIS_YN").Trim(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 물류처리 완료 상태로 표시한다 (LOGIS_YN=Y, 행 버튼 비활성).
        /// 완료되는 순간부터 경과일 체크 대상이므로 Days 배지/Priority도 함께 채운다.
        /// </summary>
        internal static void MarkLogisticsDone(DataRow row)
        {
            row["LOGIS_YN"] = "Y";
            row["LOGIS_CAN"] = false;
            ApplyAging(row);
        }

        /// <summary>
        /// 경과일 최소값(0 = 전체)과 물류처리 상태(""/N/Y)로 결과를 잘라낸다.
        /// 두 필터 모두 클라이언트에서 처리한다 — 서버 쿼리는 조건 없이 원본만 준다.
        /// </summary>
        internal static DataTable Filter(DataTable pending, int minDays, string logisticsFilter)
        {
            if (minDays <= 0 && logisticsFilter.Length == 0)
            {
                return pending;
            }

            bool wantDone = logisticsFilter == "Y";
            DataTable filtered = pending.Clone();

            foreach (DataRow row in pending.Rows)
            {
                if (minDays > 0 && ParseDays(CellText(row, "ELAPSED_DAYS")) < minDays)
                {
                    continue;
                }

                if (logisticsFilter.Length > 0 && IsLogisticsDone(row) != wantDone)
                {
                    continue;
                }

                filtered.ImportRow(row);
            }

            return filtered;
        }

        /// <summary>
        /// KPI와 경과일 분포를 집계한다. 경과일 통계(Avg/Oldest/분포)는
        /// 물류처리 완료 건만 대상 — UNIT_CNT 합계는 전체 행 대상.
        /// </summary>
        internal static PendingSummary Aggregate(DataTable result)
        {
            PendingSummary summary = new PendingSummary();

            if (result == null)
            {
                return summary;
            }

            summary.ItemCount = result.Rows.Count;
            int daysSum = 0;

            foreach (DataRow row in result.Rows)
            {
                summary.UnitTotal += ParseDays(CellText(row, "UNIT_CNT"));

                if (!IsLogisticsDone(row))
                {
                    continue;
                }

                summary.AgedCount = summary.AgedCount + 1;

                int days = ParseDays(CellText(row, "ELAPSED_DAYS"));
                daysSum += days;

                if (days > summary.DaysMax)
                {
                    summary.DaysMax = days;
                }

                summary.AgingCounts[AgingBand(days)] += 1;
            }

            if (summary.AgedCount > 0)
            {
                summary.DaysAverage = (double)daysSum / summary.AgedCount;
            }

            return summary;
        }

        /// <summary>서버 숫자 컬럼(JSON number)을 관용적으로 파싱한다 — 빈 값/소수 표기 모두 허용.</summary>
        internal static int ParseDays(string text)
        {
            double value;

            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                return (int)value;
            }

            return 0;
        }

        /// <summary>컬럼이 없거나(null 키 생략) DBNull인 경우를 빈 문자열로 읽는다.</summary>
        internal static string CellText(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
            {
                return string.Empty;
            }

            object value = row[columnName];
            return value == DBNull.Value || value == null ? string.Empty : value.ToString();
        }

        // 경과일 → 구간 인덱스 (0-2 / 3-6 / 7-13 / 14+). PRIORITY와 분포가 같은 경계를 쓴다.
        private static int AgingBand(int days)
        {
            if (days >= 14)
            {
                return 3;
            }

            if (days >= 7)
            {
                return 2;
            }

            if (days >= 3)
            {
                return 1;
            }

            return 0;
        }
    }
}
