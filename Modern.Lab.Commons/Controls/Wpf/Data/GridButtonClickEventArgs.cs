using System;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// 버튼 컬럼(GridColumnKind.Button) 셀 클릭 정보.
    /// 어느 행(Item)의 어느 컬럼(DataPropertyName) 버튼이 눌렸는지 전달한다.
    /// </summary>
    public class GridButtonClickEventArgs : EventArgs
    {
        public GridButtonClickEventArgs(object item, string dataPropertyName)
        {
            this.Item = item;
            this.DataPropertyName = dataPropertyName ?? string.Empty;
        }

        /// <summary>클릭된 행 항목 (DataTable 소스의 경우 DataRowView).</summary>
        public object Item { get; private set; }

        /// <summary>클릭된 버튼 컬럼의 DataPropertyName.</summary>
        public string DataPropertyName { get; private set; }
    }
}
