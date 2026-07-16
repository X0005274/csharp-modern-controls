using System;
using System.Collections.Generic;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// 그리드 컬럼 캡션 용어사전 — "필드 이름(DB 컬럼) → 표준 캡션"의 등록형 그릇.
    ///
    /// 라이브러리는 빈 사전(메커니즘)만 제공하고 내용(도메인 용어)은 앱이 채운다:
    /// 앱 시작 시(첫 폼 생성 전) Register/RegisterRange로 사내 표준 용어집을
    /// 부어 넣으면, 이후 캡션 인자가 없는 컬럼 정의(new ModernDataGridColumn("ITEM_ID"))가
    /// 여기 표준 캡션을 자동으로 받는다. 화면 문맥상 다른 표현이 필요하면
    /// headerText를 받는 생성자로 명시해 재정의한다 — 명시가 항상 사전을 이긴다.
    ///
    /// 별도 해석 로직이 필요하면 ModernDataGridColumn.CaptionResolver에 직접
    /// 제공자를 등록한다 — 그 경우 이 사전 대신 그 제공자가 쓰인다.
    /// </summary>
    public static class GridCaptionCatalog
    {
        // 필드 이름은 대소문자 무시로 찾는다 (DB 컬럼 표기 편차 허용).
        // 등록은 앱 시작 시 1회가 원칙이지만, 안전을 위해 조회/등록을 잠근다.
        private static readonly Dictionary<string, string> captions =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static readonly object syncRoot = new object();

        /// <summary>
        /// 표준 캡션 한 건을 등록한다. 같은 필드 이름을 다시 등록하면 덮어쓴다.
        /// 캡션이 null/빈 값이면 항목을 제거한다(필드 이름 그대로 표시로 복귀).
        /// </summary>
        public static void Register(string dataPropertyName, string caption)
        {
            if (string.IsNullOrEmpty(dataPropertyName))
            {
                return;
            }

            lock (syncRoot)
            {
                if (string.IsNullOrEmpty(caption))
                {
                    captions.Remove(dataPropertyName);
                }
                else
                {
                    captions[dataPropertyName] = caption;
                }
            }
        }

        /// <summary>표준 용어집을 일괄 등록한다. 기존 항목과 겹치면 덮어쓴다.</summary>
        public static void RegisterRange(IEnumerable<KeyValuePair<string, string>> entries)
        {
            if (entries == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> entry in entries)
            {
                Register(entry.Key, entry.Value);
            }
        }

        /// <summary>등록된 항목을 모두 지운다 (테마 전환 테스트 등 재구성용).</summary>
        public static void Clear()
        {
            lock (syncRoot)
            {
                captions.Clear();
            }
        }

        /// <summary>표준 캡션을 돌려준다. 사전에 없으면 null (호출측이 필드 이름 사용).</summary>
        public static string Resolve(string dataPropertyName)
        {
            if (string.IsNullOrEmpty(dataPropertyName))
            {
                return null;
            }

            lock (syncRoot)
            {
                string caption;
                return captions.TryGetValue(dataPropertyName, out caption) ? caption : null;
            }
        }
    }
}
