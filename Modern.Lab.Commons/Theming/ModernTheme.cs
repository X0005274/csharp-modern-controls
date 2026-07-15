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
    /// 팔레트 구성:
    ///   - Light: 연한 블루 그레이 뉴트럴(#F3F4F6 계열) + 액센트 #0078D4 램프
    ///     (2026-07-14 Win11 순정화 이전의 소프트 팔레트로 복원).
    ///   - Dark: #202020/#2B2B2B, 액센트 #4CC2FF(+검정 OnAccent — WPF 쪽 토큰).
    ///   - OrangeBlue: 웜 오렌지(#CA5010) 액센트 + 웜 파스텔 뉴트럴,
    ///     선택 강조는 블루 파스텔 (GreenTomato와 같은 구조).
    ///   - GreenTomato: 딥 그린(#217346) 액센트 + 민트 파스텔, 선택 강조는 토마토.
    ///   - CrimsonGray: 미드 그레이 모노톤(#555555) + 라이트 크림슨(#F2919E) 액센트.
    ///   - Blue: Fluent 블루(#0F6CBD) 액센트 + 하늘색 파스텔 뉴트럴.
    ///   - LightPurple: Fluent 퍼플(#5C2E91) 액센트 + 라벤더 파스텔 뉴트럴.
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
            /// <summary>기본값 — 연한 라이트 Fluent (액센트 #0078D4).</summary>
            Light,

            /// <summary>Win11 다크 (#202020, 액센트 #4CC2FF).</summary>
            Dark,

            /// <summary>웜 오렌지 액센트 + 웜 파스텔, 블루 선택 강조 테마 (라이트 기반).</summary>
            OrangeBlue,

            /// <summary>딥 그린 액센트 + 민트 파스텔, 토마토 선택 강조 테마 (라이트 기반).</summary>
            GreenTomato,

            /// <summary>미드 그레이 모노톤 + 라이트 크림슨 액센트 테마 (어두운 계열).</summary>
            CrimsonGray,

            /// <summary>Fluent 블루(#0F6CBD) 액센트 + 하늘색 파스텔 테마 (라이트 기반).</summary>
            Blue,

            /// <summary>Fluent 퍼플(#5C2E91) 액센트 + 라벤더 파스텔 테마 (라이트 기반).</summary>
            LightPurple
        }

        /// <summary>현재 테마. 앱 시작 시 한 번 설정한다.</summary>
        public static ThemeMode Mode { get; set; } = ThemeMode.Light;

        // ---- 장평 (글자 가로 비율) ----

        /// <summary>장평 허용 하한.</summary>
        public const double MinFontWidthRatio = 0.8;

        /// <summary>장평 허용 상한.</summary>
        public const double MaxFontWidthRatio = 1.2;

        private static double fontWidthRatio = 1.0;

        /// <summary>
        /// 전역 장평(글자 가로 비율). 1.0 = 원래 폭, 0.9 = 축소, 1.1 = 확대.
        /// 허용 범위(0.8~1.2) 밖의 값은 경계로 잘린다.
        ///
        /// <see cref="Mode"/>와 마찬가지로 <b>앱 시작 시 첫 컨트롤 생성 전에 한 번</b>
        /// 설정한다 — WPF 쪽(그리드 셀 등)은 컬럼/템플릿 구성 시점에 값이 굳는다.
        /// 기본 1.0이므로 설정하지 않으면 기존 시스템은 영향받지 않는다.
        /// 컨트롤별 FontWidthRatio 속성(0 = 전역 사용)으로 개별 재정의할 수 있다.
        /// </summary>
        public static double FontWidthRatio
        {
            get { return fontWidthRatio; }
            set { fontWidthRatio = ClampFontWidthRatio(value); }
        }

        /// <summary>
        /// 컨트롤별 재정의 값을 유효 장평으로 해석한다 —
        /// 0 이하(기본)는 전역 <see cref="FontWidthRatio"/>, 양수는 클램프해 사용.
        /// </summary>
        public static double ResolveFontWidthRatio(double localOverride)
        {
            return localOverride > 0d ? ClampFontWidthRatio(localOverride) : fontWidthRatio;
        }

        /// <summary>장평 값을 허용 범위(0.8~1.2)로 자른다.</summary>
        public static double ClampFontWidthRatio(double value)
        {
            if (value < MinFontWidthRatio)
            {
                return MinFontWidthRatio;
            }

            if (value > MaxFontWidthRatio)
            {
                return MaxFontWidthRatio;
            }

            return value;
        }

        /// <summary>다크 테마 여부 (Dark 전용 플래그).</summary>
        public static bool IsDark
        {
            get { return Mode == ThemeMode.Dark; }
        }

        /// <summary>
        /// 어두운 표면 계열 테마 여부 (Dark, CrimsonGray) — 다크 타이틀바, 밝은 텍스트 등
        /// "어두운 화면" 공통 처리가 걸리는 기준.
        /// </summary>
        public static bool IsDarkBased
        {
            get { return Mode == ThemeMode.Dark || Mode == ThemeMode.CrimsonGray; }
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
                    case ThemeMode.CrimsonGray: return Rgb(97, 97, 97);
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
                    case ThemeMode.CrimsonGray: return Rgb(85, 85, 85);
                    case ThemeMode.OrangeBlue: return Rgb(250, 234, 220);
                    case ThemeMode.GreenTomato: return Rgb(232, 244, 235);
                    case ThemeMode.Blue: return Rgb(230, 240, 250);
                    case ThemeMode.LightPurple: return Rgb(239, 232, 248);
                    default: return Rgb(243, 244, 246);
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
                    case ThemeMode.CrimsonGray: return Rgb(122, 122, 122);
                    case ThemeMode.OrangeBlue: return Rgb(239, 208, 178);
                    case ThemeMode.GreenTomato: return Rgb(191, 223, 203);
                    case ThemeMode.Blue: return Rgb(188, 213, 238);
                    case ThemeMode.LightPurple: return Rgb(211, 194, 236);
                    default: return Rgb(209, 213, 219);
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
                    case ThemeMode.CrimsonGray: return Rgb(112, 112, 112);
                    case ThemeMode.OrangeBlue: return Rgb(246, 226, 205);
                    case ThemeMode.GreenTomato: return Rgb(216, 236, 223);
                    case ThemeMode.Blue: return Rgb(216, 231, 245);
                    case ThemeMode.LightPurple: return Rgb(226, 214, 243);
                    default: return Rgb(229, 231, 235);
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
                    case ThemeMode.CrimsonGray: return Rgb(245, 245, 245);
                    case ThemeMode.OrangeBlue:
                    case ThemeMode.GreenTomato:
                    case ThemeMode.Blue:
                    case ThemeMode.LightPurple: return Rgb(27, 27, 27);
                    default: return Rgb(17, 24, 39);
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
                    case ThemeMode.CrimsonGray: return Rgb(204, 204, 204);
                    case ThemeMode.OrangeBlue:
                    case ThemeMode.GreenTomato:
                    case ThemeMode.Blue:
                    case ThemeMode.LightPurple: return Rgb(93, 93, 93);
                    default: return Rgb(107, 114, 128);
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
                    case ThemeMode.CrimsonGray: return Rgb(156, 156, 156);
                    case ThemeMode.OrangeBlue:
                    case ThemeMode.GreenTomato:
                    case ThemeMode.Blue:
                    case ThemeMode.LightPurple: return Rgb(157, 157, 157);
                    default: return Rgb(156, 163, 175);
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
                    case ThemeMode.CrimsonGray: return Rgb(242, 145, 158);
                    case ThemeMode.OrangeBlue: return Rgb(202, 80, 16);
                    case ThemeMode.GreenTomato: return Rgb(33, 115, 70);
                    case ThemeMode.Blue: return Rgb(15, 108, 189);
                    case ThemeMode.LightPurple: return Rgb(92, 46, 145);
                    default: return Rgb(0, 120, 212);
                }
            }
        }

        /// <summary>필수 표시/위험 빨강 (Brush.ErrorBorder)</summary>
        public static Color RequiredRed
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark:
                    case ThemeMode.CrimsonGray: return Rgb(255, 138, 148);
                    case ThemeMode.OrangeBlue:
                    case ThemeMode.GreenTomato:
                    case ThemeMode.Blue:
                    case ThemeMode.LightPurple: return Rgb(196, 43, 28);
                    default: return Rgb(220, 38, 38);
                }
            }
        }

        /// <summary>중립 배지 배경 (Brush.NeutralBackground)</summary>
        public static Color NeutralBackground
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(56, 56, 56);
                    case ThemeMode.CrimsonGray: return Rgb(112, 112, 112);
                    case ThemeMode.OrangeBlue:
                    case ThemeMode.GreenTomato:
                    case ThemeMode.Blue:
                    case ThemeMode.LightPurple: return Rgb(240, 240, 240);
                    default: return Rgb(243, 244, 246);
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
                    case ThemeMode.CrimsonGray: return Rgb(228, 226, 224);
                    case ThemeMode.OrangeBlue:
                    case ThemeMode.GreenTomato:
                    case ThemeMode.Blue:
                    case ThemeMode.LightPurple: return Rgb(59, 59, 59);
                    default: return Rgb(55, 65, 81);
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
                    case ThemeMode.CrimsonGray: return Rgb(133, 90, 98);
                    case ThemeMode.OrangeBlue: return Rgb(199, 224, 247);
                    case ThemeMode.GreenTomato: return Rgb(248, 216, 210);
                    case ThemeMode.Blue: return Rgb(191, 220, 245);
                    case ThemeMode.LightPurple: return Rgb(220, 204, 240);
                    default: return Rgb(182, 217, 242);
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
                    case ThemeMode.CrimsonGray: return Rgb(104, 104, 104);
                    case ThemeMode.OrangeBlue: return Rgb(253, 245, 236);
                    case ThemeMode.GreenTomato: return Rgb(243, 250, 245);
                    case ThemeMode.Blue: return Rgb(243, 248, 253);
                    case ThemeMode.LightPurple: return Rgb(246, 241, 251);
                    default: return Rgb(249, 250, 251);
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
                    case ThemeMode.CrimsonGray: return Rgb(110, 110, 110);
                    case ThemeMode.OrangeBlue: return Rgb(244, 220, 196);
                    case ThemeMode.GreenTomato: return Rgb(213, 235, 221);
                    case ThemeMode.Blue: return Rgb(211, 229, 246);
                    case ThemeMode.LightPurple: return Rgb(227, 216, 244);
                    default: return Rgb(239, 243, 250);
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
                    case ThemeMode.CrimsonGray: return Rgb(102, 102, 102);
                    case ThemeMode.OrangeBlue: return Rgb(252, 246, 238);
                    case ThemeMode.GreenTomato: return Rgb(242, 249, 244);
                    case ThemeMode.Blue: return Rgb(242, 247, 252);
                    case ThemeMode.LightPurple: return Rgb(248, 245, 252);
                    default: return Rgb(246, 247, 249);
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
                    case ThemeMode.CrimsonGray: return Rgb(255, 237, 239);
                    case ThemeMode.OrangeBlue: return Rgb(0, 61, 117);
                    case ThemeMode.GreenTomato: return Rgb(140, 43, 30);
                    case ThemeMode.Blue: return Rgb(10, 66, 117);
                    case ThemeMode.LightPurple: return Rgb(70, 34, 110);
                    default: return Rgb(0, 90, 158);
                }
            }
        }

        private static Color Rgb(int r, int g, int b)
        {
            return Color.FromArgb(255, r, g, b);
        }
    }
}
