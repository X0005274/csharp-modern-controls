using System.ComponentModel;

namespace Modern.Lab.Controls.Wpf.Selection
{
    /// <summary>ModernRadioGroupControl의 라디오 항목 하나를 나타내는 UI 항목 모델.</summary>
    public class RadioGroupItem : INotifyPropertyChanged
    {
        private bool isChecked;

        public event PropertyChangedEventHandler PropertyChanged;

        public RadioGroupItem(object value, string displayText)
        {
            this.Value = value;
            this.DisplayText = displayText;
            this.isChecked = false;
        }

        /// <summary>선택 값 (ValueMemberPath 기준).</summary>
        public object Value { get; private set; }

        /// <summary>라디오 옆에 표시되는 텍스트.</summary>
        public string DisplayText { get; private set; }

        /// <summary>선택 상태.</summary>
        public bool IsChecked
        {
            get
            {
                return this.isChecked;
            }
            set
            {
                if (this.isChecked == value)
                {
                    return;
                }

                this.isChecked = value;

                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("IsChecked"));
                }
            }
        }
    }
}
