using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// 줄바꿈되는 칩 목록으로 렌더링되는 분류/건수 요약
    /// (예: 부서별 또는 직급별 인원 수).
    /// - ItemsSource: 임의의 IEnumerable (DataView, IList, ...)
    /// - LabelMemberPath / CountMemberPath: 읽어올 컬럼/속성 이름
    /// - Title: 칩 위에 표시되는 선택적 캡션
    ///
    /// 칩 목록은 소스나 멤버 경로가 바뀔 때마다 다시 만들어지므로
    /// 할당 순서는 상관없다(계약 규칙 3).
    /// </summary>
    public partial class ModernSummaryListControl : UserControl
    {
        /// <summary>요약할 행 목록. 임의의 IEnumerable (DataView, IList, ...).</summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(ModernSummaryListControl),
                new PropertyMetadata(null, OnDataShapeChanged));

        /// <summary>칩 레이블로 쓸 컬럼/속성 이름.</summary>
        public static readonly DependencyProperty LabelMemberPathProperty =
            DependencyProperty.Register(
                "LabelMemberPath",
                typeof(string),
                typeof(ModernSummaryListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>칩 건수로 쓸 컬럼/속성 이름.</summary>
        public static readonly DependencyProperty CountMemberPathProperty =
            DependencyProperty.Register(
                "CountMemberPath",
                typeof(string),
                typeof(ModernSummaryListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>
        /// 칩 배경색으로 쓸 컬럼/속성 이름. 값은 "#DBEAFE" 같은 hex 또는
        /// "SkyBlue" 같은 색 이름 문자열. 비어 있거나 파싱 불가면 기본 토큰 색으로 폴백.
        /// </summary>
        public static readonly DependencyProperty ColorMemberPathProperty =
            DependencyProperty.Register(
                "ColorMemberPath",
                typeof(string),
                typeof(ModernSummaryListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>칩 왼쪽의 선택적 캡션. 비어 있으면 캡션을 숨긴다.</summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "Title",
                typeof(string),
                typeof(ModernSummaryListControl),
                new PropertyMetadata(string.Empty));

        /// <summary>공용 카드 패널 위에서 쓰도록 카드 크롬(테두리/배경/패딩)을 제거한다.</summary>
        public static readonly DependencyProperty FlatProperty =
            DependencyProperty.Register(
                "Flat",
                typeof(bool),
                typeof(ModernSummaryListControl),
                new PropertyMetadata(false));

        private readonly ObservableCollection<SummaryItem> chipItemModels;

        public ModernSummaryListControl()
        {
            this.chipItemModels = new ObservableCollection<SummaryItem>();
            this.InitializeComponent();
            this.ChipItems.ItemsSource = this.chipItemModels;
        }

        /// <summary>요약할 행 목록.</summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>칩 레이블로 쓸 컬럼/속성 이름.</summary>
        public string LabelMemberPath
        {
            get { return (string)this.GetValue(LabelMemberPathProperty); }
            set { this.SetValue(LabelMemberPathProperty, value); }
        }

        /// <summary>칩 건수로 쓸 컬럼/속성 이름.</summary>
        public string CountMemberPath
        {
            get { return (string)this.GetValue(CountMemberPathProperty); }
            set { this.SetValue(CountMemberPathProperty, value); }
        }

        /// <summary>칩 배경색으로 쓸 컬럼/속성 이름.</summary>
        public string ColorMemberPath
        {
            get { return (string)this.GetValue(ColorMemberPathProperty); }
            set { this.SetValue(ColorMemberPathProperty, value); }
        }

        /// <summary>칩 왼쪽의 선택적 캡션.</summary>
        public string Title
        {
            get { return (string)this.GetValue(TitleProperty); }
            set { this.SetValue(TitleProperty, value); }
        }

        /// <summary>공용 카드 패널 위에서 쓰도록 카드 크롬을 제거한다.</summary>
        public bool Flat
        {
            get { return (bool)this.GetValue(FlatProperty); }
            set { this.SetValue(FlatProperty, value); }
        }

        private static void OnDataShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernSummaryListControl)d).RebuildChips();
        }

        // 소스 행을 칩 항목 모델로 변환한다. null/빈 소스와 존재하지 않는
        // 컬럼은 빈 출력으로 렌더링되며 절대 예외를 던지지 않는다(계약 규칙 3).
        private void RebuildChips()
        {
            this.chipItemModels.Clear();

            IEnumerable source = this.ItemsSource;

            if (source == null)
            {
                return;
            }

            foreach (object row in source)
            {
                SummaryItem item = new SummaryItem();
                item.Label = ToDisplayString(MemberPathReader.Read(row, this.LabelMemberPath));
                item.Count = ToDisplayString(MemberPathReader.Read(row, this.CountMemberPath));
                ApplyChipColor(item, MemberPathReader.Read(row, this.ColorMemberPath));
                this.chipItemModels.Add(item);
            }
        }

        // 행의 색 값(hex 또는 색 이름 문자열)을 칩 배경 브러시로 변환한다.
        // 빈 값/파싱 불가면 브러시를 null로 두어 XAML의 기본 토큰 색으로 폴백한다
        // (계약 규칙 3: 예외 없음). 글자색은 배경과 같은 색상 계열로 자동 산출.
        private static void ApplyChipColor(SummaryItem item, object colorValue)
        {
            Color color;

            if (!ChipColorHelper.TryParseColor(ToDisplayString(colorValue), out color))
            {
                return;
            }

            SolidColorBrush backgroundBrush = new SolidColorBrush(color);
            backgroundBrush.Freeze();
            item.Background = backgroundBrush;

            SolidColorBrush foregroundBrush = new SolidColorBrush(ChipColorHelper.DeriveForeground(color));
            foregroundBrush.Freeze();
            item.LabelForeground = foregroundBrush;
            item.CountForeground = foregroundBrush;
        }

        private static string ToDisplayString(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.ToString();
        }
    }
}
