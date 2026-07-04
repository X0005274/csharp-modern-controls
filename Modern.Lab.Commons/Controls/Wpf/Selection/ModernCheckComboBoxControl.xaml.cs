using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Selection
{
    /// <summary>
    /// 체크박스 항목을 가진 다중 선택 드롭다운.
    /// - ItemsSource / DisplayMemberPath / ValueMemberPath: 바인딩 표면
    /// - GetCheckedValues / ApplyCheckedValues: 값 기준 체크 상태 조회/적용
    /// - ItemStyle: 항목 표시 스타일 (모던 체크박스 / 온오프 스위치)
    /// - Placeholder: 체크된 항목이 없을 때 표시할 힌트
    /// - CheckedChanged: 체크 상태가 바뀔 때 발생
    /// 필드에는 체크된 항목들의 표시 텍스트가 ", "로 연결되어 나타나고,
    /// 드롭다운 상단의 "전체 선택" 헤더로 일괄 체크/해제할 수 있다.
    /// </summary>
    public partial class ModernCheckComboBoxControl : UserControl
    {
        /// <summary>표시할 항목. 어떤 IEnumerable이든 가능 (DataView, IList, ...).</summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(ModernCheckComboBoxControl),
                new PropertyMetadata(null, OnItemsSourceChanged));

        /// <summary>각 항목의 표시 텍스트로 사용할 멤버 경로.</summary>
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(
                "DisplayMemberPath",
                typeof(string),
                typeof(ModernCheckComboBoxControl),
                new PropertyMetadata(string.Empty, OnDisplayShapeChanged));

        /// <summary>체크 값으로 사용할 멤버 경로.</summary>
        public static readonly DependencyProperty ValueMemberPathProperty =
            DependencyProperty.Register(
                "ValueMemberPath",
                typeof(string),
                typeof(ModernCheckComboBoxControl),
                new PropertyMetadata(string.Empty));

        /// <summary>체크된 항목이 없을 때 표시할 힌트 텍스트.</summary>
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(
                "Placeholder",
                typeof(string),
                typeof(ModernCheckComboBoxControl),
                new PropertyMetadata(string.Empty));

        /// <summary>항목 표시 스타일 (체크박스/스위치). 기본은 체크박스.</summary>
        public static readonly DependencyProperty ItemStyleProperty =
            DependencyProperty.Register(
                "ItemStyle",
                typeof(CheckItemStyle),
                typeof(ModernCheckComboBoxControl),
                new PropertyMetadata(CheckItemStyle.CheckBox, OnItemStyleChanged));

        private readonly ObservableCollection<CheckComboItem> checkItems;
        private bool suppressCheckedChanged;
        private bool suppressReopen;

        /// <summary>어느 항목이든 체크 상태가 바뀔 때 발생.</summary>
        public event EventHandler CheckedChanged;

        public ModernCheckComboBoxControl()
        {
            this.checkItems = new ObservableCollection<CheckComboItem>();
            this.InitializeComponent();
            this.CheckItemsControl.ItemsSource = this.checkItems;
            this.ApplyItemTemplate();
            this.UpdateDisplay();
        }

        /// <summary>표시할 항목.</summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>각 항목의 표시 텍스트로 사용할 멤버 경로.</summary>
        public string DisplayMemberPath
        {
            get { return (string)this.GetValue(DisplayMemberPathProperty); }
            set { this.SetValue(DisplayMemberPathProperty, value); }
        }

        /// <summary>체크 값으로 사용할 멤버 경로.</summary>
        public string ValueMemberPath
        {
            get { return (string)this.GetValue(ValueMemberPathProperty); }
            set { this.SetValue(ValueMemberPathProperty, value); }
        }

        /// <summary>체크된 항목이 없을 때 표시할 힌트 텍스트.</summary>
        public string Placeholder
        {
            get { return (string)this.GetValue(PlaceholderProperty); }
            set { this.SetValue(PlaceholderProperty, value); }
        }

        /// <summary>항목 표시 스타일 (체크박스/스위치).</summary>
        public CheckItemStyle ItemStyle
        {
            get { return (CheckItemStyle)this.GetValue(ItemStyleProperty); }
            set { this.SetValue(ItemStyleProperty, value); }
        }

        /// <summary>체크된 항목들의 값(ValueMemberPath 기준)을 목록 순서대로 반환.</summary>
        public List<object> GetCheckedValues()
        {
            List<object> values = new List<object>();

            foreach (CheckComboItem item in this.checkItems)
            {
                if (item.IsChecked)
                {
                    values.Add(this.ReadValue(item.Item));
                }
            }

            return values;
        }

        /// <summary>체크된 항목들의 원본 행을 목록 순서대로 반환.</summary>
        public List<object> GetCheckedItems()
        {
            List<object> items = new List<object>();

            foreach (CheckComboItem item in this.checkItems)
            {
                if (item.IsChecked)
                {
                    items.Add(item.Item);
                }
            }

            return items;
        }

        /// <summary>
        /// 주어진 값 목록에 있는 항목만 정확히 체크한다 (null/빈 목록 = 전체 해제).
        /// CheckedChanged는 한 번만 발생한다.
        /// </summary>
        public void ApplyCheckedValues(IEnumerable values)
        {
            List<object> wanted = new List<object>();

            if (values != null)
            {
                foreach (object value in values)
                {
                    wanted.Add(value);
                }
            }

            this.suppressCheckedChanged = true;

            try
            {
                foreach (CheckComboItem item in this.checkItems)
                {
                    object itemValue = this.ReadValue(item.Item);
                    bool shouldCheck = false;

                    foreach (object wantedValue in wanted)
                    {
                        if (object.Equals(itemValue, wantedValue))
                        {
                            shouldCheck = true;
                            break;
                        }
                    }

                    item.IsChecked = shouldCheck;
                }
            }
            finally
            {
                this.suppressCheckedChanged = false;
            }

            this.UpdateDisplay();
            this.RaiseCheckedChanged();
        }

        /// <summary>모든 항목을 체크한다. CheckedChanged는 한 번만 발생.</summary>
        public void CheckAll()
        {
            this.SetAll(true);
        }

        /// <summary>모든 항목의 체크를 해제한다. CheckedChanged는 한 번만 발생.</summary>
        public void UncheckAll()
        {
            this.SetAll(false);
        }

        private void SetAll(bool value)
        {
            this.suppressCheckedChanged = true;

            try
            {
                foreach (CheckComboItem item in this.checkItems)
                {
                    item.IsChecked = value;
                }
            }
            finally
            {
                this.suppressCheckedChanged = false;
            }

            this.UpdateDisplay();
            this.RaiseCheckedChanged();
        }

        private object ReadValue(object row)
        {
            if (string.IsNullOrEmpty(this.ValueMemberPath))
            {
                return row;
            }

            return MemberPathReader.Read(row, this.ValueMemberPath);
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernCheckComboBoxControl control = (ModernCheckComboBoxControl)d;

            control.DetachSourceListeners(e.OldValue);
            control.AttachSourceListeners(e.NewValue);
            control.RebuildItems();
        }

        private static void OnDisplayShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernCheckComboBoxControl)d).RebuildItems();
        }

        private static void OnItemStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernCheckComboBoxControl)d).ApplyItemTemplate();
        }

        // ItemStyle에 해당하는 항목 템플릿을 적용한다.
        private void ApplyItemTemplate()
        {
            string key = this.ItemStyle == CheckItemStyle.Switch ? "SwitchItemTemplate" : "CheckBoxItemTemplate";
            this.CheckItemsControl.ItemTemplate = (DataTemplate)this.Resources[key];
        }

        // 원본 컬렉션의 변경을 관찰해(수동 컬렉션은 ObservableCollection, DataView는
        // IBindingList) 직접 바인딩했을 때처럼 늦은 추가도 반영되게 한다.
        private void AttachSourceListeners(object source)
        {
            INotifyCollectionChanged observable = source as INotifyCollectionChanged;

            if (observable != null)
            {
                observable.CollectionChanged += this.OnSourceChanged;
                return;
            }

            IBindingList bindingList = source as IBindingList;

            if (bindingList != null)
            {
                bindingList.ListChanged += this.OnSourceListChanged;
            }
        }

        private void DetachSourceListeners(object source)
        {
            INotifyCollectionChanged observable = source as INotifyCollectionChanged;

            if (observable != null)
            {
                observable.CollectionChanged -= this.OnSourceChanged;
                return;
            }

            IBindingList bindingList = source as IBindingList;

            if (bindingList != null)
            {
                bindingList.ListChanged -= this.OnSourceListChanged;
            }
        }

        private void OnSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.RebuildItems();
        }

        private void OnSourceListChanged(object sender, ListChangedEventArgs e)
        {
            this.RebuildItems();
        }

        // ItemsSource로부터 체크박스 행을 다시 만든다. 체크 상태는 초기화된다 —
        // 소스 재할당은 선택을 깨끗하게 리셋한다는 계약(룰 3)에 따르며, 필요한
        // 호출자는 이후 값을 다시 적용한다.
        private void RebuildItems()
        {
            this.suppressCheckedChanged = true;

            try
            {
                foreach (CheckComboItem item in this.checkItems)
                {
                    item.PropertyChanged -= this.OnItemPropertyChanged;
                }

                this.checkItems.Clear();

                IEnumerable source = this.ItemsSource;

                if (source != null)
                {
                    foreach (object row in source)
                    {
                        CheckComboItem item = new CheckComboItem(
                            row, MemberPathReader.ReadDisplayText(row, this.DisplayMemberPath));
                        item.PropertyChanged += this.OnItemPropertyChanged;
                        this.checkItems.Add(item);
                    }
                }
            }
            finally
            {
                this.suppressCheckedChanged = false;
            }

            this.UpdateDisplay();
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
            {
                this.UpdateDisplay();

                if (!this.suppressCheckedChanged)
                {
                    this.RaiseCheckedChanged();
                }
            }
        }

        // 필드 표시 텍스트, 플레이스홀더, 전체 선택 헤더 상태를 갱신한다.
        private void UpdateDisplay()
        {
            List<string> texts = new List<string>();

            foreach (CheckComboItem item in this.checkItems)
            {
                if (item.IsChecked)
                {
                    texts.Add(item.DisplayText);
                }
            }

            string joined = string.Join(", ", texts.ToArray());

            this.DisplayText.Text = joined;
            this.PlaceholderOverlay.Visibility = joined.Length == 0 ? Visibility.Visible : Visibility.Collapsed;

            // 전체 선택 헤더: 전부 = 체크, 없음 = 해제, 일부 = 부분 선택(대시).
            // 라벨도 다음 동작을 안내하도록 바뀐다 — 전부 체크 상태에서는
            // "전체 해제", 그 외에는 "전체 선택".
            if (this.SelectAllCheck != null)
            {
                if (this.checkItems.Count == 0 || texts.Count == 0)
                {
                    this.SelectAllCheck.IsChecked = false;
                    this.SelectAllCheck.Content = "전체 선택";
                }
                else if (texts.Count == this.checkItems.Count)
                {
                    this.SelectAllCheck.IsChecked = true;
                    this.SelectAllCheck.Content = "전체 해제";
                }
                else
                {
                    this.SelectAllCheck.IsChecked = null;
                    this.SelectAllCheck.Content = "전체 선택";
                }
            }
        }

        private void RaiseCheckedChanged()
        {
            if (this.CheckedChanged != null)
            {
                this.CheckedChanged(this, EventArgs.Empty);
            }
        }

        // 전체 선택 헤더 클릭: 체크박스 자체의 토글 결과와 무관하게, 현재 항목
        // 상태 기준으로 결정한다 — 전부 체크되어 있으면 전체 해제, 아니면 전체 체크.
        private void SelectAllCheck_Click(object sender, RoutedEventArgs e)
        {
            bool allChecked = this.checkItems.Count > 0;

            foreach (CheckComboItem item in this.checkItems)
            {
                if (!item.IsChecked)
                {
                    allChecked = false;
                    break;
                }
            }

            if (allChecked)
            {
                this.UncheckAll();
            }
            else
            {
                this.CheckAll();
            }
        }

        // StaysOpen=False의 팝업 해제와 토글 자신의 클릭이 같은 마우스 다운에서
        // 경합한다: 이 가드가 없으면 팝업이 열린 상태에서 필드를 클릭할 때 닫혔다가
        // 즉시 다시 열린다. 플래그는 현재 입력 사이클 동안만 유지되어 이후의
        // 무관한 클릭을 삼키지 않는다.
        private void ItemsPopup_Closed(object sender, EventArgs e)
        {
            if (this.DropToggle.IsMouseOver)
            {
                this.suppressReopen = true;
                this.Dispatcher.BeginInvoke(
                    new Action(delegate { this.suppressReopen = false; }),
                    System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void DropToggle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.suppressReopen)
            {
                this.suppressReopen = false;
                e.Handled = true;
            }
        }
    }
}
