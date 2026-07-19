using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Equipment / Lots — 장비그룹 단위 작업 지시(디스패칭) 화면. 작업준비(투입)·
    /// 작업시작·작업종료·반출을 전부 한 화면에서 처리한다. (용어는 데모용이며
    /// 회사 적용 시 보안 정책에 맞춰 치환한다.)
    ///
    /// 업무 흐름:
    /// 1) 장비그룹을 선택하면 그룹의 장비 리스트와 대기 Lot 큐(우선순위 순)가
    ///    함께 나온다.
    /// 2) 장비마다 인포트/아웃포트가 2/2·2/1·1/1 등으로 다르다 — 인포트가 하나
    ///    라도 비어 있으면(장비가 Run 중이어도) 그 포트로 투입할 수 있다.
    /// 3) 투입(Assign) → 작업시작(Start) → 작업종료(End: 아웃포트로 이동) →
    ///    반출(Unload: 아웃포트 비움) 순서로 진행한다.
    ///
    /// "한눈에 파악 + 제어"를 위한 구성 — **처리는 전부 우클릭 컨텍스트
    /// 메뉴**로 한다 (처리 종류가 많아 버튼 컬럼으로는 담을 수 없다). 그리드가
    /// 우클릭한 행을 먼저 선택한 뒤 메뉴를 띄우므로 대상이 항상 명확하고,
    /// 메뉴가 열릴 때 장비/포트 준비 상태로 항목 활성이 정해진다:
    /// - 장비 그리드: 상태 배지(Run/Idle/Down) + 통신 모드 배지(OnLineRemote
    ///   초록=자동 진행 / OnLineLocal 호박=수동 / OffLine 빨강=통신 끊김) +
    ///   포트 사용량 요약 배지(In "1/2" — 빈자리 있으면 파랑=투입 가능 / Out
    ///   "1/1" — 완료 있으면 초록=반출 필요). 우클릭 메뉴: Prepare(포트 지정
    ///   다이얼로그 → 최우선 Lot 투입) · Start · End(캐리어 슬롯별 판정
    ///   SUCC/FAIL 입력 다이얼로그 — 웨이퍼가 있는 슬롯 전부 입력해야 확정) ·
    ///   Unload All Done(완료 포트 전체 반출 — 단일 포트 반출은 포트 액션) ·
    ///   Down 설정/해제 · 통신 모드 전환 3종. 수동 Start/End는 OnLineLocal에서만 — Remote는
    ///   장비가 자동 통신으로 진행하고(수동은 Prepare만), 오류 시 작업자가
    ///   메뉴로 Local로 내려 수동 처리한다. 회사 화면은 장비 컬럼이 많으므로
    ///   포트 상세는 그리드에 넣지 않는다.
    /// - 진행 Lot 카드(장비 리스트 하단): 그룹 장비 포트에 올라가 있는 모든
    ///   Lot(투입됨/작업중/완료)을 Lot 관점으로 — "이 그룹에서 지금 무엇이
    ///   돌아가는가"를 한 줄씩 보여준다.
    /// - 포트 카드(우측 상단): 선택 장비의 포트 상세 — 포트당 한 행(Port/
    ///   State/Lot/Since), Empty 회색 / 투입됨 호박 / 작업중 파랑 / 완료 초록.
    ///   포트를 집어서 하는 처리는 포트 액션 레지스트리로 — 우클릭 메뉴와
    ///   하단 실행 카드의 Port Actions 드롭다운(장비 Actions 좌측, 동일
    ///   스타일)이 같은 정의에서 나온다: Load(이 인포트를 미리 선택한
    ///   작업준비) · Unload Lot(이 아웃포트만 반출) · Cancel Lot(투입/작업
    ///   취소 — Lot이 대기 큐 최우선으로 복귀).
    /// - 대기 Lot 그리드(우측 하단): 우선순위 순. ↑↓ 버튼(순위 변경 — 직접
    ///   조작이 자연스러워 버튼으로 남긴 유일한 것)과 우클릭 메뉴 Assign
    ///   (특정 Lot을 선택 장비에 지정 투입 — 표준 동선은 장비 메뉴의 Prepare).
    /// - 하단 KPI: Run/Idle/Down 장비 수 · 빈 인포트 수 · 대기 Lot 수.
    /// - 하단 우측 실행 카드: Port Actions(선택 포트 대상) + Actions(선택
    ///   장비 대상) 드롭다운 — 각각 우클릭 메뉴와 같은 처리 목록의 두 번째
    ///   진입점 (발견성 + 하단 마우스 동선용).
    ///
    /// 처리 규칙: **장비와 포트가 모두 준비되어야 한다** — 작업준비(투입)는
    /// Down 아님 + 빈 인포트 + 쓸 수 있는(빈+미예약) 아웃포트를 **지정**해야
    /// 하고, 지정된 아웃포트는 그 작업에 예약된다. 시작은 여기에 더해
    /// **작업중 아님**(작업은 장비당 한 번에 하나), 종료는 지정 아웃포트가
    /// 비어 있어야 한다(점유돼 있으면 반출 먼저).
    ///
    /// 처리 흐름은 Pending Requests와 동일 패턴이다: 서버 처리(검증 + 시각
    /// 적재는 서버 역할인 EquipmentLotSimulator) → 성공 시 **재조회** → 장비
    /// 행 포커스 복원. 실패 사유는 토스트로 보여준다.
    ///
    /// 자동 갱신: 자동 작업(서버측)의 상태 변화를 반영하기 위해 주기 재조회
    /// 타이머를 돌린다 — 기본 15초, 화면에서 Off~60초 조절. 갱신은 수동
    /// Refresh와 동일하게 포커스/컬럼 필터를 유지하며, 다이얼로그·컨텍스트
    /// 메뉴가 열려 있는 동안은 건너뛴다 (조작 방해 금지).
    ///
    /// ★ 회사 환경 교체 지점 — 조회 2개(GetEquipments/GetWaitingLots)와 처리
    ///   4개(AssignLot/StartJob/EndJob/Unload)를 회사 장비 인터페이스 호출로
    ///   바꾸고 EquipmentLotSimulator를 지운다. 상태/포트 배지/버튼 활성/KPI
    ///   파생은 전부 클라이언트(EquipmentTablePresenter)가 처리한다.
    /// </summary>
    public partial class EquipmentLotForm : Form
    {
        // 마지막 조회의 장비 현황 (그리드 바인딩 + KPI 원천).
        private DataTable equipmentData;

        // 마지막 조회의 대기 Lot 큐 (우선순위 순).
        private DataTable lotData;

        // 선택 장비가 투입 가능(Down 아님 + 빈 인포트)한지 — Lot 메뉴의
        // Assign 활성과 대기 카드 타이틀의 원천 (선택 변경 시 갱신).
        private bool lotAssignable;

        // 모달 다이얼로그(작업준비 등)가 떠 있는 동안 자동 갱신을 멈추는 플래그 —
        // WinForms Timer는 모달 메시지 루프에서도 Tick이 계속 오기 때문이다.
        private bool dialogOpen;

        // 장비/대기 Lot 재조회 진행 여부와 요청 버전 — 회사 인터페이스가
        // 느려도 UI를 멈추지 않고, 뒤늦은 이전 응답은 화면에 반영하지 않는다.
        private bool searchInProgress;
        private int searchVersion;

        // 마지막 갱신 시각 — 장비 카드 타이틀 우측 인디케이터("Updated …
        // · next Ns")와 다음 갱신 카운트다운의 기준.
        private DateTime lastRefreshTime;

        // 작업중 Lot 수와 최장 작업 시작 시각 — 진행 Lot 카드 타이틀 우측의
        // 경과시간 인디케이터("Running N · oldest mm:ss") 원천. 1초 타이머가
        // 경과를 실시간으로 표기한다.
        private int runningLotCount;
        private DateTime oldestRunningStart;

        // ===== 장비 처리 액션 정의 (컨텍스트 메뉴 + 드롭다운 공용) =====

        // 한 건의 처리 — 라벨/활성 판정/실행을 한 곳에 정의한다. 컨텍스트
        // 메뉴와 Actions 드롭다운이 모두 이 목록 하나에서 만들어지므로,
        // 처리를 늘릴 때 Build…Actions에 한 항목만 추가하면 두 진입점에
        // 함께 나타나고 실행도 같은 로직을 탄다. 장비 액션(대상 = 선택 장비
        // 행)과 포트 액션(대상 = 선택 포트 행)이 같은 모양을 공유한다.
        private sealed class EquipmentAction
        {
            /// <summary>액션 식별 키 (메뉴 Tag / 드롭다운 VALUE 공용).</summary>
            internal string Key;

            /// <summary>표시 라벨.</summary>
            internal string Label;

            /// <summary>메뉴에서 이 항목 앞에 구분선을 넣을지 여부.</summary>
            internal bool SeparatorBefore;

            /// <summary>대상 행(장비 액션은 장비 행, 포트 액션은 포트 행)
            /// 기준 실행 가능 판정 (메뉴 활성).</summary>
            internal Func<DataRowView, bool> CanExecute;

            /// <summary>실행 로직 — ★ 회사 환경 교체 지점은 각 실행 메서드
            /// 본문의 시뮬레이터 호출부다.</summary>
            internal Action Execute;
        }

        // 장비 처리 목록 — 진입점(메뉴/드롭다운)과 실행의 단일 원천.
        private List<EquipmentAction> equipmentActions;

        // 포트 처리 목록 — 포트 컨텍스트 메뉴 + 포트 카드 드롭다운의 원천.
        private List<EquipmentAction> portActions;

        private void BuildEquipmentActions()
        {
            this.equipmentActions = new List<EquipmentAction>();

            this.AddEquipmentAction("PREPARE", "Prepare (Assign Top Lot)", false,
                    this.CanPrepare,
                    this.PrepareTopLot);
            this.AddEquipmentAction("START", "Start Job", false,
                    delegate(DataRowView row) { return PendingTablePresenter.FlagSet(row.Row, "START_CAN"); },
                    delegate { this.RunEquipmentAction("START"); });
            this.AddEquipmentAction("END", "End Job", false,
                    delegate(DataRowView row) { return PendingTablePresenter.FlagSet(row.Row, "END_CAN"); },
                    this.RunEndDialog);
            // 장비 단위 일괄 반출 — 완료(Done) 아웃포트 전체를 한 번에 비운다.
            // 특정 포트 하나만 반출하는 것은 포트 액션(Unload Lot)이 한다.
            this.AddEquipmentAction("UNLOAD", "Unload All Done", false,
                    delegate(DataRowView row) { return PendingTablePresenter.FlagSet(row.Row, "UNLOAD_CAN"); },
                    delegate { this.RunEquipmentAction("UNLOAD"); });
            this.AddEquipmentAction("DOWN", "Set Down", true,
                    delegate(DataRowView row) { return PendingTablePresenter.CellText(row.Row, "STATE") != "Down"; },
                    delegate { this.ApplyDownAction(true); });
            this.AddEquipmentAction("UP", "Release Down", false,
                    delegate(DataRowView row) { return PendingTablePresenter.CellText(row.Row, "STATE") == "Down"; },
                    delegate { this.ApplyDownAction(false); });

            // 통신 모드 전환 — 작업자 판단으로 수시 변경한다 (예: Remote 자동
            // 진행 중 오류 → Local로 내려 수동 처리). 현재 모드 항목은 비활성.
            this.AddEquipmentAction("COMM_LOCAL", "OnLine Local", true,
                    delegate(DataRowView row) { return PendingTablePresenter.CellText(row.Row, "COMM_MODE") != "OnLineLocal"; },
                    delegate { this.ApplyCommModeAction("OnLineLocal"); });
            this.AddEquipmentAction("COMM_REMOTE", "OnLine Remote", false,
                    delegate(DataRowView row) { return PendingTablePresenter.CellText(row.Row, "COMM_MODE") != "OnLineRemote"; },
                    delegate { this.ApplyCommModeAction("OnLineRemote"); });
            this.AddEquipmentAction("COMM_OFFLINE", "OffLine", false,
                    delegate(DataRowView row) { return PendingTablePresenter.CellText(row.Row, "COMM_MODE") != "OffLine"; },
                    delegate { this.ApplyCommModeAction("OffLine"); });
        }

        private void AddEquipmentAction(
                string key, string label, bool separatorBefore,
                Func<DataRowView, bool> canExecute, Action execute)
        {
            this.equipmentActions.Add(
                    MakeAction(key, label, separatorBefore, canExecute, execute));
        }

        private static EquipmentAction MakeAction(
                string key, string label, bool separatorBefore,
                Func<DataRowView, bool> canExecute, Action execute)
        {
            EquipmentAction action = new EquipmentAction();
            action.Key = key;
            action.Label = label;
            action.SeparatorBefore = separatorBefore;
            action.CanExecute = canExecute;
            action.Execute = execute;
            return action;
        }

        // 포트 처리 목록 — 포트를 집어서 하는 처리(이 포트에 투입 / 이 포트만
        // 반출 / 이 포트 취소)를 정의한다. 장비 단위 처리(Start/End/전체
        // Unload)는 장비 액션에 남는다.
        private void BuildPortActions()
        {
            this.portActions = new List<EquipmentAction>();

            this.portActions.Add(MakeAction("LOAD", "Load (Assign Top Lot)", false,
                    this.CanLoadPort,
                    delegate { this.LoadPortRow(this.gridPorts.SelectedItem as DataRowView); }));
            this.portActions.Add(MakeAction("UNLOAD_PORT", "Unload Lot", false,
                    delegate(DataRowView row) { return PendingTablePresenter.FlagSet(row.Row, "UNLOAD_CAN"); },
                    delegate { this.UnloadPortRow(this.gridPorts.SelectedItem as DataRowView); }));
            this.portActions.Add(MakeAction("CANCEL", "Cancel Lot", true,
                    delegate(DataRowView row) { return PendingTablePresenter.FlagSet(row.Row, "CANCEL_CAN"); },
                    delegate { this.CancelPortRow(this.gridPorts.SelectedItem as DataRowView); }));
        }

        // 이 인포트에 투입 가능 — 포트 파생 플래그(빈 인포트 + 장비 준비)에
        // 대기 Lot 존재를 더한다 (플래그는 프레젠터가 포트 행에 파생한다).
        private bool CanLoadPort(DataRowView row)
        {
            int waiting = this.lotData != null ? this.lotData.Rows.Count : 0;
            return PendingTablePresenter.FlagSet(row.Row, "LOAD_CAN") && waiting > 0;
        }

        // 포트 액션 목록으로 두 진입점을 구성한다 — 포트 컨텍스트 메뉴와
        // 포트 카드의 Port Actions 드롭다운이 같은 정의에서 나온다.
        private void PopulatePortEntryPoints()
        {
            this.menuPort.Items.Clear();

            foreach (EquipmentAction action in this.portActions)
            {
                if (action.SeparatorBefore)
                {
                    this.menuPort.Items.Add(new ToolStripSeparator());
                }

                ToolStripMenuItem item = new ToolStripMenuItem(action.Label);
                item.Tag = action.Key;
                item.Click += this.OnPortMenuItemClick;
                this.menuPort.Items.Add(item);
            }

            this.ddbPortActions.DisplayMember = "LABEL";
            this.ddbPortActions.ValueMember = "VALUE";
            this.ddbPortActions.EnabledMember = "CAN";
            this.UpdatePortActionStates();
        }

        // 포트 카드 드롭다운의 항목 활성을 선택 포트 기준으로 갱신한다 —
        // 컨텍스트 메뉴와 같은 CanExecute 판정을 쓴다.
        private void UpdatePortActionStates()
        {
            if (this.portActions == null)
            {
                return;
            }

            DataRowView row = this.gridPorts.SelectedItem as DataRowView;

            DataTable actionTable = new DataTable();
            actionTable.Columns.Add("VALUE", typeof(string));
            actionTable.Columns.Add("LABEL", typeof(string));
            actionTable.Columns.Add("CAN", typeof(bool));

            foreach (EquipmentAction action in this.portActions)
            {
                actionTable.Rows.Add(
                        action.Key, action.Label, row != null && action.CanExecute(row));
            }

            this.ddbPortActions.DataSource = actionTable;
        }

        // 포트 진입점 공용 디스패처.
        private void ExecutePortAction(string key)
        {
            if (!this.CanRunActionWhileRefreshing())
            {
                return;
            }

            foreach (EquipmentAction action in this.portActions)
            {
                if (action.Key == key)
                {
                    action.Execute();
                    return;
                }
            }
        }

        // 액션 목록으로 두 진입점을 구성한다 — 컨텍스트 메뉴 항목과 하단
        // Actions 드롭다운이 같은 정의에서 나온다.
        private void PopulateEquipmentEntryPoints()
        {
            this.menuEqp.Items.Clear();

            foreach (EquipmentAction action in this.equipmentActions)
            {
                if (action.SeparatorBefore)
                {
                    this.menuEqp.Items.Add(new ToolStripSeparator());
                }

                ToolStripMenuItem item = new ToolStripMenuItem(action.Label);
                item.Tag = action.Key;
                item.Click += this.OnEquipmentMenuItemClick;
                this.menuEqp.Items.Add(item);
            }

            this.ddbActions.DisplayMember = "LABEL";
            this.ddbActions.ValueMember = "VALUE";
            this.ddbActions.EnabledMember = "CAN";
            this.UpdateActionStates();
        }

        // 하단 Actions 드롭다운의 항목 활성을 선택 장비 기준으로 갱신한다 —
        // 컨텍스트 메뉴(열릴 때 계산)와 같은 CanExecute 판정을 쓰므로 두
        // 진입점의 활성/비활성이 항상 일치한다.
        private void UpdateActionStates()
        {
            // 초기화 순서 방어 — 액션 정의가 만들어지기 전의 호출은 무시한다.
            if (this.equipmentActions == null)
            {
                return;
            }

            DataRowView row = this.gridEqp.SelectedItem as DataRowView;

            DataTable actionTable = new DataTable();
            actionTable.Columns.Add("VALUE", typeof(string));
            actionTable.Columns.Add("LABEL", typeof(string));
            actionTable.Columns.Add("CAN", typeof(bool));

            foreach (EquipmentAction action in this.equipmentActions)
            {
                actionTable.Rows.Add(
                        action.Key, action.Label, row != null && action.CanExecute(row));
            }

            this.ddbActions.DataSource = actionTable;
        }

        // 두 진입점 공용 디스패처 — 어디서 눌러도 같은 실행 로직을 탄다.
        private void ExecuteEquipmentAction(string key)
        {
            if (!this.CanRunActionWhileRefreshing())
            {
                return;
            }

            foreach (EquipmentAction action in this.equipmentActions)
            {
                if (action.Key == key)
                {
                    action.Execute();
                    return;
                }
            }
        }

        public EquipmentLotForm()
        {
            this.InitializeComponent();

            // 로딩 커버 한 줄 — 폼 스스로 오픈 시 깜빡임을 가린다.
            Modern.Lab.WinForms.Controls.Hosting.ModernLoadCover.Attach(this);
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            // 장비 그리드: 상태 + 포트 사용량 요약 배지 + 행 단위 Start/End/
            // Unload. 포트 상세는 선택 장비의 포트 카드에서 보여준다 — 회사
            // 화면은 여기에 장비 정보 컬럼이 더 붙는다.
            this.gridEqp.ConfigureColumns(
                new ModernDataGridColumn("EQP_ID", "Equipment"),
                new ModernDataGridColumn("STATE", "State")
                {
                    Kind = GridColumnKind.Badge,
                    BadgeColorMember = "STATE_COLOR",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("COMM_MODE", "Comm", 116d)
                {
                    Kind = GridColumnKind.Badge,
                    BadgeColorMember = "COMM_COLOR",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("IN_USE", "In Ports")
                {
                    Kind = GridColumnKind.Badge,
                    BadgeColorMember = "IN_USE_COLOR",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("OUT_USE", "Out Ports")
                {
                    Kind = GridColumnKind.Badge,
                    BadgeColorMember = "OUT_USE_COLOR",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("RUN_TM", "Started") { TextAlignment = GridTextAlignment.Center });

            // 선택 장비 포트 상세: 포트당 한 행 — 상태 배지 색이 장비 그리드
            // 요약 배지와 같은 규칙을 쓴다. 취소는 포트 행 우클릭 메뉴로 한다
            // (투입됨/작업중 인포트만 활성).
            // 포트 번호는 장비 전체 연속(1,2 = In / 3,4 = Out)이고 구분은 Type
            // 컬럼이 한다. To는 지정 아웃포트의 포트 번호(화면 파생 — 서버는
            // 지정 인덱스 숫자만 준다).
            this.gridPorts.ConfigureColumns(
                new ModernDataGridColumn("PORT_NO", "Port") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("PORT_TYPE", "Type") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("PORT_STAT", "State")
                {
                    Kind = GridColumnKind.Badge,
                    BadgeColorMember = "PORT_COLOR",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("LOT_ID", "Lot ID"),
                new ModernDataGridColumn("CARRIER", "Carrier"),
                new ModernDataGridColumn("TO_PORT", "To") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("CANCEL_ACTION", "", 66d)
                {
                    Kind = GridColumnKind.Button,
                    ButtonText = "Cancel",
                    ButtonEnabledMember = "CANCEL_CAN",
                    TextAlignment = GridTextAlignment.Center
                });

            // 진행 중 Lot: 이 그룹 장비 포트에 올라가 있는 모든 Lot을 Lot
            // 관점으로 — 투입됨 호박 / 작업중 파랑 / 완료 초록.
            this.gridRun.ConfigureColumns(
                new ModernDataGridColumn("LOT_ID", "Lot ID"),
                new ModernDataGridColumn("CARRIER", "Carrier"),
                new ModernDataGridColumn("EQP_ID", "Equipment"),
                new ModernDataGridColumn("PORT", "Port") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("JOB_STAT", "State")
                {
                    Kind = GridColumnKind.Badge,
                    BadgeColorMember = "JOB_COLOR",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("EVENT_TM", "Since") { TextAlignment = GridTextAlignment.Center });

            // 대기 Lot 그리드: 우선순위 순 + ↑↓(우선순위 변경 — 맨 위/아래는
            // 비활성) + Assign 행 버튼(선택 장비가 투입 가능할 때만 활성 —
            // 우클릭 메뉴와 같은 로직). 표준 동선은 장비 메뉴의 Prepare.
            this.gridLots.ConfigureColumns(
                new ModernDataGridColumn("PRIORITY", "Prio")
                {
                    Kind = GridColumnKind.Badge,
                    BadgeColorMember = "PRIO_COLOR",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("LOT_ID", "Lot ID"),
                new ModernDataGridColumn("CARRIER", "Carrier"),
                new ModernDataGridColumn("QTY", "Qty") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("UP_ACTION", "", 38d)
                {
                    Kind = GridColumnKind.Button,
                    ButtonText = "↑",
                    ButtonEnabledMember = "UP_CAN",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("DOWN_ACTION", "", 38d)
                {
                    Kind = GridColumnKind.Button,
                    ButtonText = "↓",
                    ButtonEnabledMember = "DOWN_CAN",
                    TextAlignment = GridTextAlignment.Center
                },
                new ModernDataGridColumn("ASSIGN_ACTION", "", 66d)
                {
                    Kind = GridColumnKind.Button,
                    ButtonText = "Assign",
                    ButtonEnabledMember = "ASSIGN_CAN",
                    TextAlignment = GridTextAlignment.Center
                });

            // 장비 처리 진입점(컨텍스트 메뉴 + 하단 Actions 드롭다운)을 단일
            // 액션 정의에서 구성한다 — 처리를 늘릴 때 BuildEquipmentActions만
            // 고치면 두 진입점이 함께 갱신된다.
            // 반드시 그룹 콤보 할당(첫 조회 트리거)보다 먼저 — 조회 반영이
            // UpdateActionStates로 액션 목록을 읽기 때문이다.
            this.BuildEquipmentActions();
            this.PopulateEquipmentEntryPoints();
            this.BuildPortActions();
            this.PopulatePortEntryPoints();

            // 장비그룹 콤보 — ★ 회사 적용 시 장비그룹 조회로 교체한다.
            // DataSource 할당이 SelectedIndexChanged를 한 번 발생시키므로
            // (드롭인 계약) 첫 조회가 자동으로 실행된다.
            DataTable groupTable = new DataTable();
            groupTable.Columns.Add("VALUE", typeof(string));

            foreach (string code in EquipmentLotSimulator.GroupCodes)
            {
                groupTable.Rows.Add(code);
            }

            this.cboGroup.DisplayMember = "VALUE";
            this.cboGroup.ValueMember = "VALUE";
            this.cboGroup.DataSource = groupTable;

            // 자동 갱신 주기 — 자동 작업(서버측)의 상태 변화를 반영한다.
            // 기본 15초: 장비 상태 변화는 수십 초~분 단위라 이 정도면 충분히
            // 따라가고, 조회 부하(그룹당 2건)와 조작 방해를 최소로 유지한다.
            // 현장 특성에 맞춰 화면에서 Off~60초로 조절한다.
            DataTable refreshTable = new DataTable();
            refreshTable.Columns.Add("SEC", typeof(int));
            refreshTable.Columns.Add("LABEL", typeof(string));
            refreshTable.Rows.Add(0, "Off");
            refreshTable.Rows.Add(5, "5 sec");
            refreshTable.Rows.Add(10, "10 sec");
            refreshTable.Rows.Add(15, "15 sec");
            refreshTable.Rows.Add(30, "30 sec");
            refreshTable.Rows.Add(60, "60 sec");

            this.cboRefresh.DisplayMember = "LABEL";
            this.cboRefresh.ValueMember = "SEC";
            this.cboRefresh.DataSource = refreshTable;
            this.cboRefresh.SelectedValue = 15;

            // 갱신 인디케이터(장비 카드 타이틀 우측)를 1초 간격으로 갱신한다.
            this.timerCountdown.Start();
        }

        // ===== 자동 갱신 =====

        // 주기 선택이 바뀌면 타이머에 반영한다 (0 = Off). 새 주기는 지금부터
        // 계산되도록 타이머를 재시작한다.
        private void OnRefreshIntervalChanged(object sender, EventArgs e)
        {
            object value = this.cboRefresh.SelectedValue;
            int seconds = value is int ? (int)value : 0;

            this.timerRefresh.Stop();

            if (seconds > 0)
            {
                this.timerRefresh.Interval = seconds * 1000;
                this.timerRefresh.Start();
            }

            this.UpdateRefreshIndicator();
        }

        private void OnCountdownTick(object sender, EventArgs e)
        {
            this.UpdateRefreshIndicator();
            this.UpdateRunIndicator();
        }

        // 조회 결과에서 작업중 Lot 수와 최장 작업 시작 시각을 뽑는다 —
        // 경과시간 인디케이터의 원천 (경과 자체는 1초 타이머가 계산).
        private void UpdateRunningStats()
        {
            this.runningLotCount = 0;
            this.oldestRunningStart = DateTime.MinValue;

            if (this.equipmentData == null)
            {
                return;
            }

            foreach (DataRow row in this.equipmentData.Rows)
            {
                for (int index = 1; index <= 2; index++)
                {
                    if (PendingTablePresenter.CellText(row, "IN" + index + "_STAT").Trim() != "Running")
                    {
                        continue;
                    }

                    this.runningLotCount = this.runningLotCount + 1;

                    DateTime started;

                    if (DateTime.TryParse(
                            PendingTablePresenter.CellText(row, "IN" + index + "_TM"), out started)
                            && (this.oldestRunningStart == DateTime.MinValue
                                    || started < this.oldestRunningStart))
                    {
                        this.oldestRunningStart = started;
                    }
                }
            }
        }

        // 진행 Lot 카드 타이틀 우측 — 작업중 Lot 수와 최장 경과시간을 실시간
        // 표기한다 ("Running 3 · oldest 07:42"). 자동 작업 데모(60초 후 시작 /
        // 180초 후 종료)와 자동 갱신이 맞물려 수치가 살아 움직인다.
        private void UpdateRunIndicator()
        {
            if (this.runningLotCount == 0)
            {
                this.runCard.TitleRightText = "No running lot";
                return;
            }

            TimeSpan elapsed = DateTime.Now - this.oldestRunningStart;

            if (elapsed.TotalSeconds < 0d)
            {
                elapsed = TimeSpan.Zero;
            }

            string elapsedText = elapsed.TotalHours >= 1d
                    ? ((int)elapsed.TotalHours) + ":" + elapsed.Minutes.ToString("00")
                            + ":" + elapsed.Seconds.ToString("00")
                    : elapsed.Minutes.ToString("00") + ":" + elapsed.Seconds.ToString("00");

            this.runCard.TitleRightText = "Running " + this.runningLotCount.ToString("N0")
                    + " · oldest " + elapsedText;
        }

        // 장비 카드 타이틀 우측 인디케이터 — 마지막 갱신 시각과 다음 자동
        // 갱신까지 남은 초를 보여준다. 다이얼로그/메뉴로 갱신이 멈춘 동안은
        // paused로 표기해 "왜 안 도는지"가 보이게 한다.
        private void UpdateRefreshIndicator()
        {
            if (this.lastRefreshTime == DateTime.MinValue)
            {
                this.eqpCard.TitleRightText = string.Empty;
                return;
            }

            string text = "Updated "
                    + this.lastRefreshTime.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

            if (!this.timerRefresh.Enabled)
            {
                text = text + " · auto off";
            }
            else if (!this.CanAutoRefresh())
            {
                text = text + " · paused";
            }
            else
            {
                double remaining = (this.timerRefresh.Interval / 1000d)
                        - (DateTime.Now - this.lastRefreshTime).TotalSeconds;
                int seconds = (int)Math.Max(0d, Math.Ceiling(remaining));
                text = text + " · next " + seconds + "s";
            }

            this.eqpCard.TitleRightText = text;
        }

        // 자동 갱신 — 수동 Refresh와 동일하게 선택 장비 포커스를 유지한 채
        // 재조회한다 (그리드 컬럼 필터도 컨트롤이 유지한다). 사용자가 조작
        // 중인 순간은 건너뛴다.
        private void OnAutoRefreshTick(object sender, EventArgs e)
        {
            if (!this.CanAutoRefresh())
            {
                return;
            }

            this.ExecuteSearch(this.GetFocusedEqpId());
        }

        // 자동 갱신을 잠시 멈춰야 하는 상황 — 모달 다이얼로그(작업준비)나
        // 컨텍스트 메뉴가 열려 있으면 재바인딩이 조작을 끊으므로 건너뛴다.
        private bool CanAutoRefresh()
        {
            if (!this.Visible || this.dialogOpen)
            {
                return false;
            }

            if (this.searchInProgress || this.ddbActions.IsDropDownOpen
                    || this.ddbPortActions.IsDropDownOpen)
            {
                return false;
            }

            if (this.menuEqp.Visible || this.menuPort.Visible || this.menuLot.Visible)
            {
                return false;
            }

            return true;
        }

        // 조회 스냅샷을 백그라운드에서 읽는 동안에는 데모 시뮬레이터/회사
        // 서버 상태를 바꾸는 액션을 시작하지 않는다. 이 구간을 막아야 읽기와
        // 처리 요청이 섞여 오래된 화면이나 경쟁 상태가 생기지 않는다.
        private bool CanRunActionWhileRefreshing()
        {
            if (!this.searchInProgress)
            {
                return true;
            }

            this.toastMain.Show("Refresh is in progress. Try again shortly.", ToastKind.Info);
            return false;
        }

        // 컨텍스트 메뉴 항목 클릭 — 공용 디스패처로 넘긴다.
        private void OnEquipmentMenuItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;

            if (item != null)
            {
                this.ExecuteEquipmentAction(item.Tag as string);
            }
        }

        // 하단 Actions 드롭다운 항목 클릭 — 같은 디스패처를 탄다. 항목별
        // 활성은 UpdateActionStates의 CAN 컬럼으로 메뉴와 동일하게 제어하고,
        // 서버 검증 실패 사유는 실행 로직이 토스트로 보완한다.
        private void OnActionMenuClicked(
                object sender, Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs e)
        {
            this.ExecuteEquipmentAction(e.Value as string);
        }

        // 현재 선택된 장비그룹 코드 ("" = 미선택).
        private string GetGroup()
        {
            string value = this.cboGroup.SelectedValue as string;
            return value ?? string.Empty;
        }

        // 현황판에서 현재 포커스된 장비 ID ("" = 선택 없음).
        private string GetFocusedEqpId()
        {
            DataRowView row = this.gridEqp.SelectedItem as DataRowView;
            return row == null ? string.Empty : PendingTablePresenter.CellText(row.Row, "EQP_ID");
        }

        // ===== 조회 =====

        private void OnGroupChanged(object sender, EventArgs e)
        {
            // 그룹이 바뀌면 처음부터 — 포커스 유지 없이 새 조회.
            this.ExecuteSearch(null);
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            // 새로고침은 보던 장비를 그대로 보게 포커스를 유지한다.
            this.ExecuteSearch(this.GetFocusedEqpId());
        }

        // 그룹의 장비 현황 + 대기 Lot 큐를 다시 받아 화면 전체를 갱신한다.
        // 처리(투입/시작/종료/반출) 성공 후에도 이 재조회 하나로 반영한다 —
        // 처리 시각·포트 이동은 서버가 적재하므로 화면은 결과를 보여줄 뿐이다.
        // focusEqpId: 재조회 후 되돌릴 장비 행 (null/""면 첫 행 선택).
        private async void ExecuteSearch(string focusEqpId, string focusLotId = null)
        {
            string group = this.GetGroup();

            if (group.Length == 0)
            {
                return;
            }

            this.searchVersion = this.searchVersion + 1;
            int version = this.searchVersion;
            this.searchInProgress = true;

            try
            {
                // ★ 회사 환경 교체 지점 — 장비 현황/대기 큐 조회를 회사 장비
                // 인터페이스 호출로 바꾼다. 두 조회는 서버 부하를 예측 가능하게
                // 유지하도록 하나의 백그라운드 작업에서 순서대로 실행한다.
                DataTable[] results = await Task.Run(() => new DataTable[]
                {
                    EquipmentLotSimulator.GetEquipments(group),
                    EquipmentLotSimulator.GetWaitingLots(group)
                });

                if (this.IsDisposed || version != this.searchVersion)
                {
                    return;
                }

                this.equipmentData = results[0];
                this.lotData = results[1];

                EquipmentTablePresenter.ApplyEquipmentColumns(this.equipmentData);
                EquipmentTablePresenter.ApplyLotColumns(this.lotData);

                // 장비 바인딩의 첫 행 자동 선택이 SelectionChanged를 태우므로,
                // 그 시점에 읽는 lotData를 먼저 준비해 두고 바인딩한다.
                this.gridEqp.DataSource = this.equipmentData;
                this.gridLots.DataSource = this.lotData;
                this.gridRun.DataSource = EquipmentTablePresenter.BuildRunningLots(this.equipmentData);

                if (!string.IsNullOrEmpty(focusEqpId))
                {
                    this.FocusEquipmentRow(focusEqpId);
                }

                this.UpdatePortPanel();
                this.UpdateAssignability();
                this.RefreshSummary();
                this.UpdateRunningStats();
                this.UpdateRunIndicator();

                if (!string.IsNullOrEmpty(focusLotId))
                {
                    this.FocusLotRow(focusLotId);
                }

                // 갱신 시각 기록 + 자동 갱신 주기를 지금부터 다시 계산한다 —
                // 수동 Refresh/처리 직후에 곧바로 자동 갱신이 겹치지 않게 한다.
                this.lastRefreshTime = DateTime.Now;

                if (this.timerRefresh.Enabled)
                {
                    this.timerRefresh.Stop();
                    this.timerRefresh.Start();
                }

                this.UpdateRefreshIndicator();
            }
            catch (Exception ex)
            {
                if (this.IsDisposed || version != this.searchVersion)
                {
                    return;
                }

                this.toastMain.Show("Server call failed: " + ex.Message, ToastKind.Error);
            }
            finally
            {
                if (!this.IsDisposed && version == this.searchVersion)
                {
                    this.searchInProgress = false;
                    this.UpdateRefreshIndicator();
                }
            }
        }

        // 재조회 후 장비 행 포커스를 복원한다 (장비 리스트는 페이지가 없다).
        private void FocusEquipmentRow(string eqpId)
        {
            for (int index = 0; index < this.equipmentData.Rows.Count; index++)
            {
                if (PendingTablePresenter.CellText(this.equipmentData.Rows[index], "EQP_ID") == eqpId)
                {
                    this.gridEqp.SelectedIndex = index;
                    return;
                }
            }
        }

        // 대기 Lot 그리드에서 지정 Lot 행을 선택한다 — 우선순위 이동이나 취소
        // 복귀 후 하이라이트가 그 Lot을 따라가 시선이 끊기지 않게 한다.
        private void FocusLotRow(string lotId)
        {
            if (this.lotData == null)
            {
                return;
            }

            for (int index = 0; index < this.lotData.Rows.Count; index++)
            {
                if (PendingTablePresenter.CellText(this.lotData.Rows[index], "LOT_ID") == lotId)
                {
                    this.gridLots.SelectedIndex = index;
                    return;
                }
            }
        }

        // ===== 선택 → 투입 가능 판정 =====

        private void OnEqpSelectionChanged(object sender, EventArgs e)
        {
            this.UpdatePortPanel();
            this.UpdateAssignability();
        }

        // 선택 장비의 포트 상세를 포트 카드에 보여준다 — 어느 포트가 비었고
        // 어느 포트에 완료 Lot이 있는지 투입/반출 직전에 확인하는 자리다.
        private void UpdatePortPanel()
        {
            DataRowView row = this.gridEqp.SelectedItem as DataRowView;

            if (row == null)
            {
                this.gridPorts.DataSource = null;
                this.portCard.Text = "Ports";
                this.UpdatePortActionStates();
                return;
            }

            this.gridPorts.DataSource = EquipmentTablePresenter.BuildPortRows(row.Row);
            this.portCard.Text = "Ports — "
                    + PendingTablePresenter.CellText(row.Row, "EQP_ID");
            this.UpdatePortActionStates();
        }

        // 선택 장비의 투입 가능 여부(Down 아님 + 빈 인포트)를 대기 카드
        // 타이틀과 Lot 메뉴(Assign) 활성에 반영한다 — 어느 장비로 투입되는지가
        // 항상 눈에 보이게 한다.
        private void UpdateAssignability()
        {
            DataRowView row = this.gridEqp.SelectedItem as DataRowView;
            bool assignable = false;
            string title = "Select an equipment";

            if (row != null)
            {
                string eqpId = PendingTablePresenter.CellText(row.Row, "EQP_ID");
                string state = PendingTablePresenter.CellText(row.Row, "STATE");
                int freeIn = PendingTablePresenter.ParseDays(
                        PendingTablePresenter.CellText(row.Row, "FREE_IN"));
                int freeOut = PendingTablePresenter.ParseDays(
                        PendingTablePresenter.CellText(row.Row, "FREE_OUT"));

                if (state == "Down")
                {
                    title = eqpId + " — down";
                }
                else if (PendingTablePresenter.CellText(row.Row, "COMM_MODE").Trim() == "OffLine")
                {
                    title = eqpId + " — offline";
                }
                else if (freeIn > 0 && freeOut > 0)
                {
                    assignable = true;
                    title = "→ " + eqpId + " · in " + freeIn.ToString("N0")
                            + " / out " + freeOut.ToString("N0") + " free";
                }
                else if (freeIn == 0)
                {
                    title = eqpId + " — no free in-port";
                }
                else
                {
                    title = eqpId + " — no free out-port";
                }
            }

            this.lotAssignable = assignable;
            EquipmentTablePresenter.SetAssignable(this.lotData, assignable);
            this.lotCard.TitleRightText = title;
            this.UpdateActionStates();
        }

        // ===== KPI =====

        private void RefreshSummary()
        {
            EquipmentTablePresenter.EquipmentSummary summary =
                    EquipmentTablePresenter.Aggregate(this.equipmentData);
            int waiting = this.lotData != null ? this.lotData.Rows.Count : 0;

            this.badgeRun.Text = "Run " + summary.RunCount.ToString("N0");
            this.badgeIdle.Text = "Idle " + summary.IdleCount.ToString("N0");
            this.badgeDown.Text = "Down " + summary.DownCount.ToString("N0");
            this.badgeFreeIn.Text = "Free In-Ports " + summary.FreeInPorts.ToString("N0");
            this.badgeWaiting.Text = "Waiting " + waiting.ToString("N0");
        }

        // ===== 처리 =====

        // ===== 장비 컨텍스트 메뉴 (모든 처리의 진입점) =====

        // 메뉴가 열릴 때 항목 활성을 정한다 — 활성 판정은 액션 정의의
        // CanExecute 하나를 쓰므로 진입점이 늘어도 판정 로직은 한 곳이다.
        // 그리드가 우클릭한 행을 먼저 선택하므로 선택 장비가 곧 처리 대상이다.
        private void OnMenuEqpOpening(object sender, CancelEventArgs e)
        {
            DataRowView row = this.gridEqp.SelectedItem as DataRowView;

            if (row == null)
            {
                e.Cancel = true;
                return;
            }

            foreach (object entry in this.menuEqp.Items)
            {
                ToolStripMenuItem item = entry as ToolStripMenuItem;

                if (item == null)
                {
                    continue;
                }

                string key = item.Tag as string;

                foreach (EquipmentAction action in this.equipmentActions)
                {
                    if (action.Key == key)
                    {
                        item.Enabled = action.CanExecute(row);
                        break;
                    }
                }
            }
        }

        // 작업준비 가능 판정 — 장비 준비(Down 아님)와 포트 준비(빈 인포트 +
        // 쓸 수 있는 아웃포트), 그리고 대기 Lot이 모두 있어야 한다
        // (아웃포트는 작업준비 때 예약된다).
        private bool CanPrepare(DataRowView row)
        {
            bool down = PendingTablePresenter.CellText(row.Row, "STATE") == "Down";
            int freeIn = PendingTablePresenter.ParseDays(
                    PendingTablePresenter.CellText(row.Row, "FREE_IN"));
            int freeOut = PendingTablePresenter.ParseDays(
                    PendingTablePresenter.CellText(row.Row, "FREE_OUT"));
            int waiting = this.lotData != null ? this.lotData.Rows.Count : 0;

            return !down && freeIn > 0 && freeOut > 0 && waiting > 0;
        }

        // 작업준비: 인포트/아웃포트를 다이얼로그에서 지정한 뒤 대기 큐의
        // 최우선 Lot을 투입한다 — "우선순위 대로 투입"의 표준 동선. 지정한
        // 아웃포트는 이 작업에 예약되고 작업종료 시 Lot이 그 포트로 나간다.
        // ★ 회사 환경 교체 지점 — Prepare를 회사 인터페이스로.
        private void PrepareTopLot()
        {
            string topLotId = this.lotData != null && this.lotData.Rows.Count > 0
                    ? PendingTablePresenter.CellText(this.lotData.Rows[0], "LOT_ID")
                    : string.Empty;

            this.RunPrepareDialog(topLotId, null);
        }

        // 포트 지정 다이얼로그를 띄우고 확정 시 투입한다.
        // assignLotId가 null이면 Prepare(서버가 최우선 Lot 선택), 아니면 그
        // Lot의 지정 투입(AssignLot)이다. displayLotId는 다이얼로그 표시용.
        // preferredInPort(타입 내 1-기준, 0 = 없음)를 주면 다이얼로그가 그
        // 인포트를 미리 선택한 채 열린다 — 포트 행의 Load 진입점용.
        private void RunPrepareDialog(string displayLotId, string assignLotId, int preferredInPort = 0)
        {
            DataRowView row = this.gridEqp.SelectedItem as DataRowView;

            if (row == null)
            {
                this.toastMain.Show("Select an equipment first.", ToastKind.Warning);
                return;
            }

            string group = this.GetGroup();
            string eqpId = PendingTablePresenter.CellText(row.Row, "EQP_ID");
            DataTable ports = EquipmentTablePresenter.BuildPortRows(row.Row);

            // 아웃 캐리어는 빈 캐리어 풀에서 골라야 한다 — 없으면 투입 불가.
            // ★ 회사 환경 교체 지점 — 빈 캐리어 조회를 회사 인터페이스로.
            DataTable carriers = EquipmentLotSimulator.GetEmptyCarriers(group);

            if (carriers.Rows.Count == 0)
            {
                this.toastMain.Show("No empty carrier available in " + group + ".", ToastKind.Warning);
                return;
            }

            using (PrepareDialogForm dialog =
                    new PrepareDialogForm(eqpId, displayLotId, ports, carriers, preferredInPort))
            {
                // 다이얼로그가 떠 있는 동안 자동 갱신을 멈춘다 — 포트/캐리어를
                // 고르는 중에 화면이 재조회로 바뀌면 안 된다.
                this.dialogOpen = true;

                try
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                }
                finally
                {
                    this.dialogOpen = false;
                }

                EquipmentLotSimulator.ActionResult result = assignLotId == null
                        ? EquipmentLotSimulator.Prepare(
                                group, eqpId, dialog.SelectedInPort, dialog.SelectedOutPort,
                                dialog.SelectedCarrier)
                        : EquipmentLotSimulator.AssignLot(
                                group, eqpId, assignLotId, dialog.SelectedInPort,
                                dialog.SelectedOutPort, dialog.SelectedCarrier);

                if (!result.Success)
                {
                    this.toastMain.Show(result.Message, ToastKind.Warning);
                    return;
                }

                // 토스트의 포트 번호는 포트 카드와 같은 연속 번호로 표기한다.
                int inCount = PendingTablePresenter.ParseDays(
                        PendingTablePresenter.CellText(row.Row, "IN_CNT"));
                this.toastMain.Show(
                        "Lot " + result.LotId + " assigned to " + eqpId
                        + " (port " + dialog.SelectedInPort
                        + " → " + (inCount + dialog.SelectedOutPort)
                        + ", " + dialog.SelectedCarrier + ").",
                        ToastKind.Success);
                this.ExecuteSearch(eqpId);
            }
        }

        // 작업시작 / 반출 — 서버가 검증 + 시각 적재를 하고, 성공하면
        // 재조회 + 그 장비 포커스 유지. 실패 사유는 토스트.
        // ★ 회사 환경 교체 지점 — StartJob/Unload를 회사 인터페이스로.
        private void RunEquipmentAction(string kind)
        {
            string group = this.GetGroup();
            string eqpId = this.GetFocusedEqpId();

            if (eqpId.Length == 0)
            {
                this.toastMain.Show("Select an equipment first.", ToastKind.Warning);
                return;
            }

            EquipmentLotSimulator.ActionResult result;
            string successMessage;

            if (kind == "START")
            {
                result = EquipmentLotSimulator.StartJob(group, eqpId);
                successMessage = "Job started on " + eqpId + ".";
            }
            else
            {
                result = EquipmentLotSimulator.Unload(group, eqpId);
                successMessage = "All done out-ports unloaded on " + eqpId + ".";
            }

            if (!result.Success)
            {
                this.toastMain.Show(result.Message, ToastKind.Warning);
                return;
            }

            this.toastMain.Show(successMessage, ToastKind.Success);
            this.ExecuteSearch(eqpId);
        }

        // 작업종료: 슬롯별 판정(Judge) 입력 다이얼로그를 거쳐 종료한다 —
        // 캐리어 슬롯 현황을 보여주고 웨이퍼(WF_ID)가 있는 슬롯 전부에
        // SUCC/FAIL을 입력해야 확정된다. 확정 시 판정을 종료 전문에 실어
        // 보내고, 성공하면 재조회 + 장비 포커스 유지.
        // ★ 회사 환경 교체 지점 — GetEndJobSlots(슬롯 맵 조회)와 EndJob을
        //   회사 인터페이스로.
        private void RunEndDialog()
        {
            string group = this.GetGroup();
            string eqpId = this.GetFocusedEqpId();

            if (eqpId.Length == 0)
            {
                this.toastMain.Show("Select an equipment first.", ToastKind.Warning);
                return;
            }

            DataTable slots = EquipmentLotSimulator.GetEndJobSlots(group, eqpId);

            if (slots.Rows.Count == 0)
            {
                this.toastMain.Show("No running lot to end on " + eqpId + ".", ToastKind.Warning);
                return;
            }

            // 다이얼로그 표시용 작업중 Lot ID — 선택 장비 행에서 찾는다.
            DataRowView row = this.gridEqp.SelectedItem as DataRowView;
            string lotId = string.Empty;

            for (int index = 1; row != null && index <= 2; index++)
            {
                if (PendingTablePresenter.CellText(row.Row, "IN" + index + "_STAT").Trim() == "Running")
                {
                    lotId = PendingTablePresenter.CellText(row.Row, "IN" + index + "_LOT");
                    break;
                }
            }

            using (EndJobDialogForm dialog = new EndJobDialogForm(eqpId, lotId, slots))
            {
                // 다이얼로그가 떠 있는 동안 자동 갱신을 멈춘다 (작업준비와 동일).
                this.dialogOpen = true;

                try
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                }
                finally
                {
                    this.dialogOpen = false;
                }

                EquipmentLotSimulator.ActionResult result =
                        EquipmentLotSimulator.EndJob(group, eqpId, dialog.JudgeResults);

                if (!result.Success)
                {
                    this.toastMain.Show(result.Message, ToastKind.Warning);
                    return;
                }

                this.toastMain.Show(
                        "Job ended on " + eqpId + " — lot moved to out-port.", ToastKind.Success);
                this.ExecuteSearch(eqpId);
            }
        }

        // 통신 모드 전환 — 서버가 검증(같은 모드 재설정 거부)하고, 성공하면
        // 재조회 + 장비 포커스 유지. Remote로 올리면 수동 Start/End가 막히고
        // OffLine이면 작업 처리 전부가 막힌다 (판정은 프레젠터 파생 플래그).
        // ★ 회사 환경 교체 지점 — SetCommMode를 회사 인터페이스로.
        private void ApplyCommModeAction(string mode)
        {
            string group = this.GetGroup();
            string eqpId = this.GetFocusedEqpId();

            if (eqpId.Length == 0)
            {
                this.toastMain.Show("Select an equipment first.", ToastKind.Warning);
                return;
            }

            EquipmentLotSimulator.ActionResult result =
                    EquipmentLotSimulator.SetCommMode(group, eqpId, mode);

            if (!result.Success)
            {
                this.toastMain.Show(result.Message, ToastKind.Warning);
                return;
            }

            this.toastMain.Show(
                    "Equipment " + eqpId + " switched to " + mode + ".", ToastKind.Success);
            this.ExecuteSearch(eqpId);
        }

        // Down 설정/해제 — ★ 회사 환경 교체 지점: SetDown을 회사 인터페이스로.
        private void ApplyDownAction(bool down)
        {
            string group = this.GetGroup();
            string eqpId = this.GetFocusedEqpId();

            if (eqpId.Length == 0)
            {
                this.toastMain.Show("Select an equipment first.", ToastKind.Warning);
                return;
            }

            EquipmentLotSimulator.ActionResult result =
                    EquipmentLotSimulator.SetDown(group, eqpId, down);

            if (!result.Success)
            {
                this.toastMain.Show(result.Message, ToastKind.Warning);
                return;
            }

            this.toastMain.Show(
                    "Equipment " + eqpId + (down ? " set to down." : " released from down."),
                    ToastKind.Success);
            this.ExecuteSearch(eqpId);
        }

        // ===== 포트 컨텍스트 메뉴 + Port Actions 드롭다운 =====

        // 포트 행 우클릭 메뉴가 열릴 때 — 항목 활성은 포트 액션 정의의
        // CanExecute 하나를 쓰므로 드롭다운과 항상 일치한다.
        private void OnMenuPortOpening(object sender, CancelEventArgs e)
        {
            DataRowView row = this.gridPorts.SelectedItem as DataRowView;

            if (row == null)
            {
                e.Cancel = true;
                return;
            }

            foreach (object entry in this.menuPort.Items)
            {
                ToolStripMenuItem item = entry as ToolStripMenuItem;

                if (item == null)
                {
                    continue;
                }

                string key = item.Tag as string;

                foreach (EquipmentAction action in this.portActions)
                {
                    if (action.Key == key)
                    {
                        item.Enabled = action.CanExecute(row);
                        break;
                    }
                }
            }
        }

        // 포트 메뉴 항목 클릭 — 공용 디스패처로 넘긴다.
        private void OnPortMenuItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;

            if (item != null)
            {
                this.ExecutePortAction(item.Tag as string);
            }
        }

        // 포트 카드 드롭다운 항목 클릭 — 같은 디스패처를 탄다.
        private void OnPortActionMenuClicked(
                object sender, Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs e)
        {
            this.ExecutePortAction(e.Value as string);
        }

        // 포트 선택이 바뀌면 드롭다운 항목 활성을 다시 계산한다.
        private void OnPortSelectionChanged(object sender, EventArgs e)
        {
            this.UpdatePortActionStates();
        }

        // 이 인포트에 투입: 포트 행에서 진입하는 작업준비 — 다이얼로그가
        // 그 인포트를 미리 선택한 채 열리고(변경 가능), 대기 큐의 최우선
        // Lot이 투입된다.
        private void LoadPortRow(DataRowView row)
        {
            if (row == null)
            {
                return;
            }

            string topLotId = this.lotData != null && this.lotData.Rows.Count > 0
                    ? PendingTablePresenter.CellText(this.lotData.Rows[0], "LOT_ID")
                    : string.Empty;
            int inPort = PendingTablePresenter.ParseDays(
                    PendingTablePresenter.CellText(row.Row, "PORT_IDX"));

            this.RunPrepareDialog(topLotId, null, inPort);
        }

        // 이 아웃포트만 반출: 선택 포트의 완료 Lot을 비운다 (장비 메뉴의
        // Unload는 완료 포트 전체 반출).
        // ★ 회사 환경 교체 지점 — UnloadPort를 회사 인터페이스로.
        private void UnloadPortRow(DataRowView row)
        {
            if (row == null)
            {
                return;
            }

            string group = this.GetGroup();
            string eqpId = this.GetFocusedEqpId();
            int outPort = PendingTablePresenter.ParseDays(
                    PendingTablePresenter.CellText(row.Row, "PORT_IDX"));
            string lotId = PendingTablePresenter.CellText(row.Row, "LOT_ID");

            EquipmentLotSimulator.ActionResult result =
                    EquipmentLotSimulator.UnloadPort(group, eqpId, outPort);

            if (!result.Success)
            {
                this.toastMain.Show(result.Message, ToastKind.Warning);
                return;
            }

            this.toastMain.Show(
                    "Lot " + lotId + " unloaded from " + eqpId + ".", ToastKind.Success);
            this.ExecuteSearch(eqpId);
        }

        // 포트 그리드 행 버튼(Cancel) — 메뉴와 같은 로직을 탄다.
        private void OnPortCellButtonClick(object sender, GridButtonClickEventArgs e)
        {
            if (!this.CanRunActionWhileRefreshing())
            {
                return;
            }

            if (e.DataPropertyName == "CANCEL_ACTION")
            {
                this.CancelPortRow(e.Item as DataRowView);
            }
        }

        // 취소: 포트의 투입됨/작업중 Lot을 빼서 대기 큐 최우선으로 되돌린다
        // (완료 Lot은 반출 대상이라 취소 불가). 메뉴/행 버튼 공용.
        // ★ 회사 환경 교체 지점 — CancelPort를 회사 인터페이스로.
        private void CancelPortRow(DataRowView row)
        {
            if (row == null)
            {
                return;
            }

            string group = this.GetGroup();
            string eqpId = this.GetFocusedEqpId();
            int inPort = PendingTablePresenter.ParseDays(
                    PendingTablePresenter.CellText(row.Row, "PORT_IDX"));
            string lotId = PendingTablePresenter.CellText(row.Row, "LOT_ID");

            EquipmentLotSimulator.ActionResult result =
                    EquipmentLotSimulator.CancelPort(group, eqpId, inPort);

            if (!result.Success)
            {
                this.toastMain.Show(result.Message, ToastKind.Warning);
                return;
            }

            this.toastMain.Show(
                    "Lot " + lotId + " cancelled — returned to top of the queue.", ToastKind.Success);
            // 복귀한 Lot(큐 맨 위)으로 하이라이트를 옮겨 어디로 갔는지 보여준다.
            this.ExecuteSearch(eqpId, lotId);
        }

        // ===== 대기 Lot: 우선순위 ↑↓ 버튼 + 지정 투입 메뉴 =====

        // 대기 Lot 행 버튼: 우선순위 ↑↓(한 칸 위·아래와 순위 맞바꿈 — 서버가
        // 저장하고 재조회로 반영) + 지정 투입(Assign — 우클릭 메뉴와 같은 로직).
        // ★ 회사 환경 교체 지점 — MoveLotPriority를 회사 인터페이스로.
        private void OnLotCellButtonClick(object sender, GridButtonClickEventArgs e)
        {
            if (!this.CanRunActionWhileRefreshing())
            {
                return;
            }

            DataRowView row = e.Item as DataRowView;

            if (row == null)
            {
                return;
            }

            if (e.DataPropertyName == "ASSIGN_ACTION")
            {
                this.AssignLotRow(row);
                return;
            }

            if (e.DataPropertyName != "UP_ACTION" && e.DataPropertyName != "DOWN_ACTION")
            {
                return;
            }

            string group = this.GetGroup();
            string lotId = PendingTablePresenter.CellText(row.Row, "LOT_ID");

            EquipmentLotSimulator.ActionResult result = EquipmentLotSimulator
                    .MoveLotPriority(group, lotId, e.DataPropertyName == "UP_ACTION");

            if (!result.Success)
            {
                this.toastMain.Show(result.Message, ToastKind.Warning);
                return;
            }

            // 순서가 바뀐 것 자체가 피드백이라 토스트는 띄우지 않는다.
            // 하이라이트는 이동한 Lot을 따라가 시선이 끊기지 않게 한다.
            this.ExecuteSearch(this.GetFocusedEqpId(), lotId);
        }

        // Lot 행 우클릭 메뉴가 열릴 때 — 지정 투입은 선택 장비가 투입 가능할
        // 때만. 대상 장비를 메뉴 캡션에 표기해 어디로 가는지 명확히 한다.
        private void OnMenuLotOpening(object sender, CancelEventArgs e)
        {
            if (this.gridLots.SelectedItem == null)
            {
                e.Cancel = true;
                return;
            }

            this.menuAssign.Enabled = this.lotAssignable;
            this.menuAssign.Text = this.lotAssignable
                    ? "Assign to " + this.GetFocusedEqpId()
                    : "Assign (no target equipment)";
        }

        // 지정 투입 메뉴 — 우클릭으로 선택된 Lot 행을 공용 로직으로 넘긴다.
        private void OnMenuAssignClick(object sender, EventArgs e)
        {
            this.AssignLotRow(this.gridLots.SelectedItem as DataRowView);
        }

        // 지정 투입: Lot을 포트 지정 다이얼로그를 거쳐 선택 장비에 장착한다 —
        // 우선순위를 건너뛰는 예외 동선 (표준 동선은 장비 메뉴의 Prepare).
        // 메뉴/행 버튼 공용. ★ 회사 환경 교체 지점 — AssignLot을 회사 인터페이스로.
        private void AssignLotRow(DataRowView row)
        {
            if (!this.CanRunActionWhileRefreshing())
            {
                return;
            }

            if (row == null)
            {
                return;
            }

            string lotId = PendingTablePresenter.CellText(row.Row, "LOT_ID");
            this.RunPrepareDialog(lotId, lotId);
        }
    }
}
