using System;
using System.Data;
using System.Globalization;
using Modern.Lab.Data;

namespace Modern.Lab.Samples.Services
{
    /// <summary>
    /// Logistics & Request 화면의 파생 컬럼·필터·집계 모음 — 화면(폼)과 분리된
    /// 순수 DataTable 로직이다 (ItemHistoryPresenter와 같은 역할 분담).
    ///
    /// 데이터 개념 (2026-07-17 FAC_SEND_MAS 기반 재정의):
    /// 인터페이스 흐름은 원래 전부 자동이다 — 발송 통보(FAC_SEND_MAS) → 물류
    /// 도착 → Receive 처리(전산 ITEM Created/Released 생성) → 의뢰서 연결 →
    /// Create 처리. 이 화면은 자동이 일부/전부 실패한 건을 사람이 이어서
    /// 처리하고(수동 Receive/Create), 도착했지만 의뢰서가 없는 목록과 전체
    /// 현황을 현업에게 보여주기 위한 것이다.
    ///
    /// 행 상태(STATUS)는 "아직 안 된 첫 단계"로 정한다 — 자동이 어느 조합까지
    /// 되고 멈췄든 다음에 할 일이 하나로 정해진다. 수신이 곧 도착(동시각)이라
    /// "도착했지만 미수신" 중간 상태는 없다:
    ///   Sent             : 발송 통보(FAC_SEND_MAS)만 있고 미수신    (→ Receive)
    ///   Received         : 수신 완료, 의뢰서 미연결, 수신 후 1일 미만 (배치 대기)
    ///   Request Unlinked : 수신 후 1일이 지나도 의뢰서 미연결      (지연 — 경과일 강조)
    ///   Request Linked   : 배치가 의뢰서를 연결함, Create 미처리    (→ Create)
    ///   Completed        : Create 처리 완료(PROC_YN='Y')
    ///
    /// 발송 통보 여부(SEND_YN)는 상태와 **직교**한다 — 미확인 물류(SEND_YN='N')도
    /// 의뢰 인터페이스는 타므로 동일 파이프라인으로 진행하고, FAC_SEND_MAS에
    /// 있냐 없냐만 SEND_YN 배지(Y 초록 / N 빨강 틴트)로 구분해 보여준다.
    ///
    /// 경과일(ELAPSED_DAYS)은 두 구간에 표기한다 — 도착 전(운송 중)은 발송
    /// 통보 시각 기준(운송 지연 감지), 도착 후에는 "의뢰서와 연결이 안 된"
    /// 행만 도착 시각 기준(방치 기간). 연결된 행은 의뢰번호가 상태를 말해
    /// 주므로 비운다. 배지색: 0-2일 파랑 · 3-6일 호박 · 7-13일 주황 ·
    /// 14일+ 빨강 틴트. KPI의 Avg/Oldest는 도착 후 미연결 행만 집계한다.
    /// </summary>
    internal static class LogisticsRequestPresenter
    {
        // ===== 상태 정의 =====

        /// <summary>상태 인덱스: 발송 통보(FAC_SEND_MAS)만 있고 미수신(Sent) — 수동 Receive 대상.</summary>
        internal const int StatusTransit = 0;

        /// <summary>상태 인덱스: 수신 완료, 의뢰서 미연결, 수신 후 1일 미만 — 배치 대기.</summary>
        internal const int StatusReceived = 1;

        /// <summary>상태 인덱스: 수신 후 1일이 지나도 의뢰서 미연결 — 지연·경과일 강조 대상.</summary>
        internal const int StatusNoRequest = 2;

        /// <summary>상태 인덱스: 배치가 의뢰서를 연결함, Create 미처리 — 수동 Create 대상.</summary>
        internal const int StatusLinked = 3;

        /// <summary>상태 인덱스: Create 처리 완료.</summary>
        internal const int StatusCompleted = 4;

        /// <summary>수신 후 의뢰서 미연결이 이 일수 이상이면 Request Unlinked(지연)로 본다.</summary>
        private const int unlinkedAfterDays = 1;

