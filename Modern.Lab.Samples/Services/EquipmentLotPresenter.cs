using System;
using System.Data;
using Modern.Lab.Data;

namespace Modern.Lab.Samples.Services
{
    /// <summary>
    /// Equipment / Lots 화면의 파생 컬럼·집계 모음 — 화면(폼)과 분리된 순수
    /// DataTable 로직이다 (LogisticsRequestPresenter와 같은 역할 분담).
    ///
    /// 장비 상태(STATE)는 한눈에 보이도록 세 가지로만 정한다:
    ///   Down : 사용 불가(DOWN_YN='Y')                    (빨강 틴트)
    ///   Run  : 인포트에 작업중(Running) Lot이 하나라도 있음 (초록)
    ///   Idle : 그 외 — 투입/시작 대기                     (회색)
    ///
    /// 장비 그리드에는 포트를 **사용량 요약 배지**(IN_USE "1/2" · OUT_USE
    /// "0/1")로만 보여준다 — 회사 화면은 장비 컬럼이 많아 포트 상세는 선택
    /// 장비의 별도 카드(BuildPortRows)로 분리한다. 요약 색: 빈 인포트가
    /// 있으면 파랑(투입 가능 신호), 완료 아웃포트가 있으면 초록(반출 필요
    /// 신호), 그 외 회색.
    ///
    /// 포트 상세는 포트당 한 행(Port/State/Lot/Since)으로 — 빈 포트 회색,
    /// 투입됨 호박, 작업중 파랑, 완료 초록.
    ///
    /// 통신 모드(COMM_MODE)는 별도 배지 컬럼으로 표기하고 수동 처리 가능
    /// 여부를 가른다:
    ///   OnLineRemote : 장비가 자동 통신으로 진행 — 수동은 Prepare만 (초록)
    ///   OnLineLocal  : 전부 수동 처리 가능                     (호박)
    ///   OffLine      : 통신 끊김 — 작업 처리 불가, 모드 전환만  (빨강 틴트)
    ///
    /// 처리 가능 여부 파생(bool) — 컨텍스트 메뉴 항목 활성의 원천. **장비와
    /// 포트가 모두 준비되어야** 처리할 수 있다:
    ///   START_CAN  : Down 아님 + OnLineLocal + 작업중 아님 + 투입된(Loaded)
    ///                인포트 있음 (→ 작업시작 — 작업은 장비당 한 번에 하나)
    ///   END_CAN    : OnLineLocal + 작업중 인포트 있음 + **지정 아웃포트가
    ///                비어 있음** (→ 작업종료 — 점유돼 있으면 반출 먼저)
    ///   UNLOAD_CAN : OffLine 아님 + 완료(Done) 아웃포트 있음     (→ 반출)
    ///   FREE_IN    : 빈 인포트 수 (Down/OffLine이면 0)
    ///   FREE_OUT   : 비어 있고 **예약 안 된** 아웃포트 수 (Down/OffLine이면 0)
    ///                — 작업준비는 인/아웃 포트가 둘 다 있어야 가능하다.
    /// 대기 Lot에는 UP_CAN/DOWN_CAN(우선순위 ↑↓ 버튼 — 맨 위/아래에서 비활성)을
    /// 파생한다.
    /// </summary>
    internal static class EquipmentLotPresenter
    {
        // 장비 상태 배지색 — Down 빨강 틴트 / Run 초록 / Idle 회색.
        private const string stateDownColor = "#FEE2E2";
        private const string stateRunColor = "#DCFCE7";
        private const string stateIdleColor = "#E5E7EB";

        // 포트 배지색 — 빈 포트 회색 / 투입됨 호박 / 작업중 파랑 / 완료 초록.
        private const string portEmptyColor = "#E5E7EB";
        private const string portLoadedColor = "#FEF3C7";
        private const string portRunningColor = "#DBEAFE";
        private const string portDoneColor = "#DCFCE7";

