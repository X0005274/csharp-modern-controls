using System;
using System.Collections.Generic;
using Modern.Lab.Controls.Wpf.Data;

namespace Modern.Lab.Samples.Services
{
    /// <summary>
    /// 데모 앱의 그리드 컬럼 캡션 용어집 — 필드 이름(DB 컬럼) → 표준 캡션.
    ///
    /// Program.Main이 RegisterAll()로 라이브러리 사전(GridCaptionCatalog)에
    /// 부어 넣으며, 이후 폼은 new ModernDataGridColumn("ITEM_ID")처럼 캡션 없이
    /// 컬럼을 정의하면 여기 표준 캡션이 자동으로 붙는다. 화면 문맥상 다른
    /// 표현이 필요하면(예: EVENT_TM을 도착 화면에서 "Arrived At") headerText를
    /// 받는 생성자로 명시해 재정의한다 — 명시가 항상 사전을 이긴다.
    ///
    /// 회사 적용 시 이 용어집을 사내 표준 용어집에 맞춰 채우면(하드코딩 목록,
    /// 리소스 파일, 사내 DB 조회 등 출처 무관) 모든 화면과 엑셀 내보내기의
    /// 컬럼 캡션이 한 곳에서 관리된다.
    /// </summary>
    internal static class GridCaptionDictionary
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
                { "LOGIS_YN", "Logistics" },

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

        /// <summary>용어집 전체를 라이브러리 사전에 등록한다 — 앱 시작 시(첫 폼 생성 전) 1회.</summary>
        internal static void RegisterAll()
        {
            GridCaptionCatalog.RegisterRange(captions);
        }
    }
}
