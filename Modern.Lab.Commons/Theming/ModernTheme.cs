using System.Drawing;

namespace Modern.Lab.Theming
{
    /// <summary>
    /// 테마 선택 플래그 + GDI+ 컨트롤용 중앙 팔레트.
    ///
    /// 두 파이프라인을 하나의 플래그로 묶는다:
    ///   - WPF(ElementHost) 컨트롤: XAML의 StaticResource가 <c>Themes/Tokens.xaml</c>를
    ///     쓰는데, Light가 아니면 <c>SharedResourceDictionary</c>가
    ///     <c>Tokens.&lt;테마&gt;.xaml</c> 오버라이드를 뒤에 병합해 테마 값을 집게 한다.
    ///   - 순수 GDI+ 컨트롤(ModernLabel/StatusBadge/CardPanel/GroupBox/SplitContainer):
    ///     XAML을 읽을 수 없으므로 여기 팔레트 색을 읽는다.
    ///
    /// 팔레트는 Windows 11(WinUI 3) 순정 Fluent 값을 기준으로 한다:
    ///   - Light: 창 배경 #F3F3F3(Mica 근사), 액센트 #005FB8 램프, 무채 뉴트럴.
    ///   - Dark: #202020/#2B2B2B, 액센트 #4CC2FF(+검정 OnAccent — WPF 쪽 토큰).
    ///   - Gray: Fluent 뉴트럴 램프의 연한 미드 그레이 모노톤 (#454545).
    ///   - Purple/Orange/Tomato: Fluent 공식 액센트(#5C2E91/#CA5010/#D13438) 램프의
    ///     밝은 파스텔 (흰 카드 + 틴트 배경/테두리).
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
            /// <summary>기본값 — Win11 라이트 Fluent (액센트 #005FB8).</summary>
            Light,

            /// <summary>Win11 다크 (#202020, 액센트 #4CC2FF).</summary>
            Dark,

            /// <summary>Fluent 뉴트럴 램프의 연한 미드 그레이 모노톤 테마.</summary>
            Gray,

            /// <summary>Fluent 퍼플(#5C2E91) 파스텔 테마 (라이트 기반).</summary>
            Purple,

            /// <summary>Fluent 오렌지(#CA5010) 파스텔 테마 (라이트 기반).</summary>
            Orange,