        // 통신 모드 배지색 — Remote 초록(자동 진행 정상) / Local 호박(수동
        // 개입 중) / OffLine 빨강 틴트(통신 끊김).
        private const string commRemoteColor = "#DCFCE7";
        private const string commLocalColor = "#FEF3C7";
        private const string commOffLineColor = "#FEE2E2";

        // 우선순위 배지색 — 1순위 빨강 틴트 / 2~3순위 호박 / 그 외 회색.
        private const string prioTopColor = "#FEE2E2";
        private const string prioHighColor = "#FEF3C7";
        private const string prioNormalColor = "#E5E7EB";

        /// <summary>집계 결과 — 폼은 이 값을 KPI 배지에 그대로 표기만 한다.</summary>
        internal sealed class EquipmentSummary
        {
            /// <summary>작업중 장비 수.</summary>
            internal int RunCount;

            /// <summary>대기 장비 수.</summary>
            internal int IdleCount;

            /// <summary>Down 장비 수.</summary>
            internal int DownCount;

            /// <summary>투입 가능한 빈 인포트 수(그룹 전체).</summary>
            internal int FreeInPorts;
        }

        // ===== 장비 파생 컬럼 =====

        /// <summary>
        /// 장비 현황판에 파생 컬럼을 보장하고 전 행에 채운다: STATE/STATE_COLOR
        /// (장비 상태 배지), IN_USE/OUT_USE(포트 사용량 요약 배지), RUN_TM
        /// (작업중 Lot의 시작 시각), START_CAN/END_CAN/UNLOAD_CAN(행 버튼 활성),
        /// FREE_IN(빈 인포트 수).
        /// </summary>
        internal static void ApplyEquipmentColumns(DataTable equipments)
        {
            if (equipments == null)
            {
                return;
            }

            EnsureColumn(equipments, "STATE", typeof(string));
            EnsureColumn(equipments, "STATE_COLOR", typeof(string));
            EnsureColumn(equipments, "COMM_COLOR", typeof(string));
            EnsureColumn(equipments, "IN_USE", typeof(string));
            EnsureColumn(equipments, "IN_USE_COLOR", typeof(string));
            EnsureColumn(equipments, "OUT_USE", typeof(string));
            EnsureColumn(equipments, "OUT_USE_COLOR", typeof(string));
            EnsureColumn(equipments, "RUN_TM", typeof(string));
            EnsureColumn(equipments, "START_CAN", typeof(bool));
            EnsureColumn(equipments, "END_CAN", typeof(bool));
            EnsureColumn(equipments, "UNLOAD_CAN", typeof(bool));
            EnsureColumn(equipments, "FREE_IN", typeof(int));
            EnsureColumn(equipments, "FREE_OUT", typeof(int));

            foreach (DataRow row in equipments.Rows)
            {
                ApplyEquipmentRow(row);
            }
        }

