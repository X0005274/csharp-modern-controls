using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// 상태 표시 pill 배지 (색 있는 Label 사용을 대체).
    ///
    /// 원래 WPF ModernStatusBadgeControl을 ElementHost로 호스팅했지만, 배지는
    /// 라벨과 함께 폼에 여러 개 놓이는 표시 전용 컨트롤이라 개당 HWND + WPF
    /// 컴포지션 비용이 아까웠다. ModernLabel과 같은 이유로 디자인 토큰을
    /// 미러링해 **GDI+로 직접 그리는 순수 WinForms 컨트롤**로 전환했다.
    /// 공개 API(Text/Color)와 시각 결과는 WPF 버전과 동일하다.
    ///
    /// 승인/반려/대기, 운영/개발 같은 상태를 색 있는 알약으로 표시한다.
    /// Color에 배경색만 주면 글자색은 배경과 같은 색상 계열로 자동 유도된다
    /// (요약 칩과 동일한 규칙 — ChipColorHelper의 HSL 규칙을 GDI+로 미러링).
    /// </summary>
    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    public class ModernStatusBadge : Control
    {
        // 중립 배지 색은 중앙 팔레트(ModernTheme)에서 — 라이트/다크에 따라 바뀐다.
        private static Color NeutralBackground { get { return Modern.Lab.Theming.ModernTheme.NeutralBackground; } }
        private static Color NeutralText { get { return Modern.Lab.Theming.ModernTheme.NeutralText; } }

        // Font.Size.Helper 12 DIU = 9pt, 배지 텍스트는 SemiBold.
        private static readonly Font BadgeFont = new Font("Segoe UI Semibold", 9f);

        // pill 안쪽 패딩 (WPF 버전 Padding 10,2와 동일).
        private const int pillPaddingX = 10;
        private const int pillPaddingY = 2;

        private string colorText;
        private Color pillBackground;
        private Color pillForeground;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernStatusBadge()
        {
            this.SetStyle(
                ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.SupportsTransparentBackColor,
                true);

            this.Size = new Size(70, 24);
            this.BackColor = System.Drawing.Color.Transparent;
            this.colorText = string.Empty;
            this.pillBackground = NeutralBackground;
            this.pillForeground = NeutralText;
            this.Text = "상태";
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

        /// <summary>배지 텍스트.</summary>
        [Category("모던 컨트롤")]
        [Description("배지에 표시할 텍스트")]
        [Browsable(true)]
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue("상태")]
        public override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        /// <summary>
        /// 배경색 문자열 ("#DCFCE7" hex 또는 "SkyBlue" 색 이름).
        /// 글자색은 자동 유도. 비우면 중립 회색 배지.
        /// </summary>
        [Category("모던 컨트롤")]
        [Description("배지 배경색 (hex 또는 색 이름 문자열; 비우면 중립 회색, 글자색은 자동)")]
        [DefaultValue("")]
        public string Color
        {
            get
            {
                return this.colorText;
            }
            set
            {
                this.colorText = value ?? string.Empty;
                this.ResolvePillColors();
                this.Invalidate();
            }
        }

        /// <summary>텍스트가 바뀌면 pill 폭이 달라지므로 다시 그린다.</summary>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            this.Invalidate();
        }

        /// <summary>텍스트 폭에 맞는 pill을 왼쪽 정렬로 그린다.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Size textSize = TextRenderer.MeasureText(
                e.Graphics, this.Text, BadgeFont, Size.Empty,
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);

            int pillHeight = Math.Min(this.Height, textSize.Height + (pillPaddingY * 2));
            int pillWidth = Math.Min(this.Width, textSize.Width + (pillPaddingX * 2));
            int pillTop = (this.Height - pillHeight) / 2;
            Rectangle pill = new Rectangle(0, pillTop, pillWidth, pillHeight);

            SmoothingMode originalMode = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = CreateCapsulePath(pill))
            using (SolidBrush fillBrush = new SolidBrush(this.pillBackground))
            {
                e.Graphics.FillPath(fillBrush, path);
            }

            e.Graphics.SmoothingMode = originalMode;

            TextRenderer.DrawText(
                e.Graphics, this.Text, BadgeFont, pill, this.pillForeground,
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding
                | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                | TextFormatFlags.EndEllipsis);
        }

        // Color 문자열을 파싱해 pill 배경/글자색을 확정한다.
        // 빈 값/파싱 불가는 중립 회색 배지로 폴백하며 예외를 던지지 않는다.
        private void ResolvePillColors()
        {
            Color background;

            if (this.colorText.Trim().Length == 0 || !TryParseColor(this.colorText.Trim(), out background))
            {
                this.pillBackground = NeutralBackground;
                this.pillForeground = NeutralText;
                return;
            }

            this.pillBackground = background;
            this.pillForeground = DeriveForeground(background);
        }

        private static bool TryParseColor(string text, out Color color)
        {
            try
            {
                color = ColorTranslator.FromHtml(text);
                return true;
            }
            catch (Exception)
            {
                color = System.Drawing.Color.Empty;
                return false;
            }
        }

        // 좌우가 반원인 캡슐 모양 경로 (Radius.Pill과 동일한 인상).
        private static GraphicsPath CreateCapsulePath(Rectangle bounds)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = Math.Min(bounds.Height, bounds.Width);

            if (diameter <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 90f, 180f);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270f, 180f);
            path.CloseFigure();
            return path;
        }

        // ===== 글자색 유도 (Wpf.Common.ChipColorHelper의 규칙을 GDI+로 미러링) =====
        // 배경색에서 글자색을 유도한다: 색상(Hue)은 유지하고 명도만 반대쪽으로.
        // - 밝은 배경 → 같은 색상의 진한 톤 (채도 보강 + 명도 0.30)
        // - 어두운 배경 → 같은 색상의 아주 밝은 톤 (명도 0.95)
        // - 무채색 배경 → 밝기에 따라 중립 진회색 또는 흰색
        private static Color DeriveForeground(Color background)
        {
            double hue;
            double saturation;
            double lightness;
            RgbToHsl(background, out hue, out saturation, out lightness);

            // 상대 휘도(0~1)로 밝은/어두운 배경을 판정한다.
            double luminance = ((0.2126 * background.R) + (0.7152 * background.G) + (0.0722 * background.B)) / 255.0;

            if (saturation < 0.15)
            {
                if (luminance < 0.5)
                {
                    return System.Drawing.Color.White;
                }

                return System.Drawing.Color.FromArgb(0x37, 0x41, 0x51);
            }

            if (luminance < 0.5)
            {
                return HslToRgb(hue, saturation, 0.95);
            }

            return HslToRgb(hue, Math.Max(saturation, 0.55), 0.30);
        }

        // RGB → HSL 변환. hue/saturation/lightness 모두 0~1 범위.
        private static void RgbToHsl(Color color, out double hue, out double saturation, out double lightness)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            lightness = (max + min) / 2.0;

            if (delta == 0.0)
            {
                hue = 0.0;
                saturation = 0.0;
                return;
            }

            if (lightness > 0.5)
            {
                saturation = delta / (2.0 - max - min);
            }
            else
            {
                saturation = delta / (max + min);
            }

            if (max == r)
            {
                hue = (((g - b) / delta) + (g < b ? 6.0 : 0.0)) / 6.0;
            }
            else if (max == g)
            {
                hue = (((b - r) / delta) + 2.0) / 6.0;
            }
            else
            {
                hue = (((r - g) / delta) + 4.0) / 6.0;
            }
        }

        // HSL → RGB 변환. hue/saturation/lightness 모두 0~1 범위.
        private static Color HslToRgb(double hue, double saturation, double lightness)
        {
            double r;
            double g;
            double b;

            if (saturation == 0.0)
            {
                r = lightness;
                g = lightness;
                b = lightness;
            }
            else
            {
                double q;

                if (lightness < 0.5)
                {
                    q = lightness * (1.0 + saturation);
                }
                else
                {
                    q = lightness + saturation - (lightness * saturation);
                }

                double p = (2.0 * lightness) - q;
                r = HueToRgbChannel(p, q, hue + (1.0 / 3.0));
                g = HueToRgbChannel(p, q, hue);
                b = HueToRgbChannel(p, q, hue - (1.0 / 3.0));
            }

            return System.Drawing.Color.FromArgb(
                (int)Math.Round(r * 255.0),
                (int)Math.Round(g * 255.0),
                (int)Math.Round(b * 255.0));
        }

        // HSL 보조: 색상 위치 t에 해당하는 채널 값(0~1)을 구한다.
        private static double HueToRgbChannel(double p, double q, double t)
        {
            if (t < 0.0)
            {
                t = t + 1.0;
            }

            if (t > 1.0)
            {
                t = t - 1.0;
            }

            if (t < 1.0 / 6.0)
            {
                return p + ((q - p) * 6.0 * t);
            }

            if (t < 1.0 / 2.0)
            {
                return q;
            }

            if (t < 2.0 / 3.0)
            {
                return p + ((q - p) * ((2.0 / 3.0) - t) * 6.0);
            }

            return p;
        }
    }
}
