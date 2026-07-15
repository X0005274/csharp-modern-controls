using System;
using System.Data;

namespace Modern.Lab.Samples.Services
{
    /// <summary>
    /// Item/Unit 이력 화면의 파생 컬럼·표시 계산 모음 — 화면(폼)과 분리된
    /// 순수 DataTable 로직이다.
    ///
    /// 폼은 서버 응답을 받아 이 클래스로 파생 컬럼(DURATION/ROW_COLOR/NODE_COLOR)을
    /// 채우고 상태바 접미어·진행 단계 표를 만들어 바인딩만 한다. 회사 환경 이식 시
    /// 서버 호출부(폼의 ★ 교체 지점)만 바꾸면 이 계산은 그대로 재사용된다.
    ///
    /// 색은 하드코딩하지 않고 ModernTheme 팔레트 기준으로 고른다 —
    /// 어두운 테마에서는 같은 의미의 어두운 톤을 쓴다.
    /// </summary>
    internal static class HistoryTablePresenter
    {
        // Scrap 상태 노드의 트리 텍스트 색 — 어두운 테마에서는 밝은 빨강이어야 보인다.
        private static string ScrapForeColor
        {
            get { return Modern.Lab.Theming.ModernTheme.IsDarkBased ? "#FF99A4" : "#C42B1C"; }
        }

        // 그리드 행 배경색 (Win11 시맨틱 면 색과 동일 계열) — 어두운 테마는 어두운 톤.
        private static string ScrapRowColor
        {
            get { return Modern.Lab.Theming.ModernTheme.IsDarkBased ? "#4C2B2C" : "#FDE7E9"; }
        }

        private static string DoneRowColor
        {
            get { return Modern.Lab.Theming.ModernTheme.IsDarkBased ? "#39412A" : "#DFF6DD"; }
        }

        /// <summary>Scrap 상태 Item에 트리 텍스트 색 컬럼(NODE_COLOR)을 채운다.</summary>
        internal static void ApplyScrapColor(DataTable tree)
        {
            if (tree == null)
            {
                return;
            }

            if (!tree.Columns.Contains("NODE_COLOR"))
            {
                tree.Columns.Add("NODE_COLOR", typeof(string));
            }

            foreach (DataRow row in tree.Rows)
            {
                if (CellText(row, "STAT_TYP") == "Scrap")
                {
                    row["NODE_COLOR"] = ScrapForeColor;   // 트리 텍스트 빨강
                }
            }
        }

        /// <summary>
        /// 소요시간 파생 컬럼. 이력은 최신순(TIMEKEY DESC)이므로 각 행의
        /// DURATION = 이 이벤트 시각 − 바로 이전(더 오래된) 이벤트 시각.
        /// 가장 오래된 행(맨 아래)은 이전이 없어 빈칸.
        /// </summary>
        internal static void AddDurationColumn(DataTable history)
        {
            if (history == null)
            {
                return;
            }

            if (!history.Columns.Contains("DURATION"))
            {
                history.Columns.Add("DURATION", typeof(string));
            }

            for (int index = 0; index < history.Rows.Count; index++)
            {
                DateTime current;
                DateTime older = DateTime.MinValue;

                bool hasCurrent = TryParseEventTime(history.Rows[index], out current);
                bool hasOlder = index + 1 < history.Rows.Count
                    && TryParseEventTime(history.Rows[index + 1], out older);

                history.Rows[index]["DURATION"] =
                    hasCurrent && hasOlder ? FormatDuration(current - older) : string.Empty;
            }
        }

        /// <summary>
        /// 이력 행 배경색 컬럼(ROW_COLOR)을 이벤트로 채운다:
        /// Scrapped=빨강, JobEnd(완료)=초록, 그 외는 빈칸(기본 교차색 유지).
        /// </summary>
        internal static void AddRowColor(DataTable history)
        {
            if (history == null)
            {
                return;
            }

            if (!history.Columns.Contains("ROW_COLOR"))
            {
                history.Columns.Add("ROW_COLOR", typeof(string));
            }

            foreach (DataRow row in history.Rows)
            {
                string eventCd = CellText(row, "EVENT_CD");

                if (eventCd == "Scrapped")
                {
                    row["ROW_COLOR"] = ScrapRowColor;
                }
                else if (eventCd == "JobEnd")
                {
                    row["ROW_COLOR"] = DoneRowColor;
                }
            }
        }

