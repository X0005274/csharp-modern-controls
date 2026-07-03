using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// Category/count summary rendered as a wrapping chip list
    /// (e.g. per-department or per-rank head counts).
    /// - ItemsSource: any IEnumerable (DataView, IList, ...)
    /// - LabelMemberPath / CountMemberPath: column/property names to read
    /// - Title: optional caption above the chips
    ///
    /// The chip list is rebuilt whenever the source or a member path changes,
    /// so assignment order does not matter (contract rule 3).
    /// </summary>
    public partial class ModernSummaryListControl : UserControl
    {
        /// <summary>Rows to summarize. Any IEnumerable (DataView, IList, ...).</summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(ModernSummaryListControl),
                new PropertyMetadata(null, OnDataShapeChanged));

        /// <summary>Column/property name for the chip label.</summary>
        public static readonly DependencyProperty LabelMemberPathProperty =
            DependencyProperty.Register(
                "LabelMemberPath",
                typeof(string),
                typeof(ModernSummaryListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>Column/property name for the chip count.</summary>
        public static readonly DependencyProperty CountMemberPathProperty =
            DependencyProperty.Register(
                "CountMemberPath",
                typeof(string),
                typeof(ModernSummaryListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>Optional caption left of the chips. Empty hides the caption.</summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "Title",
                typeof(string),
                typeof(ModernSummaryListControl),
                new PropertyMetadata(string.Empty));

        /// <summary>Drops the card chrome (border/background/padding) for use on a shared card panel.</summary>
        public static readonly DependencyProperty FlatProperty =
            DependencyProperty.Register(
                "Flat",
                typeof(bool),
                typeof(ModernSummaryListControl),
                new PropertyMetadata(false));

        private readonly ObservableCollection<SummaryItem> chipItemModels;

        public ModernSummaryListControl()
        {
            this.chipItemModels = new ObservableCollection<SummaryItem>();
            this.InitializeComponent();
            this.ChipItems.ItemsSource = this.chipItemModels;
        }

        /// <summary>Rows to summarize.</summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>Column/property name for the chip label.</summary>
        public string LabelMemberPath
        {
            get { return (string)this.GetValue(LabelMemberPathProperty); }
            set { this.SetValue(LabelMemberPathProperty, value); }
        }

        /// <summary>Column/property name for the chip count.</summary>
        public string CountMemberPath
        {
            get { return (string)this.GetValue(CountMemberPathProperty); }
            set { this.SetValue(CountMemberPathProperty, value); }
        }

        /// <summary>Optional caption left of the chips.</summary>
        public string Title
        {
            get { return (string)this.GetValue(TitleProperty); }
            set { this.SetValue(TitleProperty, value); }
        }

        /// <summary>Drops the card chrome for use on a shared card panel.</summary>
        public bool Flat
        {
            get { return (bool)this.GetValue(FlatProperty); }
            set { this.SetValue(FlatProperty, value); }
        }

        private static void OnDataShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernSummaryListControl)d).RebuildChips();
        }

        // Converts source rows into chip item models. Null/empty sources and
        // missing columns render as empty output, never throw (contract rule 3).
        private void RebuildChips()
        {
            this.chipItemModels.Clear();

            IEnumerable source = this.ItemsSource;

            if (source == null)
            {
                return;
            }

            foreach (object row in source)
            {
                SummaryItem item = new SummaryItem();
                item.Label = ToDisplayString(ReadMember(row, this.LabelMemberPath));
                item.Count = ToDisplayString(ReadMember(row, this.CountMemberPath));
                this.chipItemModels.Add(item);
            }
        }

        private static object ReadMember(object row, string memberPath)
        {
            if (row == null || string.IsNullOrEmpty(memberPath))
            {
                return null;
            }

            DataRowView rowView = row as DataRowView;

            if (rowView != null)
            {
                if (rowView.Row.Table.Columns.Contains(memberPath))
                {
                    return rowView[memberPath];
                }

                return null;
            }

            PropertyDescriptor property = TypeDescriptor.GetProperties(row).Find(memberPath, true);

            if (property != null)
            {
                return property.GetValue(row);
            }

            return null;
        }

        private static string ToDisplayString(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.ToString();
        }
    }
}
