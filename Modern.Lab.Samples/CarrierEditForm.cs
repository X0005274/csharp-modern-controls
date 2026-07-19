using System;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Carrier Editor — 캐리어(FOUP/TRAY)의 수납 구조를 **실물 단면 슬롯
    /// 맵**으로 보고 유닛을 옮기거나 폐기하는 화면. (용어는 데모용이며 회사
    /// 적용 시 보안 정책에 맞춰 치환한다.)
    ///
    /// 수납 구조:
    /// - FOUP : 웨이퍼 슬롯 25개 — 세로 사다리(슬롯 스택)로 그린다.
    /// - TRAY : 칩이 들어가는 STUB 6개(가로 한 줄) + LCC 25개(5×5 격자) —
    ///   LCC 셀 안에 핑거 A~E 도트 5개, 꽂힌 핑거는 삽입 위치(Top/Left/
    ///   Right)가 도트 가장자리 액센트 틱으로 표시된다. 상세는 호버 툴팁.
    ///
    /// 화면 구성: 타입/캐리어 ID를 고르면 **좌측 맵**에 그 캐리어의 채움
    /// 구조가 그려지고(자리 번호는 고정 눈금, 유닛은 ID가 적힌 색 토큰 —
    /// 자리와 유닛이 묶여 있지 않음이 시각으로 드러난다), **우측 맵**에
    /// 대상 캐리어(같은 타입, 빈 자리 많은 순)를 나란히 본다. 원본/대상
    /// 선택은 둘 다 조회 패널에 있고, 좌/우 카드는 같은 높이·같은 정보
    /// (맵 + 채움 집계)로 대칭이다.
    ///
    /// 두 계층으로 나뉜다:
    /// - **재배치(로컬 이동)**: 가운데 셰브런 아이콘(»/›/‹/«)과 드래그앤드롭.
    ///   좌↔우로 전체(»/«)·선택(›/‹) 이동하며 원본/대상을 원하는 모습으로
    ///   맞춘다. 드롭은 놓은 자리(앵커)부터 채우고 모자라면 앞쪽 빈 자리로
    ///   이어진다(1번 유닛이 꼭 1번 자리로 갈 필요 없음).
    /// - **비즈 실행(하단 카드)**: Split / Merge / Scrap. **회사 환경에서
    ///   Split·Merge는 회사 비즈 전문(캐리어 분할/병합)을 호출하는 committing
    ///   액션**이다 — 대상이 비어 있으면 Split, 차 있으면 Merge가 표준 동선.
    ///   Scrap은 선택 유닛 폐기.
    ///
    /// 이동/검증 규칙(서버) — 같은 종류 자리로만(웨이퍼→슬롯, STUB 칩→STUB,
    /// LCC 칩→LCC 핑거), LCC 칩은 삽입 위치가 함께 따라간다. FOUP은 랏 하나로
    /// 표시되며, 옮겨온 웨이퍼는 대상 FOUP의 랏이 되므로 랏이 달라도 공간만
    /// 있으면 이동한다. 빈 자리가 부족하면 전부-아니면-전무로 거부된다. 처리
    /// 흐름은 다른 화면과 동일: 서버 처리 → 성공 시 **재조회** → 토스트.
    ///
    /// ★ 회사 환경 교체 지점 — 조회 2개(GetCarriers/GetCarrierUnits)와 처리
    ///   메서드를 회사 인터페이스로 바꾸고 CarrierEditSimulator를 지운다.
    ///   특히 **Split/Merge 핸들러(OnSplitClick/OnMergeClick)가 회사 비즈
    ///   전문 호출 지점**이고, 재배치용 이동(MoveBetween)/폐기(ScrapUnits)도
    ///   각 회사 인터페이스로 교체한다. 슬롯 맵 구성(BuildSections)은 조회
    ///   결과 컬럼 계약(KIND/POS/FINGER/INS_POS/UNIT_ID/ITEM_ID) 그대로라
    ///   손대지 않는다.
    /// </summary>
    public partial class CarrierEditForm : Form
    {
        // 좌(원본)/우(대상) 수납 현황 — 맵 구성 + 처리 대상 수집의 원천.
        private DataTable sourceData;
        private DataTable targetData;

        // 콤보 재구성 중의 연쇄 이벤트(재조회 중복)를 막는다.
        private bool loadingLists;

        // 현재 우측(대상)에 미리보기 중인 원본→대상 이동 대상. ⇒(전체) 또는
        // 원본 셀 선택(선택분)이 설정한다. Split이 이걸 그대로 비즈로 확정한다.
        private DataTable stagedUnits;

        // 아이템 ID → 배지/토큰 색 — 처음 만난 순서대로 팔레트를 배정해
        // 세션 동안 유지한다 (같은 아이템 = 같은 색, 좌/우 맵·범례 공통).
        private readonly System.Collections.Generic.Dictionary<string, string> itemColors =
                new System.Collections.Generic.Dictionary<string, string>(StringComparer.Ordinal);

        // 아이템 틴트 단계 — 테마 액센트를 **테마 표면색**과 섞는 비율(클수록
        // 표면색에 가까움). 흰색이 아니라 표면색과 섞으므로 라이트 테마에선
        // 밝은 액센트 톤, 다크 테마(크림슨/다크)에선 어두운 액센트 톤이 나와
        // 좌/우가 모두 그 테마 계열로 어울린다. 여럿이면 단계로 구분한다.
        private static readonly double[] itemTintLevels =
                new double[] { 0.62d, 0.40d, 0.52d, 0.30d, 0.72d, 0.46d, 0.22d, 0.68d };

        // 아이템 색을 얻는다 — 미배정 아이템은 다음 틴트 단계를 받는다.
        private string GetItemColor(string itemId)
        {
            string color;

            if (!this.itemColors.TryGetValue(itemId, out color))
            {
                color = ItemTint(this.itemColors.Count);
                this.itemColors[itemId] = color;
            }

            return color;
        }

        // 테마 액센트를 테마 표면색과 섞어 틴트 색 문자열("#RRGGBB")을 만든다.
        private static string ItemTint(int index)
        {
            System.Drawing.Color accent = Modern.Lab.Theming.ModernTheme.Accent;
            System.Drawing.Color surface = Modern.Lab.Theming.ModernTheme.Surface;
            double s = itemTintLevels[index % itemTintLevels.Length];

            int r = (int)(accent.R * (1d - s) + surface.R * s);
            int g = (int)(accent.G * (1d - s) + surface.G * s);
            int b = (int)(accent.B * (1d - s) + surface.B * s);

            return "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
        }

        public CarrierEditForm()
        {
            this.InitializeComponent();

            // 로딩 커버 한 줄 — 폼 스스로 오픈 시 깜빡임을 가린다.
            Modern.Lab.WinForms.Controls.Hosting.ModernLoadCover.Attach(this);
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            // 타입 콤보 — 두 수납 구조. 할당이 SelectedIndexChanged를 일으켜
            // 첫 조회(캐리어 목록 → 좌/우 현황)가 자동으로 실행된다.
            DataTable typeTable = new DataTable();
            typeTable.Columns.Add("VALUE", typeof(string));
            typeTable.Rows.Add("FOUP");
            typeTable.Rows.Add("TRAY");

            this.cboType.DisplayMember = "VALUE";
            this.cboType.ValueMember = "VALUE";
            this.cboType.DataSource = typeTable;
        }

        // ===== 타입/캐리어 선택 =====

        private string GetSelectedType()
        {
            string value = this.cboType.SelectedValue as string;
            return value ?? string.Empty;
        }

        private void OnTypeChanged(object sender, EventArgs e)
        {
            if (this.GetSelectedType().Length == 0)
            {
                return;
            }

            this.ReloadCarrierLists(null, null);
        }

        private void OnSourceChanged(object sender, EventArgs e)
        {
            if (this.loadingLists)
            {
                return;
            }

            // 원본이 바뀌면 대상 목록(자기 자신 제외)도 다시 만든다.
            this.ReloadCarrierLists(this.cboSource.SelectedValue as string, null);
        }

        private void OnTargetChanged(object sender, EventArgs e)
        {
            if (this.loadingLists)
            {
                return;
            }

            this.LoadTargetMap();
            this.ClearPreview();
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            this.ReloadCarrierLists(
                    this.cboSource.SelectedValue as string,
                    this.cboTarget.SelectedValue as string);
        }

        // 캐리어 목록(원본/대상 콤보)을 다시 만들고 양쪽 맵을 재조회한다.
        // 대상 목록은 원본을 제외한 같은 타입 캐리어 — 빈 자리가 많은 순으로
        // 정렬해 "빈 캐리어"가 위에 오게 한다. 라벨에 채움 수를 함께 표기한다.
        private void ReloadCarrierLists(string keepSourceId, string keepTargetId)
        {
            string type = this.GetSelectedType();

            if (type.Length == 0)
            {
                return;
            }

            // ★ 회사 환경 교체 지점 — 캐리어 목록 조회를 회사 인터페이스로.
            DataTable carriers = CarrierEditSimulator.GetCarriers(type);
            carriers.Columns.Add("LABEL", typeof(string));

            foreach (DataRow row in carriers.Rows)
            {
                row["LABEL"] = PendingTablePresenter.CellText(row, "CARR_ID")
                        + " (" + PendingTablePresenter.CellText(row, "FILL_CNT")
                        + "/" + PendingTablePresenter.CellText(row, "CAPACITY") + ")";
            }

            this.loadingLists = true;

            try
            {
                this.cboSource.DisplayMember = "LABEL";
                this.cboSource.ValueMember = "CARR_ID";
                this.cboSource.DataSource = carriers;

                if (!string.IsNullOrEmpty(keepSourceId))
                {
                    this.cboSource.SelectedValue = keepSourceId;
                }

                string sourceId = this.cboSource.SelectedValue as string ?? string.Empty;

                // 대상 목록 — 원본 제외 + **가득 찬 캐리어 제외**(더 담을 수
                // 없으므로) + 빈 자리 많은 순.
                DataTable targets = carriers.Clone();
                DataRow[] others = carriers.Select(
                        "CARR_ID <> '" + sourceId + "' AND FILL_CNT < CAPACITY", "FILL_CNT ASC");

                foreach (DataRow row in others)
                {
                    targets.ImportRow(row);
                }

                this.cboTarget.DisplayMember = "LABEL";
                this.cboTarget.ValueMember = "CARR_ID";
                this.cboTarget.DataSource = targets;

                if (!string.IsNullOrEmpty(keepTargetId))
                {
                    this.cboTarget.SelectedValue = keepTargetId;
                }
            }
            finally
            {
                this.loadingLists = false;
            }

            this.LoadSourceMap();
            this.LoadTargetMap();

            // 재조회 후 미리보기/스테이징을 초기화한다 (로드 시 미리보기 없음).
            this.ClearPreview();
        }

        // ===== 현황 조회 → 슬롯 맵 구성 =====

        // 좌측(원본) 맵 재조회 — 재구성 시 선택은 초기화된다.
        private void LoadSourceMap()
        {
            string carrierId = this.cboSource.SelectedValue as string ?? string.Empty;

            // ★ 회사 환경 교체 지점 — 수납 현황 조회를 회사 인터페이스로.
            this.sourceData = CarrierEditSimulator.GetCarrierUnits(this.GetSelectedType(), carrierId);
            this.mapSource.SetSections(BuildSections(this.sourceData, this.GetSelectedType()));

            // 제목 = "Source — 캐리어", 채움 집계는 우측 회색 서브타이틀,
            // 랏(아이템) 배지는 캐리어 이름 바로 오른쪽에 붙인다.
            this.sourceCard.Text = BuildCardTitle("Source", carrierId);
            this.sourceCard.TitleRightText = BuildFilledText(this.sourceData);
            this.UpdateItemBadge(this.badgeSourceItem, this.sourceData);
            this.PositionTitleBadge(this.sourceCard, this.badgeSourceItem);
        }

        // 우측(대상) 맵 재조회.
        private void LoadTargetMap()
        {
            string carrierId = this.cboTarget.SelectedValue as string ?? string.Empty;

            this.targetData = CarrierEditSimulator.GetCarrierUnits(this.GetSelectedType(), carrierId);
            this.mapTarget.SetSections(BuildSections(this.targetData, this.GetSelectedType()));

            this.targetCard.Text = BuildCardTitle("Target", carrierId);
            this.targetCard.TitleRightText = BuildFilledText(this.targetData);
            this.UpdateItemBadge(this.badgeTargetItem, this.targetData);
            this.PositionTitleBadge(this.targetCard, this.badgeTargetItem);
            this.UpdateActionStates();
        }

        // 현재 상태에서 **가능한 동작만** 활성화한다 — 이동 버튼(»/›/‹/«),
        // 컨텍스트 메뉴 항목, Split/Merge/Scrap 모두 여기서 켜고 끈다.
        //  · › 선택 이동  : 원본에서 클릭한(아직 스테이징 안 된) 셀이 있을 때
        //  · » 전체 이동  : 원본에 유닛이 있을 때
        //  · ‹ 선택 취소  : 클릭한 원본 셀이 스테이징돼 있을 때(미리보기에서 빼기)
        //  · « 전체 취소  : 스테이징이 하나라도 있을 때
        //  · Split/Merge : 스테이징이 있고 대상이 각각 빈/일부 채움일 때
        //  · Scrap       : 스테이징(선택)된 원본 유닛이 있을 때
        private void UpdateActionStates()
        {
            int targetFilled = CarrierTablePresenter.CountFilled(this.targetData);
            int targetCap = this.targetData != null ? this.targetData.Rows.Count : 0;
            bool hasTarget = this.TargetId().Length > 0 && targetCap > 0;
            bool sourceHasUnits = CarrierTablePresenter.CountFilled(this.sourceData) > 0;
            bool clickedSource = !string.IsNullOrEmpty(this.clickSourceKey);
            bool clickedStaged = clickedSource && this.stagedKeys.Contains(this.clickSourceKey);
            bool anyStaged = this.stagedKeys.Count > 0;
            bool hasStagedUnits = this.stagedUnits != null && this.stagedUnits.Rows.Count > 0;
            int stagedCount = hasStagedUnits ? this.stagedUnits.Rows.Count : 0;

            bool selRight = clickedSource && !clickedStaged;
            bool allRight = sourceHasUnits;
            bool selLeft = clickedStaged;
            bool allLeft = anyStaged;

            this.btnSelRight.Enabled = selRight;
            this.btnAllRight.Enabled = allRight;
            this.btnSelLeft.Enabled = selLeft;
            this.btnAllLeft.Enabled = allLeft;

            this.miMoveSelRight.Enabled = selRight;
            this.miMoveAllRight.Enabled = allRight;
            this.miMoveSelLeft.Enabled = selLeft;
            this.miMoveAllLeft.Enabled = allLeft;

            this.btnSplit.Enabled = hasStagedUnits && hasTarget && targetFilled == 0;
            this.btnMerge.Enabled = hasStagedUnits && hasTarget && targetFilled > 0 && targetFilled < targetCap;
            this.btnScrap.Enabled = anyStaged;

            // 화면의 주 동작은 현재 이동 계획에 따라 하나만 액센트로 강조한다.
            // 동작 조건과 이벤트는 기존과 같고, 이 메서드는 시각적 위계만 조정한다.
            this.targetCard.TitleAccent = anyStaged;
            this.lblTransferHint.Text = stagedCount > 0
                    ? stagedCount.ToString("N0") + " staged"
                    : "stage units";
            this.btnSplit.Kind = this.btnSplit.Enabled
                    ? Modern.Lab.Controls.Wpf.Input.ButtonKind.Primary
                    : Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnMerge.Kind = this.btnMerge.Enabled
                    ? Modern.Lab.Controls.Wpf.Input.ButtonKind.Primary
                    : Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnSplit.Text = stagedCount > 0
                    ? "Split " + stagedCount.ToString("N0") + " units"
                    : "Split";
            this.btnMerge.Text = stagedCount > 0
                    ? "Merge " + stagedCount.ToString("N0") + " units"
                    : "Merge";

            if (stagedCount == 0)
            {
                this.lblActionStatus.Text = "Select a source unit, then stage the move with → or ⇒.";
            }
            else if (!hasTarget)
            {
                this.lblActionStatus.Text = stagedCount.ToString("N0")
                        + " units staged · Select a target carrier to continue.";
            }
            else
            {
                this.lblActionStatus.Text = stagedCount.ToString("N0") + " units staged · Target has "
                        + (targetCap - targetFilled).ToString("N0") + " open positions.";
            }
        }

        // 카드 제목 (왼쪽) — "역할 — 캐리어". 채움 집계는 우측 서브타이틀,
        // 랏(아이템) 배지는 캐리어 이름 오른쪽에 별도로 붙는다.
        private static string BuildCardTitle(string role, string carrierId)
        {
            if (carrierId.Length == 0)
            {
                return role;
            }

            return role + " — " + carrierId;
        }

        // 카드 우측 회색 서브타이틀 — "N / M filled" (빈 캐리어/미선택은 빈 문자열).
        private static string BuildFilledText(DataTable units)
        {
            if (units == null || units.Rows.Count == 0)
            {
                return string.Empty;
            }

            int filled = CarrierTablePresenter.CountFilled(units);
            int capacity = units.Rows.Count;

            return filled.ToString("N0") + " / " + capacity.ToString("N0") + " filled";
        }

        // 랏(아이템) 배지를 카드 제목(캐리어 이름) 바로 오른쪽에 놓는다 — 제목
        // 폭을 카드 헤더와 같은 폰트(Segoe UI Semibold + 장평)로 재서 x를 잡는다.
        // 배지는 좌측 정렬 pill이라 제목 끝에서 8px 띄우면 이름 옆에 붙는다.
        private void PositionTitleBadge(
                Modern.Lab.WinForms.Controls.Layout.ModernGroupBox card,
                Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badge)
        {
            string title = card.Text ?? string.Empty;
            int x = 12;

            using (System.Drawing.Graphics g = this.CreateGraphics())
            using (System.Drawing.Font font =
                    new System.Drawing.Font("Segoe UI Semibold", card.TitleFontSize))
            {
                double ratio = Modern.Lab.Theming.ModernTheme.ResolveFontWidthRatio(card.FontWidthRatio);
                float width = (float)(g.MeasureString(title, font).Width * ratio);
                x = 12 + (int)System.Math.Ceiling(width) + 8;
            }

            badge.Location = new System.Drawing.Point(x, 4);
        }

        // 카드 제목 우측 아이템 배지 — 담긴 아이템 ID + 맵과 같은 색. FOUP은
        // 아이템 하나, TRAY처럼 섞이면 "IT-C01 +n", 빈 캐리어는 "Empty"(무채색).
        private void UpdateItemBadge(
                Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badge, DataTable units)
        {
            System.Collections.Generic.List<string> items =
                    new System.Collections.Generic.List<string>();

            if (units != null)
            {
                foreach (DataRow row in units.Rows)
                {
                    string itemId = PendingTablePresenter.CellText(row, "ITEM_ID").Trim();

                    if (itemId.Length > 0 && !items.Contains(itemId))
                    {
                        items.Add(itemId);
                    }
                }
            }

            if (items.Count == 0)
            {
                badge.Text = "Empty";
                badge.Color = string.Empty;
            }
            else if (items.Count == 1)
            {
                badge.Text = items[0];
                badge.Color = this.GetItemColor(items[0]);
            }
            else
            {
                badge.Text = items[0] + " +" + (items.Count - 1);
                badge.Color = this.GetItemColor(items[0]);
            }
        }

        // 수납 현황(자리당 한 행)을 슬롯 맵 구획으로 변환한다 — FOUP은 슬롯
        // 사다리 한 구획, TRAY는 STUB(6열 한 줄) + LCC(5×5, 핑거 도트) 두 구획.
        // 셀 키는 "KIND|POS" — 선택 키를 처리 대상 행으로 되돌릴 때 쓴다.
        // 유닛의 색은 소속 아이템(ITEM_ID)별 팔레트 색 — 범례 배지와 일치하고,
        // 툴팁에도 "유닛 — 아이템"으로 표기된다.
        private SlotMapSection[] BuildSections(DataTable units, string type)
        {
            if (type == "FOUP")
            {
                SlotMapSection slots = new SlotMapSection();
                slots.Title = "Slots";
                slots.Columns = 1;

                foreach (DataRow row in units.Rows)
                {
                    SlotMapCell cell = new SlotMapCell();
                    cell.Key = "SLOT|" + PendingTablePresenter.CellText(row, "POS");
                    cell.Label = PendingTablePresenter.CellText(row, "POS");
                    cell.UnitId = PendingTablePresenter.CellText(row, "UNIT_ID");

                    string itemId = PendingTablePresenter.CellText(row, "ITEM_ID");

                    if (itemId.Length > 0)
                    {
                        cell.Color = this.GetItemColor(itemId);
                        cell.ToolTip = cell.UnitId + " — " + itemId;
                    }

                    slots.Cells.Add(cell);
                }

                return new SlotMapSection[] { slots };
            }

            // STUB 6자리 — 3열 2행으로 배치하고 셀을 크게 (넓은 셀에 맞춰
            // 유닛 ID/번호 칩 글자를 키운다).
            SlotMapSection stubs = new SlotMapSection();
            stubs.Title = "STUB";
            stubs.Columns = 3;
            stubs.CellFontSize = 12d;

            SlotMapSection lccs = new SlotMapSection();
            lccs.Title = "LCC";
            lccs.Columns = 5;

            SlotMapCell current = null;

            foreach (DataRow row in units.Rows)
            {
                string kind = PendingTablePresenter.CellText(row, "KIND");
                string pos = PendingTablePresenter.CellText(row, "POS");
                string itemId = PendingTablePresenter.CellText(row, "ITEM_ID");

                if (kind == "STUB")
                {
                    SlotMapCell stub = new SlotMapCell();
                    stub.Key = "STUB|" + pos;
                    stub.Label = pos;
                    stub.UnitId = PendingTablePresenter.CellText(row, "UNIT_ID");

                    if (itemId.Length > 0)
                    {
                        stub.Color = this.GetItemColor(itemId);
                        stub.ToolTip = stub.UnitId + " — " + itemId;
                    }

                    stubs.Cells.Add(stub);
                    continue;
                }

                // LCC — 같은 위치의 핑거 행 5개를 셀 하나로 접는다
                // (서버가 위치 오름차순·핑거 A~E 순으로 준다).
                if (current == null || current.Key != "LCC|" + pos)
                {
                    current = new SlotMapCell();
                    current.Key = "LCC|" + pos;
                    current.Label = pos;
                    current.SubCells = new System.Collections.Generic.List<SlotMapSubCell>();
                    lccs.Cells.Add(current);
                }

                SlotMapSubCell finger = new SlotMapSubCell();
                finger.Name = PendingTablePresenter.CellText(row, "FINGER");
                finger.UnitId = PendingTablePresenter.CellText(row, "UNIT_ID");
                finger.Marker = PendingTablePresenter.CellText(row, "INS_POS");

                // 핑거마다 소속 아이템이 다를 수 있어 색/부가정보를 따로 준다.
                if (itemId.Length > 0)
                {
                    finger.Color = this.GetItemColor(itemId);
                    finger.Detail = itemId;
                }

                current.SubCells.Add(finger);
            }

            return new SlotMapSection[] { stubs, lccs };
        }

        // ===== 클릭 / 미리보기 / 선택 표시 =====

        // 선택/미리보기 상태:
        // - stagedKeys: 이동 버튼(→/⇒)으로 미리보기(스테이징)된 **원본** 셀 키 —
        //   대상(우측)에 "→ ID" 미리보기로 뜨고 계속 선택 표시된다. Split이 확정.
        //   미리보기는 **좌→우 한 방향뿐**이라 좌측(원본)에는 미리보기가 없다.
        // - clickSourceKey/clickTargetKey: 마우스로 방금 클릭한 셀(각 맵 한 개) —
        //   스테이징된 셀을 클릭하면 "스테이징+클릭" 결합 표시가 되고, 스테이징
        //   아닌 셀을 클릭하면 다른 셀 클릭 시 원복된다.
        private readonly System.Collections.Generic.HashSet<string> stagedKeys =
                new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
        private string clickSourceKey;
        private string clickTargetKey;

        // 원본 셀 클릭 — 미리보기는 하지 않는다(이동 버튼에서만). 클릭 셀을
        // 클릭 표시로 잡는다(이전 클릭 표시는 원복). 이미 스테이징된 셀을
        // 클릭하면 해제하지 않고 "스테이징+클릭" 결합 표시를 준다. 대상 클릭
        // 표시는 지운다.
        private void OnSourceCellClicked(object sender, SlotMapCellEventArgs e)
        {
            this.clickTargetKey = null;
            this.clickSourceKey = e.Key;
            this.RenderSelections();
        }

        // 대상 셀 클릭 — 이 셀을 클릭 표시만 한다(우측은 미리보기 대상이 아님).
        // 원본 클릭 표시는 지운다.
        private void OnTargetCellClicked(object sender, SlotMapCellEventArgs e)
        {
            this.clickTargetKey = e.Key;
            this.clickSourceKey = null;
            this.RenderSelections();
        }

        // 원본 맵 오른쪽 클릭 — 커서 아래 셀을 클릭 선택으로 잡고 이동
        // 컨텍스트 메뉴를 띄운다(메뉴 항목은 이동 버튼과 동일 동작).
        private void OnSourceCellRightClick(object sender, SlotMapCellEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Key))
            {
                this.OnSourceCellClicked(sender, e);
            }

            this.moveMenu.Show(System.Windows.Forms.Cursor.Position);
        }

        // 대상 맵 오른쪽 클릭 — 커서 아래 셀을 클릭 선택으로 잡고 같은 메뉴를 띄운다.
        private void OnTargetCellRightClick(object sender, SlotMapCellEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Key))
            {
                this.OnTargetCellClicked(sender, e);
            }

            this.moveMenu.Show(System.Windows.Forms.Cursor.Position);
        }

        // 스테이징된 원본 셀들의 미리보기를 **대상(우측)** 에 다시 그린다 —
        // 대상의 빈 자리를 위에서부터 순차로 채운다(front-first). 좌측(원본)에는
        // 미리보기가 없다.
        private void RebuildStagedPreview()
        {
            if (this.stagedKeys.Count == 0)
            {
                this.stagedUnits = null;
                this.mapTarget.SetPreview(null);
                return;
            }

            string[] keys = new string[this.stagedKeys.Count];
            this.stagedKeys.CopyTo(keys);
            this.stagedUnits = this.CollectUnitsByKeys(this.sourceData, keys);

            // ★ 서버 배치 계획을 그대로 미리보기에 쓴다 — 실제 이동과 일치.
            this.mapTarget.SetPreview(CarrierEditSimulator.PlanPreview(
                    this.GetSelectedType(), this.SourceId(), this.TargetId(), this.stagedUnits));
        }

        // 선택 표시 렌더 — 스테이징(강한 색)은 원본에, 클릭(약간 다른 색)은 각
        // 맵에 표시한다. 스테이징+클릭이 겹친 원본 셀은 결합 표시가 된다.
        private void RenderSelections()
        {
            string[] staged = new string[this.stagedKeys.Count];
            this.stagedKeys.CopyTo(staged);
            this.mapSource.SetSelectedKeys(staged);
            this.mapTarget.SetSelectedKeys(new string[0]);

            this.mapSource.SetClickKey(this.clickSourceKey);
            this.mapTarget.SetClickKey(this.clickTargetKey);

            this.UpdateActionStates();
        }

        // 모든 선택/미리보기/스테이징/클릭 표시를 비운다 (미리보기만 취소 —
        // 이미 확정된 서버 데이터는 건드리지 않는다).
        private void ClearPreview()
        {
            this.stagedKeys.Clear();
            this.clickSourceKey = null;
            this.clickTargetKey = null;
            this.stagedUnits = null;
            this.mapSource.SetSelectedKeys(new string[0]);
            this.mapTarget.SetSelectedKeys(new string[0]);
            this.mapSource.SetPreview(null);
            this.mapTarget.SetPreview(null);
            this.mapSource.SetClickKey(null);
            this.mapTarget.SetClickKey(null);
            this.UpdateActionStates();
        }

        // 이동 성공 후 도착 맵에서 "방금 옮긴 유닛"만 선택(강조)한다 — 어디로
        // 갔는지 보이고, 이동 안 한 것은 선택되지 않는다. movedIds는 이동 전
        // 수집한 유닛 ID 목록, dest는 도착 맵/현황.
        private void HighlightMoved(
                Modern.Lab.WinForms.Controls.Display.ModernSlotMap destMap,
                DataTable destData, System.Collections.Generic.List<string> movedIds)
        {
            System.Collections.Generic.HashSet<string> keys =
                    new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);

            foreach (DataRow row in destData.Rows)
            {
                string id = PendingTablePresenter.CellText(row, "UNIT_ID").Trim();

                if (id.Length > 0 && movedIds.Contains(id))
                {
                    keys.Add(PendingTablePresenter.CellText(row, "KIND")
                            + "|" + PendingTablePresenter.CellText(row, "POS"));
                }
            }

            string[] keyArray = new string[keys.Count];
            keys.CopyTo(keyArray);
            destMap.SetSelectedKeys(keyArray);
        }

        // ===== 대상 수집 =====

        // 셀 키("KIND|POS") 목록을 유닛 행으로 되돌린다 — LCC 셀 하나는 그
        // 안의 채워진 핑거 행 전부로 펼쳐진다. 원본/대상 어느 테이블이든
        // 같은 변환을 쓴다 (선택 이동·드래그앤드롭 공용).
        private DataTable CollectUnitsByKeys(DataTable data, string[] keys)
        {
            DataTable selected = data.Clone();

            foreach (string key in keys)
            {
                string[] parts = key.Split('|');

                if (parts.Length != 2)
                {
                    continue;
                }

                foreach (DataRow row in data.Rows)
                {
                    if (PendingTablePresenter.CellText(row, "KIND") == parts[0]
                            && PendingTablePresenter.CellText(row, "POS") == parts[1]
                            && PendingTablePresenter.CellText(row, "UNIT_ID").Trim().Length > 0)
                    {
                        selected.ImportRow(row);
                    }
                }
            }

            return selected;
        }

        // 채워진 자리 전부.
        private DataTable CollectAllUnits(DataTable data)
        {
            DataTable selected = data.Clone();

            foreach (DataRow row in data.Rows)
            {
                if (PendingTablePresenter.CellText(row, "UNIT_ID").Trim().Length > 0)
                {
                    selected.ImportRow(row);
                }
            }

            return selected;
        }

        // ===== 가운데 방향 이동 아이콘 (좌↔우) =====
        // 검증·재조회는 공용 MoveBetween.

        // ⇒ 좌→우 전체 **미리보기** — 원본 전체를 스테이징하고 대상에 "→ ID"로
        // 보여준다(실제 이동은 Split이 확정). 스테이징된 셀은 선택 표시된다.
        private void OnMoveAllRight(object sender, EventArgs e)
        {
            if (this.CollectAllUnits(this.sourceData).Rows.Count == 0)
            {
                this.toastMain.Show("Source carrier is empty.", ToastKind.Warning);
                return;
            }

            this.stagedKeys.Clear();
            this.clickSourceKey = null;
            this.clickTargetKey = null;

            foreach (DataRow row in this.sourceData.Rows)
            {
                if (PendingTablePresenter.CellText(row, "UNIT_ID").Trim().Length > 0)
                {
                    this.stagedKeys.Add(PendingTablePresenter.CellText(row, "KIND")
                            + "|" + PendingTablePresenter.CellText(row, "POS"));
                }
            }

            this.RebuildStagedPreview();
            this.RenderSelections();
        }

        // → 좌→우 선택 **미리보기** — 방금 클릭한 원본 셀을 스테이징에 추가하고
        // 대상에 미리보기한다(누적 가능). 클릭한 셀이 없으면 안내.
        private void OnMoveSelRight(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.clickSourceKey))
            {
                this.toastMain.Show("Click a source unit first, then →.", ToastKind.Warning);
                return;
            }

            this.stagedKeys.Add(this.clickSourceKey);
            this.clickSourceKey = null;
            this.RebuildStagedPreview();
            this.RenderSelections();
        }

        // ← 우→좌 선택 — 클릭한 스테이징 원본 셀 **하나만** 미리보기에서 뺀다
        // (우측 "→ ID"에서 그 유닛만 사라진다). 클릭한 스테이징 셀이 없으면 안내.
        private void OnMoveSelLeft(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.clickSourceKey)
                    || !this.stagedKeys.Contains(this.clickSourceKey))
            {
                this.toastMain.Show("Click a staged source unit first, then ←.", ToastKind.Warning);
                return;
            }

            this.stagedKeys.Remove(this.clickSourceKey);
            this.clickSourceKey = null;
            this.RebuildStagedPreview();
            this.RenderSelections();
        }

        // ⇐ 우→좌 전체 — **미리보기 전체 취소**(스테이징을 모두 비워 우측
        // 미리보기를 지운다). 이미 확정(Split/Merge)된 데이터는 건드리지 않는다.
        private void OnMoveAllLeft(object sender, EventArgs e)
        {
            this.ClearPreview();
        }

        // ===== 하단 실행 카드 (Split / Merge / Scrap — 비즈 실행) =====

        // Split: **미리보기(→/⇒)로 스테이징된 좌→우 이동을 비즈 단으로 확정**
        // 한다. 스테이징이 없으면 안내한다.
        // ★ 회사 환경 교체 지점 — 회사 캐리어 분할 비즈 전문 호출.
        private void OnSplitClick(object sender, EventArgs e)
        {
            if (this.stagedUnits == null || this.stagedUnits.Rows.Count == 0)
            {
                this.toastMain.Show(
                        "Preview first — press → or ⇒, then Split.",
                        ToastKind.Warning);
                return;
            }

            if (!this.ConfirmCommit("Split", this.stagedUnits.Rows.Count))
            {
                return;
            }

            DataTable units = this.stagedUnits;
            this.stagedUnits = null;
            this.MoveBetween(this.SourceId(), this.TargetId(), units, "Nothing to split.");
        }

        // Merge: **미리보기(→/⇒)로 스테이징된 것만** 대상(차 있는 캐리어)으로
        // 확정한다 — Split과 같은 스테이징 대상을 쓰고, 대상이 일부 차 있을
        // 때의 동선이다. ★ 회사 환경 교체 지점 — 회사 캐리어 병합 비즈 전문 호출.
        private void OnMergeClick(object sender, EventArgs e)
        {
            if (this.stagedUnits == null || this.stagedUnits.Rows.Count == 0)
            {
                this.toastMain.Show(
                        "Preview first — press → or ⇒, then Merge.",
                        ToastKind.Warning);
                return;
            }

            if (!this.ConfirmCommit("Merge", this.stagedUnits.Rows.Count))
            {
                return;
            }

            DataTable units = this.stagedUnits;
            this.stagedUnits = null;
            this.MoveBetween(this.SourceId(), this.TargetId(), units, "Nothing to merge.");
        }

        // Split/Merge 확정 다이얼로그 — 우측(대상) 캐리어를 확정한다는 것을
        // 알리고 승인받는다. ★ 회사 환경에서는 이 승인 뒤 비즈 전문을 호출한다.
        private bool ConfirmCommit(string action, int count)
        {
            DialogResult answer = MessageBox.Show(
                    this,
                    action + " " + count.ToString("N0") + " units from "
                            + this.SourceId() + " to " + this.TargetId() + "?"
                            + Environment.NewLine
                            + "This commits the target carrier " + this.TargetId() + ".",
                    "Carrier Editor — " + action,
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question);

            return answer == DialogResult.OK;
        }

        // Scrap: 원본에서 선택한 유닛을 폐기한다.
        // ★ 회사 환경 교체 지점 — ScrapUnits를 회사 인터페이스로.
        private void OnScrapClick(object sender, EventArgs e)
        {
            DataTable units = this.CollectUnitsByKeys(this.sourceData, this.mapSource.SelectedKeys);

            if (units.Rows.Count == 0)
            {
                this.toastMain.Show("No unit selected to scrap.", ToastKind.Warning);
                return;
            }

            string sourceId = this.SourceId();

            CarrierEditSimulator.ActionResult result = CarrierEditSimulator.ScrapUnits(
                    this.GetSelectedType(), sourceId, units);

            if (!result.Success)
            {
                this.toastMain.Show(result.Message, ToastKind.Warning);
                return;
            }

            this.toastMain.Show(
                    result.Count.ToString("N0") + " units scrapped from " + sourceId + ".",
                    ToastKind.Success);
            this.ReloadCarrierLists(sourceId, this.TargetId());
        }

        // ===== 이동 공용 =====

        private string SourceId()
        {
            return this.cboSource.SelectedValue as string ?? string.Empty;
        }

        private string TargetId()
        {
            return this.cboTarget.SelectedValue as string ?? string.Empty;
        }

        private void MoveBetween(string fromId, string toId, DataTable units, string emptyMessage)
        {
            this.MoveBetween(fromId, toId, units, string.Empty, 0, emptyMessage);
        }

        // 서버가 검증(같은 종류 빈 자리, 전부-아니면-전무)하고, 성공하면 양쪽
        // 재조회. FOUP은 랏이 달라도 공간만 있으면 이동하고 옮겨온 웨이퍼는
        // 대상 FOUP의 랏이 된다. 재조회는 현재 원본/대상 콤보를 유지한다.
        // ★ 회사 환경 교체 지점 — MoveUnits를 회사 인터페이스로.
        private void MoveBetween(
                string fromId, string toId, DataTable units,
                string anchorKind, int anchorPos, string emptyMessage)
        {
            if (units.Rows.Count == 0)
            {
                this.toastMain.Show(emptyMessage, ToastKind.Warning);
                return;
            }

            if (fromId.Length == 0 || toId.Length == 0)
            {
                this.toastMain.Show("Select both carriers first.", ToastKind.Warning);
                return;
            }

            // 이동 전에 옮길 유닛 ID를 기억해 둔다 — 성공 후 도착 맵에서 강조.
            System.Collections.Generic.List<string> movedIds =
                    new System.Collections.Generic.List<string>();

            foreach (DataRow row in units.Rows)
            {
                string id = PendingTablePresenter.CellText(row, "UNIT_ID").Trim();

                if (id.Length > 0)
                {
                    movedIds.Add(id);
                }
            }

            CarrierEditSimulator.ActionResult result = CarrierEditSimulator.MoveUnits(
                    this.GetSelectedType(), fromId, toId, units, anchorKind, anchorPos);

            if (!result.Success)
            {
                // 실패면 이동이 없으니 남아 있던 선택/미리보기를 정리한다.
                this.mapSource.ClearSelection();
                this.mapTarget.ClearSelection();
                this.ClearPreview();
                this.toastMain.Show(result.Message, ToastKind.Warning);
                return;
            }

            this.toastMain.Show(
                    result.Count.ToString("N0") + " units moved to " + toId + ".",
                    ToastKind.Success);
            this.ReloadCarrierLists(this.SourceId(), this.TargetId());

            // 도착 맵에서 방금 옮긴 유닛만 강조한다 (이동한 것만 선택되게).
            bool toTarget = toId == this.TargetId();
            this.HighlightMoved(
                    toTarget ? this.mapTarget : this.mapSource,
                    toTarget ? this.targetData : this.sourceData,
                    movedIds);
        }
    }
}
