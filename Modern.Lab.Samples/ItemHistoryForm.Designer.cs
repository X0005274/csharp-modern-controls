namespace Modern.Lab.Samples
{
    public partial class ItemHistoryForm
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
            this.lblItemId = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtItemId = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.btnSearch = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnReset = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.spSearch = new System.Windows.Forms.Panel();
            this.mainZone = new System.Windows.Forms.Panel();
            this.rightZone = new System.Windows.Forms.Panel();
            this.busyMain = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.gridHistory = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.gridUnitHistory = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.tabHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabControl();
            this.pageItemHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
            this.pageUnitHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
            this.spDetail = new System.Windows.Forms.Panel();
            this.detailCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.tblDetail = new System.Windows.Forms.TableLayoutPanel();
            this.badgeType = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeStat = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.capProduct = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.valProduct = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.capDescription = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.valDescription = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
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
            this.capEvent = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.valEvent = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.capStk = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.valStk = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.spStep = new System.Windows.Forms.Panel();
            this.stepCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.stepIndicator = new Modern.Lab.WinForms.Controls.Display.ModernStepIndicator();
            this.splitMain = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.leftZone = new System.Windows.Forms.Panel();
            this.treeCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.treeItemUnit = new Modern.Lab.WinForms.Controls.Selection.ModernTreeView();
            this.spUnit = new System.Windows.Forms.Panel();
            this.unitCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridUnits = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.toastMain = new Modern.Lab.WinForms.Controls.Display.ModernToast();
            this.titlePanel.SuspendLayout();
            this.searchCard.SuspendLayout();
            this.mainZone.SuspendLayout();
            this.rightZone.SuspendLayout();
            this.tabHistory.SuspendLayout();
            this.pageItemHistory.SuspendLayout();
            this.pageUnitHistory.SuspendLayout();
            this.detailCard.SuspendLayout();
            this.tblDetail.SuspendLayout();
            this.stepCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.leftZone.SuspendLayout();
            this.treeCard.SuspendLayout();
            this.unitCard.SuspendLayout();
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
            this.lblTitle.Text = "Item History";
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
            this.searchCard.Controls.Add(this.lblType);
            this.searchCard.Controls.Add(this.cboType);
            this.searchCard.Controls.Add(this.lblItemId);
            this.searchCard.Controls.Add(this.txtItemId);
            this.searchCard.Controls.Add(this.btnSearch);
            this.searchCard.Controls.Add(this.btnReset);
            this.searchCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchCard.Location = new System.Drawing.Point(12, 48);
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
            // lblItemId
            // 
            this.lblItemId.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblItemId.Location = new System.Drawing.Point(250, 12);
            this.lblItemId.Name = "lblItemId";
            this.lblItemId.Required = true;
            this.lblItemId.Size = new System.Drawing.Size(56, 32);
            this.lblItemId.TabIndex = 2;
            this.lblItemId.Text = "Item ID";
            this.lblItemId.Child = null;
            // 
            // txtItemId
            // 
            this.txtItemId.AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-";
            this.txtItemId.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtItemId.Location = new System.Drawing.Point(310, 12);
            this.txtItemId.Name = "txtItemId";
            this.txtItemId.PlaceholderText = "Item or Unit ID";
            this.txtItemId.Required = true;
            this.txtItemId.Size = new System.Drawing.Size(200, 32);
            this.txtItemId.TabIndex = 3;
            this.txtItemId.EnterPressed += new System.EventHandler(this.OnSearchClick);
            this.txtItemId.TextChanged += new System.EventHandler(this.OnItemIdTextChanged);
            this.txtItemId.Child = null;
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
            this.spSearch.Location = new System.Drawing.Point(12, 104);
            this.spSearch.Name = "spSearch";
            this.spSearch.Size = new System.Drawing.Size(1516, 8);
            this.spSearch.TabIndex = 3;
            // 
            // mainZone
            // 
            this.mainZone.Controls.Add(this.splitMain);
            this.mainZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainZone.Location = new System.Drawing.Point(12, 112);
            this.mainZone.Name = "mainZone";
            this.mainZone.Size = new System.Drawing.Size(1516, 676);
            this.mainZone.TabIndex = 4;
            // 
            // rightZone
            // 
            this.rightZone.Controls.Add(this.busyMain);
            this.rightZone.Controls.Add(this.tabHistory);
            this.rightZone.Controls.Add(this.spDetail);
            this.rightZone.Controls.Add(this.detailCard);
            this.rightZone.Controls.Add(this.spStep);
            this.rightZone.Controls.Add(this.stepCard);
            this.rightZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightZone.Location = new System.Drawing.Point(0, 0);
            this.rightZone.Name = "rightZone";
            this.rightZone.Size = new System.Drawing.Size(1164, 676);
            this.rightZone.TabIndex = 2;
            // 
            // busyMain
            // 
            this.busyMain.Location = new System.Drawing.Point(432, 370);
            this.busyMain.Message = "Loading...";
            this.busyMain.SubMessage = "이력을 불러오는 중입니다";
            this.busyMain.Name = "busyMain";
            this.busyMain.Size = new System.Drawing.Size(300, 180);
            this.busyMain.TabIndex = 3;
            this.busyMain.Visible = false;
            this.busyMain.Child = null;
            //
            // tabHistory
            //
            this.tabHistory.Controls.Add(this.pageItemHistory);
            this.tabHistory.Controls.Add(this.pageUnitHistory);
            this.tabHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabHistory.Location = new System.Drawing.Point(0, 246);
            this.tabHistory.Name = "tabHistory";
            this.tabHistory.Size = new System.Drawing.Size(1164, 430);
            this.tabHistory.TabIndex = 5;
            //
            // pageItemHistory
            //
            this.pageItemHistory.Controls.Add(this.gridHistory);
            this.pageItemHistory.Location = new System.Drawing.Point(0, 40);
            this.pageItemHistory.Name = "pageItemHistory";
            this.pageItemHistory.Size = new System.Drawing.Size(1164, 390);
            this.pageItemHistory.TabIndex = 0;
            this.pageItemHistory.Text = "Item History";
            //
            // gridHistory
            //
            this.gridHistory.AutoFitColumns = true;
            this.gridHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridHistory.Location = new System.Drawing.Point(0, 0);
            this.gridHistory.Name = "gridHistory";
            this.gridHistory.RowColorMember = "ROW_COLOR";
            this.gridHistory.ShowStatusBar = true;
            this.gridHistory.Size = new System.Drawing.Size(1164, 390);
            this.gridHistory.StatusCountFormat = "{0:N0} events";
            this.gridHistory.TabIndex = 0;
            this.gridHistory.Child = null;
            //
            // pageUnitHistory
            //
            this.pageUnitHistory.Controls.Add(this.gridUnitHistory);
            this.pageUnitHistory.Location = new System.Drawing.Point(0, 40);
            this.pageUnitHistory.Name = "pageUnitHistory";
            this.pageUnitHistory.Size = new System.Drawing.Size(1164, 390);
            this.pageUnitHistory.TabIndex = 1;
            this.pageUnitHistory.Text = "Unit History";
            //
            // gridUnitHistory
            //
            this.gridUnitHistory.AutoFitColumns = true;
            this.gridUnitHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridUnitHistory.Location = new System.Drawing.Point(0, 0);
            this.gridUnitHistory.Name = "gridUnitHistory";
            this.gridUnitHistory.RowColorMember = "ROW_COLOR";
            this.gridUnitHistory.ShowStatusBar = true;
            this.gridUnitHistory.Size = new System.Drawing.Size(1164, 390);
            this.gridUnitHistory.StatusCountFormat = "{0:N0} events";
            this.gridUnitHistory.TabIndex = 0;
            this.gridUnitHistory.Child = null;
            //
            // spDetail
            // 
            this.spDetail.Dock = System.Windows.Forms.DockStyle.Top;
            this.spDetail.Location = new System.Drawing.Point(0, 238);
            this.spDetail.Name = "spDetail";
            this.spDetail.Size = new System.Drawing.Size(1164, 8);
            this.spDetail.TabIndex = 1;
            // 
            // detailCard
            // 
            this.detailCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.detailCard.Controls.Add(this.badgeType);
            this.detailCard.Controls.Add(this.badgeStat);
            this.detailCard.Controls.Add(this.tblDetail);
            this.detailCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.detailCard.Location = new System.Drawing.Point(0, 106);
            this.detailCard.Name = "detailCard";
            this.detailCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 8);
            this.detailCard.Size = new System.Drawing.Size(1164, 190);
            this.detailCard.TabIndex = 0;
            this.detailCard.Text = "Selection";
            this.detailCard.TitleAccent = true;
            this.detailCard.TitleFontStyle = System.Drawing.FontStyle.Bold;
            //
            // tblDetail
            //
            this.tblDetail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tblDetail.ColumnCount = 8;
            this.tblDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tblDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tblDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tblDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tblDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            // 배치(3줄): 1행 Event·Event Time·Carrier·Stocker (4쌍),
            //           2행 Product(값이 Prod Type 자리까지 3칸 span)·Flow·Operation,
            //           3행 Description 전폭 한 줄 (값이 7칸 span).
            // Prod Type은 별도 칸 대신 상단 타입 배지("Prod Type - Sub Type")로 표기.
            this.tblDetail.Controls.Add(this.capEvent, 0, 0);
            this.tblDetail.Controls.Add(this.valEvent, 1, 0);
            this.tblDetail.Controls.Add(this.capEventTm, 2, 0);
            this.tblDetail.Controls.Add(this.valEventTm, 3, 0);
            this.tblDetail.Controls.Add(this.capCarrier, 4, 0);
            this.tblDetail.Controls.Add(this.valCarrier, 5, 0);
            this.tblDetail.Controls.Add(this.capStk, 6, 0);
            this.tblDetail.Controls.Add(this.valStk, 7, 0);
            this.tblDetail.Controls.Add(this.capProduct, 0, 1);
            this.tblDetail.Controls.Add(this.valProduct, 1, 1);
            this.tblDetail.SetColumnSpan(this.valProduct, 3);
            this.tblDetail.Controls.Add(this.capFlow, 4, 1);
            this.tblDetail.Controls.Add(this.valFlow, 5, 1);
            this.tblDetail.Controls.Add(this.capOper, 6, 1);
            this.tblDetail.Controls.Add(this.valOper, 7, 1);
            this.tblDetail.Controls.Add(this.capDescription, 0, 2);
            this.tblDetail.Controls.Add(this.valDescription, 1, 2);
            this.tblDetail.SetColumnSpan(this.valDescription, 7);
            this.tblDetail.Location = new System.Drawing.Point(12, 76);
            this.tblDetail.Name = "tblDetail";
            this.tblDetail.RowCount = 3;
            this.tblDetail.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tblDetail.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tblDetail.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tblDetail.Size = new System.Drawing.Size(1140, 97);
            this.tblDetail.TabIndex = 3;
            this.tblDetail.CellPaint += new System.Windows.Forms.TableLayoutCellPaintEventHandler(this.OnDetailCellPaint);
            //
            // badgeType
            //
            this.badgeType.Location = new System.Drawing.Point(12, 40);
            this.badgeType.Name = "badgeType";
            this.badgeType.Size = new System.Drawing.Size(90, 24);
            this.badgeType.TabIndex = 1;
            this.badgeType.Text = "-";
            this.badgeType.Child = null;
            //
            // badgeStat
            //
            this.badgeStat.Location = new System.Drawing.Point(110, 40);
            this.badgeStat.Name = "badgeStat";
            this.badgeStat.Size = new System.Drawing.Size(90, 24);
            this.badgeStat.TabIndex = 2;
            this.badgeStat.Text = "-";
            this.badgeStat.Child = null;
            //
            // capEvent
            //
            this.capEvent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.capEvent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.capEvent.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.capEvent.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.capEvent.Name = "capEvent";
            this.capEvent.TabIndex = 17;
            this.capEvent.Text = "Event";
            this.capEvent.Child = null;
            //
            // valEvent
            //
            this.valEvent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.valEvent.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.valEvent.Name = "valEvent";
            this.valEvent.TabIndex = 18;
            this.valEvent.Text = "-";
            this.valEvent.Child = null;
            //
            // capStk
            //
            this.capStk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.capStk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.capStk.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.capStk.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.capStk.Name = "capStk";
            this.capStk.TabIndex = 19;
            this.capStk.Text = "Stocker";
            this.capStk.Child = null;
            //
            // valStk
            //
            this.valStk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.valStk.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.valStk.Name = "valStk";
            this.valStk.TabIndex = 20;
            this.valStk.Text = "-";
            this.valStk.Child = null;
            //
            // capProduct
            //
            this.capProduct.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.capProduct.Dock = System.Windows.Forms.DockStyle.Fill;
            this.capProduct.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.capProduct.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.capProduct.Name = "capProduct";
            this.capProduct.TabIndex = 3;
            this.capProduct.Text = "Product";
            this.capProduct.Child = null;
            // 
            // valProduct
            // 
            this.valProduct.Dock = System.Windows.Forms.DockStyle.Fill;
            this.valProduct.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.valProduct.Name = "valProduct";
            this.valProduct.TabIndex = 4;
            this.valProduct.Text = "-";
            this.valProduct.Child = null;
            //
            // capDescription
            //
            this.capDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.capDescription.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.capDescription.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.capDescription.Name = "capDescription";
            this.capDescription.TabIndex = 20;
            this.capDescription.Text = "Description";
            this.capDescription.Child = null;
            //
            // valDescription
            //
            this.valDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.valDescription.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.valDescription.Name = "valDescription";
            this.valDescription.TabIndex = 21;
            this.valDescription.Text = "-";
            this.valDescription.Child = null;
            //
            // capFlow
            //
            this.capFlow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.capFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.capFlow.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.capFlow.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.capFlow.Name = "capFlow";
            this.capFlow.TabIndex = 5;
            this.capFlow.Text = "Flow";
            this.capFlow.Child = null;
            // 
            // valFlow
            // 
            this.valFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.valFlow.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.valFlow.Name = "valFlow";
            this.valFlow.TabIndex = 6;
            this.valFlow.Text = "-";
            this.valFlow.Child = null;
            // 
            // capOper
            // 
            this.capOper.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.capOper.Dock = System.Windows.Forms.DockStyle.Fill;
            this.capOper.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.capOper.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.capOper.Name = "capOper";
            this.capOper.TabIndex = 7;
            this.capOper.Text = "Operation";
            this.capOper.Child = null;
            // 
            // valOper
            // 
            this.valOper.Dock = System.Windows.Forms.DockStyle.Fill;
            this.valOper.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.valOper.Name = "valOper";
            this.valOper.TabIndex = 8;
            this.valOper.Text = "-";
            this.valOper.Child = null;
            // 
            // capEqp
            // 
            this.capEqp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.capEqp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.capEqp.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.capEqp.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.capEqp.Name = "capEqp";
            this.capEqp.TabIndex = 9;
            this.capEqp.Text = "Equipment";
            this.capEqp.Child = null;
            // 
            // valEqp
            // 
            this.valEqp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.valEqp.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.valEqp.Name = "valEqp";
            this.valEqp.TabIndex = 10;
            this.valEqp.Text = "-";
            this.valEqp.Child = null;
            // 
            // capCarrier
            // 
            this.capCarrier.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.capCarrier.Dock = System.Windows.Forms.DockStyle.Fill;
            this.capCarrier.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.capCarrier.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.capCarrier.Name = "capCarrier";
            this.capCarrier.TabIndex = 11;
            this.capCarrier.Text = "Carrier";
            this.capCarrier.Child = null;
            // 
            // valCarrier
            // 
            this.valCarrier.Dock = System.Windows.Forms.DockStyle.Fill;
            this.valCarrier.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.valCarrier.Name = "valCarrier";
            this.valCarrier.TabIndex = 12;
            this.valCarrier.Text = "-";
            this.valCarrier.Child = null;
            // 
            // capEventTm
            // 
            this.capEventTm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.capEventTm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.capEventTm.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.capEventTm.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.capEventTm.Name = "capEventTm";
            this.capEventTm.TabIndex = 13;
            this.capEventTm.Text = "Event Time";
            this.capEventTm.Child = null;
            // 
            // valEventTm
            // 
            this.valEventTm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.valEventTm.Margin = new System.Windows.Forms.Padding(10, 5, 3, 5);
            this.valEventTm.Name = "valEventTm";
            this.valEventTm.TabIndex = 14;
            this.valEventTm.Text = "-";
            this.valEventTm.Child = null;
            // 
            // spStep
            // 
            this.spStep.Dock = System.Windows.Forms.DockStyle.Top;
            this.spStep.Location = new System.Drawing.Point(0, 98);
            this.spStep.Name = "spStep";
            this.spStep.Size = new System.Drawing.Size(1164, 8);
            this.spStep.TabIndex = 1;
            // 
            // stepCard
            // 
            this.stepCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.stepCard.Controls.Add(this.stepIndicator);
            this.stepCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.stepCard.Location = new System.Drawing.Point(0, 0);
            this.stepCard.Name = "stepCard";
            this.stepCard.Padding = new System.Windows.Forms.Padding(12, 38, 12, 8);
            this.stepCard.Size = new System.Drawing.Size(1164, 98);
            this.stepCard.TabIndex = 0;
            this.stepCard.Text = "Lifecycle";
            //
            // stepIndicator
            //
            this.stepIndicator.DisplayMember = "LABEL";
            this.stepIndicator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stepIndicator.Location = new System.Drawing.Point(12, 38);
            this.stepIndicator.Name = "stepIndicator";
            this.stepIndicator.Size = new System.Drawing.Size(1140, 52);
            this.stepIndicator.StateMember = "STATE";
            this.stepIndicator.TabIndex = 0;
            this.stepIndicator.Child = null;
            //
            // splitMain
            //
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 0);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.splitMain.Panel1.Controls.Add(this.leftZone);
            this.splitMain.Panel1MinSize = 240;
            this.splitMain.Panel2.Controls.Add(this.rightZone);
            this.splitMain.Panel2MinSize = 480;
            this.splitMain.Size = new System.Drawing.Size(1516, 676);
            this.splitMain.SplitterDistance = 340;
            this.splitMain.TabIndex = 0;
            //
            // leftZone
            //
            this.leftZone.Controls.Add(this.treeCard);
            this.leftZone.Controls.Add(this.spUnit);
            this.leftZone.Controls.Add(this.unitCard);
            this.leftZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftZone.Location = new System.Drawing.Point(0, 0);
            this.leftZone.Name = "leftZone";
            this.leftZone.Size = new System.Drawing.Size(340, 676);
            this.leftZone.TabIndex = 0;
            // 
            // treeCard
            // 
            this.treeCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.treeCard.Controls.Add(this.treeItemUnit);
            this.treeCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeCard.Location = new System.Drawing.Point(0, 0);
            this.treeCard.Name = "treeCard";
            this.treeCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.treeCard.Size = new System.Drawing.Size(340, 428);
            this.treeCard.TabIndex = 0;
            this.treeCard.Text = "Item / Unit Tree";
            //
            // treeItemUnit
            //
            this.treeItemUnit.DisplayMember = "ITEM_ID";
            this.treeItemUnit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeItemUnit.ForeColorMember = "NODE_COLOR";
            this.treeItemUnit.IdMember = "ITEM_ID";
            this.treeItemUnit.Location = new System.Drawing.Point(8, 40);
            this.treeItemUnit.Name = "treeItemUnit";
            this.treeItemUnit.ParentIdMember = "PARENT_ITEM_ID";
            this.treeItemUnit.Size = new System.Drawing.Size(324, 380);
            this.treeItemUnit.TabIndex = 0;
            this.treeItemUnit.SelectedValueChanged += new System.EventHandler(this.OnTreeSelectionChanged);
            this.treeItemUnit.Child = null;
            // 
            // spUnit
            // 
            this.spUnit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.spUnit.Location = new System.Drawing.Point(0, 428);
            this.spUnit.Name = "spUnit";
            this.spUnit.Size = new System.Drawing.Size(340, 8);
            this.spUnit.TabIndex = 1;
            // 
            // unitCard
            // 
            this.unitCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.unitCard.Controls.Add(this.gridUnits);
            this.unitCard.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.unitCard.Location = new System.Drawing.Point(0, 436);
            this.unitCard.Name = "unitCard";
            this.unitCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.unitCard.Size = new System.Drawing.Size(340, 240);
            this.unitCard.TabIndex = 2;
            this.unitCard.Text = "Units";
            //
            // gridUnits
            //
            this.gridUnits.AutoFitColumns = true;
            this.gridUnits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridUnits.Location = new System.Drawing.Point(8, 40);
            this.gridUnits.Name = "gridUnits";
            this.gridUnits.RowColorMember = "ROW_COLOR";
            this.gridUnits.Size = new System.Drawing.Size(324, 192);
            this.gridUnits.TabIndex = 0;
            this.gridUnits.SelectionChanged += new System.EventHandler(this.OnUnitSelectionChanged);
            this.gridUnits.Child = null;
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
            // ItemHistoryForm
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
            this.Name = "ItemHistoryForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Item History";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.titlePanel.ResumeLayout(false);
            this.searchCard.ResumeLayout(false);
            this.mainZone.ResumeLayout(false);
            this.rightZone.ResumeLayout(false);
            this.tabHistory.ResumeLayout(false);
            this.pageItemHistory.ResumeLayout(false);
            this.pageUnitHistory.ResumeLayout(false);
            this.tblDetail.ResumeLayout(false);
            this.detailCard.ResumeLayout(false);
            this.stepCard.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.leftZone.ResumeLayout(false);
            this.treeCard.ResumeLayout(false);
            this.unitCard.ResumeLayout(false);
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
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblItemId;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtItemId;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnSearch;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReset;
        private System.Windows.Forms.Panel spSearch;
        private System.Windows.Forms.Panel mainZone;
        private System.Windows.Forms.Panel leftZone;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox treeCard;
        private Modern.Lab.WinForms.Controls.Selection.ModernTreeView treeItemUnit;
        private System.Windows.Forms.Panel spUnit;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox unitCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridUnits;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitMain;
        private System.Windows.Forms.Panel rightZone;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay busyMain;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridHistory;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridUnitHistory;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabControl tabHistory;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabPage pageItemHistory;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabPage pageUnitHistory;
        private System.Windows.Forms.Panel spDetail;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox detailCard;
        private System.Windows.Forms.TableLayoutPanel tblDetail;
        private System.Windows.Forms.Panel spStep;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox stepCard;
        private Modern.Lab.WinForms.Controls.Display.ModernStepIndicator stepIndicator;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeType;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeStat;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel capProduct;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel capDescription;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel valDescription;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel capEvent;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel valEvent;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel capStk;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel valStk;
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
