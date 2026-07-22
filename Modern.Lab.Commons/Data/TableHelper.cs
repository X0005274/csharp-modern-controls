using System;
using System.Data;
using System.Globalization;

namespace Modern.Lab.Data
{
    /// <summary>
    /// 화면·도메인 무관 DataTable 읽기 유틸 — 폼/프레젠터가 서버 조회 결과를
    /// 안전하게 읽는 최소 도구 모음이다. 어느 프로젝트든 그대로 쓴다.
    ///
    /// 이 구획(Data)의 편입 기준 — 전부 충족해야 들어온다:
    /// ① 화면/도메인 무관 ② 무상태 static ③ 입출력이 DataTable/기본형뿐
    /// ④ 2개 이상 화면(프로젝트)에서 실제 사용.
    /// 회사가 값을 편집하는 콘텐츠(용어사전 등)는 여기가 아니라 Captions
    /// 구획이다 — Data는 아무도 편집할 일이 없는 기계 부품만 담는다.
    /// </summary>
    public static class TableHelper
    {
        /// <summary>셀 값을 안전하게 문자열로 읽는다 — 컬럼 자체가 없거나
        /// (서버 JSON의 null 키 생략) DBNull인 경우 모두 빈 문자열.</summary>
        public static string CellText(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName))
            {
                return string.Empty;
            }

            object value = row[columnName];
            return value == DBNull.Value || value == null ? string.Empty : value.ToString();
        }

        /// <summary>DataRowView 편의 오버로드.</summary>
        public static string CellText(DataRowView row, string columnName)
        {
            return row == null ? string.Empty : CellText(row.Row, columnName);
        }

        /// <summary>서버 숫자 컬럼(JSON number)을 관용적으로 정수 파싱한다 —
        /// 빈 값/소수 표기 모두 허용, 실패 시 0.</summary>
        public static int ParseInt(string text)
        {
            double value;

            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                return (int)value;
            }

            return 0;
        }

        /// <summary>bool 파생 컬럼(체크박스/버튼 활성 플래그)을 관용적으로 읽는다 —
        /// 컬럼이 없거나 bool이 아니면 false.</summary>
        public static bool FlagSet(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName))
            {
                return false;
            }

            object value = row[columnName];
            return value is bool && (bool)value;
        }
    }
}
