using System.Windows;
using System.Windows.Controls;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// Modern text label.
    /// - Text: text to display
    /// - Kind: typography role (Body/Title/Label/Helper) from the token type ramp
    /// </summary>
    public partial class ModernLabelControl : UserControl
    {
        /// <summary>Text to display.</summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(ModernLabelControl),
                new PropertyMetadata("레이블"));

        /// <summary>Typography role. Defaults to Body.</summary>
        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(
                "Kind",
                typeof(LabelKind),
                typeof(ModernLabelControl),
                new PropertyMetadata(LabelKind.Body));

        /// <summary>Shows a red asterisk after the text (required-field mark).</summary>
        public static readonly DependencyProperty RequiredProperty =
            DependencyProperty.Register(
                "Required",
                typeof(bool),
                typeof(ModernLabelControl),
                new PropertyMetadata(false));

        public ModernLabelControl()
        {
            this.InitializeComponent();
        }

        /// <summary>Text to display.</summary>
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        /// <summary>Typography role (Body/Title/Label/Helper).</summary>
        public LabelKind Kind
        {
            get { return (LabelKind)this.GetValue(KindProperty); }
            set { this.SetValue(KindProperty, value); }
        }

        /// <summary>Shows a red asterisk after the text (required-field mark).</summary>
        public bool Required
        {
            get { return (bool)this.GetValue(RequiredProperty); }
            set { this.SetValue(RequiredProperty, value); }
        }
    }
}
