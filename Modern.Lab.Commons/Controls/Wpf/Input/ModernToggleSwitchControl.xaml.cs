using System;
using System.Windows;
using System.Windows.Controls;

namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>
    /// 모던 온/오프 토글 스위치 (단독 사용).
    /// - IsChecked: 켬/끔 상태 (양방향)
    /// - Text: 스위치 옆 레이블 텍스트
    /// - CheckedChanged: 상태가 바뀔 때 발생
    ///
    /// 용도 구분: 설정성 "켬/끔"에는 스위치, 다중 "선택/포함"에는 체크박스.
    /// </summary>
    public partial class ModernToggleSwitchControl : UserControl
    {
        /// <summary>켬/끔 상태. 기본적으로 양방향 바인딩.</summary>
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                "IsChecked",
                typeof(bool),
                typeof(ModernToggleSwitchControl),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnIsCheckedChanged));

        /// <summary>스위치 옆에 표시되는 레이블 텍스트.</summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(ModernToggleSwitchControl),
                new PropertyMetadata(string.Empty));

        /// <summary>상태가 바뀔 때 발생한다 (래퍼가 CheckedChanged로 재노출).</summary>
        public event EventHandler CheckedChanged;

        public ModernToggleSwitchControl()
        {
            this.InitializeComponent();
        }

        /// <summary>켬/끔 상태.</summary>
        public bool IsChecked
        {
            get { return (bool)this.GetValue(IsCheckedProperty); }
            set { this.SetValue(IsCheckedProperty, value); }
        }

        /// <summary>스위치 옆에 표시되는 레이블 텍스트.</summary>
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernToggleSwitchControl control = (ModernToggleSwitchControl)d;

            if (control.CheckedChanged != null)
            {
                control.CheckedChanged(control, EventArgs.Empty);
            }
        }
    }
}
