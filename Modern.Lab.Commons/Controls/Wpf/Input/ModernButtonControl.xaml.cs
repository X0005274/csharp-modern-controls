using System.Windows;
using System.Windows.Controls;

namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>
    /// 모던 둥근 버튼 컨트롤.
    /// - Text: 버튼에 표시되는 캡션
    /// - Kind: 색상 세트를 전환하는 시각적 종류 (Primary/Secondary/Danger)
    /// - Click: 버튼이 눌릴 때 발생
    /// </summary>
    public partial class ModernButtonControl : UserControl
    {
        /// <summary>버튼에 표시되는 캡션.</summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(ModernButtonControl),
                new PropertyMetadata("버튼"));

        /// <summary>시각적 종류(색상 / 강조 수준). 기본값은 Primary.</summary>
        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(
                "Kind",
                typeof(ButtonKind),
                typeof(ModernButtonControl),
                new PropertyMetadata(ButtonKind.Primary));

        /// <summary>캡션 앞에 표시되는 아이콘 글리프(Segoe MDL2 Assets). 비어 있으면 숨긴다.</summary>
        public static readonly DependencyProperty IconGlyphProperty =
            DependencyProperty.Register(
                "IconGlyph",
                typeof(string),
                typeof(ModernButtonControl),
                new PropertyMetadata(string.Empty));

        /// <summary>캡션/아이콘 글자 크기 재정의(px). 0 이하이면 토큰 기본값
        /// (Font.Size.Body)을 쓴다. 화살표·기호 같은 아이콘형 캡션을 크게
        /// 보이게 할 때 쓴다.</summary>
        public static readonly DependencyProperty FontSizeOverrideProperty =
            DependencyProperty.Register(
                "FontSizeOverride",
                typeof(double),
                typeof(ModernButtonControl),
                new PropertyMetadata(0d, OnFontSizeOverrideChanged));

        /// <summary>글리프/캡션 위에 붙는 아주 작은 상단 라벨(비어 있으면 숨김).
        /// 아이콘형 버튼에 "All"/"Selected" 같은 부가 설명을 얹을 때 쓴다.
        /// 값이 있으면 버튼 높이를 내용에 맞춰 늘린다(고정 높이 해제).</summary>
        public static readonly DependencyProperty TopLabelProperty =
            DependencyProperty.Register(
                "TopLabel",
                typeof(string),
                typeof(ModernButtonControl),
                new PropertyMetadata(string.Empty, OnTopLabelChanged));

        /// <summary>
        /// 버튼이 클릭될 때 발생한다. 내부 버튼의 Click을 전달한다.
        /// </summary>
        public event RoutedEventHandler Click;

        public ModernButtonControl()
        {
            this.InitializeComponent();
            this.Loaded += delegate { this.ApplyFontSize(); this.ApplyTopLabel(); };
        }

        private static void OnFontSizeOverrideChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernButtonControl)d).ApplyFontSize();
        }

        private static void OnTopLabelChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernButtonControl)d).ApplyTopLabel();
        }

        // 상단 라벨이 있으면 고정 높이(Size.ControlHeight)를 풀어 두 줄(라벨+
        // 글리프)이 잘리지 않게 내용 높이로 늘린다. 없으면 스타일 기본 높이.
        private void ApplyTopLabel()
        {
            bool hasTop = !string.IsNullOrEmpty(this.TopLabel);
            this.InnerButton.Height = hasTop
                ? double.NaN
                : (double)this.FindResource("Size.ControlHeight");
        }

        // 캡션/아이콘 글자 크기를 재정의 값(>0)으로, 아니면 토큰 기본값
        // (Font.Size.Body)으로 맞춘다. 아이콘 글리프도 함께 커진다.
        private void ApplyFontSize()
        {
            bool icon = this.FontSizeOverride > 0d;
            double size = icon
                ? this.FontSizeOverride
                : (double)this.FindResource("Font.Size.Body");

            this.CaptionText.FontSize = size;
            this.IconText.FontSize = size;

            // 아이콘형(큰 글리프) 버튼은 텍스트용 좌우 패딩(Pad.Button=16)이 과해
            // 좁은 버튼에서 화살표/기호가 잘린다 — 좌우 패딩을 줄여 온전히 보이게 한다.
            this.InnerButton.Padding = icon
                ? new Thickness(4d, 0d, 4d, 0d)
                : (Thickness)this.FindResource("Pad.Button");
        }

        /// <summary>버튼에 표시되는 캡션.</summary>
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        /// <summary>시각적 종류 (Primary/Secondary/Danger).</summary>
        public ButtonKind Kind
        {
            get { return (ButtonKind)this.GetValue(KindProperty); }
            set { this.SetValue(KindProperty, value); }
        }

        /// <summary>캡션 앞에 표시되는 아이콘 글리프(Segoe MDL2 Assets).</summary>
        public string IconGlyph
        {
            get { return (string)this.GetValue(IconGlyphProperty); }
            set { this.SetValue(IconGlyphProperty, value); }
        }

        /// <summary>캡션/아이콘 글자 크기 재정의(px). 0 이하 = 토큰 기본값.</summary>
        public double FontSizeOverride
        {
            get { return (double)this.GetValue(FontSizeOverrideProperty); }
            set { this.SetValue(FontSizeOverrideProperty, value); }
        }

        /// <summary>글리프/캡션 위에 붙는 아주 작은 상단 라벨(비어 있으면 숨김).</summary>
        public string TopLabel
        {
            get { return (string)this.GetValue(TopLabelProperty); }
            set { this.SetValue(TopLabelProperty, value); }
        }

        // 내부 버튼의 Click을 이 컨트롤의 Click으로 다시 발생시킨다.
        private void InnerButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Click != null)
            {
                this.Click(this, e);
            }
        }
    }
}
