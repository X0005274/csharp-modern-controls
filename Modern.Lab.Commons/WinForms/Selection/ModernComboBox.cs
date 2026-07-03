using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Selection
{
    /// <summary>
    /// Drop-in replacement for System.Windows.Forms.ComboBox
    /// (WPF ModernComboBoxControl hosted through ElementHost).
    ///
    /// Compatible members: DataSource (DataTable/DataView/IList/IEnumerable),
    /// DisplayMember, ValueMember, SelectedValue, SelectedItem, SelectedIndex,
    /// Items, SelectedIndexChanged, Enabled.
    ///
    /// Contract behaviors (docs/design-notes.md section 6-1):
    /// - SelectedValue may be assigned before DataSource; the value is pended
    ///   and applied when the data arrives (rule 3).
    /// - Assigning DataSource selects the first row when no pending value exists,
    ///   matching the WinForms ComboBox behavior, and raises SelectedIndexChanged
    ///   exactly once per assignment.
    /// - Null/empty data renders as an empty list, never throws.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernComboBox : WpfElementHostBase<Modern.Lab.Controls.Wpf.Selection.ModernComboBoxControl>
    {
        // Items collection used when no DataSource is assigned (combo.Items.Add(...)).
        private readonly ObservableCollection<object> manualItems;

        private object dataSource;
        private object pendingSelectedValue;
        private bool hasPendingSelectedValue;
        private bool suppressSelectionChanged;

        // Fallback storage so the property grid still works when design-time
        // WPF construction failed (Wpf == null).
        private string fallbackDisplayMember;
        private string fallbackValueMember;

        /// <summary>Raised when the selection changes (WinForms-compatible name).</summary>
        public event EventHandler SelectedIndexChanged;

        /// <summary>Creates the control with a sensible default size.</summary>
        public ModernComboBox()
        {
            this.Size = new Size(200, 32);
            this.manualItems = new ObservableCollection<object>();
            this.fallbackDisplayMember = string.Empty;
            this.fallbackValueMember = string.Empty;

            if (this.Wpf != null)
            {
                this.Wpf.ItemsSource = this.manualItems;
                this.Wpf.SelectionChanged += this.OnWpfSelectionChanged;
            }
        }

        /// <summary>
        /// Data source: DataTable, DataView, IList or any IEnumerable. Assigning
        /// resets the selection, applies a pending SelectedValue if one exists
        /// (otherwise selects the first row), and raises SelectedIndexChanged once.
        /// Assigning null falls back to the manual Items collection.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object DataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                this.dataSource = value;

                if (this.Wpf == null)
                {
                    return;
                }

                this.suppressSelectionChanged = true;

                try
                {
                    this.Wpf.SelectedItem = null;
                    this.Wpf.ItemsSource = ConvertToItemsSource(value) ?? this.manualItems;

                    if (this.hasPendingSelectedValue)
                    {
                        this.Wpf.SelectedValue = this.pendingSelectedValue;
                        this.pendingSelectedValue = null;
                        this.hasPendingSelectedValue = false;
                    }
                    else if (this.Wpf.SelectedItem == null)
                    {
                        // Match WinForms ComboBox: select the first row by default.
                        this.SelectFirstItemIfAny();
                    }
                }
                finally
                {
                    this.suppressSelectionChanged = false;
                }

                this.RaiseSelectedIndexChanged();
            }
        }

        /// <summary>Column/property name used as the display text (WinForms-compatible).</summary>
        [Category("모던 컨트롤")]
        [Description("표시 텍스트로 사용할 컬럼/속성 이름")]
        [DefaultValue("")]
        public string DisplayMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.DisplayMemberPath;
                }

                return this.fallbackDisplayMember;
            }
            set
            {
                this.fallbackDisplayMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.DisplayMemberPath = value;
                }
            }
        }

        /// <summary>Column/property name used as the value (WinForms-compatible).</summary>
        [Category("모던 컨트롤")]
        [Description("SelectedValue로 사용할 컬럼/속성 이름")]
        [DefaultValue("")]
        public string ValueMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.SelectedValuePath;
                }

                return this.fallbackValueMember;
            }
            set
            {
                this.fallbackValueMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.SelectedValuePath = value;
                }
            }
        }

        /// <summary>
        /// Value of the selected item. May be assigned before DataSource (the
        /// value is pended and applied when the data arrives — contract rule 3).
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedValue
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.SelectedValue;
                }

                return null;
            }
            set
            {
                if (this.Wpf == null)
                {
                    return;
                }

                if (this.HasBoundItems())
                {
                    this.Wpf.SelectedValue = value;
                }
                else
                {
                    this.pendingSelectedValue = value;
                    this.hasPendingSelectedValue = true;
                }
            }
        }

        /// <summary>Currently selected item (DataRowView for DataTable sources).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.SelectedItem;
                }

                return null;
            }
            set
            {
                if (this.Wpf != null)
                {
                    this.Wpf.SelectedItem = value;
                }
            }
        }

        /// <summary>Index of the selected item (-1 when nothing is selected).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.SelectedIndex;
                }

                return -1;
            }
            set
            {
                if (this.Wpf != null)
                {
                    this.Wpf.SelectedIndex = value;
                }
            }
        }

        /// <summary>
        /// Manual items collection (combo.Items.Add(...)). Used only while no
        /// DataSource is assigned; DataSource takes precedence.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList Items
        {
            get { return this.manualItems; }
        }

        /// <summary>Display text of the current selection (WinForms ComboBox.Text).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.SelectionText;
                }

                return string.Empty;
            }
            set
            {
                // Selection-by-text is not supported; select via SelectedValue or
                // SelectedIndex instead. The setter exists only for Control.Text
                // compatibility and is intentionally a no-op.
            }
        }

        // Converts the accepted DataSource shapes into a WPF ItemsSource.
        private static IEnumerable ConvertToItemsSource(object value)
        {
            if (value == null)
            {
                return null;
            }

            DataTable table = value as DataTable;

            if (table != null)
            {
                return table.DefaultView;
            }

            DataView view = value as DataView;

            if (view != null)
            {
                return view;
            }

            IEnumerable enumerable = value as IEnumerable;

            if (enumerable != null)
            {
                return enumerable;
            }

            throw new ArgumentException(
                "DataSource must be a DataTable, DataView, IList or IEnumerable.",
                "value");
        }

        private bool HasBoundItems()
        {
            return this.Wpf != null &&
                   this.Wpf.ItemsSource != null &&
                   !object.ReferenceEquals(this.Wpf.ItemsSource, this.manualItems);
        }

        private void SelectFirstItemIfAny()
        {
            IEnumerator enumerator = this.Wpf.ItemsSource.GetEnumerator();

            if (enumerator.MoveNext())
            {
                this.Wpf.SelectedItem = enumerator.Current;
            }
        }

        private void OnWpfSelectionChanged(object sender, EventArgs e)
        {
            if (!this.suppressSelectionChanged)
            {
                this.RaiseSelectedIndexChanged();
            }
        }

        private void RaiseSelectedIndexChanged()
        {
            if (this.SelectedIndexChanged != null)
            {
                this.SelectedIndexChanged(this, EventArgs.Empty);
            }
        }
    }
}
