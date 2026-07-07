using System.Windows;
using System.Windows.Controls;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// 조회/처리 중 대상 영역을 덮는 로딩 패널 (스피너 + 메시지).
    /// 표시/숨김은 래퍼(WinForms Visible)가 담당하고, 이 컨트롤은 내용만 그린다.
    /// </summary>
    public partial class ModernBusyOverlayControl : UserControl
    {
        /// <summary>스피너 아래 표시되는 안내 메시지.</summary>
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                "Message",
                typeof(string),
                typeof(ModernBusyOverlayControl),
                new PropertyMetadata("처리 중..."));

        /// <summary>주 메시지 아래 표시되는 보조 안내 문구(선택; 비어 있으면 숨김).</summary>
        public static readonly DependencyProperty SubMessageProperty =
            DependencyProperty.Register(
                "SubMessage",
                typeof(string),
                typeof(ModernBusyOverlayControl),
                new PropertyMetadata(string.Empty));

        public ModernBusyOverlayControl()
        {
            this.InitializeComponent();
        }

        /// <summary>스피너 아래 표시되는 안내 메시지.</summary>
        public string Message
        {
            get { return (string)this.GetValue(MessageProperty); }
            set { this.SetValue(MessageProperty, value); }
        }

        /// <summary>주 메시지 아래 표시되는 보조 안내 문구(선택; 비어 있으면 숨김).</summary>
        public string SubMessage
        {
            get { return (string)this.GetValue(SubMessageProperty); }
            set { this.SetValue(SubMessageProperty, value); }
        }
    }
}
