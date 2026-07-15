using System;
using System.Collections.Generic;
using System.Windows;

using Modern.Lab.Theming;

namespace Modern.Lab.Controls.Wpf.Common
{
    /// <summary>
    /// Source가 같은 ResourceDictionary를 프로세스 전체에서 한 번만 로드해
    /// 공유하는 병합 사전.
    ///
    /// WPF는 MergedDictionaries의 Source 사전을 캐시하지 않는다 — 컨트롤
    /// 인스턴스마다 Tokens.xaml(BAML)을 다시 파싱해 브러시 수십 개를 새로
    /// 만든다. 라벨/배지처럼 폼에 수십 개 놓이는 컨트롤에서는 이 비용이
    /// 폼 로드 시간과 메모리를 그대로 갉아먹으므로, 각 컨트롤 XAML은
    /// 일반 ResourceDictionary 대신 이 클래스를 병합한다.
    /// </summary>
    public class SharedResourceDictionary : ResourceDictionary
    {
        // URI → 로드된 사전. 토큰 사전은 읽기 전용으로만 쓰이므로 공유해도 안전하다.
        private static readonly Dictionary<Uri, ResourceDictionary> cache =
            new Dictionary<Uri, ResourceDictionary>();

        /// <summary>
        /// 현재 테마의 오버라이드 사전 URI. Light(기본)는 오버라이드가 없으므로 null.
        /// 오버라이드 사전은 Tokens.xaml 뒤에 병합되어 같은 키를 테마 값으로 덮는다.
        /// </summary>
        private static Uri OverrideUriFor(ModernTheme.ThemeMode mode)
        {
            string name;
            switch (mode)
            {
                case ModernTheme.ThemeMode.Dark: name = "Tokens.Dark.xaml"; break;
                case ModernTheme.ThemeMode.OrangeBlue: name = "Tokens.OrangeBlue.xaml"; break;
                case ModernTheme.ThemeMode.GreenTomato: name = "Tokens.GreenTomato.xaml"; break;
                case ModernTheme.ThemeMode.CrimsonGray: name = "Tokens.CrimsonGray.xaml"; break;
                case ModernTheme.ThemeMode.Blue: name = "Tokens.Blue.xaml"; break;
                case ModernTheme.ThemeMode.LightPurple: name = "Tokens.LightPurple.xaml"; break;
                default: return null;
            }
            return new Uri("/Modern.Lab.Commons;component/Themes/" + name, UriKind.Relative);
        }

        private Uri sourceUri;

        /// <summary>
        /// 로드할 사전의 pack URI. 같은 URI는 프로세스에서 한 번만 로드되고
        /// 이후에는 캐시된 인스턴스가 병합된다.
        /// </summary>
        public new Uri Source
        {
            get
            {
                return this.sourceUri;
            }
            set
            {
                this.sourceUri = value;

                if (value == null)
                {
                    return;
                }

                this.MergedDictionaries.Add(Load(value));

                // Light가 아닌 테마면 메인 토큰 사전 뒤에 테마 오버라이드를 병합한다 —
                // 병합 사전은 나중에 추가된 쪽이 이기므로 StaticResource가 테마 값을 집는다.
                // 라이트(기본)에서는 아무것도 하지 않으므로 다른 시스템은 영향이 없다.
                Uri overrideUri = OverrideUriFor(ModernTheme.Mode);
                if (overrideUri != null
                        && value.OriginalString.EndsWith("Tokens.xaml", StringComparison.OrdinalIgnoreCase))
                {
                    this.MergedDictionaries.Add(Load(overrideUri));
                }
            }
        }

        /// <summary>URI별로 한 번만 로드해 캐시된 사전을 돌려준다.</summary>
        private static ResourceDictionary Load(Uri uri)
        {
            lock (cache)
            {
                ResourceDictionary shared;
                if (!cache.TryGetValue(uri, out shared))
                {
                    shared = new ResourceDictionary();
                    shared.Source = uri;
                    cache.Add(uri, shared);
                }
                return shared;
            }
        }
    }
}
