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
            this.searchCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.lblName = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtName = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.lblDept = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboDept = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblRank = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboRank = new Modern.Lab.WinForms.Controls.Selection.ModernCheckComboBox();
            this.btnSearch = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnReset = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.gridEmployee = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.bottomCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.cardCount = new Modern.Lab.WinForms.Controls.Display.ModernKpiCard();
            this.listDeptCount = new Modern.Lab.WinForms.Controls.Display.ModernSummaryList();
            this.listRankCount = new Modern.Lab.WinForms.Controls.Display.ModernSummaryList();
            this.btnExecute = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnNew = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnSave = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnDelete = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnExcel = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.searchCard.SuspendLayout();
            this.bottomCard.SuspendLayout();
            this.SuspendLayout();
            // 
            // searchCard
            // 
            this.searchCard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.searchCard.Controls.Add(this.lblName);
            this.searchCard.Controls.Add(this.txtName);
            this.searchCard.Controls.Add(this.lblDept);
            this.searchCard.Controls.Add(this.cboDept);
            this.searchCard.Controls.Add(this.lblRank);
            this.searchCard.Controls.Add(this.cboRank);
            this.searchCard.Controls.Add(this.btnSearch);
            this.searchCard.Controls.Add(this.btnReset);
            this.searchCard.Location = new System.Drawing.Point(12, 12);
            this.searchCard.Name = "searchCard";
            this.searchCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.searchCard.Size = new System.Drawing.Size(936, 56);
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
            this.gridEmployee.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridEmployee.Location = new System.Drawing.Point(12, 76);
            this.gridEmployee.Name = "gridEmployee";
            this.gridEmployee.Size = new System.Drawing.Size(936, 432);
            this.gridEmployee.TabIndex = 1;
            this.gridEmployee.Child = null;
            // 
            // bottomCard
            // 
            this.bottomCard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.bottomCard.Controls.Add(this.cardCount);
            this.bottomCard.Controls.Add(this.listDeptCount);
            this.bottomCard.Controls.Add(this.listRankCount);
            this.bottomCard.Controls.Add(this.btnExecute);
            this.bottomCard.Controls.Add(this.btnNew);
            this.bottomCard.Controls.Add(this.btnSave);
            this.bottomCard.Controls.Add(this.btnDelete);
            this.bottomCard.Controls.Add(this.btnExcel);
            this.bottomCard.Location = new System.Drawing.Point(12, 516);
            this.bottomCard.Name = "bottomCard";
            this.bottomCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.bottomCard.Size = new System.Drawing.Size(936, 92);
            this.bottomCard.TabIndex = 2;
            // 
            // cardCount
            // 
            this.cardCount.Flat = true;
            this.cardCount.Location = new System.Drawing.Point(12, 12);
            this.cardCount.Name = "cardCount";
            this.cardCount.Size = new System.Drawing.Size(110, 68);
            this.cardCount.TabIndex = 0;
            this.cardCount.Title = "조회 건수";
            this.cardCount.Child = null;
            // 
            // listDeptCount
            // 
            this.listDeptCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listDeptCount.Flat = true;
            this.listDeptCount.Location = new System.Drawing.Point(130, 8);
            this.listDeptCount.Name = "listDeptCount";
            this.listDeptCount.Size = new System.Drawing.Size(354, 38);
            this.listDeptCount.TabIndex = 1;
            this.listDeptCount.Title = "부서별";
            this.listDeptCount.Child = null;
            // 
            // listRankCount
            // 
            this.listRankCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listRankCount.Flat = true;
            this.listRankCount.Location = new System.Drawing.Point(130, 46);
            this.listRankCount.Name = "listRankCount";
            this.listRankCount.Size = new System.Drawing.Size(354, 38);
            this.listRankCount.TabIndex = 2;
            this.listRankCount.Title = "직급별";
            this.listRankCount.Child = null;
            // 
            // btnExecute
            // 
            this.btnExecute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExecute.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnExecute.Location = new System.Drawing.Point(492, 30);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(80, 32);
            this.btnExecute.TabIndex = 3;
            this.btnExecute.Text = "실행";
            this.btnExecute.Click += new System.EventHandler(this.OnExecuteClick);
            this.btnExecute.Child = null;
            // 
            // btnNew
            // 
            this.btnNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNew.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnNew.Location = new System.Drawing.Point(580, 30);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(80, 32);
            this.btnNew.TabIndex = 4;
            this.btnNew.Text = "신규";
            this.btnNew.Click += new System.EventHandler(this.OnNewClick);
            this.btnNew.Child = null;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnSave.Location = new System.Drawing.Point(668, 30);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 32);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "저장";
            this.btnSave.Click += new System.EventHandler(this.OnSaveClick);
            this.btnSave.Child = null;
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Danger;
            this.btnDelete.Location = new System.Drawing.Point(756, 30);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(80, 32);
            this.btnDelete.TabIndex = 6;
            this.btnDelete.Text = "삭제";
            this.btnDelete.Click += new System.EventHandler(this.OnDeleteClick);
            this.btnDelete.Child = null;
            // 
            // btnExcel
            // 
            this.btnExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExcel.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnExcel.Location = new System.Drawing.Point(844, 30);
            this.btnExcel.Name = "btnExcel";
            this.btnExcel.Size = new System.Drawing.Size(80, 32);
            this.btnExcel.TabIndex = 7;
            this.btnExcel.Text = "엑셀";
            this.btnExcel.Click += new System.EventHandler(this.OnExcelClick);
            this.btnExcel.Child = null;
            // 
            // EmployeeManagementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(960, 620);
            this.Controls.Add(this.searchCard);
            this.Controls.Add(this.gridEmployee);
            this.Controls.Add(this.bottomCard);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(760, 480);
            this.Name = "EmployeeManagementForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "직원관리";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.searchCard.ResumeLayout(false);
            this.bottomCard.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel searchCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblName;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtName;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDept;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboDept;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblRank;
        private Modern.Lab.WinForms.Controls.Selection.ModernCheckComboBox cboRank;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnSearch;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReset;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridEmployee;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel bottomCard;
        private Modern.Lab.WinForms.Controls.Display.ModernKpiCard cardCount;
        private Modern.Lab.WinForms.Controls.Display.ModernSummaryList listDeptCount;
        private Modern.Lab.WinForms.Controls.Display.ModernSummaryList listRankCount;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnExecute;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnNew;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnSave;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnDelete;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnExcel;
    }
}
