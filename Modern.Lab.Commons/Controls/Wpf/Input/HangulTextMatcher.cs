using System;

namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>
    /// Korean-aware text matching for autocomplete (search-box behavior).
    ///
    /// Pattern characters are interpreted as follows:
    /// - Consonant jamo (ㄱ~ㅎ): matches any syllable whose initial consonant
    ///   (초성) is that jamo — enables 초성 검색 ("ㄱㅁㅅ" → "김민수").
    /// - Complete syllable: exact match; the LAST pattern character may also
    ///   match on initial+medial only when it has no final consonant, so IME
    ///   intermediate states keep matching while composing ("기" → "김민수").
    /// - Everything else: ordinal, case-insensitive comparison.
    /// Matching uses contains semantics over the candidate.
    /// </summary>
    internal static class HangulTextMatcher
    {
        private const char SyllableFirst = '가';   // U+AC00
        private const char SyllableLast = '힣';    // U+D7A3
        private const int MedialCount = 21;
        private const int FinalCount = 28;

        // Initial consonants (초성) in syllable order, as compatibility jamo.
        private const string InitialJamoTable = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";

        /// <summary>Returns true when the pattern occurs anywhere in the candidate.</summary>
        internal static bool Contains(string candidate, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return true;
            }

            if (string.IsNullOrEmpty(candidate) || candidate.Length < pattern.Length)
            {
                return false;
            }

            for (int start = 0; start <= candidate.Length - pattern.Length; start++)
            {
                if (MatchesAt(candidate, start, pattern))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool MatchesAt(string candidate, int start, string pattern)
        {
            for (int index = 0; index < pattern.Length; index++)
            {
                bool isLastPatternChar = index == pattern.Length - 1;

                if (!MatchChar(candidate[start + index], pattern[index], isLastPatternChar))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool MatchChar(char candidateChar, char patternChar, bool isLastPatternChar)
        {
            if (candidateChar == patternChar)
            {
                return true;
            }

            int initialIndex = InitialJamoTable.IndexOf(patternChar);

            if (initialIndex >= 0)
            {
                // Consonant-jamo pattern: compare against the syllable's 초성.
                return IsSyllable(candidateChar) &&
                       (candidateChar - SyllableFirst) / (MedialCount * FinalCount) == initialIndex;
            }

            if (IsSyllable(patternChar))
            {
                // A final-less syllable at the end of the pattern is an IME
                // intermediate state; match on initial+medial only.
                if (isLastPatternChar && IsSyllable(candidateChar))
                {
                    int patternOffset = patternChar - SyllableFirst;
                    int candidateOffset = candidateChar - SyllableFirst;
                    bool patternHasNoFinal = patternOffset % FinalCount == 0;

                    return patternHasNoFinal &&
                           patternOffset / FinalCount == candidateOffset / FinalCount;
                }

                return false;
            }

            return char.ToUpperInvariant(candidateChar) == char.ToUpperInvariant(patternChar);
        }

        private static bool IsSyllable(char value)
        {
            return value >= SyllableFirst && value <= SyllableLast;
        }
    }
}
