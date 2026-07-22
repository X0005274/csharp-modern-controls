namespace Modern.Lab.Samples
{
    public partial class ManualReceiveDialogForm
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
            this.lblLotId = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtLotId = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.btnCheck = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.lblSendFac = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboSendFac = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.gridUnits = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.badgeResult = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.btnReceive = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnCancel = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.SuspendLayout();
            //
            // lblSendFac
            //
            this.lblSendFac.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblSendFac.Location = new System.Drawing.Point(16, 16);
            this.lblSendFac.Name = "lblSendFac";
            this.lblSendFac.Required = true;
            this.lblSendFac.Size = new System.Drawing.Size(70, 32);
            this.lblSendFac.TabIndex = 0;
            this.lblSendFac.Text = "Send Fac";
            this.lblSendFac.Child = null;
            //
            // cboSendFac
            //
            this.cboSendFac.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSendFac.Location = new System.Drawing.Point(90, 16);
            this.cboSendFac.Name = "cboSendFac";
            this.cboSendFac.Size = new System.Drawing.Size(160, 32);
            this.cboSendFac.TabIndex = 1;
            this.cboSendFac.Child = null;
            //
            // lblLotId
            //
            this.lblLotId.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblLotId.Location = new System.Drawing.Point(16, 56);
            this.lblLotId.Name = "lblLotId";
            this.lblLotId.Required = true;
            this.lblLotId.Size = new System.Drawing.Size(70, 32);
            this.lblLotId.TabIndex = 2;
            this.lblLotId.Text = "Lot ID";
            this.lblLotId.Child = null;
            //
            // txtLotId
            //
            this.txtLotId.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtLotId.Location = new System.Drawing.Point(90, 56);
            this.txtLotId.Name = "txtLotId";
            this.txtLotId.PlaceholderText = "Lot ID not on the board";
            this.txtLotId.Size = new System.Drawing.Size(190, 32);
            this.txtLotId.TabIndex = 3;
            this.txtLotId.TextChanged += new System.EventHandler(this.OnInputChanged);
            this.txtLotId.Child = null;
            //
            // btnCheck
            //
            this.btnCheck.Location = new System.Drawing.Point(288, 56);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(64, 32);
            this.btnCheck.TabIndex = 4;
            this.btnCheck.Text = "Check";
            this.btnCheck.Click += new System.EventHandler(this.OnCheckClick);
            this.btnCheck.Child = null;
            //
            // gridUnits
            //
            this.gridUnits.AutoFitColumns = true;
            this.gridUnits.EmptyText = "Check a lot ID to list its wafers";
            this.gridUnits.Location = new System.Drawing.Point(16, 96);
            this.gridUnits.Name = "gridUnits";
            this.gridUnits.ShowStatusBar = true;
            this.gridUnits.Size = new System.Drawing.Size(408, 220);
            this.gridUnits.StatusCountFormat = "{0:N0} wafer(s)";
            this.gridUnits.TabIndex = 5;
            this.gridUnits.Child = null;
            //
            // badgeResult
            //
            this.badgeResult.Color = "#FEE2E2";
            this.badgeResult.Location = new System.Drawing.Point(16, 324);
            this.badgeResult.Name = "badgeResult";
            this.badgeResult.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeResult.Size = new System.Drawing.Size(408, 26);
            this.badgeResult.TabIndex = 6;
            this.badgeResult.Text = "-";
            this.badgeResult.Visible = false;
            this.badgeResult.Child = null;
            //
            // btnReceive
            //
            this.btnReceive.Enabled = false;
            this.btnReceive.Location = new System.Drawing.Point(240, 364);
            this.btnReceive.Name = "btnReceive";
            this.btnReceive.Size = new System.Drawing.Size(88, 32);
            this.btnReceive.TabIndex = 7;
            this.btnReceive.Text = "Receive";
            this.btnReceive.Click += new System.EventHandler(this.OnReceiveClick);
            this.btnReceive.Child = null;
            //
            // btnCancel
            //
            this.btnCancel.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnCancel.Location = new System.Drawing.Point(336, 364);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.OnCancelClick);
            this.btnCancel.Child = null;
            //
            // ManualReceiveDialogForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(440, 412);
            this.Controls.Add(this.lblSendFac);
            this.Controls.Add(this.cboSendFac);
            this.Controls.Add(this.lblLotId);
            this.Controls.Add(this.txtLotId);
            this.Controls.Add(this.btnCheck);
            this.Controls.Add(this.gridUnits);
            this.Controls.Add(this.badgeResult);
            this.Controls.Add(this.btnReceive);
            this.Controls.Add(this.btnCancel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ManualReceiveDialogForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Manual Receive";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.ResumeLayout(false);

        }

        #endregion

        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblLotId;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtLotId;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnCheck;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblSendFac;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboSendFac;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridUnits;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeResult;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReceive;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnCancel;
    }
}
