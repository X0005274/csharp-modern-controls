using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Modern.Lab.Controls.Wpf.Common;
using Modern.Lab.Controls.Wpf.Input;

namespace Modern.Lab.Controls.Wpf.Selection
{
    /// <summary>
    /// Modern dropdown selector.
    /// - ItemsSource / DisplayMemberPath / SelectedValuePath: binding surface
    /// - SelectedItem / SelectedValue: current selection (two-way)
    /// - IsEditable: search-style combo — typing filters the list
    ///   (Korean initial-consonant matching included)
    /// - Placeholder: hint shown while nothing is selected/typed
    /// - SelectionChanged: raised when the selection changes
    ///
    /// The inner ComboBox is bound to an internally filtered snapshot of
    /// ItemsSource so the editable mode can filter while typing; source
    /// collection changes (ObservableCollection / IBindingList) are observed
    /// and re-applied.
    /// </summary>
    public partial class ModernComboBoxControl : UserControl
    {
        /// <summary>Items to display. Any IEnumerable (DataView, IList, ...).</summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(ModernComboBoxControl),
                new PropertyMetadata(null, OnItemsSourceChanged));

        /// <summary>Currently selected item. Two-way by default.</summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem",
                typeof(object),
                typeof(ModernComboBoxControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>Value of the selected item (via SelectedValuePath). Two-way by default.</summary>
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register(
                "SelectedValue",
                typeof(object),
                typeof(ModernComboBoxControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>Member path used for the display text of each item.</summary>
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(
                "DisplayMemberPath",
                typeof(string),
                typeof(ModernComboBoxControl),
                new PropertyMetadata(string.Empty));

        /// <summary>Member path used for SelectedValue.</summary>
        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register(
                "SelectedValuePath",
                typeof(string),
                typeof(ModernComboBoxControl),
                new PropertyMetadata(string.Empty));

        /// <summary>Hint text shown while nothing is selected.</summary>
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(
                "Placeholder",
                typeof(string),
                typeof(ModernComboBoxControl),
                new PropertyMetadata(string.Empty));

        /// <summary>Search-style combo: typing filters the list.</summary>
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register(
                "IsEditable",
                typeof(bool),
                typeof(ModernComboBoxControl),
                new PropertyMetadata(false, OnIsEditableChanged));

        private readonly ObservableCollection<object> filteredItems;
        private TextBox editableTextBox;
        private bool isRebuildingItems;
        private bool suppressFilter;

        /// <summary>Raised when the selection changes.</summary>
        public event EventHandler SelectionChanged;

        public ModernComboBoxControl()
        {
            this.filteredItems = new ObservableCollection<object>();
            this.InitializeComponent();
            this.InnerComboBox.ItemsSource = this.filteredItems;
            this.InnerComboBox.DropDownClosed += this.OnDropDownClosed;
            this.UpdatePlaceholderVisibility();
        }

        /// <summary>Items to display.</summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>Currently selected item.</summary>
        public object SelectedItem
        {
            get { return this.GetValue(SelectedItemProperty); }
            set { this.SetValue(SelectedItemProperty, value); }
        }

        /// <summary>Value of the selected item (via SelectedValuePath).</summary>
        public object SelectedValue
        {
            get { return this.GetValue(SelectedValueProperty); }
            set { this.SetValue(SelectedValueProperty, value); }
        }

        /// <summary>Member path used for the display text of each item.</summary>
        public string DisplayMemberPath
        {
            get { return (string)this.GetValue(DisplayMemberPathProperty); }
            set { this.SetValue(DisplayMemberPathProperty, value); }
        }

        /// <summary>Member path used for SelectedValue.</summary>
        public string SelectedValuePath
        {
            get { return (string)this.GetValue(SelectedValuePathProperty); }
            set { this.SetValue(SelectedValuePathProperty, value); }
        }

        /// <summary>Hint text shown while nothing is selected.</summary>
        public string Placeholder
        {
            get { return (string)this.GetValue(PlaceholderProperty); }
            set { this.SetValue(PlaceholderProperty, value); }
        }

        /// <summary>Search-style combo: typing filters the list.</summary>
        public bool IsEditable
        {
            get { return (bool)this.GetValue(IsEditableProperty); }
            set { this.SetValue(IsEditableProperty, value); }
        }

        /// <summary>Index of the selected item (-1 when nothing is selected).</summary>
        public int SelectedIndex
        {
            get { return this.InnerComboBox.SelectedIndex; }
            set { this.InnerComboBox.SelectedIndex = value; }
        }

        /// <summary>Display text of the current selection (inner ComboBox text).</summary>
        public string SelectionText
        {
            get { return this.InnerComboBox.Text; }
        }

        /// <summary>Sets the editable text (search-style combo only).</summary>
        public void SetEditableText(string value)
        {
            if (this.IsEditable)
            {
                this.InnerComboBox.Text = value ?? string.Empty;
            }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernComboBoxControl control = (ModernComboBoxControl)d;

            control.DetachSourceListeners(e.OldValue);
            control.AttachSourceListeners(e.NewValue);
            control.RebuildFilteredItems(null);
        }

        private static void OnIsEditableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernComboBoxControl)d).UpdatePlaceholderVisibility();
        }

