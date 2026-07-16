using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Modern.Lab.Controls.Wpf.Data;

namespace Modern.Lab.Export
{
    /// <summary>
    /// 그리드 컬럼 정의를 그대로 엑셀 컬럼으로 쓰는 내보내기 도우미 —
    /// ModernDataGrid.ExportXlsx의 내부 구현.
    ///
    /// 화면 그리드의 ConfigureColumns 정의를 단일 원천으로 사용하므로, 폼이
    /// 내보내기용 컬럼/헤더 목록을 따로 관리할 필요가 없다 — 화면 컬럼을
    /// 바꾸면 엑셀도 자동으로 따라온다.
    ///
    /// - CheckBox(벌크 체크)/Button(행 동작) 컬럼은 엑셀에 의미가 없어 제외한다.
    /// - Badge 컬럼은 값(텍스트)만 내보낸다.
    /// - 데이터는 그리드의 DataSource가 아니라 호출자가 준 전체 결과를 쓴다 —
    ///   페이지 화면(그리드에는 현재 페이지 조각만 바인딩)에서도 전체가 나간다.
    /// </summary>
    internal static class GridXlsxExporter
    {
        /// <summary>
        /// 컬럼 정의 순서대로 데이터 전체를 .xlsx로 저장한다.
        /// 컬럼의 Format("N0", "yyyy-MM-dd" 등)은 원본 값이 IFormattable일 때 적용된다.
        /// </summary>
        internal static void Write(string path, string sheetName, IList<ModernDataGridColumn> columns, DataTable data)
        {
            List<ModernDataGridColumn> exportable = new List<ModernDataGridColumn>();

            foreach (ModernDataGridColumn column in columns)
            {
                if (column == null)
                {
                    continue;
                }

                if (column.Kind == GridColumnKind.CheckBox || column.Kind == GridColumnKind.Button)
                {
                    continue;
                }

                exportable.Add(column);
            }

            string[] headers = new string[exportable.Count];

            for (int index = 0; index < exportable.Count; index++)
            {
                // 다중 줄 헤더("\n")는 엑셀 한 줄 캡션으로 합친다.
                string caption = exportable[index].HeaderText;
                headers[index] = string.IsNullOrEmpty(caption)
                        ? exportable[index].DataPropertyName
                        : caption.Replace("\n", " ");
            }

            List<string[]> rows = new List<string[]>();

            foreach (DataRow row in data.Rows)
            {
                string[] cells = new string[exportable.Count];

                for (int index = 0; index < exportable.Count; index++)
                {
                    cells[index] = FormatCell(row, exportable[index]);
                }

                rows.Add(cells);
            }

            SimpleXlsxWriter.Write(path, sheetName, headers, rows);
        }

        // 셀 문자열: 그리드 표시와 같은 규칙 — Format이 있고 값이 형식화 가능하면
        // 적용하고, 실패하면 기본 문자열 표현으로 폴백한다.
        private static string FormatCell(DataRow row, ModernDataGridColumn column)
        {
            if (!row.Table.Columns.Contains(column.DataPropertyName))
            {
                return string.Empty;
            }

            object value = row[column.DataPropertyName];

            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(column.Format) && value is IFormattable)
            {
                try
                {
                    return ((IFormattable)value).ToString(column.Format, CultureInfo.CurrentCulture);
                }
                catch (FormatException)
                {
                    // 형식 오류는 기본 표현으로 폴백한다 (그리드 표시와 동일한 완화).
                }
            }

            return value.ToString();
        }
    }
}
