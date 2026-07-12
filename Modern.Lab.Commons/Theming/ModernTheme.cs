using System.Drawing;

namespace Modern.Lab.Theming
{
    /// <summary>
    /// 라이트/다크 테마 선택 플래그 + GDI+ 컨트롤용 중앙 팔레트.
    ///
    /// 두 파이프라인을 하나의 플래그로 묶는다:
    ///   - WPF(ElementHost) 컨트롤: XAML의 StaticResource가 <c>Themes/Tokens.xaml</c>를
    ///     쓰는데, <c>SharedResourceDictionary</c>가 <see cref="IsDark"/>일 때
    ///     <c>Tokens.Dark.xaml</c> 오버라이드를 뒤에 병합해 다크 값을 집게 한다.
    ///   - 순수 GDI+ 컨트automation(ModernLabel/StatusBadge/CardPanel/GroupBox): XAML을 읽을 수
    ///     없으므로 여기 팔레트 색을 읽는다.
    ///
    /// <see cref="Mode"/>는 <b>앱 시작 시 첫 컨트롤 생성 전에 한 번</b> 설정한다
    /// (런타임 토글은 지원하지 않는다 — WPF StaticResource가 로드 시 확정되기 때문).
    /// 설정하지 않으면 라이트라서, 이 라이브러리를 쓰는 다른 시스템은 영향받지 않는다.
    /// </summary>
    public static class ModernTheme
    {
        /// <summary>테마 종류.</summary>
        public enum ThemeMode
        {
            /// <summary>기본값 — 기존 라이트 Fluent 테마.</summary>
            Light,

            /// <summary>어두운 배경 테마.</summary>
            Dark
        }

        /// <summary>현재 테마. 앱 시작 시 한 번 설정한다.</summary>
        public static ThemeMode Mode { get; set; } = ThemeMode.Light;

        /// <summary>다크 테마 여부.</summary>
        public static bool IsDark
        {
            get { return Mode == ThemeMode.Dark; }
        }

        // ---- GDI+ 팔레트 (Themes/Tokens.xaml의 색을 미러링; 다크는 대응 다크 값) ----

        /// <summary>카드/입력 표면 (Brush.Surface)</summary>
        public static Color Surface
        {
            get { return IsDark ? Rgb(42, 42, 43) : Rgb(255, 255, 255); }
        }

        /// <summary>폼/페이지 바탕 (Brush.Background)</summary>
        public static Color Background
        {
            get { return IsDark ? Rgb(30, 30, 30) : Rgb(243, 244, 246); }
        }

        /// <summary>기본 테두리 (Brush.Border)</summary>
        public static Color Border
        {
            get { return IsDark ? Rgb(63, 63, 70) : Rgb(209, 213, 219); }
        }

        /// <summary>옅은 테두리/구분선 (Brush.BorderSubtle)</summary>
        public static Color BorderSubtle
        {
            get { return IsDark ? Rgb(58, 58, 60) : Rgb(229, 231, 235); }
        }

        /// <summary>본문/제목 텍스트 (Brush.TextPrimary)</summary>
        public static Color TextPrimary
        {
            get { return IsDark ? Rgb(230, 230, 230) : Rgb(17, 24, 39); }
        }

        /// <summary>보조 텍스트 (Brush.TextSecondary)</summary>
        public static Color TextSecondary
        {
            get { return IsDark ? Rgb(156, 163, 175) : Rgb(107, 114, 128); }
        }

        /// <summary>비활성 텍스트 (Brush.DisabledText)</summary>
        public static Color DisabledText
        {
            get { return IsDark ? Rgb(107, 114, 128) : Rgb(156, 163, 175); }
        }

        /// <summary>액센트 (Brush.Accent) — 다크에서는 가독성을 위해 밝게.</summary>
        public static Color Accent
        {
            get { return IsDark ? Rgb(59, 158, 255) : Rgb(0, 120, 212); }
        }

        /// <summary>필수 표시/위험 빨강 (Brush.ErrorBorder)</summary>
        public static Color RequiredRed
        {
            get { return IsDark ? Rgb(240, 113, 113) : Rgb(220, 38, 38); }
        }

        /// <summary>중립 배지 배경 (Brush.NeutralBackground)</summary>
        public static Color NeutralBackground
        {
            get { return IsDark ? Rgb(58, 58, 60) : Rgb(243, 244, 246); }
        }

        /// <summary>중립 배지 텍스트 (Brush.NeutralText)</summary>
        public static Color NeutralText
        {
            get { return IsDark ? Rgb(209, 213, 219) : Rgb(55, 65, 81); }
        }

        private static Color Rgb(int r, int g, int b)
        {
            return Color.FromArgb(255, r, g, b);
        }
    }
}