        /// <summary>웨이퍼 목록의 Scrap 행 배경(옅은 빨강) 컬럼을 채운다.</summary>
        internal static void AddUnitRowColor(DataTable units)
        {
            if (units == null)
            {
                return;
            }

            if (!units.Columns.Contains("ROW_COLOR"))
            {
                units.Columns.Add("ROW_COLOR", typeof(string));
            }

            foreach (DataRow row in units.Rows)
            {
                if (CellText(row, "STAT_TYP") == "Scrap")
                {
                    row["ROW_COLOR"] = ScrapRowColor;
                }
            }
        }

        /// <summary>상태바 우측에 붙일 총 사이클타임(가장 오래된 → 가장 최근 이벤트) 접미어.</summary>
        internal static string CycleTimeSuffix(DataTable history)
        {
            if (history == null || history.Rows.Count < 2)
            {
                return string.Empty;
            }

            DateTime newest;
            DateTime oldest;

            // 최신순이므로 첫 행이 최신, 마지막 행이 최초.
            if (TryParseEventTime(history.Rows[0], out newest)
                && TryParseEventTime(history.Rows[history.Rows.Count - 1], out oldest))
            {
                return "  ·  Cycle " + FormatDuration(newest - oldest);
            }

            return string.Empty;
        }

        /// <summary>
        /// 이력 이벤트를 시간순(왼→오른쪽) 단계(LABEL/STATE)로 만든다.
        /// 마지막(최신) 이벤트가 현재 단계이며, Scrapped면 Failed(빨강)로 표시한다.
        /// </summary>
        internal static DataTable BuildStepTable(DataTable history)
        {
            DataTable steps = new DataTable();
            steps.Columns.Add("LABEL", typeof(string));
            steps.Columns.Add("STATE", typeof(string));

            if (history == null || history.Rows.Count == 0)
            {
                return steps;
            }

            // 최신순 → 시간순으로 뒤집어 왼쪽부터 진행 순서가 되게 한다.
            for (int index = history.Rows.Count - 1; index >= 0; index--)
            {
                string eventCd = CellText(history.Rows[index], "EVENT_CD");
                bool isCurrent = index == 0; // 최신 = 현재 단계
                string state = isCurrent
                    ? (eventCd == "Scrapped" ? "Failed" : "Current")
                    : "Completed";

                steps.Rows.Add(eventCd, state);
            }

            return steps;
        }

        /// <summary>
        /// 서버 응답에 컬럼 자체가 없거나(null 키 생략) DBNull인 경우를 모두
        /// 빈 문자열로 읽는다 — 화면 표시/판정 공통 헬퍼.
        /// </summary>
        internal static string CellText(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
            {
                return string.Empty;
            }

            object value = row[columnName];
            return value == DBNull.Value || value == null ? string.Empty : value.ToString();
        }

        /// <summary>DataRowView 편의 오버로드.</summary>
        internal static string CellText(DataRowView row, string columnName)
        {
            return CellText(row.Row, columnName);
        }

        private static bool TryParseEventTime(DataRow row, out DateTime value)
        {
            value = DateTime.MinValue;
            string text = CellText(row, "EVENT_TM");

            if (text.Length == 0)
            {
                return false;
            }

            return DateTime.TryParse(text, out value);
        }

        // TimeSpan을 사람이 읽는 짧은 문자열로: "1d 4h" / "4h 12m" / "12m" / "45s".
        private static string FormatDuration(TimeSpan span)
        {
            if (span.Ticks < 0)
            {
                span = span.Negate();
            }

            if (span.TotalDays >= 1d)
            {
                return (int)span.TotalDays + "d " + span.Hours + "h";
            }

            if (span.TotalHours >= 1d)
            {
                return (int)span.TotalHours + "h " + span.Minutes + "m";
            }

            if (span.TotalMinutes >= 1d)
            {
                return span.Minutes + "m";
            }

            return span.Seconds + "s";
        }
    }
}