        // 한 행(장비)의 상태·포트 요약·버튼 활성을 계산한다.
        private static void ApplyEquipmentRow(DataRow row)
        {
            bool down = TableHelper.CellText(row, "DOWN_YN").Trim() == "Y";
            string commMode = TableHelper.CellText(row, "COMM_MODE").Trim();
            bool offline = commMode == "OffLine";
            bool remote = commMode == "OnLineRemote";
            int inCount = ParseCount(row, "IN_CNT");
            int outCount = ParseCount(row, "OUT_CNT");

            bool anyRunning = false;
            bool anyLoaded = false;
            bool anyDone = false;
            bool endReady = false;
            int usedIn = 0;
            int usedOut = 0;
            int freeOut = 0;
            string runTime = string.Empty;

            for (int index = 1; index <= 2; index++)
            {
                if (index <= inCount)
                {
                    string stat = TableHelper.CellText(row, "IN" + index + "_STAT").Trim();

                    if (stat == "Running")
                    {
                        anyRunning = true;
                        usedIn = usedIn + 1;

                        // 작업종료 가능 = 이 작업의 지정 아웃포트가 비어 있음.
                        int outIndex = ParseCount(row, "IN" + index + "_OUT");

                        if (outIndex >= 1 && TableHelper.CellText(row, "OUT" + outIndex + "_STAT").Trim().Length == 0)
                        {
                            endReady = true;
                        }

                        if (runTime.Length == 0)
                        {
                            runTime = TableHelper.CellText(row, "IN" + index + "_TM");
                        }
                    }
                    else if (stat == "Loaded")
                    {
                        anyLoaded = true;
                        usedIn = usedIn + 1;
                    }
                }

                if (index <= outCount)
                {
                    string stat = TableHelper.CellText(row, "OUT" + index + "_STAT").Trim();
                    string reservedBy = TableHelper.CellText(row, "OUT" + index + "_RESV_LOT").Trim();

                    if (stat == "Done")
                    {
                        anyDone = true;
                        usedOut = usedOut + 1;
                    }
                    else if (reservedBy.Length == 0)
                    {
                        // 비어 있고 예약도 안 된 아웃포트 — 새 작업준비가 쓸 수 있다.
                        freeOut = freeOut + 1;
                    }
                }
            }

            int freeIn = inCount - usedIn;

            if (down)
            {
                row["STATE"] = "Down";
                row["STATE_COLOR"] = stateDownColor;
            }
            else if (anyRunning)
            {
                row["STATE"] = "Run";
                row["STATE_COLOR"] = stateRunColor;
            }
            else
            {
                row["STATE"] = "Idle";
                row["STATE_COLOR"] = stateIdleColor;
            }

            // 통신 모드 배지 — 서버 값 그대로 표기하고 색만 파생한다.
            if (offline)
            {
                row["COMM_COLOR"] = commOffLineColor;
            }
            else if (remote)
            {
                row["COMM_COLOR"] = commRemoteColor;
            }
            else
            {
                row["COMM_COLOR"] = commLocalColor;
            }

            // 포트 사용량 요약 — 색이 다음 액션을 신호한다: 인포트에 빈자리가
            // 있으면 파랑(투입 가능), 완료 아웃포트가 있으면 초록(반출 필요).
            row["IN_USE"] = usedIn + "/" + inCount;
            row["IN_USE_COLOR"] = !down && freeIn > 0 ? portRunningColor : portEmptyColor;
            row["OUT_USE"] = usedOut + "/" + outCount;
            row["OUT_USE_COLOR"] = anyDone ? portDoneColor : portEmptyColor;

            row["RUN_TM"] = runTime;

            // 작업은 장비당 한 번에 하나 — 작업중이면 다음 Lot 시작 불가.
            // 수동 시작/종료는 OnLineLocal에서만 — Remote는 자동 통신으로
            // 진행하고, OffLine은 통신이 끊겨 어떤 작업 처리도 불가하다.
            row["START_CAN"] = !down && !offline && !remote && anyLoaded && !anyRunning;
            row["END_CAN"] = !down && !offline && !remote && anyRunning && endReady;
            row["UNLOAD_CAN"] = !offline && anyDone;
            row["FREE_IN"] = down || offline ? 0 : freeIn;
            row["FREE_OUT"] = down || offline ? 0 : freeOut;
        }

