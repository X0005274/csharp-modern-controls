using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Selection
{
    /// <summary>
    /// Multi-select dropdown with checkbox items (no direct WinForms
    /// counterpart; WPF ModernCheckComboBoxControl hosted through ElementHost).
    ///
    /// Follows the ComboBox data contract naming: DataSource
    /// (DataTable/DataView/IList/IEnumerable), DisplayMember, ValueMember.
    /// CheckedValues replaces SelectedValue and may be assigned before
    /// DataSource (pended and applied when the data arrives — contract rule 3).
    /// The field shows the checked items joined with ", "; nothing checked
    /// shows the placeholder (typically meaning "all").
    /// </summary>
    [ToolboxItem(true)]
    public class ModernCheckComboBox : WpfElementHostBase<Modern.Lab.Controls.Wpf.Selection.ModernCheckComboBoxControl>
    {
        private object dataSource;
        private object[] pendingCheckedValues;
        private bool hasPendingCheckedValues;
        private bool suppressCheckedChanged;

        // Fallback storage so the property grid still works when design-time
        // WPF construction failed (Wpf == null).
        private string fallbackDisplayMember;
        private string fallbackValueMember;
        private string fallbackPlaceholder;

        /// <summary>Raised when any item's check state changes.</summary>
        public event EventHandler CheckedChanged;

        /// <summary>Creates the control with a sensible default size.</summary>
        public ModernCheckComboBox()
        {
            this.Size = new Size(200, 32);
            this.fallbackDisplayMember = string.Empty;
            this.fallbackValueMember = string.Empty;
            this.fallbackPlaceholder = string.Empty;

            if (this.Wpf != null)
            {
                this.Wpf.CheckedChanged += this.OnWpfCheckedChanged;
            }
        }

        /// <summary>
        /// Data source: DataTable, DataView, IList or any IEnumerable. Assigning
        /// resets every check, applies pending CheckedValues if any, and raises
        /// CheckedChanged exactly once. Null clears the list.
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

                this.suppressCheckedChanged = true;

                try
                {
                    this.Wpf.ItemsSource = DataSourceConverter.ToItemsSource(value);

                    if (this.hasPendingCheckedValues)
                    {
                        this.Wpf.ApplyCheckedValues(this.pendingCheckedValues);
                        this.pendingCheckedValues = null;
                        this.hasPendingCheckedValues = false;
                    }
                }
                finally
                {
                    this.suppressCheckedChanged = false;
                }

                this.RaiseCheckedChanged();
            }
        }

        /// <summary>Column/property name used as the display text (WinForms-compatible name).</summary>
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

        /// <summary>Column/property name used as the checked values (WinForms-compatible name).</summary>
        [Category("모던 컨트롤")]
        [Description("CheckedValues로 사용할 컬럼/속성 이름")]
        [DefaultValue("")]
        public string ValueMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.ValueMemberPath;
                }

                return this.fallbackValueMember;
            }
            set
            {
                this.fallbackValueMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.ValueMemberPath = value;
                }
            }
        }

        /// <summary>Hint text shown while nothing is checked (same name as ModernTextBox).</summary>
        [Category("모던 컨트롤")]
        [Description("체크된 항목이 없을 때 표시할 힌트 텍스트")]
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
        /// Values of the checked items (via ValueMember). May be assigned before
        /// DataSource (pended and applied when the data arrives). Null or an
        /// empty array clears every check.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object[] CheckedValues
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.GetCheckedValues().ToArray();
                }

                return new object[0];
            }
            set
            {
                if (this.Wpf == null)
                {
                    return;
                }

                if (this.HasBoundItems())
                {
                    this.Wpf.ApplyCheckedValues(value);
                }
                else
                {
                    this.pendingCheckedValues = value;
                    this.hasPendingCheckedValues = true;
                }
            }
        }

        /// <summary>Source rows of the checked items (DataRowView for DataTable sources).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object[] CheckedItems
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.GetCheckedItems().ToArray();
                }

                return new object[0];
            }
        }

        /// <summary>Display text of the checked items joined with ", " (read-only).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get
            {
                if (this.Wpf != null)
                {
                    List<string> texts = new List<string>();

                    foreach (object item in this.Wpf.GetCheckedItems())
                    {
                        texts.Add(Modern.Lab.Controls.Wpf.Common.MemberPathReader.ReadDisplayText(item, this.DisplayMember));
                    }

                    return string.Join(", ", texts.ToArray());
                }

                return string.Empty;
            }
            set
            {
                // Check-by-text is not supported; use CheckedValues instead.
            }
        }

        private bool HasBoundItems()
        {
            return this.Wpf != null && this.Wpf.ItemsSource != null;
        }

        private void OnWpfCheckedChanged(object sender, EventArgs e)
        {
            if (!this.suppressCheckedChanged)
            {
                this.RaiseCheckedChanged();
            }
        }

        private void RaiseCheckedChanged()
        {
            if (this.CheckedChanged != null)
            {
                this.CheckedChanged(this, EventArgs.Empty);
            }
        }
    }
}
