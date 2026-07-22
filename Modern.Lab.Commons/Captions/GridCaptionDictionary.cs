using System;
using System.Collections.Generic;
using Modern.Lab.Controls.Wpf.Data;

namespace Modern.Lab.Captions
{
    /// <summary>
    /// 회사 표준 그리드 컬럼 캡션 용어집 — 필드 이름(DB 컬럼)에서 표준 캡션으로 변환한다.
    /// </summary>
    public static class GridCaptionDictionary
    {
        // 필드 이름 대소문자 무시는 GridCaptionCatalog가 처리한다.
        private static readonly Dictionary<string, string> captions =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // ---- 식별자 ----
                { "ITEM_ID", "Item ID" },
                { "UNIT_ID", "Unit ID" },
                { "ORG_ITEM_ID", "Org Item" },
                { "PARENT_ITEM_ID", "Parent Item" },
                { "ORG_UNIT_ID", "Org Unit" },
                { "PARENT_UNIT_ID", "Parent Unit" },
                { "TIMEKEY", "Time Key" },

                // ---- 이벤트/상태 ----
                { "EVENT_TM", "Event Time" },
                { "EVENT_CD", "Event" },
                { "DURATION", "Duration" },
                { "STAT_TYP", "Status" },
                { "PRIORITY", "Priority" },
                { "ELAPSED_DAYS", "Days" },

                // ---- 의뢰서 ----
                { "REQ_NO", "Request No" },
                { "REQ_TM", "Requested At" },
                { "SAMPLE_NM", "Sample" },
                { "PROC_YN", "Processed" },
                { "PROC_TM", "Processed At" },

                // ---- 발송/수신 인터페이스 (FAC_SEND_MAS) ----
                { "STATUS", "Status" },
                { "SEND_YN", "Sent" },
                { "SEND_FAC", "Send Fac" },
                { "SEND_TM", "Sent At" },
                { "RECV_YN", "Received" },
                { "RECV_TM", "Received At" },
                { "RECV_DESC", "Receive Note" },
                { "ITEM_STAT", "Item Status" },

                // ---- 공정/위치 ----
                { "OPER_ID", "Operation" },
                { "STATION_ID", "Equipment" },
                { "FLOW_ID", "Flow" },
                { "BOX_ID", "Carrier" },
                { "STORE_ID", "Stocker" },

                // ---- 제품/분류 ----
                { "MODEL_ID", "Product" },
                { "ITEM_TYP", "Prod Type" },
                { "SUB_TYP", "Sub Type" },
                { "UNIT_CNT", "Units" },
                { "DESCRIPTION", "Description" }
            };

        /// <summary>
        /// 회사 표준 용어집 전체를 라이브러리 사전에 등록한다.
        /// 첫 그리드 또는 폼을 만들기 전에 앱당 한 번 호출한다.
        /// </summary>
        public static void RegisterAll()
        {
            GridCaptionCatalog.RegisterRange(captions);
        }
    }
}
