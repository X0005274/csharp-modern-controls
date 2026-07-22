namespace Modern.Lab.Samples
{
    public partial class LogisticsRequestForm
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
            this.boardMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miReceive = new System.Windows.Forms.ToolStripMenuItem();
            this.miCreate = new System.Windows.Forms.ToolStripMenuItem();
            this.titlePanel = new System.Windows.Forms.Panel();
            this.lblTitle = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.badgeEnv = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.spTitle = new System.Windows.Forms.Panel();
            this.searchCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.lblItemId = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboItemId = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblStatus = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboStatus = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblSend = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboSend = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblElapsed = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboElapsed = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.btnSearch = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnReset = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.gapSearch = new System.Windows.Forms.Panel();
            this.midPanel = new System.Windows.Forms.Panel();
            this.splitMid = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.boardCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridBoard = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.pagination = new Modern.Lab.WinForms.Controls.Data.ModernPagination();
            this.unitCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridUnits = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.gapPipe = new System.Windows.Forms.Panel();
            this.pipeCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.stepPipeline = new Modern.Lab.WinForms.Controls.Display.ModernStepIndicator();
            this.gapBottom = new System.Windows.Forms.Panel();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.kpiStrip = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.badgeTransit = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeReceived = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeNoReq = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeLinked = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeDone = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeUnmatched = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeAvg = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeOldest = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.gapAction = new System.Windows.Forms.Panel();
            this.actionCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.btnExport = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnReceive = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnManualReceive = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnCreate = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.busyMain = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.toastMain = new Modern.Lab.WinForms.Controls.Display.ModernToast();
            this.titlePanel.SuspendLayout();
            this.searchCard.SuspendLayout();
            this.midPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMid)).BeginInit();
            this.splitMid.Panel1.SuspendLayout();
            this.splitMid.Panel2.SuspendLayout();
            this.splitMid.SuspendLayout();
            this.boardCard.SuspendLayout();
            this.unitCard.SuspendLayout();
            this.pipeCard.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.kpiStrip.SuspendLayout();
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
            this.lblTitle.Text = "Logistics & Request";
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
            this.searchCard.Controls.Add(this.lblItemId);
            this.searchCard.Controls.Add(this.cboItemId);
            this.searchCard.Controls.Add(this.lblStatus);
            this.searchCard.Controls.Add(this.cboStatus);
            this.searchCard.Controls.Add(this.lblSend);
            this.searchCard.Controls.Add(this.cboSend);
            this.searchCard.Controls.Add(this.lblElapsed);
            this.searchCard.Controls.Add(this.cboElapsed);
            this.searchCard.Controls.Add(this.btnSearch);
            this.searchCard.Controls.Add(this.btnReset);
            this.searchCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchCard.Location = new System.Drawing.Point(12, 48);
            this.searchCard.Name = "searchCard";
            this.searchCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.searchCard.Size = new System.Drawing.Size(1516, 56);
            this.searchCard.TabIndex = 2;
            //
            // lblItemId
            //
            this.lblItemId.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblItemId.Location = new System.Drawing.Point(12, 12);
            this.lblItemId.Name = "lblItemId";
            this.lblItemId.Size = new System.Drawing.Size(56, 32);
            this.lblItemId.TabIndex = 0;
            this.lblItemId.Text = "Item ID";
            this.lblItemId.Child = null;
            //
            // cboItemId
            //
            this.cboItemId.Location = new System.Drawing.Point(72, 12);
            this.cboItemId.Name = "cboItemId";
            this.cboItemId.PlaceholderText = "All / type to filter";
            this.cboItemId.Size = new System.Drawing.Size(200, 32);
            this.cboItemId.TabIndex = 1;
            this.cboItemId.Child = null;
            //
            // lblStatus
            //
            this.lblStatus.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblStatus.Location = new System.Drawing.Point(296, 12);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(48, 32);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Status";
            this.lblStatus.Child = null;
            //
            // cboStatus
            //
            this.cboStatus.Location = new System.Drawing.Point(348, 12);
            this.cboStatus.Name = "cboStatus";
            this.cboStatus.Size = new System.Drawing.Size(140, 32);
            this.cboStatus.TabIndex = 3;
            this.cboStatus.Child = null;
            //
            // lblSend
            //
            this.lblSend.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblSend.Location = new System.Drawing.Point(512, 12);
            this.lblSend.Name = "lblSend";
            this.lblSend.Size = new System.Drawing.Size(64, 32);
            this.lblSend.TabIndex = 4;
            this.lblSend.Text = "Sent";
            this.lblSend.Child = null;
            //
            // cboSend
            //
            this.cboSend.Location = new System.Drawing.Point(580, 12);
            this.cboSend.Name = "cboSend";
            this.cboSend.Size = new System.Drawing.Size(130, 32);
            this.cboSend.TabIndex = 5;
            this.cboSend.Child = null;
            //
            // lblElapsed
            //
            this.lblElapsed.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblElapsed.Location = new System.Drawing.Point(734, 12);
            this.lblElapsed.Name = "lblElapsed";
            this.lblElapsed.Size = new System.Drawing.Size(56, 32);
            this.lblElapsed.TabIndex = 6;
            this.lblElapsed.Text = "Elapsed";
            this.lblElapsed.Child = null;
            //
            // cboElapsed
            //
            this.cboElapsed.Location = new System.Drawing.Point(794, 12);
            this.cboElapsed.Name = "cboElapsed";
            this.cboElapsed.Size = new System.Drawing.Size(130, 32);
            this.cboElapsed.TabIndex = 7;
            this.cboElapsed.Child = null;
            //
            // btnSearch
            //
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Location = new System.Drawing.Point(1336, 12);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(80, 32);
            this.btnSearch.TabIndex = 8;
            this.btnSearch.Text = "Search";
            this.btnSearch.Click += new System.EventHandler(this.OnSearchClick);
            this.btnSearch.Child = null;
            //
            // btnReset
            //
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReset.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnReset.Location = new System.Drawing.Point(1424, 12);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(80, 32);
            this.btnReset.TabIndex = 9;
            this.btnReset.Text = "Reset";
            this.btnReset.Click += new System.EventHandler(this.OnResetClick);
            this.btnReset.Child = null;
            //
            // gapSearch
            //
            this.gapSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.gapSearch.Location = new System.Drawing.Point(12, 104);
            this.gapSearch.Name = "gapSearch";
            this.gapSearch.Size = new System.Drawing.Size(1516, 8);
            this.gapSearch.TabIndex = 3;
            //
            // midPanel
            //
            this.midPanel.Controls.Add(this.splitMid);
            this.midPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.midPanel.Location = new System.Drawing.Point(12, 112);
            this.midPanel.Name = "midPanel";
            this.midPanel.Size = new System.Drawing.Size(1516, 612);
            this.midPanel.TabIndex = 4;
            //
            // splitMid
            //
            this.splitMid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMid.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitMid.Location = new System.Drawing.Point(0, 0);
            this.splitMid.Name = "splitMid";
            this.splitMid.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.splitMid.Panel1.Controls.Add(this.boardCard);
            this.splitMid.Panel1MinSize = 600;
            this.splitMid.Panel2.Controls.Add(this.unitCard);
            this.splitMid.Panel2.Controls.Add(this.gapPipe);
            this.splitMid.Panel2.Controls.Add(this.pipeCard);
            this.splitMid.Panel2.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.splitMid.Panel2MinSize = 260;
            this.splitMid.Size = new System.Drawing.Size(1516, 612);
            this.splitMid.SplitterDistance = 1130;
            this.splitMid.TabIndex = 0;
            //
            // boardCard
            //
            this.boardCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.boardCard.Controls.Add(this.gridBoard);
            this.boardCard.Controls.Add(this.pagination);
            this.boardCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.boardCard.Location = new System.Drawing.Point(0, 0);
            this.boardCard.Name = "boardCard";
            this.boardCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.boardCard.Size = new System.Drawing.Size(1130, 612);
            this.boardCard.TabIndex = 0;
            this.boardCard.Text = "Send / Receive Status";
            this.boardCard.TitleAccent = true;
            //
            // boardMenu — 현황판 행 우클릭 메뉴 (행 버튼과 동일 판정/처리).
            //
            this.boardMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miReceive,
            this.miCreate});
            this.boardMenu.Name = "boardMenu";
            this.boardMenu.Size = new System.Drawing.Size(140, 48);
            this.boardMenu.Opening += new System.ComponentModel.CancelEventHandler(this.OnBoardMenuOpening);
            //
            // miReceive
            //
            this.miReceive.Name = "miReceive";
            this.miReceive.Size = new System.Drawing.Size(139, 22);
            this.miReceive.Text = "Receive";
            this.miReceive.Click += new System.EventHandler(this.OnMenuReceiveClick);
            //
            // miCreate
            //
            this.miCreate.Name = "miCreate";
            this.miCreate.Size = new System.Drawing.Size(139, 22);
            this.miCreate.Text = "Create";
            this.miCreate.Click += new System.EventHandler(this.OnMenuCreateClick);
            //
            // gridBoard
            //
            this.gridBoard.AutoFitColumns = true;
            this.gridBoard.ContextMenuStrip = this.boardMenu;
            this.gridBoard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridBoard.Location = new System.Drawing.Point(8, 40);
            this.gridBoard.Name = "gridBoard";
            this.gridBoard.Size = new System.Drawing.Size(1114, 528);
            this.gridBoard.TabIndex = 0;
            this.gridBoard.ColumnFiltersChanged += new System.EventHandler(this.OnBoardColumnFiltersChanged);
            this.gridBoard.SelectionChanged += new System.EventHandler(this.OnBoardSelectionChanged);
            this.gridBoard.CellButtonClick += new System.EventHandler<Modern.Lab.Controls.Wpf.Data.GridButtonClickEventArgs>(this.OnGridCellButtonClick);
            this.gridBoard.VisibleRowCapacityChanged += new System.EventHandler(this.OnGridCapacityChanged);
            this.gridBoard.Child = null;
            //
            // pagination
            //
            this.pagination.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pagination.Location = new System.Drawing.Point(8, 568);
            this.pagination.Name = "pagination";
            this.pagination.Size = new System.Drawing.Size(1114, 36);
            this.pagination.TabIndex = 1;
            this.pagination.TotalCountFormat = "{0:N0} items";
            this.pagination.PageChanged += new System.EventHandler(this.OnPageChanged);
            this.pagination.Child = null;
            //
            // unitCard
            //
            this.unitCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.unitCard.Controls.Add(this.gridUnits);
            this.unitCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.unitCard.Location = new System.Drawing.Point(8, 0);
            this.unitCard.Name = "unitCard";
            this.unitCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.unitCard.Size = new System.Drawing.Size(370, 612);
            this.unitCard.TabIndex = 0;
            this.unitCard.Text = "Units";
            //
            // gridUnits
            //
            this.gridUnits.AutoFitColumns = true;
            this.gridUnits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridUnits.Location = new System.Drawing.Point(8, 40);
            this.gridUnits.Name = "gridUnits";
            this.gridUnits.ShowStatusBar = true;
            this.gridUnits.Size = new System.Drawing.Size(354, 452);
            this.gridUnits.StatusCountFormat = "{0:N0} units";
            this.gridUnits.TabIndex = 0;
            this.gridUnits.Child = null;
            //
            // gapPipe
            //
            this.gapPipe.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gapPipe.Location = new System.Drawing.Point(8, 500);
            this.gapPipe.Name = "gapPipe";
            this.gapPipe.Size = new System.Drawing.Size(370, 8);
            this.gapPipe.TabIndex = 1;
            //
            // pipeCard
            //
            this.pipeCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.pipeCard.Controls.Add(this.stepPipeline);
            this.pipeCard.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pipeCard.Location = new System.Drawing.Point(8, 492);
            this.pipeCard.Name = "pipeCard";
            this.pipeCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.pipeCard.Size = new System.Drawing.Size(370, 120);
            this.pipeCard.TabIndex = 2;
            this.pipeCard.Text = "Pipeline";
            //
            // stepPipeline
            //
            this.stepPipeline.DisplayMember = "LABEL";
            this.stepPipeline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stepPipeline.Location = new System.Drawing.Point(8, 40);
            this.stepPipeline.Name = "stepPipeline";
            this.stepPipeline.Size = new System.Drawing.Size(354, 72);
            this.stepPipeline.StateMember = "STATE";
            this.stepPipeline.TabIndex = 0;
            this.stepPipeline.Child = null;
            //
            // gapBottom
            //
            this.gapBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gapBottom.Location = new System.Drawing.Point(12, 724);
            this.gapBottom.Name = "gapBottom";
            this.gapBottom.Size = new System.Drawing.Size(1516, 8);
            this.gapBottom.TabIndex = 5;
            //
            // bottomPanel
            //
            this.bottomPanel.Controls.Add(this.kpiStrip);
            this.bottomPanel.Controls.Add(this.gapAction);
            this.bottomPanel.Controls.Add(this.actionCard);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(12, 732);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(1516, 56);
            this.bottomPanel.TabIndex = 6;
            //
            // kpiStrip
            //
            this.kpiStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.kpiStrip.Controls.Add(this.badgeTransit);
            this.kpiStrip.Controls.Add(this.badgeReceived);
            this.kpiStrip.Controls.Add(this.badgeNoReq);
            this.kpiStrip.Controls.Add(this.badgeLinked);
            this.kpiStrip.Controls.Add(this.badgeDone);
            this.kpiStrip.Controls.Add(this.badgeUnmatched);
            this.kpiStrip.Controls.Add(this.badgeAvg);
            this.kpiStrip.Controls.Add(this.badgeOldest);
            this.kpiStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiStrip.Location = new System.Drawing.Point(0, 0);
            this.kpiStrip.Name = "kpiStrip";
            this.kpiStrip.Size = new System.Drawing.Size(1208, 56);
            this.kpiStrip.TabIndex = 0;
            //
            // badgeTransit
            //
            this.badgeTransit.Color = "#E5E7EB";
            this.badgeTransit.Location = new System.Drawing.Point(12, 16);
            this.badgeTransit.Name = "badgeTransit";
            this.badgeTransit.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeTransit.Size = new System.Drawing.Size(100, 24);
            this.badgeTransit.TabIndex = 0;
            this.badgeTransit.Text = "-";
            this.badgeTransit.Child = null;
            //
            // badgeReceived
            //
            this.badgeReceived.Color = "#DBEAFE";
            this.badgeReceived.Location = new System.Drawing.Point(120, 16);
            this.badgeReceived.Name = "badgeReceived";
            this.badgeReceived.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeReceived.Size = new System.Drawing.Size(100, 24);
            this.badgeReceived.TabIndex = 1;
            this.badgeReceived.Text = "-";
            this.badgeReceived.Child = null;
            //
            // badgeNoReq
            //
            this.badgeNoReq.Color = "#FEF3C7";
            this.badgeNoReq.Location = new System.Drawing.Point(228, 16);
            this.badgeNoReq.Name = "badgeNoReq";
            this.badgeNoReq.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeNoReq.Size = new System.Drawing.Size(120, 24);
            this.badgeNoReq.TabIndex = 2;
            this.badgeNoReq.Text = "-";
            this.badgeNoReq.Child = null;
            //
            // badgeLinked
            //
            this.badgeLinked.Color = "#E0E7FF";
            this.badgeLinked.Location = new System.Drawing.Point(356, 16);
            this.badgeLinked.Name = "badgeLinked";
            this.badgeLinked.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeLinked.Size = new System.Drawing.Size(100, 24);
            this.badgeLinked.TabIndex = 3;
            this.badgeLinked.Text = "-";
            this.badgeLinked.Child = null;
            //
            // badgeDone
            //
            this.badgeDone.Color = "#DCFCE7";
            this.badgeDone.Location = new System.Drawing.Point(464, 16);
            this.badgeDone.Name = "badgeDone";
            this.badgeDone.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeDone.Size = new System.Drawing.Size(120, 24);
            this.badgeDone.TabIndex = 4;
            this.badgeDone.Text = "-";
            this.badgeDone.Child = null;
            //
            // badgeUnmatched
            //
            this.badgeUnmatched.Color = "#FEE2E2";
            this.badgeUnmatched.Location = new System.Drawing.Point(592, 16);
            this.badgeUnmatched.Name = "badgeUnmatched";
            this.badgeUnmatched.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeUnmatched.Size = new System.Drawing.Size(120, 24);
            this.badgeUnmatched.TabIndex = 5;
            this.badgeUnmatched.Text = "-";
            this.badgeUnmatched.Child = null;
            //
            // badgeAvg
            //
            this.badgeAvg.Location = new System.Drawing.Point(720, 16);
            this.badgeAvg.Name = "badgeAvg";
            this.badgeAvg.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeAvg.Size = new System.Drawing.Size(100, 24);
            this.badgeAvg.TabIndex = 6;
            this.badgeAvg.Text = "-";
            this.badgeAvg.Child = null;
            //
            // badgeOldest
            //
            this.badgeOldest.Location = new System.Drawing.Point(828, 16);
            this.badgeOldest.Name = "badgeOldest";
            this.badgeOldest.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeOldest.Size = new System.Drawing.Size(110, 24);
            this.badgeOldest.TabIndex = 7;
            this.badgeOldest.Text = "-";
            this.badgeOldest.Child = null;
            //
            // gapAction
            //
            this.gapAction.Dock = System.Windows.Forms.DockStyle.Right;
            this.gapAction.Location = new System.Drawing.Point(1208, 0);
            this.gapAction.Name = "gapAction";
            this.gapAction.Size = new System.Drawing.Size(8, 56);
            this.gapAction.TabIndex = 1;
            //
            // actionCard
            //
            this.actionCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.actionCard.Controls.Add(this.btnExport);
            this.actionCard.Controls.Add(this.btnReceive);
            this.actionCard.Controls.Add(this.btnManualReceive);
            this.actionCard.Controls.Add(this.btnCreate);
            this.actionCard.Dock = System.Windows.Forms.DockStyle.Right;
            this.actionCard.Location = new System.Drawing.Point(1090, 0);
            this.actionCard.Name = "actionCard";
            this.actionCard.Size = new System.Drawing.Size(426, 56);
            this.actionCard.TabIndex = 2;
            //
            // btnExport
            //
            this.btnExport.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Excel;
            this.btnExport.Location = new System.Drawing.Point(12, 12);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(80, 32);
            this.btnExport.TabIndex = 0;
            this.btnExport.Text = "Excel";
            this.btnExport.Click += new System.EventHandler(this.OnExportClick);
            this.btnExport.Child = null;
            //
            // btnManualReceive
            //
            this.btnManualReceive.Location = new System.Drawing.Point(100, 12);
            this.btnManualReceive.Name = "btnManualReceive";
            this.btnManualReceive.Size = new System.Drawing.Size(118, 32);
            this.btnManualReceive.TabIndex = 1;
            this.btnManualReceive.Text = "Manual Receive";
            this.btnManualReceive.Click += new System.EventHandler(this.OnManualReceiveClick);
            this.btnManualReceive.Child = null;
            //
            // btnReceive
            //
            this.btnReceive.Location = new System.Drawing.Point(226, 12);
            this.btnReceive.Name = "btnReceive";
            this.btnReceive.Size = new System.Drawing.Size(90, 32);
            this.btnReceive.TabIndex = 2;
            this.btnReceive.Text = "Receive";
            this.btnReceive.Click += new System.EventHandler(this.OnReceiveClick);
            this.btnReceive.Child = null;
            //
            // btnCreate
            //
            this.btnCreate.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnCreate.Location = new System.Drawing.Point(324, 12);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(90, 32);
            this.btnCreate.TabIndex = 3;
            this.btnCreate.Text = "Create";
            this.btnCreate.Click += new System.EventHandler(this.OnCreateClick);
            this.btnCreate.Child = null;
            //
            // busyMain
            //
            this.busyMain.Location = new System.Drawing.Point(618, 310);
            this.busyMain.Message = "Loading...";
            this.busyMain.Name = "busyMain";
            this.busyMain.Size = new System.Drawing.Size(300, 180);
            this.busyMain.SubMessage = "Fetching send status";
            this.busyMain.TabIndex = 7;
            this.busyMain.Visible = false;
            this.busyMain.Child = null;
            //
            // toastMain
            //
            this.toastMain.Location = new System.Drawing.Point(1220, 720);
            this.toastMain.Name = "toastMain";
            this.toastMain.Size = new System.Drawing.Size(280, 44);
            this.toastMain.TabIndex = 8;
            this.toastMain.Visible = false;
            this.toastMain.Child = null;
            //
            // LogisticsRequestForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1540, 800);
            this.Controls.Add(this.toastMain);
            this.Controls.Add(this.busyMain);
            this.Controls.Add(this.midPanel);
            this.Controls.Add(this.gapBottom);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.gapSearch);
            this.Controls.Add(this.searchCard);
            this.Controls.Add(this.spTitle);
            this.Controls.Add(this.titlePanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1240, 660);
            this.Name = "LogisticsRequestForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Logistics & Request";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.titlePanel.ResumeLayout(false);
            this.searchCard.ResumeLayout(false);
            this.midPanel.ResumeLayout(false);
            this.splitMid.Panel1.ResumeLayout(false);
            this.splitMid.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMid)).EndInit();
            this.splitMid.ResumeLayout(false);
            this.boardCard.ResumeLayout(false);
            this.unitCard.ResumeLayout(false);
            this.pipeCard.ResumeLayout(false);
            this.bottomPanel.ResumeLayout(false);
            this.kpiStrip.ResumeLayout(false);
            this.actionCard.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel titlePanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTitle;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeEnv;
        private System.Windows.Forms.Panel spTitle;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel searchCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblItemId;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboItemId;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblStatus;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboStatus;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblSend;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboSend;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblElapsed;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboElapsed;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnSearch;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReset;
        private System.Windows.Forms.Panel gapSearch;
        private System.Windows.Forms.Panel midPanel;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitMid;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox boardCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridBoard;
        private Modern.Lab.WinForms.Controls.Data.ModernPagination pagination;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox unitCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridUnits;
        private System.Windows.Forms.Panel gapPipe;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox pipeCard;
        private Modern.Lab.WinForms.Controls.Display.ModernStepIndicator stepPipeline;
        private System.Windows.Forms.Panel gapBottom;
        private System.Windows.Forms.Panel bottomPanel;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel kpiStrip;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeTransit;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeReceived;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeNoReq;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeLinked;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeDone;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeUnmatched;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeAvg;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeOldest;
        private System.Windows.Forms.Panel gapAction;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel actionCard;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnExport;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReceive;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnManualReceive;
        private System.Windows.Forms.ContextMenuStrip boardMenu;
        private System.Windows.Forms.ToolStripMenuItem miReceive;
        private System.Windows.Forms.ToolStripMenuItem miCreate;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnCreate;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay busyMain;
        private Modern.Lab.WinForms.Controls.Display.ModernToast toastMain;
    }
}