        /// <summary>상태 표시명 — 필터 콤보 값·STATUS 컬럼 값·KPI 집계가 같은 문자열을 쓴다.</summary>
        internal static readonly string[] StatusNames =
        {
            "Sent", "Received", "Request Unlinked", "Request Linked", "Completed"
        };

        // 상태 배지 배경색 (StatusNames와 같은 순서) —
        // 회색(미수신)·파랑(수신)·호박(연결 지연)·남색 틴트(연결)·초록(완료).
        private static readonly string[] statusColors =
        {
            "#E5E7EB", "#DBEAFE", "#FEF3C7", "#E0E7FF", "#DCFCE7"
        };

        // 경과일 구간별 배지 배경색 (0-2 / 3-6 / 7-13 / 14+ 일 — 구간이
        // 심해질수록 파랑 → 호박 → 주황 → 빨강 틴트).
        private static readonly string[] agingBadgeColors = { "#DBEAFE", "#FEF3C7", "#FFE0CC", "#FEE2E2" };

        // 발송 통보 여부(SEND_YN) 배지색 — Y(FAC_SEND_MAS 있음) 초록 / N(미확인) 빨강 틴트.
        private const string sendNotifiedColor = "#DCFCE7";
        private const string sendUnmatchedColor = "#FEE2E2";

        /// <summary>집계 결과 — 폼은 이 값을 KPI 배지에 그대로 표기만 한다.</summary>
        internal sealed class LogisticsSummary
        {
            /// <summary>상태별 건수 — StatusNames와 같은 순서.</summary>
            internal readonly int[] StatusCounts = new int[5];

            /// <summary>발송 통보 없이 도착한 미확인 물류 건수(SEND_YN='N').</summary>
            internal int UnmatchedCount;

            /// <summary>수신 완료 &amp; 의뢰서 미연결(Received) 건수 — 경과 통계의 대상.</summary>
            internal int NoLinkCount;

            /// <summary>미연결 건 경과일 평균 (NoLinkCount가 0이면 0).</summary>
            internal double DaysAverage;

            /// <summary>미연결 건 경과일 최대 (NoLinkCount가 0이면 0).</summary>
            internal int DaysMax;
        }

        // ===== 파생 컬럼 =====

        /// <summary>
        /// 현황판에 워크리스트 파생 컬럼을 보장하고 전 행에 채운다:
        /// STATUS/STATUS_COLOR(파이프라인 상태 배지), SEND_YN/SEND_COLOR
        /// (발송 통보 여부 배지), ELAPSED_DAYS/DAYS_COLOR(미연결 행의 경과 강조),
        /// CHK(벌크 체크박스), RECV_CAN/CRT_CAN(행 단위 Receive/Create 버튼 활성).
        /// </summary>
        internal static void ApplyWorkflowColumns(DataTable board)
        {
            if (board == null)
            {
                return;
            }

            // 회사 DB가 일시를 "2026-07-12 18:15:38.0"처럼 소수점 초를 붙여
            // 내려도 화면엔 초까지만 통일해 보이도록 시각 컬럼을 정규화한다.
            NormalizeTimeColumns(board, "SEND_TM", "RECV_TM", "PROC_TM");

            EnsureColumn(board, "STATUS", typeof(string));
            EnsureColumn(board, "STATUS_COLOR", typeof(string));
            EnsureColumn(board, "SEND_YN", typeof(string));
            EnsureColumn(board, "SEND_COLOR", typeof(string));
            EnsureColumn(board, "ELAPSED_DAYS", typeof(int));
            EnsureColumn(board, "DAYS_COLOR", typeof(string));
            EnsureColumn(board, "CHK", typeof(bool));
            EnsureColumn(board, "RECV_CAN", typeof(bool));
            EnsureColumn(board, "CRT_CAN", typeof(bool));

            foreach (DataRow row in board.Rows)
            {
                ApplyStatus(row);

                if (row.IsNull("CHK"))
                {
                    row["CHK"] = false;
                }
            }
        }

