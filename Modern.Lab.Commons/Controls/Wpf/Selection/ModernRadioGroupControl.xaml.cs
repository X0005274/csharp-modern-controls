using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Selection
{
    /// <summary>
    /// 모던 라디오 그룹 (배타 선택).
    /// - ItemsSource: 임의의 IEnumerable (DataView, IList, ...)
    /// - DisplayMemberPath / ValueMemberPath: 표시 텍스트 / 선택 값 컬럼
    /// - SelectedValue: 선택된 값 (null = 미선택)
    /// - Vertical: true면 세로 나열 (기본은 가로)
    ///
    /// 할당 순서 내성: SelectedValue를 ItemsSource보다 먼저 설정해도
    /// 목록 구성 시 적용된다. 재할당 시 값이 목록에 없으면 미선택으로 초기화된다.
    /// </summary>
    public partial class ModernRadioGroupControl : UserControl
    {
        /// <summary>표시할 항목 목록.</summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(ModernRadioGroupControl),
                new PropertyMetadata(null, OnDataShapeChanged));

        /// <summary>표시 텍스트로 쓸 컬럼/속성 이름.</summary>
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(
                "DisplayMemberPath",
                typeof(string),
                typeof(ModernRadioGroupControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>선택 값으로 쓸 컬럼/속성 이름.</summary>
        public static readonly DependencyProperty ValueMemberPathProperty =
            DependencyProperty.Register(
                "ValueMemberPath",
                typeof(string),
                typeof(ModernRadioGroupControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>선택된 값. null은 미선택을 의미한다.</summary>
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register(
                "SelectedValue",
                typeof(object),
                typeof(ModernRadioGroupControl),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedValuePropertyChanged));

        /// <summary>true면 항목을 세로로 나열한다 (기본은 가로).</summary>
        public static readonly DependencyProperty VerticalProperty =
            DependencyProperty.Register(
                "Vertical",
                typeof(bool),
                typeof(ModernRadioGroupControl),
                new PropertyMetadata(false));

        private readonly ObservableCollection<RadioGroupItem> radioItems;

        // 항목 체크 → SelectedValue 갱신 → 항목 재적용의 순환을 막는 가드.
        private bool suppressItemSync;

        /// <summary>선택 값이 바뀔 때 발생한다.</summary>
        public event EventHandler SelectedValueChanged;

        public ModernRadioGroupControl()
        {
            this.radioItems = new ObservableCollection<RadioGroupItem>();
            this.InitializeComponent();
            this.RadioItemsControl.ItemsSource = this.radioItems;
        }

        /// <summary>표시할 항목 목록.</summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>표시 텍스트로 쓸 컬럼/속성 이름.</summary>
        public string DisplayMemberPath
        {
            get { return (string)this.GetValue(DisplayMemberPathProperty); }
            set { this.SetValue(DisplayMemberPathProperty, value); }
        }

        /// <summary>선택 값으로 쓸 컬럼/속성 이름.</summary>
        public string ValueMemberPath
        {
            get { return (string)this.GetValue(ValueMemberPathProperty); }
            set { this.SetValue(ValueMemberPathProperty, value); }
        }

        /// <summary>선택된 값. null은 미선택을 의미한다.</summary>
        public object SelectedValue
        {
            get { return this.GetValue(SelectedValueProperty); }
            set { this.SetValue(SelectedValueProperty, value); }
        }

        /// <summary>true면 항목을 세로로 나열한다.</summary>
        public bool Vertical
        {
            get { return (bool)this.GetValue(VerticalProperty); }
            set { this.SetValue(VerticalProperty, value); }
        }

        private static void OnDataShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernRadioGroupControl)d).RebuildItems();
        }

        private static void OnSelectedValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernRadioGroupControl control = (ModernRadioGroupControl)d;

            // 값이 바뀔 때마다 항목 체크 상태를 재적용한다 — 클릭된 항목은 유지되고
            // 나머지는 해제되어 배타성이 모델 수준에서 보장된다.
            // (WPF RadioButton은 ItemsControl 항목마다 논리 부모가 달라
            //  GroupName 없이는 자동 배타 그룹핑이 되지 않는다.)
            control.ApplySelectedValueToItems();

            // 실제 값이 달라졌을 때만 통지한다.
            if (!Equals(e.OldValue, e.NewValue) && control.SelectedValueChanged != null)
            {
                control.SelectedValueChanged(control, EventArgs.Empty);
            }
        }

        // 소스 행을 라디오 항목으로 재구성한다. null/빈 소스와 존재하지 않는
        // 컬럼은 빈 목록/빈 텍스트로 처리하며 예외를 던지지 않는다(계약 규칙 3).
        private void RebuildItems()
        {
            foreach (RadioGroupItem existing in this.radioItems)
            {
                existing.PropertyChanged -= this.OnItemPropertyChanged;
            }

            this.radioItems.Clear();

            IEnumerable source = this.ItemsSource;

            if (source != null)
            {
                foreach (object row in source)
                {
                    object value = string.IsNullOrEmpty(this.ValueMemberPath)
                        ? row
                        : MemberPathReader.Read(row, this.ValueMemberPath);

                    string displayText = MemberPathReader.ReadDisplayText(row, this.DisplayMemberPath);

                    RadioGroupItem item = new RadioGroupItem(value, displayText);
                    item.PropertyChanged += this.OnItemPropertyChanged;
                    this.radioItems.Add(item);
                }
            }

            // 보류/기존 SelectedValue 적용. 새 목록에 없는 값이면 미선택으로 정리.
            this.ApplySelectedValueToItems();

            if (this.SelectedValue != null && !this.HasCheckedItem())
            {
                this.SelectedValue = null;
            }
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RadioGroupItem item = (RadioGroupItem)sender;

            // 체크 해제(다른 항목 선택에 따른 부수 효과)는 무시하고,
            // 새로 체크된 항목만 SelectedValue로 반영한다. 이후 DP 콜백의
            // ApplySelectedValueToItems가 나머지 항목을 해제한다.
            if (!item.IsChecked || this.suppressItemSync)
            {
                return;
            }

            this.SelectedValue = item.Value;

            // 같은 값의 항목을 다시 클릭한 경우(DP 변경 없음)에도
            // 다른 항목이 체크된 채 남지 않도록 상태를 정리한다.
            this.ApplySelectedValueToItems();
        }

        private void ApplySelectedValueToItems()
        {
            object selected = this.SelectedValue;

            this.suppressItemSync = true;

            try
            {
                foreach (RadioGroupItem item in this.radioItems)
                {
                    item.IsChecked = selected != null && Equals(item.Value, selected);
                }
            }
            finally
            {
                this.suppressItemSync = false;
            }
        }

        private bool HasCheckedItem()
        {
            foreach (RadioGroupItem item in this.radioItems)
            {
                if (item.IsChecked)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
