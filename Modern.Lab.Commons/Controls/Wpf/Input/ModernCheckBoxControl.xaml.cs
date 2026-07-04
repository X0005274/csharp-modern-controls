using System;
using System.Windows;
using System.Windows.Controls;

namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>
    /// 모던 체크박스 (단독 사용).
    /// - IsChecked: 체크 상태 (양방향)
    /// - Text: 체크박스 옆 레이블 텍스트
    /// - CheckedChanged: 체크 상태가 바뀔 때 발생
    /// </summary>
    public partial class ModernCheckBoxControl : UserControl
    {
        /// <summary>체크 상태. 기본적으로 양방향 바인딩.</summary>
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                "IsChecked",
                typeof(bool),
                typeof(ModernCheckBoxControl),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnIsCheckedChanged));

        /// <summary>체크박스 옆에 표시되는 레이블 텍스트.</summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(ModernCheckBoxControl),
                new PropertyMetadata(string.Empty));

        /// <summary>체크 상태가 바뀔 때 발생한다 (래퍼가 CheckedChanged로 재노출).</summary>
        public event EventHandler CheckedChanged;

        public ModernCheckBoxControl()
        {
            this.InitializeComponent();
        }

        /// <summary>체크 상태.</summary>
        public bool IsChecked
        {
            get { return (bool)this.GetValue(IsCheckedProperty); }
            set { this.SetValue(IsCheckedProperty, value); }
        }

        /// <summary>체크박스 옆에 표시되는 레이블 텍스트.</summary>
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernCheckBoxControl control = (ModernCheckBoxControl)d;

            if (control.CheckedChanged != null)
            {
                control.CheckedChanged(control, EventArgs.Empty);
            }
        }
    }
}
