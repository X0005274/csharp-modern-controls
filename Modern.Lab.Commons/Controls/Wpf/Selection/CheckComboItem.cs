using System.ComponentModel;

namespace Modern.Lab.Controls.Wpf.Selection
{
    /// <summary>UI item model for one checkbox row in ModernCheckComboBoxControl.</summary>
    public class CheckComboItem : INotifyPropertyChanged
    {
        private readonly object item;
        private string displayText;
        private bool isChecked;

        public event PropertyChangedEventHandler PropertyChanged;

        public CheckComboItem(object item, string displayText)
        {
            this.item = item;
            this.displayText = displayText ?? string.Empty;
            this.isChecked = false;
        }

        /// <summary>The bound source row (DataRowView, object, ...).</summary>
        public object Item
        {
            get { return this.item; }
        }

        /// <summary>Text shown next to the checkbox.</summary>
        public string DisplayText
        {
            get
            {
                return this.displayText;
            }
            set
            {
                this.displayText = value;
                this.RaisePropertyChanged("DisplayText");
            }
        }

        /// <summary>Check state (two-way bound to the checkbox).</summary>
        public bool IsChecked
        {
            get
            {
                return this.isChecked;
            }
            set
            {
                if (this.isChecked != value)
                {
                    this.isChecked = value;
                    this.RaisePropertyChanged("IsChecked");
                }
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