        /// <summary>
        /// 한 행의 파이프라인 상태와 강조(상태 배지·경과일·버튼 활성)를 계산한다.
        /// </summary>
        internal static void ApplyStatus(DataRow row)
        {
            // 수신이 곧 도착(동시각) — RECV_TM/RECV_YN이 있으면 이미 수신된
            // 것이므로 Receive 대상이 아니다. 수신 후에는 배치가 의뢰서를
            // 연결(REQ_NO)하고, 연결되면 Create 대상이 된다.
            bool received = TableHelper.CellText(row, "RECV_YN").Trim() == "Y"
                    || TableHelper.CellText(row, "RECV_TM").Trim().Length > 0;
            bool linked = TableHelper.CellText(row, "REQ_NO").Trim().Length > 0;
            bool created = TableHelper.CellText(row, "PROC_YN").Trim() == "Y";

            // 발송 통보 여부 — 'Y'가 아니면 미확인('N')으로 정규화한다
            // (회사 조인 쿼리에서 FAC_SEND_MAS 쪽이 NULL인 행 포함).
            bool notified = TableHelper.CellText(row, "SEND_YN").Trim() == "Y";
            row["SEND_YN"] = notified ? "Y" : "N";
            row["SEND_COLOR"] = notified ? sendNotifiedColor : sendUnmatchedColor;

            int status;

            if (!received)
            {
                status = StatusTransit;
            }
            else if (!linked)
            {
                // 수신 후 1일 미만은 배치 대기(Received), 1일이 지나면
                // 연결 지연(Request Unlinked)으로 승격해 강조한다.
                int waitedDays = ElapsedDays(TableHelper.CellText(row, "RECV_TM").Trim());
                status = waitedDays >= unlinkedAfterDays ? StatusNoRequest : StatusReceived;
            }
            else if (!created)
            {
                status = StatusLinked;
            }
            else
            {
                status = StatusCompleted;
            }

            row["STATUS"] = StatusNames[status];
            row["STATUS_COLOR"] = statusColors[status];
            // Receive는 발송 통보가 있는(SEND_YN='Y') 미수신 건만 — 수신
            // 일시(RECV_TM)가 이미 있으면 활성화되지 않는다. 통보 없는
            // 아이템은 Manual Receive(전용 다이얼로그)로 처리한다.
            row["RECV_CAN"] = !received && notified;
            row["CRT_CAN"] = status == StatusLinked;

            // 경과일: 미수신(운송/수신 대기)은 발송 통보 시각 기준 — 지연을
            // 그대로 보여준다. 수신 후에는 "의뢰서 미연결" 행만 수신 시각
            // (RECV_TM) 기준으로 배치 대기 기간을 보여준다.
            string agingBasis = string.Empty;

            if (!received)
            {
                agingBasis = TableHelper.CellText(row, "SEND_TM").Trim();
            }
            else if (!linked)
            {
                agingBasis = TableHelper.CellText(row, "RECV_TM").Trim();
            }

            if (agingBasis.Length > 0)
            {
                int days = ElapsedDays(agingBasis);
                row["ELAPSED_DAYS"] = days;
                row["DAYS_COLOR"] = agingBadgeColors[AgingBand(days)];
            }
            else
            {
                row["ELAPSED_DAYS"] = DBNull.Value;
                row["DAYS_COLOR"] = string.Empty;
            }
        }

        // ===== 필터 + 집계 =====

        /// <summary>
        /// 상태 필터(StatusNames 값, "" = 전체) · 발송 통보 필터(""/Y/N) ·
        /// 경과일 최소값(0 = 전체)으로 현황판을 잘라낸다. 전부 클라이언트에서
        /// 처리한다 — 서버 조회는 조건 없이 원본만 준다. 경과일은 미연결 행만
        /// 값이 있으므로, 경과일 필터를 걸면 연결/미도착 행은 자연히 제외된다.
        /// </summary>
        internal static DataTable Filter(DataTable board, string statusFilter, string sendFilter, int minDays)
        {
            if (statusFilter.Length == 0 && sendFilter.Length == 0 && minDays <= 0)
            {
                return board;
            }

            DataTable filtered = board.Clone();

            foreach (DataRow row in board.Rows)
            {
                if (statusFilter.Length > 0 && TableHelper.CellText(row, "STATUS") != statusFilter)
                {
                    continue;
                }

                if (sendFilter.Length > 0 && TableHelper.CellText(row, "SEND_YN") != sendFilter)
                {
                    continue;
                }

                if (minDays > 0)
                {
                    if (row.IsNull("ELAPSED_DAYS")
                            || TableHelper.ParseInt(TableHelper.CellText(row, "ELAPSED_DAYS")) < minDays)
                    {
                        continue;
                    }
                }

                filtered.ImportRow(row);
            }

            return filtered;
        }

