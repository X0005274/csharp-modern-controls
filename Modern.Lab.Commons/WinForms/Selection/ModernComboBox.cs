using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Selection
{
    /// <summary>
    /// Drop-in replacement for System.Windows.Forms.ComboBox
    /// (WPF ModernComboBoxControl hosted through ElementHost).
    ///
    /// Compatible members: DataSource (DataTable/DataView/IList/IEnumerable),
    /// DisplayMember, ValueMember, SelectedValue, SelectedItem, SelectedIndex,
    /// Items, SelectedIndexChanged, Enabled, DropDownStyle (DropDownList =
    /// select-only; DropDown/Simple = search-style combo where typing filters
    /// the list with Korean initial-consonant matching).
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
        private string fallbackPlaceholder;
        private ComboBoxStyle fallbackDropDownStyle;

        /// <summary>Raised when the selection changes (WinForms-compatible name).</summary>
        public event EventHandler SelectedIndexChanged;

        /// <summary>Creates the control with a sensible default size.</summary>
        public ModernComboBox()
        {
            this.Size = new Size(200, 32);
            this.manualItems = new ObservableCollection<object>();
            this.fallbackDisplayMember = string.Empty;
            this.fallbackValueMember = string.Empty;
            this.fallbackPlaceholder = string.Empty;

            // Default matches System.Windows.Forms.ComboBox: DropDown (editable).
            // Typing filters the list; clearing the text clears the selection.
            this.fallbackDropDownStyle = ComboBoxStyle.DropDown;

            if (this.Wpf != null)
            {
                this.Wpf.ItemsSource = this.manualItems;
                this.Wpf.IsEditable = true;
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
                    this.Wpf.ItemsSource = DataSourceConverter.ToItemsSource(value) ?? this.manualItems;

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

        /// <summary>
        /// Hint text shown while nothing is selected/typed. Same property name
        /// as ModernTextBox for a consistent API.
        /// </summary>
        [Category("모던 컨트롤")]
        [Description("미선택/미입력 상태에서 표시할 힌트 텍스트")]
        [Localizable(true)]
        [DefaultValue("")]
        public string PlaceholderText
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Placeholder;
                }

                return this.fallbackPlaceholder;
            }
            set
            {
                this.fallbackPlaceholder = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Placeholder = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>
        /// Selection style (WinForms-compatible name and default). DropDown
        /// (default, editable) filters the bound list while typing (초성 검색
        /// included) and clearing the text clears the selection; DropDownList
        /// is select-only; Simple behaves as DropDown.
        /// </summary>
        [Category("모던 컨트롤")]
        [Description("DropDown(기본)/Simple=입력으로 목록 필터링(검색형), DropDownList=선택 전용")]
        [DefaultValue(ComboBoxStyle.DropDown)]
        public ComboBoxStyle DropDownStyle
        {
            get
            {
                return this.fallbackDropDownStyle;
            }
            set
            {
                this.fallbackDropDownStyle = value;

                if (this.Wpf != null)
                {
                    this.Wpf.IsEditable = value != ComboBoxStyle.DropDownList;
                }

                this.InvalidateDesignTimePreview();
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

        /// <summary>
        /// Display text of the current selection (WinForms ComboBox.Text). The
        /// setter writes the editable text in DropDown/Simple style; in
        /// DropDownList it is a no-op — select via SelectedValue/SelectedIndex.
        /// </summary>
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
                if (this.Wpf != null)
                {
                    this.Wpf.SetEditableText(value);
                }
            }
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
