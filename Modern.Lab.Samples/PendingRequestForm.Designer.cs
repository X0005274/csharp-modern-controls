namespace Modern.Lab.Samples
{
    public partial class PendingRequestForm
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
            this.titlePanel = new System.Windows.Forms.Panel();
            this.lblTitle = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.badgeEnv = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.spTitle = new System.Windows.Forms.Panel();
            this.searchCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.lblItemId = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboItemId = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblElapsed = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboElapsed = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblLogistics = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.rdoLogistics = new Modern.Lab.WinForms.Controls.Selection.ModernRadioGroup();
            this.btnSearch = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnReset = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.gapSearch = new System.Windows.Forms.Panel();
            this.midPanel = new System.Windows.Forms.Panel();
            this.splitMid = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.itemCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridItems = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.pagination = new Modern.Lab.WinForms.Controls.Data.ModernPagination();
            this.unitCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridUnits = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.gapBottom = new System.Windows.Forms.Panel();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.kpiStrip = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.badgePending = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeUnits = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeAvg = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeOldest = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.lblAging = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.badgeAging0 = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeAging1 = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeAging2 = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeAging3 = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.gapAction = new System.Windows.Forms.Panel();
            this.actionCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.btnExport = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnReturn = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnLogistics = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.busyMain = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.toastMain = new Modern.Lab.WinForms.Controls.Display.ModernToast();
            this.titlePanel.SuspendLayout();
            this.searchCard.SuspendLayout();
            this.midPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMid)).BeginInit();
            this.splitMid.Panel1.SuspendLayout();
            this.splitMid.Panel2.SuspendLayout();
            this.splitMid.SuspendLayout();
            this.itemCard.SuspendLayout();
            this.unitCard.SuspendLayout();
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
            this.lblTitle.Text = "Pending Requests";
            this.lblTitle.TitleBar = true;
            this.lblTitle.Child = null;
            //
            // badgeEnv
            //
            this.badgeEnv.Color = "#DBEAFE";
            this.badgeEnv.Dock = System.Windows.Forms.DockStyle.Right;
            this.badgeEnv.Location = new System.Drawing.Point(1456, 0);
            this.badgeEnv.Name = "badgeEnv";
            this.badgeEnv.Size = new System.Drawing.Size(60, 28);
            this.badgeEnv.TabIndex = 1;
            this.badgeEnv.Text = "MES";
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
            this.searchCard.Controls.Add(this.lblElapsed);
            this.searchCard.Controls.Add(this.cboElapsed);
            this.searchCard.Controls.Add(this.lblLogistics);
            this.searchCard.Controls.Add(this.rdoLogistics);
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
            // lblElapsed
            //
            this.lblElapsed.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblElapsed.Location = new System.Drawing.Point(296, 12);
            this.lblElapsed.Name = "lblElapsed";
            this.lblElapsed.Size = new System.Drawing.Size(56, 32);
            this.lblElapsed.TabIndex = 2;
            this.lblElapsed.Text = "Elapsed";
            this.lblElapsed.Child = null;
            //
            // cboElapsed
            //
            this.cboElapsed.Location = new System.Drawing.Point(356, 12);
            this.cboElapsed.Name = "cboElapsed";
            this.cboElapsed.Size = new System.Drawing.Size(140, 32);
            this.cboElapsed.TabIndex = 3;
            this.cboElapsed.Child = null;
            //
            // lblLogistics
            //
            this.lblLogistics.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblLogistics.Location = new System.Drawing.Point(520, 12);
            this.lblLogistics.Name = "lblLogistics";
            this.lblLogistics.Size = new System.Drawing.Size(64, 32);
            this.lblLogistics.TabIndex = 4;
            this.lblLogistics.Text = "Logistics";
            this.lblLogistics.Child = null;
            //
            // rdoLogistics
            //
            this.rdoLogistics.Location = new System.Drawing.Point(588, 12);
            this.rdoLogistics.Name = "rdoLogistics";
            this.rdoLogistics.Size = new System.Drawing.Size(280, 32);
            this.rdoLogistics.TabIndex = 5;
            this.rdoLogistics.Child = null;
            //
            // btnSearch
            //
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Location = new System.Drawing.Point(1336, 12);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(80, 32);
            this.btnSearch.TabIndex = 6;
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
            this.btnReset.TabIndex = 7;
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
            this.splitMid.Panel1.Controls.Add(this.itemCard);
            this.splitMid.Panel1MinSize = 600;
            this.splitMid.Panel2.Controls.Add(this.unitCard);
            this.splitMid.Panel2.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.splitMid.Panel2MinSize = 260;
            this.splitMid.Size = new System.Drawing.Size(1516, 612);
            this.splitMid.SplitterDistance = 1130;
            this.splitMid.TabIndex = 0;
            //
            // itemCard
            //
            this.itemCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.itemCard.Controls.Add(this.gridItems);
            this.itemCard.Controls.Add(this.pagination);
            this.itemCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.itemCard.Location = new System.Drawing.Point(0, 0);
            this.itemCard.Name = "itemCard";
            this.itemCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.itemCard.Size = new System.Drawing.Size(1130, 612);
            this.itemCard.TabIndex = 0;
            this.itemCard.Text = "Pending Items";
            this.itemCard.TitleAccent = true;
            //
            // gridItems
            //
            this.gridItems.AutoFitColumns = true;
            this.gridItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridItems.Location = new System.Drawing.Point(8, 40);
            this.gridItems.Name = "gridItems";
            this.gridItems.Size = new System.Drawing.Size(1114, 528);
            this.gridItems.TabIndex = 0;
            this.gridItems.SelectionChanged += new System.EventHandler(this.OnItemSelectionChanged);
            this.gridItems.CellButtonClick += new System.EventHandler<Modern.Lab.Controls.Wpf.Data.GridButtonClickEventArgs>(this.OnGridCellButtonClick);
            this.gridItems.Child = null;
            //
            // pagination
            //
            this.pagination.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pagination.Location = new System.Drawing.Point(8, 568);
            this.pagination.Name = "pagination";
            this.pagination.PageSize = 15;
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
            this.gridUnits.Size = new System.Drawing.Size(354, 564);
            this.gridUnits.StatusCountFormat = "{0:N0} units";
            this.gridUnits.TabIndex = 0;
            this.gridUnits.Child = null;
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
            this.kpiStrip.Controls.Add(this.badgePending);
            this.kpiStrip.Controls.Add(this.badgeUnits);
            this.kpiStrip.Controls.Add(this.badgeAvg);
            this.kpiStrip.Controls.Add(this.badgeOldest);
            this.kpiStrip.Controls.Add(this.lblAging);
            this.kpiStrip.Controls.Add(this.badgeAging0);
            this.kpiStrip.Controls.Add(this.badgeAging1);
            this.kpiStrip.Controls.Add(this.badgeAging2);
            this.kpiStrip.Controls.Add(this.badgeAging3);
            this.kpiStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiStrip.Location = new System.Drawing.Point(0, 0);
            this.kpiStrip.Name = "kpiStrip";
            this.kpiStrip.Size = new System.Drawing.Size(1164, 56);
            this.kpiStrip.TabIndex = 0;
            //
            // badgePending
            //
            this.badgePending.Location = new System.Drawing.Point(12, 16);
            this.badgePending.Name = "badgePending";
            this.badgePending.Size = new System.Drawing.Size(116, 24);
            this.badgePending.TabIndex = 0;
            this.badgePending.Text = "-";
            this.badgePending.Child = null;
            //
            // badgeUnits
            //
            this.badgeUnits.Location = new System.Drawing.Point(136, 16);
            this.badgeUnits.Name = "badgeUnits";
            this.badgeUnits.Size = new System.Drawing.Size(100, 24);
            this.badgeUnits.TabIndex = 1;
            this.badgeUnits.Text = "-";
            this.badgeUnits.Child = null;
            //
            // badgeAvg
            //
            this.badgeAvg.Location = new System.Drawing.Point(244, 16);
            this.badgeAvg.Name = "badgeAvg";
            this.badgeAvg.Size = new System.Drawing.Size(108, 24);
            this.badgeAvg.TabIndex = 2;
            this.badgeAvg.Text = "-";
            this.badgeAvg.Child = null;
            //
            // badgeOldest
            //
            this.badgeOldest.Location = new System.Drawing.Point(360, 16);
            this.badgeOldest.Name = "badgeOldest";
            this.badgeOldest.Size = new System.Drawing.Size(116, 24);
            this.badgeOldest.TabIndex = 3;
            this.badgeOldest.Text = "-";
            this.badgeOldest.Child = null;
            //
            // lblAging
            //
            this.lblAging.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblAging.Location = new System.Drawing.Point(508, 16);
            this.lblAging.Name = "lblAging";
            this.lblAging.Size = new System.Drawing.Size(44, 24);
            this.lblAging.TabIndex = 4;
            this.lblAging.Text = "Aging";
            this.lblAging.Child = null;
            //
            // badgeAging0
            //
            this.badgeAging0.Color = "#DBEAFE";
            this.badgeAging0.Location = new System.Drawing.Point(556, 16);
            this.badgeAging0.Name = "badgeAging0";
            this.badgeAging0.Size = new System.Drawing.Size(92, 24);
            this.badgeAging0.TabIndex = 5;
            this.badgeAging0.Text = "-";
            this.badgeAging0.Child = null;
            //
            // badgeAging1
            //
            this.badgeAging1.Color = "#FEF3C7";
            this.badgeAging1.Location = new System.Drawing.Point(656, 16);
            this.badgeAging1.Name = "badgeAging1";
            this.badgeAging1.Size = new System.Drawing.Size(92, 24);
            this.badgeAging1.TabIndex = 6;
            this.badgeAging1.Text = "-";
            this.badgeAging1.Child = null;
            //
            // badgeAging2
            //
            this.badgeAging2.Color = "#FFE0CC";
            this.badgeAging2.Location = new System.Drawing.Point(756, 16);
            this.badgeAging2.Name = "badgeAging2";
            this.badgeAging2.Size = new System.Drawing.Size(92, 24);
            this.badgeAging2.TabIndex = 7;
            this.badgeAging2.Text = "-";
            this.badgeAging2.Child = null;
            //
            // badgeAging3
            //
            this.badgeAging3.Color = "#FEE2E2";
            this.badgeAging3.Location = new System.Drawing.Point(856, 16);
            this.badgeAging3.Name = "badgeAging3";
            this.badgeAging3.Size = new System.Drawing.Size(92, 24);
            this.badgeAging3.TabIndex = 8;
            this.badgeAging3.Text = "-";
            this.badgeAging3.Child = null;
            //
            // gapAction
            //
            this.gapAction.Dock = System.Windows.Forms.DockStyle.Right;
            this.gapAction.Location = new System.Drawing.Point(1164, 0);
            this.gapAction.Name = "gapAction";
            this.gapAction.Size = new System.Drawing.Size(8, 56);
            this.gapAction.TabIndex = 1;
            //
            // actionCard
            //
            this.actionCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.actionCard.Controls.Add(this.btnExport);
            this.actionCard.Controls.Add(this.btnReturn);
            this.actionCard.Controls.Add(this.btnLogistics);
            this.actionCard.Dock = System.Windows.Forms.DockStyle.Right;
            this.actionCard.Location = new System.Drawing.Point(1172, 0);
            this.actionCard.Name = "actionCard";
            this.actionCard.Size = new System.Drawing.Size(344, 56);
            this.actionCard.TabIndex = 2;
            //
            // btnExport
            //
            this.btnExport.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnExport.Location = new System.Drawing.Point(12, 12);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(110, 32);
            this.btnExport.TabIndex = 0;
            this.btnExport.Text = "Export Excel";
            this.btnExport.Click += new System.EventHandler(this.OnExportClick);
            this.btnExport.Child = null;
            //
            // btnReturn
            //
            this.btnReturn.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Danger;
            this.btnReturn.Location = new System.Drawing.Point(130, 12);
            this.btnReturn.Name = "btnReturn";
            this.btnReturn.Size = new System.Drawing.Size(90, 32);
            this.btnReturn.TabIndex = 1;
            this.btnReturn.Text = "Return";
            this.btnReturn.Click += new System.EventHandler(this.OnReturnClick);
            this.btnReturn.Child = null;
            //
            // btnLogistics
            //
            this.btnLogistics.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnLogistics.Location = new System.Drawing.Point(228, 12);
            this.btnLogistics.Name = "btnLogistics";
            this.btnLogistics.Size = new System.Drawing.Size(104, 32);
            this.btnLogistics.TabIndex = 2;
            this.btnLogistics.Text = "Logistics";
            this.btnLogistics.Click += new System.EventHandler(this.OnLogisticsClick);
            this.btnLogistics.Child = null;
            //
            // busyMain
            //
            this.busyMain.Location = new System.Drawing.Point(618, 310);
            this.busyMain.Message = "Loading...";
            this.busyMain.Name = "busyMain";
            this.busyMain.Size = new System.Drawing.Size(300, 180);
            this.busyMain.SubMessage = "Fetching pending items";
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
            // PendingRequestForm
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
            this.Name = "PendingRequestForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pending Requests";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.titlePanel.ResumeLayout(false);
            this.searchCard.ResumeLayout(false);
            this.midPanel.ResumeLayout(false);
            this.splitMid.Panel1.ResumeLayout(false);
            this.splitMid.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMid)).EndInit();
            this.splitMid.ResumeLayout(false);
            this.itemCard.ResumeLayout(false);
            this.unitCard.ResumeLayout(false);
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
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblElapsed;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboElapsed;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblLogistics;
        private Modern.Lab.WinForms.Controls.Selection.ModernRadioGroup rdoLogistics;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnSearch;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReset;
        private System.Windows.Forms.Panel gapSearch;
        private System.Windows.Forms.Panel midPanel;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitMid;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox itemCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridItems;
        private Modern.Lab.WinForms.Controls.Data.ModernPagination pagination;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox unitCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridUnits;
        private System.Windows.Forms.Panel gapBottom;
        private System.Windows.Forms.Panel bottomPanel;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel kpiStrip;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgePending;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeUnits;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeAvg;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeOldest;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblAging;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeAging0;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeAging1;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeAging2;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeAging3;
        private System.Windows.Forms.Panel gapAction;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel actionCard;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnExport;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReturn;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnLogistics;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay busyMain;
        private Modern.Lab.WinForms.Controls.Display.ModernToast toastMain;
    }
}
