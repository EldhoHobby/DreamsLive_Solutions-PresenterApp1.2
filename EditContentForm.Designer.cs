namespace DreamsLive_Solutions_PresenterApp1
{
    partial class EditContentForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing && _originalImage != null)
            {
                _originalImage.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.picEdit = new System.Windows.Forms.PictureBox();
            this.panelFooter = new System.Windows.Forms.Panel();
            this.btnDone = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnPresentNow = new System.Windows.Forms.Button();
            this.btnRotateL = new System.Windows.Forms.Button();
            this.btnRotateR = new System.Windows.Forms.Button();
            this.chkAutoSend = new System.Windows.Forms.CheckBox();
            this.chkLiveSync = new System.Windows.Forms.CheckBox();
            this.chkEnableScroll = new System.Windows.Forms.CheckBox();
            this.btnUp = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picEdit)).BeginInit();
            this.panelFooter.SuspendLayout();
            this.SuspendLayout();

            // picEdit
            this.picEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picEdit.Location = new System.Drawing.Point(0, 0);
            this.picEdit.Name = "picEdit";
            this.picEdit.Size = new System.Drawing.Size(1000, 700);
            this.picEdit.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picEdit.TabIndex = 0;
            this.picEdit.TabStop = false;

            // panelFooter
            this.panelFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(34)))));
            this.panelFooter.Controls.Add(this.chkLiveSync);
            this.panelFooter.Controls.Add(this.chkAutoSend);
            this.panelFooter.Controls.Add(this.chkEnableScroll);
            this.panelFooter.Controls.Add(this.btnUp);
            this.panelFooter.Controls.Add(this.btnDown);
            this.panelFooter.Controls.Add(this.btnLeft);
            this.panelFooter.Controls.Add(this.btnRight);
            this.panelFooter.Controls.Add(this.btnSettings);
            this.panelFooter.Controls.Add(this.btnRotateL);
            this.panelFooter.Controls.Add(this.btnRotateR);
            this.panelFooter.Controls.Add(this.btnPresentNow);
            this.panelFooter.Controls.Add(this.btnDone);
            this.panelFooter.Controls.Add(this.btnClose);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFooter.Location = new System.Drawing.Point(0, 700);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Padding = new System.Windows.Forms.Padding(10);
            this.panelFooter.Size = new System.Drawing.Size(1000, 100);
            this.panelFooter.TabIndex = 1;

            // chkAutoSend
            this.chkAutoSend.AutoSize = true;
            this.chkAutoSend.ForeColor = System.Drawing.Color.White;
            this.chkAutoSend.Location = new System.Drawing.Point(20, 40);
            this.chkAutoSend.Name = "chkAutoSend";
            this.chkAutoSend.Size = new System.Drawing.Size(100, 20);
            this.chkAutoSend.TabIndex = 5;
            this.chkAutoSend.Text = "Auto Send";
            this.chkAutoSend.UseVisualStyleBackColor = true;
            this.chkAutoSend.CheckedChanged += new System.EventHandler(this.chkAutoSend_CheckedChanged);

            // chkLiveSync
            this.chkLiveSync.AutoSize = true;
            this.chkLiveSync.ForeColor = System.Drawing.Color.White;
            this.chkLiveSync.Location = new System.Drawing.Point(20, 20);
            this.chkLiveSync.Name = "chkLiveSync";
            this.chkLiveSync.Size = new System.Drawing.Size(100, 20);
            this.chkLiveSync.TabIndex = 6;
            this.chkLiveSync.Text = "Live Sync";
            this.chkLiveSync.UseVisualStyleBackColor = true;
            this.chkLiveSync.CheckedChanged += new System.EventHandler(this.chkLiveSync_CheckedChanged);

            // chkEnableScroll
            this.chkEnableScroll.AutoSize = true;
            this.chkEnableScroll.ForeColor = System.Drawing.Color.White;
            this.chkEnableScroll.Location = new System.Drawing.Point(20, 60);
            this.chkEnableScroll.Name = "chkEnableScroll";
            this.chkEnableScroll.Size = new System.Drawing.Size(120, 20);
            this.chkEnableScroll.TabIndex = 7;
            this.chkEnableScroll.Text = "Enable Auto Scroll";
            this.chkEnableScroll.UseVisualStyleBackColor = true;

            // btnUp
            this.btnUp.Location = new System.Drawing.Point(320, 10);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(30, 30);
            this.btnUp.TabIndex = 8;
            this.btnUp.Text = "↑";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);

            // btnDown
            this.btnDown.Location = new System.Drawing.Point(320, 60);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(30, 30);
            this.btnDown.TabIndex = 9;
            this.btnDown.Text = "↓";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);

            // btnLeft
            this.btnLeft.Location = new System.Drawing.Point(290, 35);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(30, 30);
            this.btnLeft.TabIndex = 10;
            this.btnLeft.Text = "←";
            this.btnLeft.UseVisualStyleBackColor = true;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);

            // btnRight
            this.btnRight.Location = new System.Drawing.Point(350, 35);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(30, 30);
            this.btnRight.TabIndex = 11;
            this.btnRight.Text = "→";
            this.btnRight.UseVisualStyleBackColor = true;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);

            // btnSettings
            this.btnSettings.Location = new System.Drawing.Point(150, 5);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(110, 23);
            this.btnSettings.TabIndex = 16;
            this.btnSettings.Text = "Scroll Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);

            // btnRotateL
            this.btnRotateL.Location = new System.Drawing.Point(150, 30);
            this.btnRotateL.Name = "btnRotateL";
            this.btnRotateL.Size = new System.Drawing.Size(50, 40);
            this.btnRotateL.TabIndex = 3;
            this.btnRotateL.Text = "↺";
            this.btnRotateL.Font = new System.Drawing.Font("Arial", 16F);
            this.btnRotateL.UseVisualStyleBackColor = true;
            this.btnRotateL.Click += new System.EventHandler(this.btnRotateL_Click);

            // btnRotateR
            this.btnRotateR.Location = new System.Drawing.Point(210, 30);
            this.btnRotateR.Name = "btnRotateR";
            this.btnRotateR.Size = new System.Drawing.Size(50, 40);
            this.btnRotateR.TabIndex = 4;
            this.btnRotateR.Text = "↻";
            this.btnRotateR.Font = new System.Drawing.Font("Arial", 16F);
            this.btnRotateR.UseVisualStyleBackColor = true;
            this.btnRotateR.Click += new System.EventHandler(this.btnRotateR_Click);

            // btnPresentNow
            this.btnPresentNow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            this.btnPresentNow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPresentNow.ForeColor = System.Drawing.Color.White;
            this.btnPresentNow.Location = new System.Drawing.Point(500, 30);
            this.btnPresentNow.Name = "btnPresentNow";
            this.btnPresentNow.Size = new System.Drawing.Size(150, 40);
            this.btnPresentNow.TabIndex = 2;
            this.btnPresentNow.Text = "Present Now";
            this.btnPresentNow.UseVisualStyleBackColor = false;
            this.btnPresentNow.Click += new System.EventHandler(this.btnPresentNow_Click);

            // btnDone
            this.btnDone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.btnDone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDone.ForeColor = System.Drawing.Color.White;
            this.btnDone.Location = new System.Drawing.Point(670, 30);
            this.btnDone.Name = "btnDone";
            this.btnDone.Size = new System.Drawing.Size(100, 40);
            this.btnDone.TabIndex = 0;
            this.btnDone.Text = "Done";
            this.btnDone.UseVisualStyleBackColor = false;
            this.btnDone.Click += new System.EventHandler(this.btnDone_Click);

            // btnClose
            this.btnClose.Location = new System.Drawing.Point(880, 30);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 40);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            // EditContentForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 800);
            this.Controls.Add(this.picEdit);
            this.Controls.Add(this.panelFooter);
            this.Name = "EditContentForm";
            ((System.ComponentModel.ISupportInitialize)(this.picEdit)).EndInit();
            this.panelFooter.ResumeLayout(false);
            this.panelFooter.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.PictureBox picEdit;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.Button btnDone;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnPresentNow;
        private System.Windows.Forms.Button btnRotateL;
        private System.Windows.Forms.Button btnRotateR;
        private System.Windows.Forms.CheckBox chkAutoSend;
        private System.Windows.Forms.CheckBox chkLiveSync;
        private System.Windows.Forms.CheckBox chkEnableScroll;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.Button btnSettings;
    }
}
