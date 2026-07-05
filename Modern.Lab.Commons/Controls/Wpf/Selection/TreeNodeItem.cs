using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

namespace Modern.Lab.Controls.Wpf.Selection
{
    /// <summary>ModernTreeViewControl의 트리 노드 하나를 나타내는 UI 항목 모델.</summary>
    public class TreeNodeItem : INotifyPropertyChanged
    {
        private bool isExpanded;
        private bool isSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        public TreeNodeItem(object value, string displayText, object row)
        {
            this.Value = value;
            this.DisplayText = displayText;
            this.Row = row;
            this.Children = new ObservableCollection<TreeNodeItem>();
            this.isExpanded = true;
            this.isSelected = false;
        }

        /// <summary>노드 텍스트 색 (null = 테마 기본색 상속).</summary>
        public Brush Foreground { get; internal set; }

        /// <summary>사용자 지정 텍스트 색 보유 여부 — XAML DataTrigger 키.</summary>
        public bool HasCustomForeground
        {
            get { return this.Foreground != null; }
        }

        /// <summary>노드 값 (IdMemberPath 기준).</summary>
        public object Value { get; private set; }

        /// <summary>노드에 표시되는 텍스트.</summary>
        public string DisplayText { get; private set; }

        /// <summary>원본 행 (DataRowView 등).</summary>
        public object Row { get; private set; }

        /// <summary>부모 노드 (루트는 null). 조상 펼침에 사용.</summary>
        public TreeNodeItem Parent { get; internal set; }

        /// <summary>자식 노드 목록.</summary>
        public ObservableCollection<TreeNodeItem> Children { get; private set; }

        /// <summary>펼침 상태.</summary>
        public bool IsExpanded
        {
            get
            {
                return this.isExpanded;
            }
            set
            {
                if (this.isExpanded == value)
                {
                    return;
                }

                this.isExpanded = value;
                this.RaisePropertyChanged("IsExpanded");
            }
        }

        /// <summary>선택 상태.</summary>
        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }
            set
            {
                if (this.isSelected == value)
                {
                    return;
                }

                this.isSelected = value;
                this.RaisePropertyChanged("IsSelected");
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
