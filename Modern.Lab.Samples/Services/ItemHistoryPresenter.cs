using System;
using System.Data;
using Modern.Lab.Data;

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
    internal static class ItemHistoryPresenter
    {
        // Scrap 상태 노드의 트리 텍스트 색 — 어두운 테마에서는 밝은 빨강이어야 보인다.
        private static string ScrapForeColor
        {
            get { return Modern.Lab.Theming.ModernTheme.IsDarkBased ? "#FF99A4" : "#C42B1C"; }
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
                if (TableHelper.CellText(row, "STAT_TYP") == "Scrapped")
                {
                    row["NODE_COLOR"] = ScrapForeColor;   // 트리 텍스트 빨강
                }
            }
        }

        // 상태(STAT_TYP)별 배지 배경색 — Selection 카드 상태 배지가 쓴다.
        // Created는 빈 값 = 중립 회색 배지.
        private static readonly System.Collections.Generic.Dictionary<string, string> statBadgeColors =
            new System.Collections.Generic.Dictionary<string, string>
            {
                { "Created", "" },
                { "Released", "#DCFCE7" },
                { "Run", "#DBEAFE" },
                { "Move", "#E0E7FF" },
                { "Hold", "#FEF3C7" },
                { "Store", "#D1FAE5" },
                { "Scrapped", "#FEE2E2" }
            };

        /// <summary>상태(STAT_TYP)별 배지 배경색. 미지정 상태는 빈 값(중립 배지).</summary>
        internal static string StatBadgeColor(string statTyp)
        {
            string color;
            return statBadgeColors.TryGetValue(statTyp, out color) ? color : string.Empty;
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
                string eventCd = TableHelper.CellText(history.Rows[index], "EVENT_CD");
                bool isCurrent = index == 0; // 최신 = 현재 단계
                string state = isCurrent
                    ? (eventCd == "Scrap" ? "Failed" : "Current")
                    : "Completed";

                steps.Rows.Add(eventCd, state);
            }

            return steps;
        }

        // (CellText 같은 화면 무관 유틸은 Modern.Lab.Data.TableHelper로 승격됐다.)

        private static bool TryParseEventTime(DataRow row, out DateTime value)
        {
            value = DateTime.MinValue;
            string text = TableHelper.CellText(row, "EVENT_TM");

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
