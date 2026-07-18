using System.ComponentModel;
using System.Data;

namespace Modern.Lab.Controls.Wpf.Common
{
    /// <summary>
    /// 바인딩된 행 항목에서 이름으로 지정된 컬럼/속성 값을 읽고 쓰며, WinForms의
    /// DisplayMember/ValueMember가 값을 해석하는 방식을 그대로 따른다. WPF 바인딩
    /// 밖에서 표시 텍스트가 필요한 컨트롤(필터링, 칩)과 행 값 일괄 기록이 필요한
    /// 컨트롤(그리드 헤더 체크박스)이 공유한다.
    /// </summary>
    internal static class MemberPathReader
    {
        /// <summary>멤버 값을 반환하며, 해석할 수 없으면 null을 반환한다.</summary>
        internal static object Read(object row, string memberPath)
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

        /// <summary>
        /// 행 항목에 이름으로 지정된 멤버 값을 쓴다 (Read의 쓰기 대응).
        /// DataRowView는 DataRow에 직접 써서 DataTable 이벤트(ColumnChanged)가 전파되게
        /// 하고, 일반 객체는 TypeDescriptor 속성으로 쓴다 (INotifyPropertyChanged 전제).
        /// 해석할 수 없는 경로/읽기 전용 속성은 조용히 무시한다.
        /// </summary>
        internal static void Write(object row, string memberPath, object value)
        {
            if (row == null || string.IsNullOrEmpty(memberPath))
            {
                return;
            }

            DataRowView rowView = row as DataRowView;

            if (rowView != null)
            {
                if (rowView.Row.Table.Columns.Contains(memberPath))
                {
                    rowView.Row[memberPath] = value;
                }

                return;
            }

            PropertyDescriptor property = TypeDescriptor.GetProperties(row).Find(memberPath, true);

            if (property != null && !property.IsReadOnly)
            {
                property.SetValue(row, value);
            }
        }

        /// <summary>
        /// 멤버 값을 표시 텍스트로 반환한다. 멤버 경로가 설정되지 않은 경우
        /// 항목 자체의 ToString()으로 폴백하며, 절대 null을 반환하지 않는다.
        /// </summary>
        internal static string ReadDisplayText(object row, string memberPath)
        {
            if (row == null)
            {
                return string.Empty;
            }

            object value = string.IsNullOrEmpty(memberPath) ? row : Read(row, memberPath);

            if (value == null)
            {
                return string.Empty;
            }

            return value.ToString();
        }
    }
}
