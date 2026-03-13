namespace DreamsLive_Solutions_PresenterApp1
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlTitleBar = new System.Windows.Forms.Panel();
            this.btnAppClose = new System.Windows.Forms.Button();
            this.lblFormTitle = new System.Windows.Forms.Label();
            this.chkSkipOnePage = new System.Windows.Forms.CheckBox();
            this.chkTwoPagePdf = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlTitleBar.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlTitleBar
            //
            this.pnlTitleBar.Controls.Add(this.lblFormTitle);
            this.pnlTitleBar.Controls.Add(this.btnAppClose);
            this.pnlTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Location = new System.Drawing.Point(0, 0);
            this.pnlTitleBar.Name = "pnlTitleBar";
            this.pnlTitleBar.Size = new System.Drawing.Size(250, 32);
            this.pnlTitleBar.TabIndex = 63;
            this.pnlTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlTitleBar_MouseDown);
            //
            // btnAppClose
            //
            this.btnAppClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnAppClose.FlatAppearance.BorderSize = 0;
            this.btnAppClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAppClose.Font = new System.Drawing.Font("Segoe MDL2 Assets", 10F);
            this.btnAppClose.Location = new System.Drawing.Point(205, 0);
            this.btnAppClose.Name = "btnAppClose";
            this.btnAppClose.Size = new System.Drawing.Size(45, 32);
            this.btnAppClose.TabIndex = 0;
            this.btnAppClose.Text = "";
            this.btnAppClose.UseVisualStyleBackColor = true;
            this.btnAppClose.Click += new System.EventHandler(this.btnAppClose_Click);
            //
            // lblFormTitle
            //
            this.lblFormTitle.AutoSize = true;
            this.lblFormTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFormTitle.Location = new System.Drawing.Point(10, 8);
            this.lblFormTitle.Name = "lblFormTitle";
            this.lblFormTitle.Size = new System.Drawing.Size(81, 15);
            this.lblFormTitle.TabIndex = 1;
            this.lblFormTitle.Text = "Scroll Settings";
            this.lblFormTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlTitleBar_MouseDown);
            //
            // chkSkipOnePage
            //
            this.chkSkipOnePage.AutoSize = true;
            this.chkSkipOnePage.Location = new System.Drawing.Point(30, 50);
            this.chkSkipOnePage.Name = "chkSkipOnePage";
            this.chkSkipOnePage.Size = new System.Drawing.Size(120, 17);
            this.chkSkipOnePage.TabIndex = 0;
            this.chkSkipOnePage.Text = "Skip 1 page (PDF)";
            this.chkSkipOnePage.UseVisualStyleBackColor = true;
            //
            // chkTwoPagePdf
            //
            this.chkTwoPagePdf.AutoSize = true;
            this.chkTwoPagePdf.Location = new System.Drawing.Point(30, 60);
            this.chkTwoPagePdf.Name = "chkTwoPagePdf";
            this.chkTwoPagePdf.Size = new System.Drawing.Size(95, 17);
            this.chkTwoPagePdf.TabIndex = 1;
            this.chkTwoPagePdf.Text = "2 page PDF";
            this.chkTwoPagePdf.UseVisualStyleBackColor = true;
            //
            // btnSave
            //
            this.btnSave.Location = new System.Drawing.Point(30, 110);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            //
            // btnCancel
            //
            this.btnCancel.Location = new System.Drawing.Point(120, 110);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // SettingsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(250, 160);
            this.Controls.Add(this.pnlTitleBar);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.chkTwoPagePdf);
            this.Controls.Add(this.chkSkipOnePage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Scroll Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.CheckBox chkSkipOnePage;
        private System.Windows.Forms.CheckBox chkTwoPagePdf;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Button btnAppClose;
        private System.Windows.Forms.Label lblFormTitle;
    }
}
