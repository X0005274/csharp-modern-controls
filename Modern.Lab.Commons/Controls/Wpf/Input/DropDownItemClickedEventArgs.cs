using System;

namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>드롭다운 버튼에서 메뉴 항목이 클릭되었을 때의 정보.</summary>
    public class DropDownItemClickedEventArgs : EventArgs
    {
        public DropDownItemClickedEventArgs(object value, string displayText)
        {
            this.Value = value;
            this.DisplayText = displayText;
        }

        /// <summary>클릭된 항목의 값 (ValueMember 기준).</summary>
        public object Value { get; private set; }

        /// <summary>클릭된 항목의 표시 텍스트.</summary>
        public string DisplayText { get; private set; }
    }
}