        /// <summary>
        /// 선택 장비의 포트 상세 목록을 만든다 — 포트당 한 행. 포트 번호
        /// (PORT_NO)는 장비 전체 연속 번호(인포트 먼저, 그다음 아웃포트 —
        /// 2/2 장비면 1·2가 In, 3·4가 Out)이고 구분은 PORT_TYPE(In/Out)이다.
        /// 인포트의 TO_PORT는 작업준비 때 지정한 아웃포트의 포트 번호 —
        /// 서버는 지정 인덱스(IN{i}_OUT 숫자)만 주고 표기는 여기서 만든다.
        /// 아웃포트가 예약돼 있으면 State가 "Reserved"(호박) + 예약한 Lot으로
        /// 표시된다. PORT_IDX(타입 내 1-기준 번호)는 처리 호출용 내부 컬럼이다.
        ///
        /// 포트별 처리 플래그 — 포트 컨텍스트 메뉴/드롭다운 활성의 원천:
        ///   LOAD_CAN   : 빈 인포트 + 장비가 투입 가능(Down/OffLine 아님 +
        ///                쓸 수 있는 아웃포트 있음) — 이 포트를 지정해 투입
        ///   UNLOAD_CAN : 완료(Done) 아웃포트 + OffLine 아님 — 이 포트만 반출
        ///   CANCEL_CAN : 인포트의 투입됨/작업중 Lot + OffLine 아님 —
        ///                아웃포트 완료 Lot은 반출 대상이라 취소 불가
        /// (전제: equipment 행은 ApplyEquipmentColumns를 거쳐 FREE_OUT 등
        /// 파생 컬럼이 채워져 있다.)
        /// </summary>
        internal static DataTable BuildPortRows(DataRow equipment)
        {
            DataTable table = new DataTable();
            table.Columns.Add("PORT_NO", typeof(int));
            table.Columns.Add("PORT_TYPE", typeof(string));
            table.Columns.Add("PORT_STAT", typeof(string));
            table.Columns.Add("PORT_COLOR", typeof(string));
            table.Columns.Add("LOT_ID", typeof(string));
            table.Columns.Add("CARRIER", typeof(string));
            table.Columns.Add("TO_PORT", typeof(string));
            table.Columns.Add("PORT_IDX", typeof(int));
            table.Columns.Add("LOAD_CAN", typeof(bool));
            table.Columns.Add("UNLOAD_CAN", typeof(bool));
            table.Columns.Add("CANCEL_CAN", typeof(bool));

            if (equipment == null)
            {
                return table;
            }

            int inCount = ParseCount(equipment, "IN_CNT");
            int outCount = ParseCount(equipment, "OUT_CNT");

            for (int index = 1; index <= inCount; index++)
            {
                AddInPortRow(table, equipment, index, inCount);
            }

            for (int index = 1; index <= outCount; index++)
            {
                AddOutPortRow(table, equipment, index, inCount);
            }

            return table;
        }

        // 인포트 상세 행 — 빈 포트 회색 / 투입됨 호박 / 작업중 파랑.
        // To 컬럼에 지정 아웃포트의 포트 번호를 표기해 작업이 어디로 나가는지
        // 보여준다.
        private static void AddInPortRow(DataTable table, DataRow equipment, int index, int inCount)
        {
            string prefix = "IN" + index;
            string stat = TableHelper.CellText(equipment, prefix + "_STAT").Trim();
            string lot = TableHelper.CellText(equipment, prefix + "_LOT").Trim();
            string carrier = TableHelper.CellText(equipment, prefix + "_CAR").Trim();
            int outIndex = ParseCount(equipment, prefix + "_OUT");
            string color;

            if (stat == "Running")
            {
                color = portRunningColor;
            }
            else if (stat == "Loaded")
            {
                color = portLoadedColor;
            }
            else
            {
                stat = "Empty";
                color = portEmptyColor;
            }

            // 취소도 통신이 되어야 처리할 수 있다 (OffLine이면 불가).
            bool offline = TableHelper.CellText(equipment, "COMM_MODE").Trim() == "OffLine";
            bool cancellable = (stat == "Loaded" || stat == "Running") && !offline;

            // 이 인포트를 지정한 투입(Load) 가능 — 포트가 비어 있고 장비가
            // 투입 가능한 상태(Down/OffLine이면 FREE_OUT이 0으로 파생된다)
            // 여야 한다. 대기 Lot 존재 여부는 폼이 판정에 더한다.
            bool down = TableHelper.CellText(equipment, "DOWN_YN").Trim() == "Y";
            int freeOut = TableHelper.ParseInt(
                    TableHelper.CellText(equipment, "FREE_OUT"));
            bool loadable = stat == "Empty" && !down && !offline && freeOut > 0;

            // 지정 아웃포트의 연속 포트 번호 = 인포트 수 + 타입 내 번호.
            string toPort = outIndex >= 1
                    ? (inCount + outIndex).ToString("N0")
                    : string.Empty;
            table.Rows.Add(index, "In", stat, color, lot, carrier, toPort, index,
                    loadable, false, cancellable);
        }

