using System.Collections.Generic;
using System.Data;
using System.Web.Script.Serialization;

namespace Modern.Lab.Samples.Services
{
    /// <summary>
    /// JSON 배열(객체 목록) → DataTable 변환기.
    /// 서버가 null 컬럼의 키를 생략할 수 있으므로(MyBatis map 특성),
    /// 컬럼은 전체 행의 키 합집합으로 만든다.
    /// </summary>
    internal static class JsonTableConverter
    {
        internal static DataTable ToDataTable(string json)
        {
            DataTable table = new DataTable();

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            object[] rows = serializer.DeserializeObject(json) as object[];

            if (rows == null)
            {
                return table;
            }

            // 1차: 컬럼 합집합 수집 (처음 등장한 순서 유지)
            foreach (object item in rows)
            {
                Dictionary<string, object> dict = item as Dictionary<string, object>;

                if (dict == null)
                {
                    continue;
                }

                foreach (string key in dict.Keys)
                {
                    if (!table.Columns.Contains(key))
                    {
                        table.Columns.Add(key);
                    }
                }
            }

            // 2차: 행 채우기 (없는 키/null은 DBNull)
            foreach (object item in rows)
            {
                Dictionary<string, object> dict = item as Dictionary<string, object>;

                if (dict == null)
                {
                    continue;
                }

                DataRow row = table.NewRow();

                foreach (KeyValuePair<string, object> pair in dict)
                {
                    if (pair.Value != null)
                    {
                        row[pair.Key] = pair.Value;
                    }
                }

                table.Rows.Add(row);
            }

            return table;
        }
    }
}
