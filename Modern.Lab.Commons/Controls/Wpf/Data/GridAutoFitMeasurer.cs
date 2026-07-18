using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// ModernDataGridControl의 AutoFitColumns 측정 엔진.
    /// 헤더 캡션과 데이터 내용의 실제 픽셀 폭을 FormattedText로 측정해
    /// 각 컬럼의 고정 픽셀 너비를 계산하고, 다중 줄 헤더("\n")에 맞춘
    /// 헤더 높이 계산도 담당한다. 폰트/DPI/토큰 리소스는 소유 컨트롤
    /// (owner)에서 읽는다.
    /// </summary>
    internal static class GridAutoFitMeasurer
    {
        // ===== AutoFitColumns 측정 상수 =====
        // 컬럼당 실제 픽셀 측정할 후보 문자열 수 — 글자 수 상위 후보만 측정해
        // 큰 데이터에서도 비용을 일정하게 유지한다.
        private const int autoFitCandidateCount = 8;
        // 자동 맞춤 너비의 하한/상한 (지나치게 좁은 컬럼과 폭주 컬럼 방지).
        private const double autoFitMinWidth = 48d;
        private const double autoFitMaxWidth = 600d;
        // 셀 좌우 패딩(Pad.Field 12,0) + 우측 구분선/렌더링 오차 여유.
        private const double autoFitCellPadding = 28d;
        // 헤더 전용 추가 여유: 오른쪽 고정 정렬 글리프(여백 6 + 글리프 폭).
        private const double autoFitSortGlyphReserve = 18d;
        // 배지 셀 추가 여유: 알약 좌우 패딩(10+10) + 곡률 여유.
        private const double autoFitBadgeReserve = 22d;
        // 버튼 셀 추가 여유: 버튼 좌우 패딩(14+14) + 테두리(1+1).
        private const double autoFitButtonChromeReserve = 30d;

        /// <summary>
        /// 각 컬럼 너비를 max(헤더 캡션 폭 + 글리프 여유, 데이터 최대 폭)으로
        /// 재계산해 고정 픽셀 너비로 넣는다. WPF DataGridLength.Auto는 가상화로
        /// 실체화된 행만 보고 계산해 스크롤 중 너비가 계속 변하므로 쓰지 않는다.
        /// </summary>
        /// <param name="grid">너비를 적용할 내부 DataGrid.</param>
        /// <param name="definitions">ApplyColumns로 받은 컬럼 정의 목록.</param>
        /// <param name="itemsSource">측정 대상 데이터 (null 허용).</param>
        /// <param name="owner">폰트/DPI/토큰 리소스를 읽을 소유 컨트롤.</param>
        /// <param name="widthRatio">유효 장평 — 측정 폭에 곱한다.</param>
        internal static void ApplyAutoFitWidths(
            DataGrid grid,
            IList<ModernDataGridColumn> definitions,
            IEnumerable itemsSource,
            Control owner,
            double widthRatio)
        {
            double pixelsPerDip = VisualTreeHelper.GetDpi(owner).PixelsPerDip;
            Typeface headerTypeface = new Typeface(owner.FontFamily, FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
            Typeface bodyTypeface = new Typeface(owner.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            double headerFontSize = (double)owner.FindResource("Font.Size.Label");
            double bodyFontSize = (double)owner.FindResource("Font.Size.Body");

            int count = Math.Min(definitions.Count, grid.Columns.Count);

            for (int index = 0; index < count; index++)
            {
                ModernDataGridColumn definition = definitions[index];

                // 체크박스 컬럼은 내용 측정 대상이 아니다 — 정의 폭(없으면 최소 폭)을 유지한다.
                if (definition.Kind == GridColumnKind.CheckBox)
                {
                    grid.Columns[index].Width =
                        new DataGridLength(definition.Width > 0d ? definition.Width : autoFitMinWidth);
                    continue;
                }

                double width = (MeasureText(definition.HeaderText, headerTypeface, headerFontSize, pixelsPerDip) * widthRatio)
                    + autoFitCellPadding + autoFitSortGlyphReserve;

                if (definition.Kind == GridColumnKind.Button)
                {
                    // 버튼 컬럼은 캡션 폭 기준 — 데이터 내용은 측정하지 않는다.
                    // 캡션이 SemiBold로 그려지므로 측정도 SemiBold 폭으로 한다.
                    double captionWidth = (MeasureText(definition.ButtonText, headerTypeface, bodyFontSize, pixelsPerDip) * widthRatio)
                        + autoFitCellPadding + autoFitButtonChromeReserve;

                    if (captionWidth > width)
                    {
                        width = captionWidth;
                    }
                }
                else
                {
                    // 배지 셀은 알약 좌우 패딩만큼 여유를 더한다.
                    double reserve = definition.Kind == GridColumnKind.Badge ? autoFitBadgeReserve : 0d;

                    // SemiBold 강조 컬럼은 실제 굵기 기준으로 잰다 (Regular보다 약간 넓음).
                    Typeface cellTypeface = definition.TextSemiBold ? headerTypeface : bodyTypeface;

                    foreach (string candidate in CollectLongestCellTexts(itemsSource, definition))
                    {
                        double cellWidth = (MeasureText(candidate, cellTypeface, bodyFontSize, pixelsPerDip) * widthRatio)
                            + autoFitCellPadding + reserve;

                        if (cellWidth > width)
                        {
                            width = cellWidth;
                        }
                    }
                }

                if (width < autoFitMinWidth)
                {
                    width = autoFitMinWidth;
                }

                if (width > autoFitMaxWidth)
                {
                    width = autoFitMaxWidth;
                }

                grid.Columns[index].Width = new DataGridLength(Math.Ceiling(width));
            }
        }

        /// <summary>
        /// 헤더 캡션의 최대 줄 수("\n" 기준)에 맞는 헤더 높이를 계산한다.
        /// HeaderText에 "Event\nTime"처럼 명시적 줄바꿈을 넣으면 2줄 이상 헤더가
        /// 되고, 줄바꿈이 없으면 토큰 기본 높이(Size.GridHeaderHeight) 그대로다.
        /// (헤더 문자열의 줄바꿈은 WPF TextBlock이 그대로 줄로 렌더링하므로
        /// 높이만 확보하면 된다. AutoFit 폭 측정도 FormattedText가 최장 줄 폭을
        /// 돌려주므로 별도 처리가 필요 없다.)
        /// </summary>
        /// <param name="owner">폰트/DPI/토큰 리소스를 읽을 소유 컨트롤.</param>
        /// <param name="definitions">컬럼 정의 목록 (null이면 기본 높이).</param>
        internal static double ComputeHeaderHeight(Control owner, IList<ModernDataGridColumn> definitions)
        {
            int maxLines = 1;

            if (definitions != null)
            {
                foreach (ModernDataGridColumn definition in definitions)
                {
                    int lines = CountHeaderLines(definition.HeaderText);

                    if (lines > maxLines)
                    {
                        maxLines = lines;
                    }
                }
            }

            double baseHeight = (double)owner.FindResource("Size.GridHeaderHeight");

            if (maxLines == 1)
            {
                return baseHeight;
            }

            // 추가 줄마다 헤더 캡션 한 줄 높이(Label 크기 실측)만큼 늘린다 —
            // 폰트 토큰이 바뀌어도 하드코딩 없이 따라온다.
            double pixelsPerDip = VisualTreeHelper.GetDpi(owner).PixelsPerDip;
            Typeface headerTypeface = new Typeface(
                owner.FontFamily, FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
            double headerFontSize = (double)owner.FindResource("Font.Size.Label");

            FormattedText lineProbe = new FormattedText(
                "Ag",
                CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                headerTypeface,
                headerFontSize,
                Brushes.Black,
                pixelsPerDip);

            return baseHeight + ((maxLines - 1) * lineProbe.Height);
        }

        // 헤더 캡션의 줄 수 ("\n" 개수 + 1; 빈 캡션은 1줄).
        private static int CountHeaderLines(string headerText)
        {
            if (string.IsNullOrEmpty(headerText))
            {
                return 1;
            }

            int lines = 1;

            for (int index = 0; index < headerText.Length; index++)
            {
                if (headerText[index] == '\n')
                {
                    lines = lines + 1;
                }
            }

            return lines;
        }

        // 한 컬럼의 셀 텍스트 중 "글자 수 상위" 후보만 모은다. 픽셀 폭은 글자 수와
        // 단조 일치하지 않지만(글자별 폭 차이), 상위 여러 개를 함께 측정하면
        // 실용적으로 안전하다 — 전체 행을 모두 픽셀 측정하는 비용을 피한다.
        private static List<string> CollectLongestCellTexts(IEnumerable source, ModernDataGridColumn definition)
        {
            List<string> candidates = new List<string>();

            if (source == null)
            {
                return candidates;
            }

            foreach (object row in source)
            {
                string text = FormatCellText(MemberPathReader.Read(row, definition.DataPropertyName), definition.Format);

                if (text.Length == 0 || candidates.Contains(text))
                {
                    continue;
                }

                if (candidates.Count < autoFitCandidateCount)
                {
                    candidates.Add(text);
                    continue;
                }

                // 가장 짧은 후보를 찾아 더 긴 텍스트로 교체한다.
                int shortestIndex = 0;

                for (int index = 1; index < candidates.Count; index++)
                {
                    if (candidates[index].Length < candidates[shortestIndex].Length)
                    {
                        shortestIndex = index;
                    }
                }

                if (text.Length > candidates[shortestIndex].Length)
                {
                    candidates[shortestIndex] = text;
                }
            }

            return candidates;
        }

        // 셀에 표시될 문자열을 만든다 — 컬럼 Format이 있으면 바인딩 StringFormat과
        // 같은 규칙("{0:형식}")으로 적용하고, 실패하면 기본 문자열 표현을 쓴다.
        private static string FormatCellText(object value, string format)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(format))
            {
                try
                {
                    return string.Format(CultureInfo.CurrentCulture, "{0:" + format + "}", value);
                }
                catch (FormatException)
                {
                    // 형식 오류는 기본 표현으로 폴백한다 (바인딩 표시와 동일한 완화).
                }
            }

            return value.ToString();
        }

        // FormattedText로 문자열의 실제 렌더링 폭을 잰다 (빈 문자열은 0).
        private static double MeasureText(string text, Typeface typeface, double fontSize, double pixelsPerDip)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0d;
            }

            FormattedText formatted = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black,
                pixelsPerDip);

            return formatted.WidthIncludingTrailingWhitespace;
        }
    }
}
