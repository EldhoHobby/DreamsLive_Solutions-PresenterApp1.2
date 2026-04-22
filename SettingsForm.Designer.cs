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
            this.chkSkipOnePage = new System.Windows.Forms.CheckBox();
            this.chkTwoPagePdf = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.labelAudience = new System.Windows.Forms.Label();
            this.cmbAudienceScreen = new System.Windows.Forms.ComboBox();
            this.labelNotes = new System.Windows.Forms.Label();
            this.cmbNotesScreen = new System.Windows.Forms.ComboBox();
            this.btnResyncDisplays = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // chkSkipOnePage
            //
            this.chkSkipOnePage.AutoSize = true;
            this.chkSkipOnePage.Location = new System.Drawing.Point(30, 30);
            this.chkSkipOnePage.Name = "chkSkipOnePage";
            this.chkSkipOnePage.Size = new System.Drawing.Size(113, 17);
            this.chkSkipOnePage.TabIndex = 0;
            this.chkSkipOnePage.Text = "Skip 1 page (PDF)";
            this.chkSkipOnePage.UseVisualStyleBackColor = true;
            //
            // chkTwoPagePdf
            //
            this.chkTwoPagePdf.AutoSize = true;
            this.chkTwoPagePdf.Location = new System.Drawing.Point(30, 60);
            this.chkTwoPagePdf.Name = "chkTwoPagePdf";
            this.chkTwoPagePdf.Size = new System.Drawing.Size(83, 17);
            this.chkTwoPagePdf.TabIndex = 1;
            this.chkTwoPagePdf.Text = "2 page PDF";
            this.chkTwoPagePdf.UseVisualStyleBackColor = true;
            //
            // btnSave
            //
            this.btnSave.Location = new System.Drawing.Point(120, 246);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            //
            // btnCancel
            //
            this.btnCancel.Location = new System.Drawing.Point(210, 246);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // labelAudience
            //
            this.labelAudience.AutoSize = true;
            this.labelAudience.Location = new System.Drawing.Point(27, 100);
            this.labelAudience.Name = "labelAudience";
            this.labelAudience.Size = new System.Drawing.Size(117, 13);
            this.labelAudience.TabIndex = 4;
            this.labelAudience.Text = "Audience (HDMI 1):";
            //
            // cmbAudienceScreen
            //
            this.cmbAudienceScreen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAudienceScreen.FormattingEnabled = true;
            this.cmbAudienceScreen.Location = new System.Drawing.Point(30, 116);
            this.cmbAudienceScreen.Name = "cmbAudienceScreen";
            this.cmbAudienceScreen.Size = new System.Drawing.Size(255, 21);
            this.cmbAudienceScreen.TabIndex = 5;
            //
            // labelNotes
            //
            this.labelNotes.AutoSize = true;
            this.labelNotes.Location = new System.Drawing.Point(27, 150);
            this.labelNotes.Name = "labelNotes";
            this.labelNotes.Size = new System.Drawing.Size(100, 13);
            this.labelNotes.TabIndex = 6;
            this.labelNotes.Text = "Notes (HDMI 2):";
            //
            // cmbNotesScreen
            //
            this.cmbNotesScreen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNotesScreen.FormattingEnabled = true;
            this.cmbNotesScreen.Location = new System.Drawing.Point(30, 166);
            this.cmbNotesScreen.Name = "cmbNotesScreen";
            this.cmbNotesScreen.Size = new System.Drawing.Size(255, 21);
            this.cmbNotesScreen.TabIndex = 7;
            //
            // btnResyncDisplays
            //
            this.btnResyncDisplays.Location = new System.Drawing.Point(30, 203);
            this.btnResyncDisplays.Name = "btnResyncDisplays";
            this.btnResyncDisplays.Size = new System.Drawing.Size(120, 23);
            this.btnResyncDisplays.TabIndex = 8;
            this.btnResyncDisplays.Text = "Re-Sync Displays";
            this.btnResyncDisplays.UseVisualStyleBackColor = true;
            this.btnResyncDisplays.Click += new System.EventHandler(this.btnResyncDisplays_Click);
            //
            // SettingsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 290);
            this.Controls.Add(this.btnResyncDisplays);
            this.Controls.Add(this.cmbNotesScreen);
            this.Controls.Add(this.labelNotes);
            this.Controls.Add(this.cmbAudienceScreen);
            this.Controls.Add(this.labelAudience);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.chkTwoPagePdf);
            this.Controls.Add(this.chkSkipOnePage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
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
        private System.Windows.Forms.Label labelAudience;
        private System.Windows.Forms.ComboBox cmbAudienceScreen;
        private System.Windows.Forms.Label labelNotes;
        private System.Windows.Forms.ComboBox cmbNotesScreen;
        private System.Windows.Forms.Button btnResyncDisplays;
    }
}
