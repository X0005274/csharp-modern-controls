using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Modern.Lab.WinForms.Controls.Hosting
{
    /// <summary>
    /// 계약이 허용하는 WinForms식 DataSource 형태
    /// (DataTable / DataView / IList / IEnumerable)를 WPF ItemsSource로 변환한다.
    /// 허용 형태가 동일하게 유지되도록 모든 데이터 바인딩 래퍼가 공유한다.
    /// </summary>
    internal static class DataSourceConverter
    {
        /// <summary>null 입력에는 null을 반환하고, 지원하지 않는 형태에는 예외를 던진다.</summary>
        internal static IEnumerable ToItemsSource(object value)
        {
            if (value == null)
            {
                return null;
            }

            DataTable table = value as DataTable;

            if (table != null)
            {
                return table.DefaultView;
            }

            DataView view = value as DataView;

            if (view != null)
            {
                return view;
            }

            IEnumerable enumerable = value as IEnumerable;

            if (enumerable != null)
            {
                return enumerable;
            }

            throw new ArgumentException(
                "DataSource must be a DataTable, DataView, IList or IEnumerable.",
                "value");
        }

        /// <summary>
        /// DataTable/DataView 소스에 지정한 컬럼이 없으면 빈 문자열 컬럼으로 추가해
        /// 바인딩을 보장한다. JSON→DataTable류 변환은 값이 전부 null인 컬럼을 만들지
        /// 않는 경우가 많으므로(서버가 null 키를 생략), 각 폼이 컬럼 목록을 중복
        /// 하드코딩하는 대신 컨트롤이 자기 멤버 정의로 직접 보장할 때 쓴다.
        /// DataTable/DataView가 아닌 소스와 null/빈 이름은 조용히 무시한다.
        /// </summary>
        internal static void EnsureColumns(object value, IEnumerable<string> memberNames)
        {
            if (memberNames == null)
            {
                return;
            }

            DataTable table = value as DataTable;

            if (table == null)
            {
                DataView view = value as DataView;

                if (view != null)
                {
                    table = view.Table;
                }
            }

            if (table == null)
            {
                return;
            }

            foreach (string name in memberNames)
            {
                if (!string.IsNullOrEmpty(name) && !table.Columns.Contains(name))
                {
                    table.Columns.Add(name, typeof(string));
                }
            }
        }
    }
}
