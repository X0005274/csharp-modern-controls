using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Display;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// System.Windows.Forms.Label의 드롭인 대체 컨트롤.
    ///
    /// 원래 WPF ModernLabelControl을 ElementHost로 호스팅했지만, 라벨은 한 폼에
    /// 수십 개씩 놓이는 컨트롤이라 개당 HWND + WPF 컴포지션 비용이 폼 리사이즈를
    /// 눈에 띄게 무겁게 만들었다(실측: Lot History 폼 리사이즈 1스텝 ~556ms).
    /// 그래서 ModernGroupBox/ModernCardPanel과 같은 방식으로 디자인 토큰을
    /// 미러링해 **GDI+로 직접 그리는 순수 WinForms 컨트롤**로 전환했다.
    /// 공개 API(Text/Kind/Required/TitleBar)와 시각 결과는 WPF 버전과 동일하다.
    ///
    /// 호환 멤버: Text(override, localizable), Enabled.
    /// Label과 달리 AutoSize가 없으므로 컨트롤에 명시적 크기를 지정한다
    /// (긴 텍스트는 말줄임표로 잘린다).
    /// </summary>
    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    public class ModernLabel : Control
    {
        // 색은 중앙 팔레트(ModernTheme)에서 읽는다 — 라이트/다크 테마에 따라 값이 바뀐다.
        // (GDI+는 XAML 리소스를 읽을 수 없으므로 Themes/Tokens.xaml 값을 팔레트로 미러링.)
        private static Color TextPrimaryColor { get { return Modern.Lab.Theming.ModernTheme.TextPrimary; } }
        private static Color TextSecondaryColor { get { return Modern.Lab.Theming.ModernTheme.TextSecondary; } }
        private static Color DisabledTextColor { get { return Modern.Lab.Theming.ModernTheme.DisabledText; } }
        private static Color AccentColor { get { return Modern.Lab.Theming.ModernTheme.Accent; } }
        private static Color RequiredColor { get { return Modern.Lab.Theming.ModernTheme.RequiredRed; } }

        // 타입 램프 미러: Body/Label/Helper 12 DIU = 9pt, Title 16 DIU = 12pt.
        // 구조 요소(Title/Label)는 SemiBold, 본문/도움말은 Regular.
        private static readonly Font BodyFont = new Font("Segoe UI", 9f);
        private static readonly Font TitleFont = new Font("Segoe UI Semibold", 12f);
        private static readonly Font LabelFont = new Font("Segoe UI Semibold", 9f);
        private static readonly Font HelperFont = new Font("Segoe UI", 9f);

        // 타이틀 바(액센트 세로 막대) 치수: 폭 3 + 오른쪽 여백 8 (WPF 버전과 동일).
        private const int titleBarWidth = 3;
        private const int titleBarGap = 8;

        // 필수 별표 앞 여백 (WPF 버전 Margin 3,0,0,0과 동일).
        private const int requiredGap = 3;

        private LabelKind kind;
        private bool required;
        private bool titleBar;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernLabel()
        {
            this.SetStyle(
                ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.SupportsTransparentBackColor,
                true);

            this.Size = new Size(120, 24);
            this.BackColor = Color.Transparent;
            this.kind = LabelKind.Body;
            this.required = false;
            this.titleBar = false;
            this.Text = "레이블";
        }

        /// <summary>
        /// ElementHost 시절 .Designer.cs가 직렬화한 "Child = null" 대입과의
        /// 소스 호환용 무동작 속성. 새 코드에서는 사용하지 않는다.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Child
        {
            get { return null; }
            set { }
        }

        /// <summary>Kind가 Title일 때 텍스트 왼쪽에 액센트색 세로 타이틀 바를 표시한다.</summary>
        [Category("모던 컨트롤")]
        [Description("Kind가 Title일 때 왼쪽에 액센트색 세로 타이틀 바 표시")]
        [DefaultValue(false)]
        public bool TitleBar
        {
            get
            {
                return this.titleBar;
            }
            set
            {
                this.titleBar = value;
                this.Invalidate();
            }
        }

        /// <summary>텍스트 뒤에 빨간 별표를 표시한다(필수 필드 표시).</summary>
        [Category("모던 컨트롤")]
        [Description("필수 입력 표시(빨간 별표)를 텍스트 뒤에 붙일지 여부")]
        [DefaultValue(false)]
        public bool Required
        {
            get
            {
                return this.required;
            }
            set
            {
                this.required = value;
                this.Invalidate();
            }
        }

        /// <summary>표시할 텍스트.</summary>
        [Category("모던 컨트롤")]
        [Description("표시할 텍스트")]
        [Browsable(true)]
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue("레이블")]
        public override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        /// <summary>타이포그래피 역할 (Body/Title/Label/Helper).</summary>
        [Category("모던 컨트롤")]
        [Description("타이포그래피 역할(Body/Title/Label/Helper)")]
        [DefaultValue(LabelKind.Body)]
        public LabelKind Kind
        {
            get
            {
                return this.kind;
            }
            set
            {
                this.kind = value;
                this.Invalidate();
            }
        }

        /// <summary>텍스트가 바뀌면 다시 그린다.</summary>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            this.Invalidate();
        }

        /// <summary>Enabled 변경 시 색이 바뀌므로 다시 그린다.</summary>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            this.Invalidate();
        }

        /// <summary>토큰 타입 램프에 따라 텍스트(+타이틀 바/필수 별표)를 그린다.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Font font = this.GetKindFont();
            Color color = this.Enabled ? this.GetKindColor() : DisabledTextColor;
            int left = 0;

            // 세로 타이틀 바: Kind=Title이면서 TitleBar=true일 때만.
            if (this.titleBar && this.kind == LabelKind.Title)
            {
                int barHeight = font.Height - 2;
                int barTop = (this.Height - barHeight) / 2;

                using (SolidBrush accentBrush = new SolidBrush(AccentColor))
                {
                    e.Graphics.FillRectangle(accentBrush, 0, barTop, titleBarWidth, barHeight);
                }

                left = titleBarWidth + titleBarGap;
            }

            // 필수 별표가 있으면 그 폭만큼 텍스트 영역을 줄여 별표가 잘리지 않게 한다.
            Size starSize = Size.Empty;

            if (this.required)
            {
                starSize = TextRenderer.MeasureText(e.Graphics, "*", LabelFont, Size.Empty, textFlags);
            }

            Rectangle textBounds = new Rectangle(
                left, 0, this.Width - left - starSize.Width - (this.required ? requiredGap : 0), this.Height);

            TextRenderer.DrawText(
                e.Graphics, this.Text, font, textBounds, color, textFlags | TextFormatFlags.EndEllipsis);

            // 필수 별표: 실제 그려진 텍스트 폭 바로 뒤 (텍스트가 잘리면 영역 끝).
            if (this.required)
            {
                Size textSize = TextRenderer.MeasureText(e.Graphics, this.Text, font, textBounds.Size, textFlags);
                int starLeft = left + Math.Min(textSize.Width, textBounds.Width) + requiredGap;

                TextRenderer.DrawText(
                    e.Graphics, "*", LabelFont,
                    new Rectangle(starLeft, 0, starSize.Width + 2, this.Height),
                    RequiredColor, textFlags);
            }
        }

        // 공통 텍스트 플래그: 한 줄, 세로 중앙, 니모닉(&) 해석 없음.
        // NoPadding: GDI 기본 좌우 여백을 빼 WPF TextBlock과 같은 폭으로 그린다
        // (없으면 같은 크기의 컨트롤에서 WPF에는 들어가던 텍스트가 말줄임된다).
        private const TextFormatFlags textFlags =
            TextFormatFlags.SingleLine
            | TextFormatFlags.VerticalCenter
            | TextFormatFlags.Left
            | TextFormatFlags.NoPrefix
            | TextFormatFlags.NoPadding;

        private Font GetKindFont()
        {
            switch (this.kind)
            {
                case LabelKind.Title:
                    return TitleFont;
                case LabelKind.Label:
                    return LabelFont;
                case LabelKind.Helper:
                    return HelperFont;
                default:
                    return BodyFont;
            }
        }

        private Color GetKindColor()
        {
            switch (this.kind)
            {
                case LabelKind.Label:
                case LabelKind.Helper:
                    return TextSecondaryColor;
                default:
                    return TextPrimaryColor;
            }
        }
    }
}
