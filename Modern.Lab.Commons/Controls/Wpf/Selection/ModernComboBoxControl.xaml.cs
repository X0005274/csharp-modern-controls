using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Modern.Lab.Controls.Wpf.Selection
{
    /// <summary>
    /// Modern dropdown selector.
    /// - ItemsSource / DisplayMemberPath / SelectedValuePath: binding surface
    /// - SelectedItem / SelectedValue: current selection (two-way)
    /// - Placeholder: hint shown while nothing is selected
    /// - SelectionChanged: raised when the selection changes
    /// </summary>
    public partial class ModernComboBoxControl : UserControl
    {
        /// <summary>Items to display. Any IEnumerable (DataView, IList, ...).</summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(ModernComboBoxControl),
                new PropertyMetadata(null));

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

        /// <summary>Raised when the selection changes.</summary>
        public event EventHandler SelectionChanged;

        public ModernComboBoxControl()
        {
            this.InitializeComponent();
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

        private void InnerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, EventArgs.Empty);
            }
        }
    }
}
