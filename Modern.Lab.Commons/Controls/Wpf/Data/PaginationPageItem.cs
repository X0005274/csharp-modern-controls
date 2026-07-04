using System.ComponentModel;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>ModernPaginationControl의 페이지 번호 버튼 하나를 나타내는 UI 항목 모델.</summary>
    public class PaginationPageItem : INotifyPropertyChanged
    {
        private bool isCurrent;

        public event PropertyChangedEventHandler PropertyChanged;

        public PaginationPageItem(int number, bool isCurrent)
        {
            this.Number = number;
            this.isCurrent = isCurrent;
        }

        /// <summary>페이지 번호 (1부터).</summary>
        public int Number { get; private set; }

        /// <summary>현재 페이지 여부.</summary>
        public bool IsCurrent
        {
            get
            {
                return this.isCurrent;
            }
            set
            {
                if (this.isCurrent == value)
                {
                    return;
                }

                this.isCurrent = value;

                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("IsCurrent"));
                }
            }
        }
    }
}
