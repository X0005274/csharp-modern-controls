using System;
using System.Text;

namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>
    /// 마스크 입력 필드(숫자/날짜/년월)가 공유하는 순수 문자열 도우미.
    ///
    /// - 원본 텍스트에서 유효 문자(기본은 숫자)만 추려내고,
    /// - 재형식화된 텍스트에서 "앞에 있던 유효 문자 n개 뒤" 캐럿 위치를 복원하고,
    /// - 천단위 콤마 그룹핑과 구분자 마스크(yyyy-MM-dd 등) 조립을 담당한다.
    ///
    /// UI(WPF) 의존이 전혀 없는 순수 함수만 담는다 — 캐럿 복원 규칙이
    /// 세 컨트롤에서 동일하게 유지되도록 이 한 곳에서만 구현한다.
    /// </summary>
    internal static class DigitMaskHelper
    {
        // 기본 유효 문자 판정(숫자). 매 호출마다 델리게이트가 새로 생기지 않도록 캐시한다.
        private static readonly Func<char, bool> digitPredicate = char.IsDigit;

        /// <summary>텍스트의 [0, endExclusive) 구간에 있는 숫자 개수를 센다.</summary>
        internal static int CountDigits(string text, int endExclusive)
        {
            return CountSignificant(text, endExclusive, digitPredicate);
        }

        /// <summary>텍스트의 [0, endExclusive) 구간에서 isSignificant가 참인 유효 문자 개수를 센다.</summary>
        internal static int CountSignificant(string text, int endExclusive, Func<char, bool> isSignificant)
        {
            int count = 0;
            int limit = Math.Min(endExclusive, text.Length);

            for (int i = 0; i < limit; i++)
            {
                if (isSignificant(text[i]))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>텍스트에서 숫자만 앞에서부터 최대 maxCount자리까지 추려낸다.</summary>
        internal static string ExtractDigits(string text, int maxCount)
        {
            StringBuilder builder = new StringBuilder(maxCount);

            foreach (char ch in text)
            {
                if (char.IsDigit(ch))
                {
                    builder.Append(ch);

                    if (builder.Length >= maxCount)
                    {
                        break;
                    }
                }
            }

            return builder.ToString();
        }

        /// <summary>형식화된 텍스트에서 n번째 숫자 바로 뒤의 캐럿 위치를 구한다.</summary>
        internal static int CaretIndexAfterDigits(string formatted, int digitCount)
        {
            return CaretIndexAfterSignificant(formatted, digitCount, digitPredicate);
        }

        /// <summary>형식화된 텍스트에서 n번째 유효 문자 바로 뒤의 캐럿 위치를 구한다.</summary>
        internal static int CaretIndexAfterSignificant(string formatted, int significantCount, Func<char, bool> isSignificant)
        {
            if (significantCount <= 0)
            {
                return 0;
            }

            int seen = 0;

            for (int i = 0; i < formatted.Length; i++)
            {
                if (isSignificant(formatted[i]))
                {
                    seen++;

                    if (seen == significantCount)
                    {
                        return i + 1;
                    }
                }
            }

            return formatted.Length;
        }

        /// <summary>정수부 숫자에 뒤에서부터 3자리마다 콤마를 넣는다.</summary>
        internal static string GroupDigits(string digits)
        {
            if (digits.Length <= 3)
            {
                return digits;
            }

            StringBuilder builder = new StringBuilder();
            int leading = digits.Length % 3;

            if (leading > 0)
            {
                builder.Append(digits, 0, leading);
            }

            for (int i = leading; i < digits.Length; i += 3)
            {
                if (builder.Length > 0)
                {
                    builder.Append(',');
                }

                builder.Append(digits, i, 3);
            }

            return builder.ToString();
        }

        /// <summary>
        /// 숫자 나열을 진행형 마스크로 형식화한다 — 그룹 경계까지 숫자가 채워졌을 때만
        /// 구분자를 끼워 넣는다. 예: 그룹 {4,2,2} + '-' 이면
        /// "2015" → "2015", "201507" → "2015-07", "20150713" → "2015-07-13".
        /// 마지막 그룹은 남은 숫자를 전부 흡수한다 (원본 Substring 동작과 동일).
        /// </summary>
        internal static string FormatGroups(string digits, char separator, int[] groupLengths)
        {
            StringBuilder builder = new StringBuilder(digits.Length + groupLengths.Length);
            int position = 0;

            for (int group = 0; group < groupLengths.Length && position < digits.Length; group++)
            {
                bool isLastGroup = group == groupLengths.Length - 1;
                int remaining = digits.Length - position;
                int take = isLastGroup ? remaining : Math.Min(groupLengths[group], remaining);

                if (position > 0)
                {
                    builder.Append(separator);
                }

                builder.Append(digits, position, take);
                position += take;
            }

            return builder.ToString();
        }
    }
}
