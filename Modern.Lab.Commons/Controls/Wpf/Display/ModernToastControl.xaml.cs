using System.Windows;
using System.Windows.Controls;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// 자동 소멸 알림(토스트)의 내용 카드 — 종류별 색 아이콘 + 메시지.
    /// 표시/숨김/위치/타이머는 래퍼(ModernToast)가 담당하고, 이 컨트롤은 내용만 그린다.
    /// </summary>
    public partial class ModernToastControl : UserControl
    {
        /// <summary>알림 메시지.</summary>
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                "Message",
                typeof(string),
                typeof(ModernToastControl),
                new PropertyMetadata(string.Empty));

        /// <summary>알림 종류 (아이콘·색 결정).</summary>
        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(
                "Kind",
                typeof(ToastKind),
                typeof(ModernToastControl),
                new PropertyMetadata(ToastKind.Info));

        public ModernToastControl()
        {
            this.InitializeComponent();
        }

        /// <summary>알림 메시지.</summary>
        public string Message
        {
            get { return (string)this.GetValue(MessageProperty); }
            set { this.SetValue(MessageProperty, value); }
        }

        /// <summary>알림 종류.</summary>
        public ToastKind Kind
        {
            get { return (ToastKind)this.GetValue(KindProperty); }
            set { this.SetValue(KindProperty, value); }
        }
    }
}
