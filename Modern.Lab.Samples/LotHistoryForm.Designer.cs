namespace Modern.Lab.Samples
{
    public partial class LotHistoryForm
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
            this.lblType = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboType = new Modern.Lab.WinForms.Controls.Selection.ModernCheckComboBox();
            this.lblLotId = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtLotId = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.btnSearch = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnReset = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.spSearch = new System.Windows.Forms.Panel();
            this.mainZone = new System.Windows.Forms.Panel();
            this.leftZone = new System.Windows.Forms.Panel();
            this.treeCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.treeLotWf = new Modern.Lab.WinForms.Controls.Selection.ModernTreeView();
            this.spWafer = new System.Windows.Forms.Panel();
            this.waferCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridWafers = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.spTree = new System.Windows.Forms.Panel();
            this.rightZone = new System.Windows.Forms.Panel();
            this.busyMain = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.gridHistory = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.spDetail = new System.Windows.Forms.Panel();
            this.detailCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.lblSelId = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.badgeType = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeStat = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.capProduct = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.valProduct = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.capFlow = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.valFlow = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.capOper = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.valOper = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.capEqp = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.valEqp = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.capCarrier = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.valCarrier = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.capEventTm = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.valEventTm = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.toastMain = new Modern.Lab.WinForms.Controls.Display.ModernToast();
            this.titlePanel.SuspendLayout();
            this.searchCard.SuspendLayout();
            this.mainZone.SuspendLayout();
            this.leftZone.SuspendLayout();
            this.treeCard.SuspendLayout();
            this.waferCard.SuspendLayout();
            this.rightZone.SuspendLayout();
            this.detailCard.SuspendLayout();
            this.SuspendLayout();
            //
            // titlePanel
            //
            this.titlePanel.Controls.Add(this.lblTitle);
            this.titlePanel.Controls.Add(this.badgeEnv);
            this.titlePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.titlePanel.Name = "titlePanel";
            this.titlePanel.Size = new System.Drawing.Size(1516, 28);
            this.titlePanel.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Title;
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1456, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Lot History";
            this.lblTitle.TitleBar = true;
            this.lblTitle.Child = null;
            //
            // badgeEnv
            //
            this.badgeEnv.Color = "#DBEAFE";
            this.badgeEnv.Dock = System.Windows.Forms.DockStyle.Right;
            this.badgeEnv.Name = "badgeEnv";
            this.badgeEnv.Size = new System.Drawing.Size(60, 28);
            this.badgeEnv.TabIndex = 1;
            this.badgeEnv.Text = "MES";
            this.badgeEnv.Child = null;
            //
            // spTitle
            //
            this.spTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.spTitle.Name = "spTitle";
            this.spTitle.Size = new System.Drawing.Size(1516, 8);
            this.spTitle.TabIndex = 1;
            //
            // searchCard
            //
            this.searchCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.searchCard.Controls.Add(this.lblType);
            this.searchCard.Controls.Add(this.cboType);
            this.searchCard.Controls.Add(this.lblLotId);
            this.searchCard.Controls.Add(this.txtLotId);
            this.searchCard.Controls.Add(this.btnSearch);
            this.searchCard.Controls.Add(this.btnReset);
            this.searchCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchCard.Name = "searchCard";
            this.searchCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.searchCard.Size = new System.Drawing.Size(1516, 56);
            this.searchCard.TabIndex = 2;
            //
            // lblType
            //
            this.lblType.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblType.Location = new System.Drawing.Point(12, 12);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(40, 32);
            this.lblType.TabIndex = 0;
            this.lblType.Text = "Type";
            this.lblType.Child = null;
            //
            // cboType
            //
            this.cboType.Location = new System.Drawing.Point(56, 12);
            this.cboType.Name = "cboType";
            this.cboType.PlaceholderText = "All Types";
            this.cboType.Size = new System.Drawing.Size(170, 32);
            this.cboType.TabIndex = 1;
            this.cboType.Child = null;
            //
            // lblLotId
            //
            this.lblLotId.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblLotId.Location = new System.Drawing.Point(250, 12);
            this.lblLotId.Name = "lblLotId";
            this.lblLotId.Required = true;
            this.lblLotId.Size = new System.Drawing.Size(56, 32);
            this.lblLotId.TabIndex = 2;
            this.lblLotId.Text = "Lot ID";
            this.lblLotId.Child = null;
            //
            // txtLotId
            //
            this.txtLotId.AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-";
            this.txtLotId.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtLotId.Location = new System.Drawing.Point(310, 12);
            this.txtLotId.Name = "txtLotId";
            this.txtLotId.PlaceholderText = "Lot or Wafer ID";
            this.txtLotId.Required = true;
            this.txtLotId.Size = new System.Drawing.Size(200, 32);
            this.txtLotId.TabIndex = 3;
            this.txtLotId.EnterPressed += new System.EventHandler(this.OnSearchClick);
            this.txtLotId.TextChanged += new System.EventHandler(this.OnLotIdTextChanged);
            this.txtLotId.Child = null;
            //
            // btnSearch
            //
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Location = new System.Drawing.Point(1336, 12);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(80, 32);
            this.btnSearch.TabIndex = 4;
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
            this.btnReset.TabIndex = 5;
            this.btnReset.Text = "Reset";
            this.btnReset.Click += new System.EventHandler(this.OnResetClick);
            this.btnReset.Child = null;
            //
            // spSearch
            //
            this.spSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.spSearch.Name = "spSearch";
            this.spSearch.Size = new System.Drawing.Size(1516, 8);
            this.spSearch.TabIndex = 3;
            //
            // mainZone
            //
            this.mainZone.Controls.Add(this.rightZone);
            this.mainZone.Controls.Add(this.spTree);
            this.mainZone.Controls.Add(this.leftZone);
            this.mainZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainZone.Name = "mainZone";
            this.mainZone.Size = new System.Drawing.Size(1516, 676);
            this.mainZone.TabIndex = 4;
            //
            // leftZone
            //
            this.leftZone.Controls.Add(this.treeCard);
            this.leftZone.Controls.Add(this.spWafer);
            this.leftZone.Controls.Add(this.waferCard);
            this.leftZone.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftZone.Name = "leftZone";
            this.leftZone.Size = new System.Drawing.Size(340, 676);
            this.leftZone.TabIndex = 0;
            //
            // treeCard
            //
            this.treeCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.treeCard.Controls.Add(this.treeLotWf);
            this.treeCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeCard.Name = "treeCard";
            this.treeCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.treeCard.Size = new System.Drawing.Size(340, 428);
            this.treeCard.TabIndex = 0;
            this.treeCard.Text = "Lot / Wafer Tree";
            //
            // treeLotWf
            //
            this.treeLotWf.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeLotWf.Name = "treeLotWf";
            this.treeLotWf.TabIndex = 0;
            this.treeLotWf.SelectedValueChanged += new System.EventHandler(this.OnTreeSelectionChanged);
            this.treeLotWf.Child = null;
            //
            // spWafer
            //
            this.spWafer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.spWafer.Name = "spWafer";
            this.spWafer.Size = new System.Drawing.Size(340, 8);
            this.spWafer.TabIndex = 1;
            //
            // waferCard
            //
            this.waferCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.waferCard.Controls.Add(this.gridWafers);
            this.waferCard.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.waferCard.Name = "waferCard";
            this.waferCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.waferCard.Size = new System.Drawing.Size(340, 240);
            this.waferCard.TabIndex = 2;
            this.waferCard.Text = "Wafers";
            //
            // gridWafers
            //
            this.gridWafers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridWafers.Name = "gridWafers";
            this.gridWafers.TabIndex = 0;
            this.gridWafers.Child = null;
            //
            // spTree
            //
            this.spTree.Dock = System.Windows.Forms.DockStyle.Left;
            this.spTree.Name = "spTree";
            this.spTree.Size = new System.Drawing.Size(12, 676);
            this.spTree.TabIndex = 1;
            //
            // rightZone
            //
            this.rightZone.Controls.Add(this.busyMain);
            this.rightZone.Controls.Add(this.gridHistory);
            this.rightZone.Controls.Add(this.spDetail);
            this.rightZone.Controls.Add(this.detailCard);
            this.rightZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightZone.Name = "rightZone";
            this.rightZone.Size = new System.Drawing.Size(1164, 676);
            this.rightZone.TabIndex = 2;
            //
            // detailCard
            //
            this.detailCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.detailCard.Controls.Add(this.lblSelId);
            this.detailCard.Controls.Add(this.badgeType);
            this.detailCard.Controls.Add(this.badgeStat);
            this.detailCard.Controls.Add(this.capProduct);
            this.detailCard.Controls.Add(this.valProduct);
            this.detailCard.Controls.Add(this.capFlow);
            this.detailCard.Controls.Add(this.valFlow);
            this.detailCard.Controls.Add(this.capOper);
            this.detailCard.Controls.Add(this.valOper);
            this.detailCard.Controls.Add(this.capEqp);
            this.detailCard.Controls.Add(this.valEqp);
            this.detailCard.Controls.Add(this.capCarrier);
            this.detailCard.Controls.Add(this.valCarrier);
            this.detailCard.Controls.Add(this.capEventTm);
            this.detailCard.Controls.Add(this.valEventTm);
            this.detailCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.detailCard.Name = "detailCard";
            this.detailCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 8);
            this.detailCard.Size = new System.Drawing.Size(1164, 132);
            this.detailCard.TabIndex = 0;
            this.detailCard.Text = "Selection";
            //
            // lblSelId
            //
            this.lblSelId.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Title;
            this.lblSelId.Location = new System.Drawing.Point(12, 40);
            this.lblSelId.Name = "lblSelId";
            this.lblSelId.Size = new System.Drawing.Size(320, 26);
            this.lblSelId.TabIndex = 0;
            this.lblSelId.Text = "-";
            this.lblSelId.Child = null;
            //
            // badgeType
            //
            this.badgeType.Location = new System.Drawing.Point(344, 42);
            this.badgeType.Name = "badgeType";
            this.badgeType.Size = new System.Drawing.Size(90, 24);
            this.badgeType.TabIndex = 1;
            this.badgeType.Text = "-";
            this.badgeType.Child = null;
            //
            // badgeStat
            //
            this.badgeStat.Location = new System.Drawing.Point(442, 42);
            this.badgeStat.Name = "badgeStat";
            this.badgeStat.Size = new System.Drawing.Size(90, 24);
            this.badgeStat.TabIndex = 2;
            this.badgeStat.Text = "-";
            this.badgeStat.Child = null;
            //
            // capProduct
            //
            this.capProduct.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.capProduct.Location = new System.Drawing.Point(12, 76);
            this.capProduct.Name = "capProduct";
            this.capProduct.Size = new System.Drawing.Size(60, 22);
            this.capProduct.TabIndex = 3;
            this.capProduct.Text = "Product";
            this.capProduct.Child = null;
            //
            // valProduct
            //
            this.valProduct.Location = new System.Drawing.Point(76, 76);
            this.valProduct.Name = "valProduct";
            this.valProduct.Size = new System.Drawing.Size(250, 22);
            this.valProduct.TabIndex = 4;
            this.valProduct.Text = "-";
            this.valProduct.Child = null;
            //
            // capFlow
            //
            this.capFlow.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.capFlow.Location = new System.Drawing.Point(340, 76);
            this.capFlow.Name = "capFlow";
            this.capFlow.Size = new System.Drawing.Size(40, 22);
            this.capFlow.TabIndex = 5;
            this.capFlow.Text = "Flow";
            this.capFlow.Child = null;
            //
            // valFlow
            //
            this.valFlow.Location = new System.Drawing.Point(384, 76);
            this.valFlow.Name = "valFlow";
            this.valFlow.Size = new System.Drawing.Size(180, 22);
            this.valFlow.TabIndex = 6;
            this.valFlow.Text = "-";
            this.valFlow.Child = null;
            //
            // capOper
            //
            this.capOper.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.capOper.Location = new System.Drawing.Point(580, 76);
            this.capOper.Name = "capOper";
            this.capOper.Size = new System.Drawing.Size(70, 22);
            this.capOper.TabIndex = 7;
            this.capOper.Text = "Operation";
            this.capOper.Child = null;
            //
            // valOper
            //
            this.valOper.Location = new System.Drawing.Point(654, 76);
            this.valOper.Name = "valOper";
            this.valOper.Size = new System.Drawing.Size(140, 22);
            this.valOper.TabIndex = 8;
            this.valOper.Text = "-";
            this.valOper.Child = null;
            //
            // capEqp
            //
            this.capEqp.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.capEqp.Location = new System.Drawing.Point(12, 102);
            this.capEqp.Name = "capEqp";
            this.capEqp.Size = new System.Drawing.Size(70, 22);
            this.capEqp.TabIndex = 9;
            this.capEqp.Text = "Equipment";
            this.capEqp.Child = null;
            //
            // valEqp
            //
            this.valEqp.Location = new System.Drawing.Point(86, 102);
            this.valEqp.Name = "valEqp";
            this.valEqp.Size = new System.Drawing.Size(240, 22);
            this.valEqp.TabIndex = 10;
            this.valEqp.Text = "-";
            this.valEqp.Child = null;
            //
            // capCarrier
            //
            this.capCarrier.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.capCarrier.Location = new System.Drawing.Point(340, 102);
            this.capCarrier.Name = "capCarrier";
            this.capCarrier.Size = new System.Drawing.Size(50, 22);
            this.capCarrier.TabIndex = 11;
            this.capCarrier.Text = "Carrier";
            this.capCarrier.Child = null;
            //
            // valCarrier
            //
            this.valCarrier.Location = new System.Drawing.Point(394, 102);
            this.valCarrier.Name = "valCarrier";
            this.valCarrier.Size = new System.Drawing.Size(170, 22);
            this.valCarrier.TabIndex = 12;
            this.valCarrier.Text = "-";
            this.valCarrier.Child = null;
            //
            // capEventTm
            //
            this.capEventTm.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.capEventTm.Location = new System.Drawing.Point(580, 102);
            this.capEventTm.Name = "capEventTm";
            this.capEventTm.Size = new System.Drawing.Size(74, 22);
            this.capEventTm.TabIndex = 13;
            this.capEventTm.Text = "Event Time";
            this.capEventTm.Child = null;
            //
            // spDetail
            //
            this.spDetail.Dock = System.Windows.Forms.DockStyle.Top;
            this.spDetail.Name = "spDetail";
            this.spDetail.Size = new System.Drawing.Size(1164, 8);
            this.spDetail.TabIndex = 1;
            //
            // valEventTm
            //
            this.valEventTm.Location = new System.Drawing.Point(658, 102);
            this.valEventTm.Name = "valEventTm";
            this.valEventTm.Size = new System.Drawing.Size(170, 22);
            this.valEventTm.TabIndex = 14;
            this.valEventTm.Text = "-";
            this.valEventTm.Child = null;
            //
            // gridHistory
            //
            this.gridHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridHistory.Name = "gridHistory";
            this.gridHistory.ShowStatusBar = true;
            this.gridHistory.StatusCountFormat = "{0:N0} events";
            this.gridHistory.Size = new System.Drawing.Size(1164, 536);
            this.gridHistory.TabIndex = 2;
            this.gridHistory.Child = null;
            //
            // busyMain
            //
            this.busyMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.busyMain.Message = "Loading...";
            this.busyMain.Name = "busyMain";
            this.busyMain.TabIndex = 3;
            this.busyMain.Child = null;
            //
            // toastMain
            //
            this.toastMain.Location = new System.Drawing.Point(1220, 720);
            this.toastMain.Name = "toastMain";
            this.toastMain.Size = new System.Drawing.Size(280, 44);
            this.toastMain.TabIndex = 5;
            this.toastMain.Child = null;
            //
            // LotHistoryForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1540, 800);
            this.Controls.Add(this.toastMain);
            this.Controls.Add(this.mainZone);
            this.Controls.Add(this.spSearch);
            this.Controls.Add(this.searchCard);
            this.Controls.Add(this.spTitle);
            this.Controls.Add(this.titlePanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1240, 660);
            this.Name = "LotHistoryForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Lot History";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.titlePanel.ResumeLayout(false);
            this.searchCard.ResumeLayout(false);
            this.treeCard.ResumeLayout(false);
            this.waferCard.ResumeLayout(false);
            this.leftZone.ResumeLayout(false);
            this.detailCard.ResumeLayout(false);
            this.rightZone.ResumeLayout(false);
            this.mainZone.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel titlePanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTitle;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeEnv;
        private System.Windows.Forms.Panel spTitle;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel searchCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblType;
        private Modern.Lab.WinForms.Controls.Selection.ModernCheckComboBox cboType;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblLotId;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtLotId;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnSearch;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReset;
        private System.Windows.Forms.Panel spSearch;
        private System.Windows.Forms.Panel mainZone;
        private System.Windows.Forms.Panel leftZone;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox treeCard;
        private Modern.Lab.WinForms.Controls.Selection.ModernTreeView treeLotWf;
        private System.Windows.Forms.Panel spWafer;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox waferCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridWafers;
        private System.Windows.Forms.Panel spTree;
        private System.Windows.Forms.Panel rightZone;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay busyMain;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridHistory;
        private System.Windows.Forms.Panel spDetail;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox detailCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblSelId;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeType;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeStat;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel capProduct;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel valProduct;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel capFlow;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel valFlow;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel capOper;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel valOper;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel capEqp;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel valEqp;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel capCarrier;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel valCarrier;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel capEventTm;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel valEventTm;
        private Modern.Lab.WinForms.Controls.Display.ModernToast toastMain;
    }
}