            /// <summary>Fluent 레드(#D13438) 파스텔 테마 (라이트 기반).</summary>
            Tomato
        }

        /// <summary>현재 테마. 앱 시작 시 한 번 설정한다.</summary>
        public static ThemeMode Mode { get; set; } = ThemeMode.Light;

        /// <summary>다크 테마 여부 (Dark 전용 플래그).</summary>
        public static bool IsDark
        {
            get { return Mode == ThemeMode.Dark; }
        }

        /// <summary>
        /// 어두운 표면 계열 테마 여부 (Dark, Gray) — 다크 타이틀바, 밝은 텍스트 등
        /// "어두운 화면" 공통 처리가 걸리는 기준.
        /// </summary>
        public static bool IsDarkBased
        {
            get { return Mode == ThemeMode.Dark || Mode == ThemeMode.Gray; }
        }

        // ---- GDI+ 팔레트 (Themes/Tokens.xaml의 색을 미러링; 테마별 대응 값) ----

        /// <summary>카드/입력 표면 (Brush.Surface)</summary>
        public static Color Surface
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(43, 43, 43);
                    case ThemeMode.Gray: return Rgb(97, 97, 97);
                    default: return Rgb(255, 255, 255);
                }
            }
        }

        /// <summary>폼/페이지 바탕 (Brush.Background) — 테마 톤이 가장 크게 갈리는 곳.</summary>
        public static Color Background
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(32, 32, 32);
                    case ThemeMode.Gray: return Rgb(85, 85, 85);
                    case ThemeMode.Purple: return Rgb(239, 232, 248);
                    case ThemeMode.Orange: return Rgb(250, 234, 220);
                    case ThemeMode.Tomato: return Rgb(250, 231, 232);
                    default: return Rgb(243, 243, 243);
                }
            }
        }

        /// <summary>기본 테두리 (Brush.Border)</summary>
        public static Color Border
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(65, 65, 65);
                    case ThemeMode.Gray: return Rgb(122, 122, 122);
                    case ThemeMode.Purple: return Rgb(211, 194, 236);
                    case ThemeMode.Orange: return Rgb(239, 208, 178);
                    case ThemeMode.Tomato: return Rgb(240, 198, 200);
                    default: return Rgb(217, 217, 217);
                }
            }
        }

        /// <summary>옅은 테두리/구분선 (Brush.BorderSubtle)</summary>
        public static Color BorderSubtle
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(56, 56, 56);
                    case ThemeMode.Gray: return Rgb(112, 112, 112);
                    case ThemeMode.Purple: return Rgb(226, 214, 243);
                    case ThemeMode.Orange: return Rgb(246, 226, 205);
                    case ThemeMode.Tomato: return Rgb(247, 222, 223);
                    default: return Rgb(232, 232, 232);
                }
            }
        }

        /// <summary>본문/제목 텍스트 (Brush.TextPrimary)</summary>
        public static Color TextPrimary
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(240, 240, 240);
                    case ThemeMode.Gray: return Rgb(245, 245, 245);
                    default: return Rgb(27, 27, 27);
                }
            }
        }

        /// <summary>보조 텍스트 (Brush.TextSecondary)</summary>
        public static Color TextSecondary
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(160, 160, 160);
                    case ThemeMode.Gray: return Rgb(204, 204, 204);
                    default: return Rgb(93, 93, 93);
                }
            }
        }

        /// <summary>비활성 텍스트 (Brush.DisabledText)</summary>
        public static Color DisabledText
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(110, 110, 110);
                    case ThemeMode.Gray: return Rgb(156, 156, 156);
                    default: return Rgb(157, 157, 157);
                }
            }
        }

        /// <summary>액센트 (Brush.Accent) — 테마의 주 색. 어두운 계열에서는 밝게.</summary>
        public static Color Accent
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(76, 194, 255);
                    case ThemeMode.Gray: return Rgb(224, 222, 220);
                    case ThemeMode.Purple: return Rgb(92, 46, 145);
                    case ThemeMode.Orange: return Rgb(202, 80, 16);
                    case ThemeMode.Tomato: return Rgb(209, 52, 56);
                    default: return Rgb(0, 95, 184);
                }
            }
        }

        /// <summary>필수 표시/위험 빨강 (Brush.ErrorBorder)</summary>
        public static Color RequiredRed
        {
            get { return IsDarkBased ? Rgb(255, 138, 148) : Rgb(196, 43, 28); }
        }

        /// <summary>중립 배지 배경 (Brush.NeutralBackground)</summary>
        public static Color NeutralBackground
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(56, 56, 56);
                    case ThemeMode.Gray: return Rgb(112, 112, 112);
                    default: return Rgb(240, 240, 240);
                }
            }
        }

        /// <summary>중립 배지 텍스트 (Brush.NeutralText)</summary>
        public static Color NeutralText
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(208, 208, 208);
                    case ThemeMode.Gray: return Rgb(228, 226, 224);
                    default: return Rgb(59, 59, 59);
                }
            }
        }

        /// <summary>선택 강조 배경 (Brush.SelectedBackground)</summary>
        public static Color SelectionBackground
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(30, 73, 100);
                    case ThemeMode.Gray: return Rgb(124, 124, 124);
                    case ThemeMode.Purple: return Rgb(220, 204, 240);
                    case ThemeMode.Orange: return Rgb(246, 217, 188);
                    case ThemeMode.Tomato: return Rgb(246, 208, 210);
                    default: return Rgb(199, 224, 247);
                }
            }
        }

        /// <summary>보조 표면 — 카드 위 살짝 다른 톤 (Brush.SurfaceAlt / HeaderBackground)</summary>
        public static Color SurfaceAlt
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(51, 51, 51);
                    case ThemeMode.Gray: return Rgb(104, 104, 104);
                    case ThemeMode.Purple: return Rgb(246, 241, 251);
                    case ThemeMode.Orange: return Rgb(253, 245, 236);
                    case ThemeMode.Tomato: return Rgb(253, 244, 244);
                    default: return Rgb(250, 250, 250);
                }
            }
        }

        /// <summary>그리드 열 헤더 배경 (Brush.GridHeaderBackground)</summary>
        public static Color GridHeaderBackground
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(54, 54, 54);
                    case ThemeMode.Gray: return Rgb(110, 110, 110);
                    case ThemeMode.Purple: return Rgb(227, 216, 244);
                    case ThemeMode.Orange: return Rgb(244, 220, 196);
                    case ThemeMode.Tomato: return Rgb(246, 216, 218);
                    default: return Rgb(240, 240, 240);
                }
            }
        }

        /// <summary>그리드 교차 행 배경 (Brush.GridRowAlt)</summary>
        public static Color GridRowAlt
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(48, 48, 48);
                    case ThemeMode.Gray: return Rgb(102, 102, 102);
                    case ThemeMode.Purple: return Rgb(248, 245, 252);
                    case ThemeMode.Orange: return Rgb(252, 246, 238);
                    case ThemeMode.Tomato: return Rgb(252, 243, 243);
                    default: return Rgb(246, 246, 246);
                }
            }
        }

        /// <summary>선택 강조 텍스트 (Brush.SelectedText) — SelectionBackground 위 글자색.</summary>
        public static Color SelectedText
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(183, 227, 255);
                    case ThemeMode.Gray: return Rgb(250, 250, 250);
                    case ThemeMode.Purple: return Rgb(70, 34, 110);
                    case ThemeMode.Orange: return Rgb(122, 48, 5);
                    case ThemeMode.Tomato: return Rgb(124, 19, 22);
                    default: return Rgb(0, 61, 117);
                }
            }
        }

        private static Color Rgb(int r, int g, int b)
        {
            return Color.FromArgb(255, r, g, b);
        }
    }
}
