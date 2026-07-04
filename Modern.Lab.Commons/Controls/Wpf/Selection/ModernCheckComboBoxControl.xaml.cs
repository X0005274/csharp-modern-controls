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
    /// Multi-select dropdown with checkbox items.
    /// - ItemsSource / DisplayMemberPath / ValueMemberPath: binding surface
    /// - GetCheckedValues / ApplyCheckedValues: check state by value
    /// - Placeholder: hint shown while nothing is checked
    /// - CheckedChanged: raised when any check state changes
    /// The field shows the checked items' display texts joined with ", ".
    /// </summary>
    public partial class ModernCheckComboBoxControl : UserControl
    {
        /// <summary>Items to display. Any IEnumerable (DataView, IList, ...).</summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(ModernCheckComboBoxControl),
                new PropertyMetadata(null, OnItemsSourceChanged));

        /// <summary>Member path used for the display text of each item.</summary>
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(
                "DisplayMemberPath",
                typeof(string),
                typeof(ModernCheckComboBoxControl),
                new PropertyMetadata(string.Empty, OnDisplayShapeChanged));

        /// <summary>Member path used for the checked values.</summary>
        public static readonly DependencyProperty ValueMemberPathProperty =
            DependencyProperty.Register(
                "ValueMemberPath",
                typeof(string),
                typeof(ModernCheckComboBoxControl),
                new PropertyMetadata(string.Empty));

        /// <summary>Hint text shown while nothing is checked.</summary>
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(
                "Placeholder",
                typeof(string),
                typeof(ModernCheckComboBoxControl),
                new PropertyMetadata(string.Empty));

        private readonly ObservableCollection<CheckComboItem> checkItems;
        private bool suppressCheckedChanged;
        private bool suppressReopen;

        /// <summary>Raised when any item's check state changes.</summary>
        public event EventHandler CheckedChanged;

        public ModernCheckComboBoxControl()
        {
            this.checkItems = new ObservableCollection<CheckComboItem>();
            this.InitializeComponent();
            this.CheckItemsControl.ItemsSource = this.checkItems;
            this.UpdateDisplay();
        }

        /// <summary>Items to display.</summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>Member path used for the display text of each item.</summary>
        public string DisplayMemberPath
        {
            get { return (string)this.GetValue(DisplayMemberPathProperty); }
            set { this.SetValue(DisplayMemberPathProperty, value); }
        }

        /// <summary>Member path used for the checked values.</summary>
        public string ValueMemberPath
        {
            get { return (string)this.GetValue(ValueMemberPathProperty); }
            set { this.SetValue(ValueMemberPathProperty, value); }
        }

        /// <summary>Hint text shown while nothing is checked.</summary>
        public string Placeholder
        {
            get { return (string)this.GetValue(PlaceholderProperty); }
            set { this.SetValue(PlaceholderProperty, value); }
        }

        /// <summary>Values (via ValueMemberPath) of the checked items, in list order.</summary>
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

        /// <summary>Source rows of the checked items, in list order.</summary>
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
        /// Checks exactly the items whose value occurs in the given list
        /// (null or empty clears every check). Raises CheckedChanged once.
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

        // Rebuilds the checkbox rows from ItemsSource. Check states reset —
        // reassigning the source resets the selection cleanly (contract rule 3);
        // callers re-apply values afterwards when needed.
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
        }

        private void RaiseCheckedChanged()
        {
            if (this.CheckedChanged != null)
            {
                this.CheckedChanged(this, EventArgs.Empty);
            }
        }

        // StaysOpen=False dismissal and the toggle's own click race on the same
        // mouse-down: without this guard, clicking the field while the popup is
        // open closes and instantly reopens it. The flag lives only for the
        // current input cycle so it can never eat a later, unrelated click.
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
