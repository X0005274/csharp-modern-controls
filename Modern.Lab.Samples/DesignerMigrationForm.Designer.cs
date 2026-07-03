namespace Modern.Lab.Samples
{
    public partial class DesignerMigrationForm
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
            this.btnSearch = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.gridEmployee = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.searchCard.SuspendLayout();
            this.SuspendLayout();
            //
            // searchCard
            //
            this.searchCard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchCard.Controls.Add(this.lblName);
            this.searchCard.Controls.Add(this.txtName);
            this.searchCard.Controls.Add(this.lblDept);
            this.searchCard.Controls.Add(this.cboDept);
            this.searchCard.Controls.Add(this.btnSearch);
            this.searchCard.Location = new System.Drawing.Point(12, 12);
            this.searchCard.Name = "searchCard";
            this.searchCard.Size = new System.Drawing.Size(836, 56);
            this.searchCard.TabIndex = 0;
            //
            // lblName
            //
            this.lblName.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblName.Location = new System.Drawing.Point(12, 12);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(40, 32);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "이름";
            //
            // txtName
            //
            this.txtName.Location = new System.Drawing.Point(56, 12);
            this.txtName.Name = "txtName";
            this.txtName.PlaceholderText = "이름 검색";
            this.txtName.Size = new System.Drawing.Size(160, 32);
            this.txtName.TabIndex = 1;
            //
            // lblDept
            //
            this.lblDept.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDept.Location = new System.Drawing.Point(232, 12);
            this.lblDept.Name = "lblDept";
            this.lblDept.Size = new System.Drawing.Size(40, 32);
            this.lblDept.TabIndex = 2;
            this.lblDept.Text = "부서";
            //
            // cboDept
            //
            this.cboDept.Location = new System.Drawing.Point(276, 12);
            this.cboDept.Name = "cboDept";
            this.cboDept.Size = new System.Drawing.Size(140, 32);
            this.cboDept.TabIndex = 3;
            //
            // btnSearch
            //
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Location = new System.Drawing.Point(744, 12);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(80, 32);
            this.btnSearch.TabIndex = 4;
            this.btnSearch.Text = "조회";
            this.btnSearch.Click += new System.EventHandler(this.OnSearchClick);
            //
            // gridEmployee
            //
            this.gridEmployee.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridEmployee.Location = new System.Drawing.Point(12, 80);
            this.gridEmployee.Name = "gridEmployee";
            this.gridEmployee.Size = new System.Drawing.Size(836, 428);
            this.gridEmployee.TabIndex = 1;
            //
            // DesignerMigrationForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(860, 520);
            this.Controls.Add(this.searchCard);
            this.Controls.Add(this.gridEmployee);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "DesignerMigrationForm";
            this.Text = "디자이너 마이그레이션 샘플";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.searchCard.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel searchCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblName;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtName;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDept;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboDept;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnSearch;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridEmployee;
    }
}
