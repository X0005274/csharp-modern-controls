using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// 상태 표시 pill 배지 (승인/반려/대기, 운영/개발 등).
    /// - Text: 배지 텍스트
    /// - Color: 배경색 문자열 ("#DCFCE7" hex 또는 "SkyBlue" 색 이름).
    ///   글자색은 배경과 같은 색상 계열로 자동 유도된다.
    ///   비어 있거나 파싱 불가면 중립 회색 배지로 폴백한다.
    /// </summary>
    public partial class ModernStatusBadgeControl : UserControl
    {
        /// <summary>배지 텍스트.</summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(ModernStatusBadgeControl),
                new PropertyMetadata(string.Empty));

        /// <summary>배경색 문자열 (hex/색 이름). 비우면 중립 회색.</summary>
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(
                "Color",
                typeof(string),
                typeof(ModernStatusBadgeControl),
                new PropertyMetadata(string.Empty, OnColorChanged));

        public ModernStatusBadgeControl()
        {
            this.InitializeComponent();
        }

        /// <summary>배지 텍스트.</summary>
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        /// <summary>배경색 문자열 (hex/색 이름). 비우면 중립 회색.</summary>
        public string Color
        {
            get { return (string)this.GetValue(ColorProperty); }
            set { this.SetValue(ColorProperty, value); }
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernStatusBadgeControl)d).ApplyColor();
        }

        // Color 문자열을 배경 브러시로, 글자색은 배경에서 유도해서 적용한다.
        // 파싱 불가/빈 값은 기본 토큰(중립 회색)으로 폴백하며 예외를 던지지 않는다.
        private void ApplyColor()
        {
            System.Windows.Media.Color parsed;

            if (!ChipColorHelper.TryParseColor(this.Color, out parsed))
            {
                this.PillBorder.Background = (Brush)this.FindResource("Brush.NeutralBackground");
                this.PillText.Foreground = (Brush)this.FindResource("Brush.NeutralText");
                return;
            }

            SolidColorBrush backgroundBrush = new SolidColorBrush(parsed);
            backgroundBrush.Freeze();
            this.PillBorder.Background = backgroundBrush;

            SolidColorBrush foregroundBrush = new SolidColorBrush(ChipColorHelper.DeriveForeground(parsed));
            foregroundBrush.Freeze();
            this.PillText.Foreground = foregroundBrush;
        }
    }
}