        // 아웃포트 상세 행 — 빈 포트 회색 / 예약됨 호박(예약한 Lot 표기) /
        // 완료 초록.
        private static void AddOutPortRow(DataTable table, DataRow equipment, int index, int inCount)
        {
            string prefix = "OUT" + index;
            string stat = TableHelper.CellText(equipment, prefix + "_STAT").Trim();
            string lot = TableHelper.CellText(equipment, prefix + "_LOT").Trim();
            string carrier = TableHelper.CellText(equipment, prefix + "_CAR").Trim();
            string reservedBy = TableHelper.CellText(equipment, prefix + "_RESV_LOT").Trim();
            string color;

            if (stat == "Done")
            {
                color = portDoneColor;
            }
            else if (reservedBy.Length > 0)
            {
                // 예약된 아웃포트 — 예약한 Lot과, 배정된 아웃 캐리어를 보여준다.
                stat = "Reserved";
                lot = reservedBy;
                carrier = TableHelper.CellText(equipment, prefix + "_RESV_CAR").Trim();
                color = portLoadedColor;
            }
            else
            {
                stat = "Empty";
                color = portEmptyColor;
            }

            // 이 아웃포트만 반출(Unload) 가능 — 완료 Lot이 있고 통신이 되어야
            // 한다 (장비 메뉴의 Unload는 완료 포트 전체 반출로 별도 유지).
            bool unloadable = stat == "Done"
                    && TableHelper.CellText(equipment, "COMM_MODE").Trim() != "OffLine";

            table.Rows.Add(inCount + index, "Out", stat, color, lot, carrier, string.Empty, index,
                    false, unloadable, false);
        }

        /// <summary>
        /// 그룹에서 진행 중인 Lot 목록을 만든다 — 장비 포트에 올라가 있는 모든
        /// Lot(투입됨/작업중/완료)을 Lot 관점으로 한 행씩. 장비 리스트 하단에서
        /// "이 그룹에 지금 어떤 Lot이 돌아가는가"를 보여주는 원천이다.
        /// </summary>
        internal static DataTable BuildRunningLots(DataTable equipments)
        {
            DataTable table = new DataTable();
            table.Columns.Add("LOT_ID", typeof(string));
            table.Columns.Add("CARRIER", typeof(string));
            table.Columns.Add("EQP_ID", typeof(string));
            table.Columns.Add("PORT", typeof(string));
            table.Columns.Add("JOB_STAT", typeof(string));
            table.Columns.Add("JOB_COLOR", typeof(string));
            table.Columns.Add("EVENT_TM", typeof(string));

            if (equipments == null)
            {
                return table;
            }

            foreach (DataRow row in equipments.Rows)
            {
                string eqpId = TableHelper.CellText(row, "EQP_ID");
                int inCount = ParseCount(row, "IN_CNT");
                int outCount = ParseCount(row, "OUT_CNT");

                for (int index = 1; index <= inCount; index++)
                {
                    AddRunningLotRow(table, row, eqpId, "In " + index, "IN" + index);
                }

                for (int index = 1; index <= outCount; index++)
                {
                    AddRunningLotRow(table, row, eqpId, "Out " + index, "OUT" + index);
                }
            }

            return table;
        }

        // 점유된 포트 한 칸을 진행 중 Lot 행으로 (빈 포트는 건너뛴다).
        private static void AddRunningLotRow(
                DataTable table, DataRow equipment, string eqpId, string label, string prefix)
        {
            string stat = TableHelper.CellText(equipment, prefix + "_STAT").Trim();

            if (stat.Length == 0)
            {
                return;
            }

            string color;

            if (stat == "Running")
            {
                color = portRunningColor;
            }
            else if (stat == "Loaded")
            {
                color = portLoadedColor;
            }
            else
            {
                color = portDoneColor;
            }

            table.Rows.Add(
                    TableHelper.CellText(equipment, prefix + "_LOT"),
                    TableHelper.CellText(equipment, prefix + "_CAR"),
                    eqpId, label, stat, color,
                    TableHelper.CellText(equipment, prefix + "_TM"));
        }