        /// <summary>
        /// KPI를 집계한다 — 상태별 건수(전체 현황), 미확인 물류 건수(SEND_YN='N'),
        /// 미연결 건의 경과 통계(Avg/Oldest).
        /// </summary>
        internal static LogisticsSummary Aggregate(DataTable result)
        {
            LogisticsSummary summary = new LogisticsSummary();

            if (result == null)
            {
                return summary;
            }

            int daysSum = 0;

            foreach (DataRow row in result.Rows)
            {
                int status = Array.IndexOf(StatusNames, TableHelper.CellText(row, "STATUS"));

                if (status >= 0)
                {
                    summary.StatusCounts[status] += 1;
                }

                if (TableHelper.CellText(row, "SEND_YN") == "N")
                {
                    summary.UnmatchedCount += 1;
                }

                // 운송 중 행의 경과일(발송 기준)은 미연결 통계에서 제외한다 —
                // Avg/Oldest는 "도착했는데 의뢰서가 없는" 건의 방치 기간 지표다.
                if (status != StatusTransit
                        && row.Table.Columns.Contains("ELAPSED_DAYS") && !row.IsNull("ELAPSED_DAYS"))
                {
                    summary.NoLinkCount += 1;

                    int days = TableHelper.ParseInt(TableHelper.CellText(row, "ELAPSED_DAYS"));
                    daysSum += days;

                    if (days > summary.DaysMax)
                    {
                        summary.DaysMax = days;
                    }
                }
            }

            if (summary.NoLinkCount > 0)
            {
                summary.DaysAverage = (double)daysSum / summary.NoLinkCount;
            }

            return summary;
        }

        // ===== 내부 헬퍼 =====
        // (CellText/ParseInt/FlagSet 같은 화면 무관 유틸은 Modern.Lab.Data.TableHelper로 승격됐다.)

        // 시각 컬럼 표기 정규화 — 회사 DB(Oracle TIMESTAMP)가 "2026-07-12
        // 18:15:38.0"처럼 소수점 초를 붙여 내려도 초까지만("yyyy-MM-dd
        // HH:mm:ss") 통일한다. 해석 불가 문자열은 손대지 않고 그대로 둔다.
        private static void NormalizeTimeColumns(DataTable board, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (!board.Columns.Contains(columnName))
                {
                    continue;
                }

                foreach (DataRow row in board.Rows)
                {
                    string text = TableHelper.CellText(row, columnName).Trim();
                    DateTime parsed;

                    if (text.Length > 0 && DateTime.TryParse(
                            text, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                    {
                        row[columnName] = parsed.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
            }
        }

        // 파생 컬럼이 없으면 만든다 — 서버가 이미 내려준 컬럼은 그대로 둔다.
        private static void EnsureColumn(DataTable table, string columnName, Type columnType)
        {
            if (!table.Columns.Contains(columnName))
            {
                table.Columns.Add(columnName, columnType);
            }
        }

        // 도착시각 → 오늘까지의 경과일 (해석 불가/미래 도착은 0일).
        private static int ElapsedDays(string arriveText)
        {
            DateTime arrived;

            if (!DateTime.TryParse(arriveText, out arrived))
            {
                return 0;
            }

            int days = (int)(DateTime.Today - arrived.Date).TotalDays;
            return days > 0 ? days : 0;
        }

        // 경과일 → 구간 인덱스 (0-2 / 3-6 / 7-13 / 14+).
        private static int AgingBand(int days)
        {
            if (days >= 14)
            {
                return 3;
            }

            if (days >= 7)
            {
                return 2;
            }

            if (days >= 3)
            {
                return 1;
            }

            return 0;
        }
    }
}
