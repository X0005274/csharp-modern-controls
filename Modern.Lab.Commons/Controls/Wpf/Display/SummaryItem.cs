using System.ComponentModel;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>UI item model for one chip in ModernSummaryListControl.</summary>
    public class SummaryItem : INotifyPropertyChanged
    {
        private string label;
        private string count;

        public event PropertyChangedEventHandler PropertyChanged;

        public SummaryItem()
        {
            this.label = string.Empty;
            this.count = string.Empty;
        }

        /// <summary>Category text (e.g. department or rank name).</summary>
        public string Label
        {
            get
            {
                return this.label;
            }
            set
            {
                this.label = value;
                this.RaisePropertyChanged("Label");
            }
        }

        /// <summary>Count text shown next to the label.</summary>
        public string Count
        {
            get
            {
                return this.count;
            }
            set
            {
                this.count = value;
                this.RaisePropertyChanged("Count");
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