        // ===== 대기 Lot 파생 컬럼 =====

        /// <summary>
        /// 대기 Lot 큐에 파생 컬럼을 보장하고 채운다: PRIO_COLOR(우선순위 배지색 —
        /// 1순위 빨강 / 2~3순위 호박 / 그 외 회색), UP_CAN/DOWN_CAN(우선순위 ↑↓
        /// 버튼 — 맨 위/아래 행에서 비활성), ASSIGN_CAN(지정 투입 버튼 활성 —
        /// 값은 폼이 선택 장비의 투입 가능 여부로 일괄 갱신한다).
        /// 큐는 우선순위 오름차순으로 정렬되어 온다는 전제다.
        /// </summary>
        internal static void ApplyLotColumns(DataTable lots)
        {
            if (lots == null)
            {
                return;
            }

            EnsureColumn(lots, "PRIO_COLOR", typeof(string));
            EnsureColumn(lots, "UP_CAN", typeof(bool));
            EnsureColumn(lots, "DOWN_CAN", typeof(bool));
            EnsureColumn(lots, "ASSIGN_CAN", typeof(bool));

            for (int index = 0; index < lots.Rows.Count; index++)
            {
                DataRow row = lots.Rows[index];
                int priority = TableHelper.ParseInt(
                        TableHelper.CellText(row, "PRIORITY"));

                if (priority <= 1)
                {
                    row["PRIO_COLOR"] = prioTopColor;
                }
                else if (priority <= 3)
                {
                    row["PRIO_COLOR"] = prioHighColor;
                }
                else
                {
                    row["PRIO_COLOR"] = prioNormalColor;
                }

                row["UP_CAN"] = index > 0;
                row["DOWN_CAN"] = index < lots.Rows.Count - 1;
                row["ASSIGN_CAN"] = false;
            }
        }

        /// <summary>지정 투입 버튼 활성을 일괄 갱신한다 — 선택 장비가 투입
        /// 가능(Down 아님 + 빈 인포트 + 쓸 수 있는 아웃포트)할 때만 전 행이
        /// 활성이 된다.</summary>
        internal static void SetAssignable(DataTable lots, bool assignable)
        {
            if (lots == null || !lots.Columns.Contains("ASSIGN_CAN"))
            {
                return;
            }

            foreach (DataRow row in lots.Rows)
            {
                row["ASSIGN_CAN"] = assignable;
            }
        }

        // ===== 집계 =====

        /// <summary>KPI를 집계한다 — 상태별 장비 수와 투입 가능한 빈 인포트 수.</summary>
        internal static EquipmentSummary Aggregate(DataTable equipments)
        {
            EquipmentSummary summary = new EquipmentSummary();

            if (equipments == null)
            {
                return summary;
            }

            foreach (DataRow row in equipments.Rows)
            {
                string state = TableHelper.CellText(row, "STATE");

                if (state == "Down")
                {
                    summary.DownCount += 1;
                }
                else if (state == "Run")
                {
                    summary.RunCount += 1;
                }
                else
                {
                    summary.IdleCount += 1;
                }

                summary.FreeInPorts += ParseCount(row, "FREE_IN");
            }

            return summary;
        }

        // ===== 공용 헬퍼 =====

        // int 컬럼을 관용적으로 읽는다 (DBNull/문자열 표기 허용).
        private static int ParseCount(DataRow row, string columnName)
        {
            return TableHelper.ParseInt(
                    TableHelper.CellText(row, columnName));
        }

        // 파생 컬럼이 없으면 만든다 — 서버가 이미 내려준 컬럼은 그대로 둔다.
        private static void EnsureColumn(DataTable table, string columnName, Type columnType)
        {
            if (!table.Columns.Contains(columnName))
            {
                table.Columns.Add(columnName, columnType);
            }
        }
    }
}
