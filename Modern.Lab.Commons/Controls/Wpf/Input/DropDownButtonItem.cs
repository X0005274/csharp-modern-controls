namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>ModernDropDownButtonControl의 메뉴 항목 하나를 나타내는 UI 항목 모델.</summary>
    public class DropDownButtonItem
    {
        public DropDownButtonItem(object value, string displayText)
        {
            this.Value = value;
            this.DisplayText = displayText;
        }

        /// <summary>항목 값 (ValueMemberPath 기준).</summary>
        public object Value { get; private set; }

        /// <summary>메뉴에 표시되는 텍스트.</summary>
        public string DisplayText { get; private set; }
    }
}
