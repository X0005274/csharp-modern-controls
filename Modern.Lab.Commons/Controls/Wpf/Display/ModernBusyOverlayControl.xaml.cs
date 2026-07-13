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

        /// <summary>
        /// 스피너 회전 여부. 래퍼가 표시할 때 true, 숨길 때 false로 되돌린다 —
        /// WinForms Visible=false는 WPF Visibility를 바꾸지 않아 Loaded 기반
        /// 무한 애니메이션은 숨긴 뒤에도 계속 돌기 때문에 명시적으로 제어한다.
        /// </summary>
        public static readonly DependencyProperty IsSpinningProperty =
            DependencyProperty.Register(
                "IsSpinning",
                typeof(bool),
                typeof(ModernBusyOverlayControl),
                new PropertyMetadata(false));

        public ModernBusyOverlayControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 카드 바깥 여백(루트 그리드) 배경을 지정한다. 래퍼가 표시 직전에 부모
        /// BackColor를 넘겨, 팝업 카드 주변 사각 영역이 어떤 배경(라이트 흰 그리드 /
        /// 다크 폼 배경) 위에서도 박스로 드러나지 않고 묻히게 한다.
        /// </summary>
        public void SetOuterBackground(System.Windows.Media.Color color)
        {
            this.OuterGrid.Background = new System.Windows.Media.SolidColorBrush(color);
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

        /// <summary>스피너 회전 여부 (표시 중에만 true로 유지한다).</summary>
        public bool IsSpinning
        {
            get { return (bool)this.GetValue(IsSpinningProperty); }
            set { this.SetValue(IsSpinningProperty, value); }
        }
    }
}