        // Observes source collection changes (ObservableCollection for the
        // manual Items collection, IBindingList for DataView) so late adds show
        // up like they did with a direct ItemsSource binding.
        private void AttachSourceListeners(object source)
        {
            INotifyCollectionChanged observable = source as INotifyCollectionChanged;

            if (observable != null)
            {
                observable.CollectionChanged += this.OnSourceCollectionChanged;
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
                observable.CollectionChanged -= this.OnSourceCollectionChanged;
                return;
            }

            IBindingList bindingList = source as IBindingList;

            if (bindingList != null)
            {
                bindingList.ListChanged -= this.OnSourceListChanged;
            }
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.RebuildFilteredItems(null);
        }

        private void OnSourceListChanged(object sender, ListChangedEventArgs e)
        {
            this.RebuildFilteredItems(null);
        }

        // Rebuilds the inner list from ItemsSource, keeping only entries whose
        // display text matches the filter (Korean-aware). Preserves selection
        // and typed text; suppresses SelectionChanged noise while rebuilding.
        private void RebuildFilteredItems(string filterText)
        {
            this.isRebuildingItems = true;

            try
            {
                object previousSelection = this.InnerComboBox.SelectedItem;
                string previousEditorText = this.editableTextBox != null ? this.editableTextBox.Text : null;
                int previousCaret = this.editableTextBox != null ? this.editableTextBox.CaretIndex : 0;

                this.filteredItems.Clear();

                IEnumerable source = this.ItemsSource;

                if (source != null)
                {
                    foreach (object item in source)
                    {
                        if (string.IsNullOrEmpty(filterText) ||
                            HangulTextMatcher.Contains(
                                MemberPathReader.ReadDisplayText(item, this.DisplayMemberPath), filterText))
                        {
                            this.filteredItems.Add(item);
                        }
                    }
                }

                if (previousSelection != null && this.filteredItems.Contains(previousSelection))
                {
                    this.InnerComboBox.SelectedItem = previousSelection;
                }

                // Clearing/restoring the selection rewrites the editable text;
                // put the user's typed text (and caret) back. The synchronous
                // TextChanged this raises is ignored via isRebuildingItems.
                if (this.editableTextBox != null && previousEditorText != null &&
                    !string.Equals(this.editableTextBox.Text, previousEditorText, StringComparison.Ordinal))
                {
                    this.editableTextBox.Text = previousEditorText;
                    this.editableTextBox.CaretIndex = Math.Min(previousCaret, previousEditorText.Length);
                }
            }
            finally
            {
                this.isRebuildingItems = false;
            }

            this.UpdatePlaceholderVisibility();
        }

        // Grabs the editable text area from the template and hooks typing.
        // Loaded fires after the host form has already bound data, so the
        // placeholder is re-evaluated here against the real editor state.
        private void InnerComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox editor = this.InnerComboBox.Template.FindName("PART_EditableTextBox", this.InnerComboBox) as TextBox;

            if (!object.ReferenceEquals(editor, this.editableTextBox))
            {
                if (this.editableTextBox != null)
                {
                    this.editableTextBox.TextChanged -= this.OnEditableTextChanged;
                }

                this.editableTextBox = editor;

                if (this.editableTextBox != null)
                {
                    this.editableTextBox.TextChanged += this.OnEditableTextChanged;
                }
            }

            this.UpdatePlaceholderVisibility();
        }

        // Fires during IME composition too, so the first consonant already
        // filters the list and hides the placeholder.
        private void OnEditableTextChanged(object sender, TextChangedEventArgs e)
        {
            this.UpdatePlaceholderVisibility();

            if (!this.IsEditable || this.isRebuildingItems)
            {
                return;
            }

            if (this.suppressFilter)
            {
                this.suppressFilter = false;
                return;
            }

            if (!this.InnerComboBox.IsKeyboardFocusWithin)
            {
                return;
            }

            string typed = this.editableTextBox.Text;

            if (string.IsNullOrEmpty(typed))
            {
                // Clearing the text clears the selection (empty means "all").
                if (this.InnerComboBox.SelectedItem != null)
                {
                    this.InnerComboBox.SelectedItem = null;
                    // The selection change sets suppressFilter expecting a text
                    // rewrite, but the text is already empty — clear the flag.
                    this.suppressFilter = false;
                }

                this.RebuildFilteredItems(null);
                this.InnerComboBox.IsDropDownOpen = false;
                return;
            }

            this.RebuildFilteredItems(typed);
            this.InnerComboBox.IsDropDownOpen = this.filteredItems.Count > 0;
        }

        // Reopening the dropdown from the chevron should show the full list.
        // Also clears a stale suppress flag (a selection that did not change
        // the text would otherwise eat the next keystroke's filtering).
        private void OnDropDownClosed(object sender, EventArgs e)
        {
            if (this.IsEditable)
            {
                this.RebuildFilteredItems(null);
                this.suppressFilter = false;
            }
        }

        private void UpdatePlaceholderVisibility()
        {
            bool empty;

            if (this.IsEditable)
            {
                string editorText = this.editableTextBox != null ? this.editableTextBox.Text : this.InnerComboBox.Text;
                empty = string.IsNullOrEmpty(editorText);
            }
            else
            {
                empty = this.InnerComboBox.SelectedItem == null;
            }

            this.PlaceholderOverlay.Visibility = empty ? Visibility.Visible : Visibility.Collapsed;
        }

        private void InnerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.isRebuildingItems)
            {
                return;
            }

            // Committing a selection rewrites the editable text with the item's
            // display text; that change must not re-trigger filtering.
            if (this.IsEditable)
            {
                this.suppressFilter = true;
            }

            this.UpdatePlaceholderVisibility();

            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, EventArgs.Empty);
            }
        }
    }
}
