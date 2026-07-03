using System.ComponentModel;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// Category/count chip list (new concept, no direct WinForms counterpart;
    /// WPF ModernSummaryListControl hosted through ElementHost).
    ///
    /// Follows the ComboBox data contract naming: DataSource holds the rows,
    /// DisplayMember names the label column, ValueMember names the count column.
    /// Member assignment order does not matter.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernSummaryList : WpfElementHostBase<Modern.Lab.Controls.Wpf.Display.ModernSummaryListControl>
    {
        private object dataSource;

        // Fallback storage so the property grid still works when design-time
        // WPF construction failed (Wpf == null).
        private string fallbackTitle;
        private string fallbackDisplayMember;
        private string fallbackValueMember;

        /// <summary>Creates the control with a sensible default size.</summary>
        public ModernSummaryList()
        {
            this.Size = new Size(320, 88);
            this.fallbackTitle = string.Empty;
            this.fallbackDisplayMember = string.Empty;
            this.fallbackValueMember = string.Empty;
        }

        /// <summary>
        /// Rows to summarize: DataTable, DataView, IList or any IEnumerable with
        /// a label column and a count column. Null clears the chips.
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

                if (this.Wpf != null)
                {
                    this.Wpf.ItemsSource = DataSourceConverter.ToItemsSource(value);
                }
            }
        }

        /// <summary>Column/property name for the chip label (WinForms-compatible name).</summary>
        [Category("모던 컨트롤")]
        [Description("칩 라벨로 사용할 컬럼/속성 이름")]
        [DefaultValue("")]
        public string DisplayMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.LabelMemberPath;
                }

                return this.fallbackDisplayMember;
            }
            set
            {
                this.fallbackDisplayMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.LabelMemberPath = value;
                }
            }
        }

        /// <summary>Column/property name for the chip count (WinForms-compatible name).</summary>
        [Category("모던 컨트롤")]
        [Description("칩 인원수/건수로 사용할 컬럼/속성 이름")]
        [DefaultValue("")]
        public string ValueMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.CountMemberPath;
                }

                return this.fallbackValueMember;
            }
            set
            {
                this.fallbackValueMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.CountMemberPath = value;
                }
            }
        }

        /// <summary>Optional caption above the chips. Empty hides the caption.</summary>
        [Category("모던 컨트롤")]
        [Description("칩 목록 위에 표시할 제목(비우면 숨김)")]
        [Localizable(true)]
        [DefaultValue("")]
        public string Title
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Title;
                }

                return this.fallbackTitle;
            }
            set
            {
                this.fallbackTitle = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Title = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }
    }
}
