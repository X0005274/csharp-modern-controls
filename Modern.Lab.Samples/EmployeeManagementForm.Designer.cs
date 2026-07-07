namespace Modern.Lab.Samples
{
    public partial class EmployeeManagementForm
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
            this.lblPageTitle = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.badgeEnv = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.spacerTitleGap = new System.Windows.Forms.Panel();
            this.searchCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.lblName = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtName = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.lblDept = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboDept = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblRank = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboRank = new Modern.Lab.WinForms.Controls.Selection.ModernCheckComboBox();
            this.lblHireDate = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.dtHireFrom = new Modern.Lab.WinForms.Controls.Input.ModernDatePicker();
            this.lblHireTilde = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.dtHireTo = new Modern.Lab.WinForms.Controls.Input.ModernDatePicker();
            this.lblSalary = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.numSalaryFrom = new Modern.Lab.WinForms.Controls.Input.ModernNumericTextBox();
            this.lblSalaryTilde = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.numSalaryTo = new Modern.Lab.WinForms.Controls.Input.ModernNumericTextBox();
            this.chkRecentOnly = new Modern.Lab.WinForms.Controls.Input.ModernCheckBox();
            this.lblSort = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.radioSort = new Modern.Lab.WinForms.Controls.Selection.ModernRadioGroup();
            this.tglShowEmail = new Modern.Lab.WinForms.Controls.Input.ModernToggleSwitch();
            this.lblHireMonth = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.monthHire = new Modern.Lab.WinForms.Controls.Input.ModernMonthPicker();
            this.btnSearch = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnReset = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.gridEmployee = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.busyOverlay = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.pagerEmployee = new Modern.Lab.WinForms.Controls.Data.ModernPagination();
            this.gridZone = new System.Windows.Forms.Panel();
            this.toastMain = new Modern.Lab.WinForms.Controls.Display.ModernToast();
            this.treeCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.treeOrg = new Modern.Lab.WinForms.Controls.Selection.ModernTreeView();
            this.spacerTreeGap = new System.Windows.Forms.Panel();
            this.spacerSearchGap = new System.Windows.Forms.Panel();
            this.spacerBottomGap = new System.Windows.Forms.Panel();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.countCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.spacerCountGap = new System.Windows.Forms.Panel();
            this.statsCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.spacerBtnGap = new System.Windows.Forms.Panel();
            this.buttonsCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.cardCount = new Modern.Lab.WinForms.Controls.Display.ModernKpiCard();
            this.listDeptCount = new Modern.Lab.WinForms.Controls.Display.ModernSummaryList();
            this.listRankCount = new Modern.Lab.WinForms.Controls.Display.ModernSummaryList();
            this.btnExecute = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnNew = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnSave = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnDelete = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.ddExcel = new Modern.Lab.WinForms.Controls.Input.ModernDropDownButton();
            this.titlePanel.SuspendLayout();
            this.gridZone.SuspendLayout();
            this.treeCard.SuspendLayout();
            this.searchCard.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.countCard.SuspendLayout();
            this.statsCard.SuspendLayout();
            this.buttonsCard.SuspendLayout();
            this.SuspendLayout();
            //
            // titlePanel
            //
            this.titlePanel.Controls.Add(this.lblPageTitle);
            this.titlePanel.Controls.Add(this.badgeEnv);
            this.titlePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.titlePanel.Name = "titlePanel";
            this.titlePanel.Size = new System.Drawing.Size(936, 28);
            this.titlePanel.TabIndex = 5;
            //
            // lblPageTitle
            //
            this.lblPageTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPageTitle.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Title;
            this.lblPageTitle.Name = "lblPageTitle";
            this.lblPageTitle.Size = new System.Drawing.Size(872, 28);
            this.lblPageTitle.TabIndex = 0;
            this.lblPageTitle.Text = "직원관리";
            this.lblPageTitle.TitleBar = true;
            this.lblPageTitle.Child = null;
            //
            // badgeEnv
            //
            this.badgeEnv.Color = "#DCFCE7";
            this.badgeEnv.Dock = System.Windows.Forms.DockStyle.Right;
            this.badgeEnv.Name = "badgeEnv";
            this.badgeEnv.Size = new System.Drawing.Size(64, 28);
            this.badgeEnv.TabIndex = 1;
            this.badgeEnv.Text = "운영";
            this.badgeEnv.Child = null;
            //
            // spacerTitleGap
            //
            this.spacerTitleGap.Dock = System.Windows.Forms.DockStyle.Top;
            this.spacerTitleGap.Name = "spacerTitleGap";
            this.spacerTitleGap.Size = new System.Drawing.Size(936, 8);
            this.spacerTitleGap.TabIndex = 6;
            //
            // searchCard
            //
            this.searchCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.searchCard.Controls.Add(this.lblName);
            this.searchCard.Controls.Add(this.txtName);
            this.searchCard.Controls.Add(this.lblDept);
            this.searchCard.Controls.Add(this.cboDept);
            this.searchCard.Controls.Add(this.lblRank);
            this.searchCard.Controls.Add(this.cboRank);
            this.searchCard.Controls.Add(this.lblHireDate);
            this.searchCard.Controls.Add(this.dtHireFrom);
            this.searchCard.Controls.Add(this.lblHireTilde);
            this.searchCard.Controls.Add(this.dtHireTo);
            this.searchCard.Controls.Add(this.lblSalary);
            this.searchCard.Controls.Add(this.numSalaryFrom);
            this.searchCard.Controls.Add(this.lblSalaryTilde);
            this.searchCard.Controls.Add(this.numSalaryTo);
            this.searchCard.Controls.Add(this.chkRecentOnly);
            this.searchCard.Controls.Add(this.lblSort);
            this.searchCard.Controls.Add(this.radioSort);
            this.searchCard.Controls.Add(this.tglShowEmail);
            this.searchCard.Controls.Add(this.lblHireMonth);
            this.searchCard.Controls.Add(this.monthHire);
            this.searchCard.Controls.Add(this.btnSearch);
            this.searchCard.Controls.Add(this.btnReset);
            this.searchCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchCard.Name = "searchCard";
            this.searchCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.searchCard.Size = new System.Drawing.Size(936, 144);
            this.searchCard.TabIndex = 0;
            // 
            // lblName
            // 
            this.lblName.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblName.Location = new System.Drawing.Point(12, 12);
            this.lblName.Name = "lblName";
            this.lblName.Required = true;
            this.lblName.Size = new System.Drawing.Size(40, 32);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "이름";
            this.lblName.Child = null;
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(56, 12);
            this.txtName.Name = "txtName";
            this.txtName.PlaceholderText = "이름 검색";
            this.txtName.Required = true;
            this.txtName.Size = new System.Drawing.Size(160, 32);
            this.txtName.TabIndex = 1;
            this.txtName.EnterPressed += new System.EventHandler(this.OnSearchClick);
            this.txtName.Child = null;
            // 
            // lblDept
            // 
            this.lblDept.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDept.Location = new System.Drawing.Point(232, 12);
            this.lblDept.Name = "lblDept";
            this.lblDept.Size = new System.Drawing.Size(40, 32);
            this.lblDept.TabIndex = 2;
            this.lblDept.Text = "부서";
            this.lblDept.Child = null;
            // 
            // cboDept
            // 
            this.cboDept.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cboDept.Location = new System.Drawing.Point(276, 12);
            this.cboDept.Name = "cboDept";
            this.cboDept.PlaceholderText = "부서 전체";
            this.cboDept.Size = new System.Drawing.Size(140, 32);
            this.cboDept.TabIndex = 3;
            this.cboDept.Child = null;
            // 
            // lblRank
            // 
            this.lblRank.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblRank.Location = new System.Drawing.Point(432, 12);
            this.lblRank.Name = "lblRank";
            this.lblRank.Size = new System.Drawing.Size(40, 32);
            this.lblRank.TabIndex = 4;
            this.lblRank.Text = "직급";
            this.lblRank.Child = null;
            // 
            // cboRank
            // 
            this.cboRank.Location = new System.Drawing.Point(476, 12);
            this.cboRank.Name = "cboRank";
            this.cboRank.PlaceholderText = "직급 전체";
            this.cboRank.Size = new System.Drawing.Size(150, 32);
            this.cboRank.TabIndex = 5;
            this.cboRank.Child = null;
            //
            // lblHireDate
            //
            this.lblHireDate.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblHireDate.Location = new System.Drawing.Point(12, 56);
            this.lblHireDate.Name = "lblHireDate";
            this.lblHireDate.Size = new System.Drawing.Size(40, 32);
            this.lblHireDate.TabIndex = 8;
            this.lblHireDate.Text = "입사일";
            this.lblHireDate.Child = null;
            //
            // dtHireFrom
            //
            this.dtHireFrom.Location = new System.Drawing.Point(56, 56);
            this.dtHireFrom.Name = "dtHireFrom";
            this.dtHireFrom.Size = new System.Drawing.Size(130, 32);
            this.dtHireFrom.TabIndex = 9;
            this.dtHireFrom.Child = null;
            //
            // lblHireTilde
            //
            this.lblHireTilde.Location = new System.Drawing.Point(192, 56);
            this.lblHireTilde.Name = "lblHireTilde";
            this.lblHireTilde.Size = new System.Drawing.Size(16, 32);
            this.lblHireTilde.TabIndex = 10;
            this.lblHireTilde.Text = "~";
            this.lblHireTilde.Child = null;
            //
            // dtHireTo
            //
            this.dtHireTo.Location = new System.Drawing.Point(214, 56);
            this.dtHireTo.Name = "dtHireTo";
            this.dtHireTo.Size = new System.Drawing.Size(130, 32);
            this.dtHireTo.TabIndex = 11;
            this.dtHireTo.Child = null;
            //
            // lblSalary
            //
            this.lblSalary.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblSalary.Location = new System.Drawing.Point(376, 56);
            this.lblSalary.Name = "lblSalary";
            this.lblSalary.Size = new System.Drawing.Size(40, 32);
            this.lblSalary.TabIndex = 12;
            this.lblSalary.Text = "연봉";
            this.lblSalary.Child = null;
            //
            // numSalaryFrom
            //
            this.numSalaryFrom.Location = new System.Drawing.Point(420, 56);
            this.numSalaryFrom.Name = "numSalaryFrom";
            this.numSalaryFrom.PlaceholderText = "만원";
            this.numSalaryFrom.Size = new System.Drawing.Size(100, 32);
            this.numSalaryFrom.TabIndex = 13;
            this.numSalaryFrom.Child = null;
            //
            // lblSalaryTilde
            //
            this.lblSalaryTilde.Location = new System.Drawing.Point(526, 56);
            this.lblSalaryTilde.Name = "lblSalaryTilde";
            this.lblSalaryTilde.Size = new System.Drawing.Size(16, 32);
            this.lblSalaryTilde.TabIndex = 14;
            this.lblSalaryTilde.Text = "~";
            this.lblSalaryTilde.Child = null;
            //
            // numSalaryTo
            //
            this.numSalaryTo.Location = new System.Drawing.Point(548, 56);
            this.numSalaryTo.Name = "numSalaryTo";
            this.numSalaryTo.PlaceholderText = "만원";
            this.numSalaryTo.Size = new System.Drawing.Size(100, 32);
            this.numSalaryTo.TabIndex = 15;
            this.numSalaryTo.Child = null;
            //
            // chkRecentOnly
            //
            this.chkRecentOnly.Location = new System.Drawing.Point(672, 60);
            this.chkRecentOnly.Name = "chkRecentOnly";
            this.chkRecentOnly.Size = new System.Drawing.Size(160, 24);
            this.chkRecentOnly.TabIndex = 16;
            this.chkRecentOnly.Text = "2020년 이후 입사";
            this.chkRecentOnly.CheckedChanged += new System.EventHandler(this.OnRecentOnlyCheckedChanged);
            this.chkRecentOnly.Child = null;
            //
            // lblSort
            //
            this.lblSort.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblSort.Location = new System.Drawing.Point(12, 100);
            this.lblSort.Name = "lblSort";
            this.lblSort.Size = new System.Drawing.Size(40, 32);
            this.lblSort.TabIndex = 17;
            this.lblSort.Text = "정렬";
            this.lblSort.Child = null;
            //
            // radioSort
            //
            this.radioSort.Location = new System.Drawing.Point(56, 100);
            this.radioSort.Name = "radioSort";
            this.radioSort.Size = new System.Drawing.Size(340, 32);
            this.radioSort.TabIndex = 18;
            this.radioSort.SelectedValueChanged += new System.EventHandler(this.OnSortChanged);
            this.radioSort.Child = null;
            //
            // tglShowEmail
            //
            this.tglShowEmail.Checked = true;
            this.tglShowEmail.Location = new System.Drawing.Point(420, 104);
            this.tglShowEmail.Name = "tglShowEmail";
            this.tglShowEmail.Size = new System.Drawing.Size(150, 24);
            this.tglShowEmail.TabIndex = 19;
            this.tglShowEmail.Text = "이메일 표시";
            this.tglShowEmail.CheckedChanged += new System.EventHandler(this.OnShowEmailToggled);
            this.tglShowEmail.Child = null;
            //
            // lblHireMonth
            //
            this.lblHireMonth.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblHireMonth.Location = new System.Drawing.Point(596, 100);
            this.lblHireMonth.Name = "lblHireMonth";
            this.lblHireMonth.Size = new System.Drawing.Size(40, 32);
            this.lblHireMonth.TabIndex = 20;
            this.lblHireMonth.Text = "입사월";
            this.lblHireMonth.Child = null;
            //
            // monthHire
            //
            this.monthHire.Location = new System.Drawing.Point(640, 100);
            this.monthHire.Name = "monthHire";
            this.monthHire.Size = new System.Drawing.Size(110, 32);
            this.monthHire.TabIndex = 21;
            this.monthHire.Child = null;
            //
            // btnSearch
            //
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Location = new System.Drawing.Point(756, 12);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(80, 32);
            this.btnSearch.TabIndex = 6;
            this.btnSearch.Text = "조회";
            this.btnSearch.Click += new System.EventHandler(this.OnSearchClick);
            this.btnSearch.Child = null;
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReset.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnReset.Location = new System.Drawing.Point(844, 12);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(80, 32);
            this.btnReset.TabIndex = 7;
            this.btnReset.Text = "초기화";
            this.btnReset.Click += new System.EventHandler(this.OnResetClick);
            this.btnReset.Child = null;
            // 
            // gridEmployee
            // 
            this.gridEmployee.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridEmployee.Name = "gridEmployee";
            this.gridEmployee.Size = new System.Drawing.Size(936, 432);
            this.gridEmployee.TabIndex = 1;
            this.gridEmployee.VisibleRowCapacityChanged += new System.EventHandler(this.OnGridCapacityChanged);
            this.gridEmployee.Child = null;
            //
            // busyOverlay
            //
            this.busyOverlay.Location = new System.Drawing.Point(300, 240);
            this.busyOverlay.Message = "직원 정보를 조회하는 중...";
            this.busyOverlay.Name = "busyOverlay";
            this.busyOverlay.Size = new System.Drawing.Size(300, 180);
            this.busyOverlay.TabIndex = 6;
            this.busyOverlay.Child = null;
            //
            // gridZone
            //
            this.gridZone.Controls.Add(this.busyOverlay);
            this.gridZone.Controls.Add(this.gridEmployee);
            this.gridZone.Controls.Add(this.pagerEmployee);
            this.gridZone.Controls.Add(this.spacerTreeGap);
            this.gridZone.Controls.Add(this.treeCard);
            this.gridZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridZone.Name = "gridZone";
            this.gridZone.Size = new System.Drawing.Size(936, 420);
            this.gridZone.TabIndex = 8;
            //
            // treeCard
            //
            this.treeCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.treeCard.Controls.Add(this.treeOrg);
            this.treeCard.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeCard.Name = "treeCard";
            this.treeCard.Padding = new System.Windows.Forms.Padding(8);
            this.treeCard.Size = new System.Drawing.Size(190, 420);
            this.treeCard.TabIndex = 0;
            //
            // treeOrg
            //
            this.treeOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeOrg.Name = "treeOrg";
            this.treeOrg.TabIndex = 0;
            this.treeOrg.SelectedValueChanged += new System.EventHandler(this.OnOrgTreeSelectionChanged);
            this.treeOrg.Child = null;
            //
            // spacerTreeGap
            //
            this.spacerTreeGap.Dock = System.Windows.Forms.DockStyle.Left;
            this.spacerTreeGap.Name = "spacerTreeGap";
            this.spacerTreeGap.Size = new System.Drawing.Size(12, 420);
            this.spacerTreeGap.TabIndex = 1;
            //
            // toastMain
            //
            this.toastMain.Location = new System.Drawing.Point(664, 560);
            this.toastMain.Name = "toastMain";
            this.toastMain.Size = new System.Drawing.Size(280, 44);
            this.toastMain.TabIndex = 9;
            this.toastMain.Child = null;
            //
            // pagerEmployee
            //
            this.pagerEmployee.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pagerEmployee.Name = "pagerEmployee";
            this.pagerEmployee.Size = new System.Drawing.Size(936, 32);
            this.pagerEmployee.TabIndex = 7;
            this.pagerEmployee.PageChanged += new System.EventHandler(this.OnPagerPageChanged);
            this.pagerEmployee.Child = null;
            //
            // spacerSearchGap
            //
            this.spacerSearchGap.Dock = System.Windows.Forms.DockStyle.Top;
            this.spacerSearchGap.Name = "spacerSearchGap";
            this.spacerSearchGap.Size = new System.Drawing.Size(936, 8);
            this.spacerSearchGap.TabIndex = 3;
            //
            // spacerBottomGap
            //
            this.spacerBottomGap.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.spacerBottomGap.Name = "spacerBottomGap";
            this.spacerBottomGap.Size = new System.Drawing.Size(936, 8);
            this.spacerBottomGap.TabIndex = 4;
            //
            // bottomPanel
            //
            this.bottomPanel.Controls.Add(this.statsCard);
            this.bottomPanel.Controls.Add(this.spacerBtnGap);
            this.bottomPanel.Controls.Add(this.buttonsCard);
            this.bottomPanel.Controls.Add(this.spacerCountGap);
            this.bottomPanel.Controls.Add(this.countCard);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(936, 128);
            this.bottomPanel.TabIndex = 2;
            //
            // countCard
            //
            this.countCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.countCard.Controls.Add(this.cardCount);
            this.countCard.Dock = System.Windows.Forms.DockStyle.Left;
            this.countCard.Name = "countCard";
            this.countCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.countCard.Size = new System.Drawing.Size(100, 128);
            this.countCard.TabIndex = 0;
            //
            // spacerCountGap
            //
            this.spacerCountGap.Dock = System.Windows.Forms.DockStyle.Left;
            this.spacerCountGap.Name = "spacerCountGap";
            this.spacerCountGap.Size = new System.Drawing.Size(12, 92);
            this.spacerCountGap.TabIndex = 1;
            //
            // statsCard
            //
            this.statsCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.statsCard.Controls.Add(this.listRankCount);
            this.statsCard.Controls.Add(this.listDeptCount);
            this.statsCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statsCard.Name = "statsCard";
            this.statsCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 12);
            this.statsCard.Size = new System.Drawing.Size(532, 128);
            this.statsCard.TabIndex = 2;
            this.statsCard.Text = "조회 통계";
            //
            // spacerBtnGap
            //
            this.spacerBtnGap.Dock = System.Windows.Forms.DockStyle.Right;
            this.spacerBtnGap.Name = "spacerBtnGap";
            this.spacerBtnGap.Size = new System.Drawing.Size(12, 92);
            this.spacerBtnGap.TabIndex = 3;
            //
            // buttonsCard
            //
            this.buttonsCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.buttonsCard.Controls.Add(this.btnExecute);
            this.buttonsCard.Controls.Add(this.btnNew);
            this.buttonsCard.Controls.Add(this.btnSave);
            this.buttonsCard.Controls.Add(this.btnDelete);
            this.buttonsCard.Controls.Add(this.ddExcel);
            this.buttonsCard.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonsCard.Name = "buttonsCard";
            this.buttonsCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.buttonsCard.Size = new System.Drawing.Size(280, 128);
            this.buttonsCard.TabIndex = 4;
            // 
            // cardCount
            // 
            this.cardCount.Flat = true;
            this.cardCount.Location = new System.Drawing.Point(12, 30);
            this.cardCount.Name = "cardCount";
            this.cardCount.Size = new System.Drawing.Size(76, 68);
            this.cardCount.TabIndex = 0;
            this.cardCount.Title = "조회 건수";
            this.cardCount.Child = null;
            // 
            // listDeptCount
            // 
            this.listDeptCount.Dock = System.Windows.Forms.DockStyle.Top;
            this.listDeptCount.Flat = true;
            this.listDeptCount.Name = "listDeptCount";
            this.listDeptCount.Size = new System.Drawing.Size(508, 38);
            this.listDeptCount.TabIndex = 1;
            this.listDeptCount.Title = "부서별";
            this.listDeptCount.Child = null;
            // 
            // listRankCount
            // 
            this.listRankCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listRankCount.Flat = true;
            this.listRankCount.Name = "listRankCount";
            this.listRankCount.TabIndex = 2;
            this.listRankCount.Title = "직급별";
            this.listRankCount.Child = null;
            // 
            // btnExecute
            // 
            this.btnExecute.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnExecute.Location = new System.Drawing.Point(12, 28);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(80, 32);
            this.btnExecute.TabIndex = 3;
            this.btnExecute.Text = "실행";
            this.btnExecute.Click += new System.EventHandler(this.OnExecuteClick);
            this.btnExecute.Child = null;
            // 
            // btnNew
            // 
            this.btnNew.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnNew.Location = new System.Drawing.Point(100, 28);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(80, 32);
            this.btnNew.TabIndex = 4;
            this.btnNew.Text = "신규";
            this.btnNew.Click += new System.EventHandler(this.OnNewClick);
            this.btnNew.Child = null;
            // 
            // btnSave
            // 
            this.btnSave.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnSave.Location = new System.Drawing.Point(188, 28);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 32);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "저장";
            this.btnSave.Click += new System.EventHandler(this.OnSaveClick);
            this.btnSave.Child = null;
            // 
            // btnDelete
            // 
            this.btnDelete.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Danger;
            this.btnDelete.Location = new System.Drawing.Point(12, 68);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(80, 32);
            this.btnDelete.TabIndex = 6;
            this.btnDelete.Text = "삭제";
            this.btnDelete.Click += new System.EventHandler(this.OnDeleteClick);
            this.btnDelete.Child = null;
            //
            // ddExcel
            //
            this.ddExcel.Location = new System.Drawing.Point(100, 68);
            this.ddExcel.Name = "ddExcel";
            this.ddExcel.Size = new System.Drawing.Size(84, 32);
            this.ddExcel.TabIndex = 7;
            this.ddExcel.Text = "엑셀";
            this.ddExcel.ItemClicked += new System.EventHandler<Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs>(this.OnExcelItemClicked);
            this.ddExcel.Child = null;
            // 
            // EmployeeManagementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(960, 620);
            this.Controls.Add(this.toastMain);
            this.Controls.Add(this.gridZone);
            this.Controls.Add(this.spacerBottomGap);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.spacerSearchGap);
            this.Controls.Add(this.searchCard);
            this.Controls.Add(this.spacerTitleGap);
            this.Controls.Add(this.titlePanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(760, 480);
            this.Name = "EmployeeManagementForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "직원관리";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.titlePanel.ResumeLayout(false);
            this.treeCard.ResumeLayout(false);
            this.gridZone.ResumeLayout(false);
            this.searchCard.ResumeLayout(false);
            this.countCard.ResumeLayout(false);
            this.statsCard.ResumeLayout(false);
            this.buttonsCard.ResumeLayout(false);
            this.bottomPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel titlePanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblPageTitle;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeEnv;
        private System.Windows.Forms.Panel spacerTitleGap;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel searchCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblName;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtName;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDept;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboDept;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblRank;
        private Modern.Lab.WinForms.Controls.Selection.ModernCheckComboBox cboRank;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblHireDate;
        private Modern.Lab.WinForms.Controls.Input.ModernDatePicker dtHireFrom;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblHireTilde;
        private Modern.Lab.WinForms.Controls.Input.ModernDatePicker dtHireTo;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblSalary;
        private Modern.Lab.WinForms.Controls.Input.ModernNumericTextBox numSalaryFrom;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblSalaryTilde;
        private Modern.Lab.WinForms.Controls.Input.ModernNumericTextBox numSalaryTo;
        private Modern.Lab.WinForms.Controls.Input.ModernCheckBox chkRecentOnly;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblSort;
        private Modern.Lab.WinForms.Controls.Selection.ModernRadioGroup radioSort;
        private Modern.Lab.WinForms.Controls.Input.ModernToggleSwitch tglShowEmail;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblHireMonth;
        private Modern.Lab.WinForms.Controls.Input.ModernMonthPicker monthHire;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnSearch;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReset;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridEmployee;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay busyOverlay;
        private Modern.Lab.WinForms.Controls.Data.ModernPagination pagerEmployee;
        private System.Windows.Forms.Panel gridZone;
        private Modern.Lab.WinForms.Controls.Display.ModernToast toastMain;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel treeCard;
        private Modern.Lab.WinForms.Controls.Selection.ModernTreeView treeOrg;
        private System.Windows.Forms.Panel spacerTreeGap;
        private System.Windows.Forms.Panel spacerSearchGap;
        private System.Windows.Forms.Panel spacerBottomGap;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.Panel spacerCountGap;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel countCard;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox statsCard;
        private System.Windows.Forms.Panel spacerBtnGap;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel buttonsCard;
        private Modern.Lab.WinForms.Controls.Display.ModernKpiCard cardCount;
        private Modern.Lab.WinForms.Controls.Display.ModernSummaryList listDeptCount;
        private Modern.Lab.WinForms.Controls.Display.ModernSummaryList listRankCount;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnExecute;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnNew;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnSave;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnDelete;
        private Modern.Lab.WinForms.Controls.Input.ModernDropDownButton ddExcel;
    }
}
