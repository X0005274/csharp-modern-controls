namespace Modern.Lab.Samples
{
    public partial class CarrierEditForm
    {
        /// <summary>Required designer variable.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Clean up any resources being used.</summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.moveMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miMoveSelRight = new System.Windows.Forms.ToolStripMenuItem();
            this.miMoveAllRight = new System.Windows.Forms.ToolStripMenuItem();
            this.miMoveSelLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.miMoveAllLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.titlePanel = new System.Windows.Forms.Panel();
            this.lblTitle = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.badgeEnv = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.spTitle = new System.Windows.Forms.Panel();
            this.searchCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.lblType = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboType = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblCarrier = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboSource = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.btnRefresh = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.gapSearch = new System.Windows.Forms.Panel();
            this.midPanel = new System.Windows.Forms.Panel();
            this.tableMid = new System.Windows.Forms.TableLayoutPanel();
            this.sourceCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.mapSource = new Modern.Lab.WinForms.Controls.Display.ModernSlotMap();
            this.centerPanel = new System.Windows.Forms.Panel();
            this.lblTransfer = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.lblTransferHint = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.btnAllRight = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnSelRight = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnSelLeft = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnAllLeft = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.badgeSourceItem = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.targetCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.mapTarget = new Modern.Lab.WinForms.Controls.Display.ModernSlotMap();
            this.badgeTargetItem = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.lblTarget = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboTarget = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.gapBottom = new System.Windows.Forms.Panel();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.actionCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.lblActionStatus = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.btnSplit = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnMerge = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnScrap = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.toastMain = new Modern.Lab.WinForms.Controls.Display.ModernToast();
            this.tipItems = new System.Windows.Forms.ToolTip(this.components);
            this.titlePanel.SuspendLayout();
            this.searchCard.SuspendLayout();
            this.midPanel.SuspendLayout();
            this.tableMid.SuspendLayout();
            this.sourceCard.SuspendLayout();
            this.centerPanel.SuspendLayout();
            this.targetCard.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.actionCard.SuspendLayout();
            this.SuspendLayout();
            //
            // titlePanel
            //
            this.titlePanel.Controls.Add(this.lblTitle);
            this.titlePanel.Controls.Add(this.badgeEnv);
            this.titlePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.titlePanel.Location = new System.Drawing.Point(12, 12);
            this.titlePanel.Name = "titlePanel";
            this.titlePanel.Size = new System.Drawing.Size(1516, 28);
            this.titlePanel.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Title;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1456, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Carrier Editor";
            this.lblTitle.TitleBar = true;
            this.lblTitle.Child = null;
            //
            // badgeEnv
            //
            this.badgeEnv.Color = "#DBEAFE";
            this.badgeEnv.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeEnv.Dock = System.Windows.Forms.DockStyle.Right;
            this.badgeEnv.Location = new System.Drawing.Point(1456, 0);
            this.badgeEnv.Name = "badgeEnv";
            this.badgeEnv.Size = new System.Drawing.Size(60, 28);
            this.badgeEnv.TabIndex = 1;
            this.badgeEnv.Text = "AOS";
            this.badgeEnv.Child = null;
            //
            // spTitle
            //
            this.spTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.spTitle.Location = new System.Drawing.Point(12, 40);
            this.spTitle.Name = "spTitle";
            this.spTitle.Size = new System.Drawing.Size(1516, 8);
            this.spTitle.TabIndex = 1;
            //
            // searchCard
            //
            this.searchCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.searchCard.Controls.Add(this.lblType);
            this.searchCard.Controls.Add(this.cboType);
            this.searchCard.Controls.Add(this.lblCarrier);
            this.searchCard.Controls.Add(this.cboSource);
            this.searchCard.Controls.Add(this.lblTarget);
            this.searchCard.Controls.Add(this.cboTarget);
            this.searchCard.Controls.Add(this.btnRefresh);
            this.searchCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchCard.Location = new System.Drawing.Point(12, 48);
            this.searchCard.Name = "searchCard";
            this.searchCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.searchCard.Size = new System.Drawing.Size(1516, 72);
            this.searchCard.TabIndex = 2;
            //
            // lblType
            //
            this.lblType.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblType.Location = new System.Drawing.Point(12, 8);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(120, 18);
            this.lblType.TabIndex = 0;
            this.lblType.Text = "Carrier type";
            this.lblType.Child = null;
            //
            // cboType
            //
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.Location = new System.Drawing.Point(12, 32);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(120, 32);
            this.cboType.TabIndex = 1;
            this.cboType.SelectedIndexChanged += new System.EventHandler(this.OnTypeChanged);
            this.cboType.Child = null;
            //
            // lblCarrier
            //
            this.lblCarrier.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblCarrier.Location = new System.Drawing.Point(156, 8);
            this.lblCarrier.Name = "lblCarrier";
            this.lblCarrier.Size = new System.Drawing.Size(190, 18);
            this.lblCarrier.TabIndex = 2;
            this.lblCarrier.Text = "Source carrier";
            this.lblCarrier.Child = null;
            //
            // cboSource
            //
            this.cboSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSource.Location = new System.Drawing.Point(156, 32);
            this.cboSource.Name = "cboSource";
            this.cboSource.Size = new System.Drawing.Size(190, 32);
            this.cboSource.TabIndex = 3;
            this.cboSource.SelectedIndexChanged += new System.EventHandler(this.OnSourceChanged);
            this.cboSource.Child = null;
            //
            // lblTarget
            //
            // 대상 캐리어 선택도 조회 패널에 함께 둔다 — 좌/우 카드가 같은
            // 높이·같은 정보(맵 + 아이템/채움 집계)를 갖게 한다. 담긴
            // 아이템 ID는 각 카드 타이틀 우측에 표기된다.
            this.lblTarget.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblTarget.Location = new System.Drawing.Point(366, 8);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(190, 18);
            this.lblTarget.TabIndex = 4;
            this.lblTarget.Text = "Target carrier";
            this.lblTarget.Child = null;
            //
            // cboTarget
            //
            this.cboTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTarget.Location = new System.Drawing.Point(366, 32);
            this.cboTarget.Name = "cboTarget";
            this.cboTarget.Size = new System.Drawing.Size(190, 32);
            this.cboTarget.TabIndex = 5;
            this.cboTarget.SelectedIndexChanged += new System.EventHandler(this.OnTargetChanged);
            this.cboTarget.Child = null;
            //
            // btnRefresh
            //
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Location = new System.Drawing.Point(1424, 20);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(80, 32);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "↻";
            this.btnRefresh.TopLabel = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.OnRefreshClick);
            this.btnRefresh.Child = null;
            //
            // gapSearch
            //
            this.gapSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.gapSearch.Location = new System.Drawing.Point(12, 120);
            this.gapSearch.Name = "gapSearch";
            this.gapSearch.Size = new System.Drawing.Size(1516, 8);
            this.gapSearch.TabIndex = 3;
            //
            // midPanel
            //
            this.midPanel.Controls.Add(this.tableMid);
            this.midPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.midPanel.Location = new System.Drawing.Point(12, 128);
            this.midPanel.Name = "midPanel";
            this.midPanel.Size = new System.Drawing.Size(1516, 660);
            this.midPanel.TabIndex = 4;
            //
            // tableMid
            //
            // 좌(50%) | 중앙 이동 레일(96px 고정) | 우(50%) — 리사이즈
            // 시에도 좌/우 카드가 항상 같은 폭으로 함께 늘어난다.
            this.tableMid.ColumnCount = 3;
            this.tableMid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableMid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.tableMid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableMid.Controls.Add(this.sourceCard, 0, 0);
            this.tableMid.Controls.Add(this.centerPanel, 1, 0);
            this.tableMid.Controls.Add(this.targetCard, 2, 0);
            this.tableMid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableMid.Location = new System.Drawing.Point(0, 0);
            this.tableMid.Name = "tableMid";
            this.tableMid.RowCount = 1;
            this.tableMid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableMid.Size = new System.Drawing.Size(1516, 660);
            this.tableMid.TabIndex = 0;
            //
            // sourceCard
            //
            this.sourceCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.sourceCard.Controls.Add(this.badgeSourceItem);
            this.sourceCard.Controls.Add(this.mapSource);
            this.sourceCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceCard.Location = new System.Drawing.Point(0, 0);
            this.sourceCard.Name = "sourceCard";
            this.sourceCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.sourceCard.Size = new System.Drawing.Size(668, 660);
            this.sourceCard.TabIndex = 0;
            this.sourceCard.Text = "Source carrier";
            this.sourceCard.TitleAccent = false;
            //
            // badgeSourceItem
            //
            // 담긴 아이템 ID를 카드 제목 우측에 색 배지로 표기 (색 = 맵 토큰과
            // 동일). 제목 밴드(높이 32) 우측에 앵커로 붙는다.
            this.badgeSourceItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.badgeSourceItem.Color = "#DBEAFE";
            this.badgeSourceItem.Location = new System.Drawing.Point(120, 4);
            this.badgeSourceItem.Name = "badgeSourceItem";
            this.badgeSourceItem.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeSourceItem.Size = new System.Drawing.Size(116, 24);
            this.badgeSourceItem.TabIndex = 1;
            this.badgeSourceItem.Text = "-";
            this.badgeSourceItem.Child = null;
            //
            // mapSource
            //
            // 원본 슬롯 맵 — 채워진 셀 클릭으로 이동/폐기 대상을 고른다.
            this.mapSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapSource.EnableDragOut = false;
            this.mapSource.Location = new System.Drawing.Point(8, 40);
            this.mapSource.Name = "mapSource";
            this.mapSource.Size = new System.Drawing.Size(652, 612);
            this.mapSource.TabIndex = 0;
            this.mapSource.CellClicked += new System.EventHandler<Modern.Lab.Controls.Wpf.Display.SlotMapCellEventArgs>(this.OnSourceCellClicked);
            this.mapSource.CellRightClick += new System.EventHandler<Modern.Lab.Controls.Wpf.Display.SlotMapCellEventArgs>(this.OnSourceCellRightClick);
            this.mapSource.Child = null;
            //
            // centerPanel
            //
            // 가운데 이동 레일 — 위는 대상에 추가할 이동 계획, 아래는 계획 취소다.
            // 가운데 라벨은 이 열이 실제 이동이 아니라 "확정 전 계획"임을 분명히 한다.
            this.centerPanel.Controls.Add(this.lblTransfer);
            this.centerPanel.Controls.Add(this.lblTransferHint);
            this.centerPanel.Controls.Add(this.btnAllRight);
            this.centerPanel.Controls.Add(this.btnSelRight);
            this.centerPanel.Controls.Add(this.btnSelLeft);
            this.centerPanel.Controls.Add(this.btnAllLeft);
            this.centerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.centerPanel.Location = new System.Drawing.Point(668, 0);
            this.centerPanel.Name = "centerPanel";
            this.centerPanel.Size = new System.Drawing.Size(96, 660);
            this.centerPanel.TabIndex = 1;
            //
            // lblTransfer
            //
            this.lblTransfer.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblTransfer.Location = new System.Drawing.Point(8, 218);
            this.lblTransfer.Name = "lblTransfer";
            this.lblTransfer.Size = new System.Drawing.Size(80, 18);
            this.lblTransfer.TabIndex = 0;
            this.lblTransfer.Text = "PLAN";
            this.lblTransfer.Child = null;
            //
            // lblTransferHint
            //
            this.lblTransferHint.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Helper;
            this.lblTransferHint.Location = new System.Drawing.Point(8, 238);
            this.lblTransferHint.Name = "lblTransferHint";
            this.lblTransferHint.Size = new System.Drawing.Size(80, 18);
            this.lblTransferHint.TabIndex = 1;
            this.lblTransferHint.Text = "stage";
            this.lblTransferHint.Child = null;
            //
            // btnAllRight
            //
            // 좌→우 전체 이동 ⇒ (선택과 무관하게 원본 전부를 대상으로).
            this.btnAllRight.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnAllRight.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnAllRight.GlyphSize = 20D;
            this.btnAllRight.TopLabel = "All";
            this.btnAllRight.Location = new System.Drawing.Point(24, 264);
            this.btnAllRight.Name = "btnAllRight";
            this.btnAllRight.Size = new System.Drawing.Size(48, 44);
            this.btnAllRight.TabIndex = 2;
            this.btnAllRight.Text = "»";
            this.btnAllRight.Click += new System.EventHandler(this.OnMoveAllRight);
            this.btnAllRight.Child = null;
            //
            // btnSelRight
            //
            // 좌→우 선택 이동 → (원본에서 클릭해 고른 유닛만 대상으로).
            this.btnSelRight.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnSelRight.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnSelRight.GlyphSize = 20D;
            this.btnSelRight.TopLabel = "Selected";
            this.btnSelRight.Location = new System.Drawing.Point(24, 312);
            this.btnSelRight.Name = "btnSelRight";
            this.btnSelRight.Size = new System.Drawing.Size(48, 44);
            this.btnSelRight.TabIndex = 3;
            this.btnSelRight.Text = "›";
            this.btnSelRight.Click += new System.EventHandler(this.OnMoveSelRight);
            this.btnSelRight.Child = null;
            //
            // btnSelLeft
            //
            // 우→좌 선택 이동 ← (대상에서 클릭해 고른 유닛만 원본으로).
            this.btnSelLeft.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnSelLeft.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnSelLeft.GlyphSize = 20D;
            this.btnSelLeft.TopLabel = "Selected";
            this.btnSelLeft.Location = new System.Drawing.Point(24, 376);
            this.btnSelLeft.Name = "btnSelLeft";
            this.btnSelLeft.Size = new System.Drawing.Size(48, 44);
            this.btnSelLeft.TabIndex = 4;
            this.btnSelLeft.Text = "‹";
            this.btnSelLeft.Click += new System.EventHandler(this.OnMoveSelLeft);
            this.btnSelLeft.Child = null;
            //
            // btnAllLeft
            //
            // 우→좌 전체 이동 ⇐ (대상 전부를 원본으로 — 리셋 개념).
            this.btnAllLeft.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnAllLeft.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnAllLeft.GlyphSize = 20D;
            this.btnAllLeft.TopLabel = "All";
            this.btnAllLeft.Location = new System.Drawing.Point(24, 424);
            this.btnAllLeft.Name = "btnAllLeft";
            this.btnAllLeft.Size = new System.Drawing.Size(48, 44);
            this.btnAllLeft.TabIndex = 5;
            this.btnAllLeft.Text = "«";
            this.btnAllLeft.Click += new System.EventHandler(this.OnMoveAllLeft);
            this.btnAllLeft.Child = null;
            //
            // targetCard
            //
            // 대상 카드만 이동 계획이 생겼을 때 액센트로 강조한다. 평상시에는
            // 원본과 동일한 중립 헤더라 "아직 확정 전" 상태를 과장하지 않는다.
            this.targetCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.targetCard.Controls.Add(this.badgeTargetItem);
            this.targetCard.Controls.Add(this.mapTarget);
            this.targetCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.targetCard.Location = new System.Drawing.Point(764, 0);
            this.targetCard.Name = "targetCard";
            this.targetCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.targetCard.Size = new System.Drawing.Size(668, 660);
            this.targetCard.TabIndex = 2;
            this.targetCard.Text = "Target carrier";
            this.targetCard.TitleAccent = false;
            //
            // mapTarget
            //
            // 대상 슬롯 맵 — 원본 선택(→/⇒)에 따라 "들어갈 빈 자리" 미리보기가
            // 위에서부터 순차로 뜬다. 드래그앤드롭은 쓰지 않는다.
            this.mapTarget.AcceptDrops = false;
            this.mapTarget.AllowSelection = true;
            this.mapTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapTarget.Location = new System.Drawing.Point(8, 40);
            this.mapTarget.Name = "mapTarget";
            this.mapTarget.Size = new System.Drawing.Size(652, 612);
            this.mapTarget.TabIndex = 0;
            this.mapTarget.CellClicked += new System.EventHandler<Modern.Lab.Controls.Wpf.Display.SlotMapCellEventArgs>(this.OnTargetCellClicked);
            this.mapTarget.CellRightClick += new System.EventHandler<Modern.Lab.Controls.Wpf.Display.SlotMapCellEventArgs>(this.OnTargetCellRightClick);
            this.mapTarget.Child = null;
            //
            // badgeTargetItem
            //
            // 대상 캐리어가 담은 아이템 ID 배지 (비어 있으면 "Empty").
            this.badgeTargetItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.badgeTargetItem.Color = "#DBEAFE";
            this.badgeTargetItem.Location = new System.Drawing.Point(120, 4);
            this.badgeTargetItem.Name = "badgeTargetItem";
            this.badgeTargetItem.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeTargetItem.Size = new System.Drawing.Size(116, 24);
            this.badgeTargetItem.TabIndex = 1;
            this.badgeTargetItem.Text = "-";
            this.badgeTargetItem.Child = null;
            //
            // gapBottom
            //
            this.gapBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gapBottom.Location = new System.Drawing.Point(12, 732);
            this.gapBottom.Name = "gapBottom";
            this.gapBottom.Size = new System.Drawing.Size(1516, 8);
            this.gapBottom.TabIndex = 6;
            //
            // bottomPanel
            //
            // 하단 실행 바 — 이동 계획 요약과 확정 액션을 한 영역에 둔다.
            this.bottomPanel.Controls.Add(this.actionCard);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(12, 740);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(1516, 56);
            this.bottomPanel.TabIndex = 7;
            //
            // actionCard
            //
            this.actionCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.actionCard.Controls.Add(this.lblActionStatus);
            this.actionCard.Controls.Add(this.btnSplit);
            this.actionCard.Controls.Add(this.btnMerge);
            this.actionCard.Controls.Add(this.btnScrap);
            this.actionCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionCard.Location = new System.Drawing.Point(0, 0);
            this.actionCard.Name = "actionCard";
            this.actionCard.Size = new System.Drawing.Size(1516, 56);
            this.actionCard.TabIndex = 0;
            //
            // lblActionStatus
            //
            this.lblActionStatus.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Helper;
            this.lblActionStatus.Location = new System.Drawing.Point(12, 16);
            this.lblActionStatus.Name = "lblActionStatus";
            this.lblActionStatus.Size = new System.Drawing.Size(940, 24);
            this.lblActionStatus.TabIndex = 0;
            this.lblActionStatus.Text = "Select a source unit, then stage the move with → or ⇒.";
            this.lblActionStatus.Child = null;
            //
            // btnSplit
            //
            // Split — 회사 비즈(캐리어 분할 전문) 호출 지점. 대상이 비어
            // 있을 때의 표준 동선이라 committing 액션으로 강조(Primary).
            this.btnSplit.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Primary;
            this.btnSplit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSplit.Location = new System.Drawing.Point(1248, 12);
            this.btnSplit.Name = "btnSplit";
            this.btnSplit.Size = new System.Drawing.Size(80, 32);
            this.btnSplit.TabIndex = 1;
            this.btnSplit.Text = "Split";
            this.btnSplit.Click += new System.EventHandler(this.OnSplitClick);
            this.btnSplit.Child = null;
            //
            // btnMerge
            //
            // Merge — 회사 비즈(캐리어 병합 전문) 호출 지점. 대상이 차 있을
            // 때의 표준 동선이라 committing 액션으로 강조(Execute 초록).
            this.btnMerge.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnMerge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMerge.Location = new System.Drawing.Point(1336, 12);
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.Size = new System.Drawing.Size(80, 32);
            this.btnMerge.TabIndex = 2;
            this.btnMerge.Text = "Merge";
            this.btnMerge.Click += new System.EventHandler(this.OnMergeClick);
            this.btnMerge.Child = null;
            //
            // btnScrap
            //
            this.btnScrap.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Danger;
            this.btnScrap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnScrap.Location = new System.Drawing.Point(1424, 12);
            this.btnScrap.Name = "btnScrap";
            this.btnScrap.Size = new System.Drawing.Size(80, 32);
            this.btnScrap.TabIndex = 3;
            this.btnScrap.Text = "Scrap";
            this.btnScrap.Click += new System.EventHandler(this.OnScrapClick);
            this.btnScrap.Child = null;
            //
            // toastMain
            //
            this.toastMain.Location = new System.Drawing.Point(1220, 720);
            this.toastMain.Name = "toastMain";
            this.toastMain.Size = new System.Drawing.Size(280, 44);
            this.toastMain.TabIndex = 5;
            this.toastMain.Visible = false;
            this.toastMain.Child = null;
            //
            // moveMenu — 맵 오른쪽 클릭 시 뜨는 이동 컨텍스트 메뉴 (이동 버튼과 동일 동작).
            //
            this.moveMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.miMoveSelRight,
                this.miMoveAllRight,
                this.miMoveSelLeft,
                this.miMoveAllLeft});
            this.moveMenu.Name = "moveMenu";
            //
            // miMoveSelRight
            //
            this.miMoveSelRight.Name = "miMoveSelRight";
            this.miMoveSelRight.Text = "Move Selected  ›";
            this.miMoveSelRight.Click += new System.EventHandler(this.OnMoveSelRight);
            //
            // miMoveAllRight
            //
            this.miMoveAllRight.Name = "miMoveAllRight";
            this.miMoveAllRight.Text = "Move All  »";
            this.miMoveAllRight.Click += new System.EventHandler(this.OnMoveAllRight);
            //
            // miMoveSelLeft
            //
            this.miMoveSelLeft.Name = "miMoveSelLeft";
            this.miMoveSelLeft.Text = "Cancel Selected  ‹";
            this.miMoveSelLeft.Click += new System.EventHandler(this.OnMoveSelLeft);
            //
            // miMoveAllLeft
            //
            this.miMoveAllLeft.Name = "miMoveAllLeft";
            this.miMoveAllLeft.Text = "Cancel All  «";
            this.miMoveAllLeft.Click += new System.EventHandler(this.OnMoveAllLeft);
            //
            // CarrierEditForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1540, 800);
            this.Controls.Add(this.toastMain);
            this.Controls.Add(this.midPanel);
            this.Controls.Add(this.gapBottom);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.gapSearch);
            this.Controls.Add(this.searchCard);
            this.Controls.Add(this.spTitle);
            this.Controls.Add(this.titlePanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1240, 660);
            this.Name = "CarrierEditForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Carrier Editor";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.titlePanel.ResumeLayout(false);
            this.searchCard.ResumeLayout(false);
            this.midPanel.ResumeLayout(false);
            this.tableMid.ResumeLayout(false);
            this.sourceCard.ResumeLayout(false);
            this.centerPanel.ResumeLayout(false);
            this.targetCard.ResumeLayout(false);
            this.bottomPanel.ResumeLayout(false);
            this.actionCard.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel titlePanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTitle;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeEnv;
        private System.Windows.Forms.Panel spTitle;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel searchCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblType;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboType;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblCarrier;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboSource;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnRefresh;
        private System.Windows.Forms.Panel gapSearch;
        private System.Windows.Forms.Panel midPanel;
        private System.Windows.Forms.TableLayoutPanel tableMid;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox sourceCard;
        private Modern.Lab.WinForms.Controls.Display.ModernSlotMap mapSource;
        private System.Windows.Forms.Panel centerPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTransfer;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTransferHint;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnAllRight;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnSelRight;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnSelLeft;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnAllLeft;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeSourceItem;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeTargetItem;
        private System.Windows.Forms.Panel gapBottom;
        private System.Windows.Forms.Panel bottomPanel;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel actionCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblActionStatus;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnSplit;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnMerge;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnScrap;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox targetCard;
        private Modern.Lab.WinForms.Controls.Display.ModernSlotMap mapTarget;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTarget;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboTarget;
        private Modern.Lab.WinForms.Controls.Display.ModernToast toastMain;
        private System.Windows.Forms.ToolTip tipItems;
        private System.Windows.Forms.ContextMenuStrip moveMenu;
        private System.Windows.Forms.ToolStripMenuItem miMoveSelRight;
        private System.Windows.Forms.ToolStripMenuItem miMoveAllRight;
        private System.Windows.Forms.ToolStripMenuItem miMoveSelLeft;
        private System.Windows.Forms.ToolStripMenuItem miMoveAllLeft;
    }
}
