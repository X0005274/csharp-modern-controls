using System;
using System.Windows.Media;

namespace Modern.Lab.Controls.Wpf.Common
{
    /// <summary>
    /// 칩/배지류가 공유하는 색 처리 유틸리티.
    /// 배경색 문자열 파싱과, 배경에서 같은 색상 계열의 글자색을 유도하는
    /// 규칙(밝은 배경 → 진한 톤, 어두운 배경 → 밝은 톤)을 담는다.
    /// </summary>
    internal static class ChipColorHelper
    {
        /// <summary>
        /// "#DBEAFE" 같은 hex 또는 "SkyBlue" 같은 색 이름 문자열을 파싱한다.
        /// 빈 값/파싱 불가면 false를 반환하며 예외를 던지지 않는다.
        /// </summary>
        internal static bool TryParseColor(string text, out Color color)
        {
            color = Colors.Transparent;

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            try
            {
                color = (Color)ColorConverter.ConvertFromString(text.Trim());
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 배경색에서 글자색을 유도한다: 색상(Hue)은 유지하고 명도만 반대쪽으로.
        /// - 밝은 배경 → 같은 색상의 진한 톤 (채도 보강 + 명도 0.30)
        /// - 어두운 배경 → 같은 색상의 아주 밝은 톤 (명도 0.95)
        /// - 무채색 배경 → 밝기에 따라 중립 진회색 또는 흰색
        /// </summary>
        internal static Color DeriveForeground(Color background)
        {
            double hue;
            double saturation;
            double lightness;
            RgbToHsl(background, out hue, out saturation, out lightness);

            // 상대 휘도(0~1)로 밝은/어두운 배경을 판정한다 (HSL 명도보다 지각에 가깝다).
            double luminance = ((0.2126 * background.R) + (0.7152 * background.G) + (0.0722 * background.B)) / 255.0;

            if (saturation < 0.15)
            {
                if (luminance < 0.5)
                {
                    return Colors.White;
                }

                return Color.FromRgb(0x37, 0x41, 0x51);
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

            return Color.FromRgb(
                (byte)Math.Round(r * 255.0),
                (byte)Math.Round(g * 255.0),
                (byte)Math.Round(b * 255.0));
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
