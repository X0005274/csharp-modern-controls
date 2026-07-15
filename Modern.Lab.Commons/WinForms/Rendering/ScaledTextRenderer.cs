using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Modern.Lab.WinForms.Rendering
{
    /// <summary>
    /// 장평(글자 가로 비율)을 지원하는 GDI/GDI+ 텍스트 렌더러.
    ///
    /// 장평이 1.0(기본)이면 기존과 완전히 동일하게 <see cref="TextRenderer"/>로
    /// 위임한다 — 장평을 쓰지 않는 시스템은 동작이 전혀 바뀌지 않는다.
    ///
    /// 장평이 1.0이 아니면 GDI+ <c>DrawString</c> + 가로 <c>ScaleTransform</c>으로
    /// 그린다. GDI <c>LOGFONT.lfWidth</c> 방식(워드프로세서식 장평)을 먼저 검증했으나
    /// **lfWidth를 지정하면 GDI 폰트 링크(한글 → Malgun Gothic 폴백)가 깨져 한글이
    /// 대체 글리프로 렌더링**되는 것을 실측으로 확인했다(ASCII는 정확, 한글은 붕괴).
    /// GDI+는 자체 폰트 폴백을 수행하므로 한글이 정상 렌더링되고, 벡터 스케일이라
    /// 임의 비율이 정밀하게 적용된다. 품질은 ClearTypeGridFit 힌트로 유지한다.
    ///
    /// 측정은 GenericTypographic(여백 없는 어드밴스 폭) 기준이라 TextRenderer의
    /// NoPadding 폭과 근사하게 일치한다 — 장평 경로에서만 쓰이므로 1.0 경로의
    /// 픽셀 결과에는 영향이 없다.
    /// </summary>
    public static class ScaledTextRenderer
    {
        // 이 값보다 1.0에 가까우면 장평 없음으로 보고 TextRenderer로 위임한다.
        private const double ratioEpsilon = 0.001;

        // 측정 전용 Graphics (1×1 비트맵) — DC 없는 측정 호출에 공유한다.
        private static readonly object measureLock = new object();
        private static Bitmap measureBitmap;
        private static Graphics measureGraphics;

        /// <summary>장평을 적용해 텍스트를 그린다. 비율 1.0이면 TextRenderer와 동일.</summary>
        public static void DrawText(
            Graphics graphics, string text, Font font, Rectangle bounds,
            Color foreColor, TextFormatFlags flags, double widthRatio)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (Math.Abs(widthRatio - 1d) < ratioEpsilon)
            {
                TextRenderer.DrawText(graphics, text, font, bounds, foreColor, flags);
                return;
            }

            System.Drawing.Drawing2D.GraphicsState state = graphics.Save();

            try
            {
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                graphics.ScaleTransform((float)widthRatio, 1f);

                // 가로 스케일 좌표계에서 원래 화면 위치/폭이 유지되도록 X만 환산한다.
                RectangleF layout = new RectangleF(
                    (float)(bounds.X / widthRatio),
                    bounds.Y,
                    (float)(bounds.Width / widthRatio),
                    bounds.Height);

                using (StringFormat format = BuildFormat(flags))
                using (SolidBrush brush = new SolidBrush(foreColor))
                {
                    graphics.DrawString(text, font, brush, layout, format);
                }
            }
            finally
            {
                graphics.Restore(state);
            }
        }

        /// <summary>장평을 적용한 텍스트 크기를 잰다. 비율 1.0이면 TextRenderer와 동일.</summary>
        public static Size MeasureText(
            IDeviceContext dc, string text, Font font, Size proposedSize,
            TextFormatFlags flags, double widthRatio)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Size.Empty;
            }

            if (Math.Abs(widthRatio - 1d) < ratioEpsilon)
            {
                return TextRenderer.MeasureText(dc, text, font, proposedSize, flags);
            }

            Graphics graphics = dc as Graphics;
            return graphics != null
                ? MeasureCore(graphics, text, font, flags, widthRatio)
                : MeasureCore(null, text, font, flags, widthRatio);
        }

        /// <summary>DC 없이 장평 적용 텍스트 크기를 잰다.</summary>
        public static Size MeasureText(string text, Font font, TextFormatFlags flags, double widthRatio)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Size.Empty;
            }

            if (Math.Abs(widthRatio - 1d) < ratioEpsilon)
            {
                return TextRenderer.MeasureText(text, font, Size.Empty, flags);
            }

            return MeasureCore(null, text, font, flags, widthRatio);
        }

        // 스케일 없는 어드밴스 폭을 재서 비율을 곱한다 (그릴 때와 같은 규칙).
        private static Size MeasureCore(
            Graphics graphics, string text, Font font, TextFormatFlags flags, double widthRatio)
        {
            using (StringFormat format = BuildFormat(flags))
            {
                SizeF size;

                if (graphics != null)
                {
                    size = graphics.MeasureString(text, font, int.MaxValue, format);
                }
                else
                {
                    lock (measureLock)
                    {
                        if (measureGraphics == null)
                        {
                            measureBitmap = new Bitmap(1, 1);
                            measureGraphics = Graphics.FromImage(measureBitmap);
                        }

                        size = measureGraphics.MeasureString(text, font, int.MaxValue, format);
                    }
                }

                return new Size(
                    (int)Math.Ceiling(size.Width * widthRatio),
                    (int)Math.Ceiling(size.Height));
            }
        }

        // TextFormatFlags의 정렬/말줄임 의도를 StringFormat으로 옮긴다.
        // GenericTypographic 기반이라 GDI+ 기본 좌우 여백이 없다 —
        // TextRenderer 사용부의 NoPadding 규칙과 같은 인상.
        private static StringFormat BuildFormat(TextFormatFlags flags)
        {
            StringFormat format = (StringFormat)StringFormat.GenericTypographic.Clone();
            format.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.None;
            format.FormatFlags = format.FormatFlags | StringFormatFlags.NoWrap;

            if ((flags & TextFormatFlags.HorizontalCenter) == TextFormatFlags.HorizontalCenter)
            {
                format.Alignment = StringAlignment.Center;
            }
            else if ((flags & TextFormatFlags.Right) == TextFormatFlags.Right)
            {
                format.Alignment = StringAlignment.Far;
            }

            if ((flags & TextFormatFlags.VerticalCenter) == TextFormatFlags.VerticalCenter)
            {
                format.LineAlignment = StringAlignment.Center;
            }
            else if ((flags & TextFormatFlags.Bottom) == TextFormatFlags.Bottom)
            {
                format.LineAlignment = StringAlignment.Far;
            }

            if ((flags & TextFormatFlags.EndEllipsis) == TextFormatFlags.EndEllipsis)
            {
                format.Trimming = StringTrimming.EllipsisCharacter;
            }

            return format;
        }
    }
}
