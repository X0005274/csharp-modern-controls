using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>
    /// 드롭다운 버튼 (버튼 + 메뉴): 클릭하면 항목 메뉴가 열리고,
    /// 항목을 클릭하면 ItemClicked가 발생한다.
    /// - Text: 버튼 캡션 (셰브런은 자동으로 붙는다)
    /// - ItemsSource / DisplayMemberPath / ValueMemberPath: 메뉴 항목 (코드/명칭 계약)
    /// </summary>
    public partial class ModernDropDownButtonControl : UserControl
    {
        /// <summary>버튼 캡션.</summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(string.Empty));

        /// <summary>메뉴 항목 목록.</summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(null, OnDataShapeChanged));

        /// <summary>메뉴 표시 텍스트로 쓸 컬럼/속성 이름.</summary>
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(
                "DisplayMemberPath",
                typeof(string),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>항목 값으로 쓸 컬럼/속성 이름.</summary>
        public static readonly DependencyProperty ValueMemberPathProperty =
            DependencyProperty.Register(
                "ValueMemberPath",
                typeof(string),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>항목 실행 가능 여부로 쓸 컬럼/속성 이름 (bool 또는
        /// "Y"/"true"/"1"). 비우면 모든 항목이 활성이다. 비활성 항목은
        /// 회색으로 표시되고 클릭되지 않는다.</summary>
        public static readonly DependencyProperty EnabledMemberPathProperty =
            DependencyProperty.Register(
                "EnabledMemberPath",
                typeof(string),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>버튼 시각 종류 — ModernButton과 같은 문법(Secondary 기본,
        /// Execute = Success 초록 채움 등).</summary>
        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(
                "Kind",
                typeof(ButtonKind),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(ButtonKind.Secondary));

        /// <summary>메뉴 팝업이 현재 열려 있는지 여부.</summary>
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register(
                "IsDropDownOpen",
                typeof(bool),
                typeof(ModernDropDownButtonControl),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private readonly ObservableCollection<DropDownButtonItem> menuItems;

        /// <summary>메뉴 항목이 클릭될 때 발생한다.</summary>
        public event EventHandler<DropDownItemClickedEventArgs> ItemClicked;

        public ModernDropDownButtonControl()
        {
            this.menuItems = new ObservableCollection<DropDownButtonItem>();
            this.InitializeComponent();
            this.MenuItemsControl.ItemsSource = this.menuItems;
        }

        /// <summary>버튼 캡션.</summary>
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        /// <summary>메뉴 항목 목록.</summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>메뉴 표시 텍스트로 쓸 컬럼/속성 이름.</summary>
        public string DisplayMemberPath
        {
            get { return (string)this.GetValue(DisplayMemberPathProperty); }
            set { this.SetValue(DisplayMemberPathProperty, value); }
        }

        /// <summary>항목 값으로 쓸 컬럼/속성 이름.</summary>
        public string ValueMemberPath
        {
            get { return (string)this.GetValue(ValueMemberPathProperty); }
            set { this.SetValue(ValueMemberPathProperty, value); }
        }

        /// <summary>항목 실행 가능 여부로 쓸 컬럼/속성 이름 (비우면 전부 활성).</summary>
        public string EnabledMemberPath
        {
            get { return (string)this.GetValue(EnabledMemberPathProperty); }
            set { this.SetValue(EnabledMemberPathProperty, value); }
        }

        /// <summary>버튼 시각 종류 (Secondary 기본, Execute = 초록 채움).</summary>
        public ButtonKind Kind
        {
            get { return (ButtonKind)this.GetValue(KindProperty); }
            set { this.SetValue(KindProperty, value); }
        }

        /// <summary>메뉴 팝업이 현재 열려 있는지 여부.</summary>
        public bool IsDropDownOpen
        {
            get { return (bool)this.GetValue(IsDropDownOpenProperty); }
            private set { this.SetValue(IsDropDownOpenProperty, value); }
        }

        private static void OnDataShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernDropDownButtonControl)d).RebuildItems();
        }

        // 소스 행을 메뉴 항목으로 재구성한다. null/빈 소스와 존재하지 않는
        // 컬럼은 빈 메뉴/빈 텍스트로 처리하며 예외를 던지지 않는다(계약 규칙 3).
        private void RebuildItems()
        {
            this.menuItems.Clear();

            IEnumerable source = this.ItemsSource;

            if (source == null)
            {
                return;
            }

            foreach (object row in source)
            {
                object value = string.IsNullOrEmpty(this.ValueMemberPath)
                    ? row
                    : MemberPathReader.Read(row, this.ValueMemberPath);

                string displayText = MemberPathReader.ReadDisplayText(row, this.DisplayMemberPath);
                this.menuItems.Add(new DropDownButtonItem(value, displayText, this.ReadEnabled(row)));
            }
        }

        // 항목 실행 가능 여부를 관용적으로 읽는다 — bool 또는 "Y"/"true"/"1"
        // 문자열을 허용하고, 경로가 비어 있으면 항상 활성이다.
        private bool ReadEnabled(object row)
        {
            if (string.IsNullOrEmpty(this.EnabledMemberPath))
            {
                return true;
            }

            object value = MemberPathReader.Read(row, this.EnabledMemberPath);

            if (value is bool)
            {
                return (bool)value;
            }

            string text = value == null ? string.Empty : value.ToString().Trim();
            return string.Equals(text, "Y", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(text, "true", StringComparison.OrdinalIgnoreCase)
                    || text == "1";
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DropDownButtonItem item = ((Button)sender).DataContext as DropDownButtonItem;

            this.DropToggle.IsChecked = false;

            if (item != null && this.ItemClicked != null)
            {
                this.ItemClicked(this, new DropDownItemClickedEventArgs(item.Value, item.DisplayText));
            }
        }
    }
}
