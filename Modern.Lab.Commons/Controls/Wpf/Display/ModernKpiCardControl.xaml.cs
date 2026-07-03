using System.Windows;
using System.Windows.Controls;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// Small statistic card.
    /// - Title: caption above the number (e.g. "조회 건수")
    /// - Value: the number/text to highlight
    /// </summary>
    public partial class ModernKpiCardControl : UserControl
    {
        /// <summary>Caption above the value.</summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "Title",
                typeof(string),
                typeof(ModernKpiCardControl),
                new PropertyMetadata("제목"));

        /// <summary>Highlighted value text.</summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(string),
                typeof(ModernKpiCardControl),
                new PropertyMetadata("0"));

        /// <summary>Drops the card chrome (border/background/padding) for use on a shared card panel.</summary>
        public static readonly DependencyProperty FlatProperty =
            DependencyProperty.Register(
                "Flat",
                typeof(bool),
                typeof(ModernKpiCardControl),
                new PropertyMetadata(false));

        public ModernKpiCardControl()
        {
            this.InitializeComponent();
        }

        /// <summary>Caption above the value.</summary>
        public string Title
        {
            get { return (string)this.GetValue(TitleProperty); }
            set { this.SetValue(TitleProperty, value); }
        }

        /// <summary>Highlighted value text.</summary>
        public string Value
        {
            get { return (string)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        /// <summary>Drops the card chrome for use on a shared card panel.</summary>
        public bool Flat
        {
            get { return (bool)this.GetValue(FlatProperty); }
            set { this.SetValue(FlatProperty, value); }
        }
    }
}
