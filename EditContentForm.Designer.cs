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
            this.tlpFooter = new System.Windows.Forms.TableLayoutPanel();
            this.flpActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnPresentNow = new System.Windows.Forms.Button();
            this.btnDone = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.tlpPager = new System.Windows.Forms.TableLayoutPanel();
            this.btnPrevPageEdit = new System.Windows.Forms.Button();
            this.lblPageIndicator = new System.Windows.Forms.Label();
            this.btnNextPageEdit = new System.Windows.Forms.Button();
            this.tlpAdvanced = new System.Windows.Forms.TableLayoutPanel();
            this.flpRotateZoom = new System.Windows.Forms.FlowLayoutPanel();
            this.btnRotateL = new System.Windows.Forms.Button();
            this.btnRotateR = new System.Windows.Forms.Button();
            this.btnZoomFit = new System.Windows.Forms.Button();
            this.flpChecks = new System.Windows.Forms.FlowLayoutPanel();
            this.chkLiveSync = new System.Windows.Forms.CheckBox();
            this.chkAutoSend = new System.Windows.Forms.CheckBox();
            this.chkEnableAutoScroll = new System.Windows.Forms.CheckBox();
            this.tlpDpad = new System.Windows.Forms.TableLayoutPanel();
            this.btnUp = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picEdit)).BeginInit();
            this.panelFooter.SuspendLayout();
            this.tlpFooter.SuspendLayout();
            this.flpActions.SuspendLayout();
            this.tlpPager.SuspendLayout();
            this.tlpAdvanced.SuspendLayout();
            this.flpRotateZoom.SuspendLayout();
            this.flpChecks.SuspendLayout();
            this.tlpDpad.SuspendLayout();
            this.SuspendLayout();

            //
            // picEdit  (the canvas — expands to fill all space above the footer)
            //
            this.picEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picEdit.Name = "picEdit";
            this.picEdit.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picEdit.TabIndex = 0;
            this.picEdit.TabStop = false;

            //
            // panelFooter  (Dock Bottom; stays full-width and bottom-stacked on resize)
            //
            this.panelFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(34)))));
            this.panelFooter.Controls.Add(this.tlpFooter);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.panelFooter.Size = new System.Drawing.Size(1000, 262);
            this.panelFooter.TabIndex = 1;

            //
            // tlpFooter  (single column; 3 stacked rows: actions / pagination / advanced)
            //
            this.tlpFooter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFooter.ColumnCount = 1;
            this.tlpFooter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFooter.RowCount = 3;
            this.tlpFooter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            this.tlpFooter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.tlpFooter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFooter.Controls.Add(this.flpActions, 0, 0);
            this.tlpFooter.Controls.Add(this.tlpPager, 0, 1);
            this.tlpFooter.Controls.Add(this.tlpAdvanced, 0, 2);
            this.tlpFooter.Name = "tlpFooter";

            //
            // flpActions  (Anchor.None -> horizontally centered in the full-width row)
            //
            this.flpActions.AutoSize = true;
            this.flpActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpActions.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.flpActions.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flpActions.WrapContents = false;
            this.flpActions.Margin = new System.Windows.Forms.Padding(0);
            this.flpActions.Controls.Add(this.btnPresentNow);
            this.flpActions.Controls.Add(this.btnDone);
            this.flpActions.Controls.Add(this.btnClose);
            this.flpActions.Name = "flpActions";

            //
            // btnPresentNow  (Lavender / white)
            //
            this.btnPresentNow.Size = new System.Drawing.Size(150, 42);
            this.btnPresentNow.Margin = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.btnPresentNow.Name = "btnPresentNow";
            this.btnPresentNow.Text = "Present Now";
            this.btnPresentNow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPresentNow.UseVisualStyleBackColor = false;
            this.btnPresentNow.TabIndex = 2;
            this.btnPresentNow.Click += new System.EventHandler(this.btnPresentNow_Click);

            //
            // btnDone  (Green / white)
            //
            this.btnDone.Size = new System.Drawing.Size(110, 42);
            this.btnDone.Margin = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.btnDone.Name = "btnDone";
            this.btnDone.Text = "Done";
            this.btnDone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDone.UseVisualStyleBackColor = false;
            this.btnDone.TabIndex = 0;
            this.btnDone.Click += new System.EventHandler(this.btnDone_Click);

            //
            // btnClose  (White / dark text + border)
            //
            this.btnClose.Size = new System.Drawing.Size(110, 42);
            this.btnClose.Margin = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Text = "Close";
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(208)))), ((int)(((byte)(215)))));
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.TabIndex = 1;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            //
            // tlpPager  (full-width: [Prev] | "x / y" | [Next])
            //
            this.tlpPager.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPager.ColumnCount = 3;
            this.tlpPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.tlpPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24F));
            this.tlpPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.tlpPager.RowCount = 1;
            this.tlpPager.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPager.Controls.Add(this.btnPrevPageEdit, 0, 0);
            this.tlpPager.Controls.Add(this.lblPageIndicator, 1, 0);
            this.tlpPager.Controls.Add(this.btnNextPageEdit, 2, 0);
            this.tlpPager.Margin = new System.Windows.Forms.Padding(4, 2, 4, 4);
            this.tlpPager.Name = "tlpPager";

            //
            // btnPrevPageEdit
            //
            this.btnPrevPageEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPrevPageEdit.Margin = new System.Windows.Forms.Padding(4);
            this.btnPrevPageEdit.Name = "btnPrevPageEdit";
            this.btnPrevPageEdit.Text = "‹  Prev";
            this.btnPrevPageEdit.TabIndex = 11;
            this.btnPrevPageEdit.UseVisualStyleBackColor = true;

            //
            // lblPageIndicator
            //
            this.lblPageIndicator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPageIndicator.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPageIndicator.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblPageIndicator.ForeColor = System.Drawing.Color.White;
            this.lblPageIndicator.Name = "lblPageIndicator";
            this.lblPageIndicator.Text = "1 / 1";

            //
            // btnNextPageEdit
            //
            this.btnNextPageEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNextPageEdit.Margin = new System.Windows.Forms.Padding(4);
            this.btnNextPageEdit.Name = "btnNextPageEdit";
            this.btnNextPageEdit.Text = "Next  ›";
            this.btnNextPageEdit.TabIndex = 12;
            this.btnNextPageEdit.UseVisualStyleBackColor = true;

            //
            // tlpAdvanced  (Anchor.None -> centered cluster of 3 groups at the bottom)
            //
            this.tlpAdvanced.AutoSize = true;
            this.tlpAdvanced.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpAdvanced.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tlpAdvanced.ColumnCount = 3;
            this.tlpAdvanced.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpAdvanced.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpAdvanced.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpAdvanced.RowCount = 1;
            this.tlpAdvanced.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpAdvanced.Controls.Add(this.flpRotateZoom, 0, 0);
            this.tlpAdvanced.Controls.Add(this.flpChecks, 1, 0);
            this.tlpAdvanced.Controls.Add(this.tlpDpad, 2, 0);
            this.tlpAdvanced.Name = "tlpAdvanced";

            //
            // flpRotateZoom  (rotate L/R + Zoom to Fit), vertically centered in its cell
            //
            this.flpRotateZoom.AutoSize = true;
            this.flpRotateZoom.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpRotateZoom.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.flpRotateZoom.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flpRotateZoom.WrapContents = false;
            this.flpRotateZoom.Margin = new System.Windows.Forms.Padding(4, 0, 16, 0);
            this.flpRotateZoom.Controls.Add(this.btnRotateL);
            this.flpRotateZoom.Controls.Add(this.btnRotateR);
            this.flpRotateZoom.Controls.Add(this.btnZoomFit);
            this.flpRotateZoom.Name = "flpRotateZoom";

            //
            // btnRotateL
            //
            this.btnRotateL.Size = new System.Drawing.Size(48, 40);
            this.btnRotateL.Margin = new System.Windows.Forms.Padding(3);
            this.btnRotateL.Name = "btnRotateL";
            this.btnRotateL.Text = "↺";
            this.btnRotateL.Font = new System.Drawing.Font("Arial", 16F);
            this.btnRotateL.TabIndex = 3;
            this.btnRotateL.UseVisualStyleBackColor = true;
            this.btnRotateL.Click += new System.EventHandler(this.btnRotateL_Click);

            //
            // btnRotateR
            //
            this.btnRotateR.Size = new System.Drawing.Size(48, 40);
            this.btnRotateR.Margin = new System.Windows.Forms.Padding(3);
            this.btnRotateR.Name = "btnRotateR";
            this.btnRotateR.Text = "↻";
            this.btnRotateR.Font = new System.Drawing.Font("Arial", 16F);
            this.btnRotateR.TabIndex = 4;
            this.btnRotateR.UseVisualStyleBackColor = true;
            this.btnRotateR.Click += new System.EventHandler(this.btnRotateR_Click);

            //
            // btnZoomFit  ("Full Zoom Out": reset the view to the whole image/page, uncropped)
            //
            this.btnZoomFit.Size = new System.Drawing.Size(110, 40);
            this.btnZoomFit.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
            this.btnZoomFit.Name = "btnZoomFit";
            this.btnZoomFit.Text = "Zoom to Fit";
            this.btnZoomFit.TabIndex = 14;
            this.btnZoomFit.UseVisualStyleBackColor = true;

            //
            // flpChecks  (Live Sync / Auto Send / Auto Scroll), vertically centered
            //
            this.flpChecks.AutoSize = true;
            this.flpChecks.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpChecks.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.flpChecks.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpChecks.WrapContents = false;
            this.flpChecks.Margin = new System.Windows.Forms.Padding(4, 0, 16, 0);
            this.flpChecks.Controls.Add(this.chkLiveSync);
            this.flpChecks.Controls.Add(this.chkAutoSend);
            this.flpChecks.Controls.Add(this.chkEnableAutoScroll);
            this.flpChecks.Name = "flpChecks";

            //
            // chkLiveSync
            //
            this.chkLiveSync.AutoSize = true;
            this.chkLiveSync.ForeColor = System.Drawing.Color.White;
            this.chkLiveSync.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chkLiveSync.Name = "chkLiveSync";
            this.chkLiveSync.Text = "Live Sync";
            this.chkLiveSync.TabIndex = 6;
            this.chkLiveSync.UseVisualStyleBackColor = true;
            this.chkLiveSync.CheckedChanged += new System.EventHandler(this.chkLiveSync_CheckedChanged);

            //
            // chkAutoSend
            //
            this.chkAutoSend.AutoSize = true;
            this.chkAutoSend.ForeColor = System.Drawing.Color.White;
            this.chkAutoSend.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chkAutoSend.Name = "chkAutoSend";
            this.chkAutoSend.Text = "Auto Send";
            this.chkAutoSend.TabIndex = 5;
            this.chkAutoSend.UseVisualStyleBackColor = true;
            this.chkAutoSend.CheckedChanged += new System.EventHandler(this.chkAutoSend_CheckedChanged);

            //
            // chkEnableAutoScroll
            //
            this.chkEnableAutoScroll.AutoSize = true;
            this.chkEnableAutoScroll.ForeColor = System.Drawing.Color.White;
            this.chkEnableAutoScroll.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chkEnableAutoScroll.Name = "chkEnableAutoScroll";
            this.chkEnableAutoScroll.Text = "Enable Auto Scroll";
            this.chkEnableAutoScroll.TabIndex = 13;
            this.chkEnableAutoScroll.UseVisualStyleBackColor = true;

            //
            // tlpDpad  (3x3 cross), vertically centered
            //
            this.tlpDpad.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tlpDpad.ColumnCount = 3;
            this.tlpDpad.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpDpad.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpDpad.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpDpad.RowCount = 3;
            this.tlpDpad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpDpad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpDpad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpDpad.Controls.Add(this.btnUp, 1, 0);
            this.tlpDpad.Controls.Add(this.btnLeft, 0, 1);
            this.tlpDpad.Controls.Add(this.btnRight, 2, 1);
            this.tlpDpad.Controls.Add(this.btnDown, 1, 2);
            this.tlpDpad.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.tlpDpad.Name = "tlpDpad";
            this.tlpDpad.Size = new System.Drawing.Size(102, 102);

            //
            // btnUp / btnLeft / btnRight / btnDown  (Click handlers wired in code)
            //
            this.btnUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnUp.Margin = new System.Windows.Forms.Padding(1);
            this.btnUp.Name = "btnUp";
            this.btnUp.Text = "↑";
            this.btnUp.TabIndex = 7;
            this.btnUp.UseVisualStyleBackColor = true;

            this.btnLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLeft.Margin = new System.Windows.Forms.Padding(1);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Text = "←";
            this.btnLeft.TabIndex = 9;
            this.btnLeft.UseVisualStyleBackColor = true;

            this.btnRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRight.Margin = new System.Windows.Forms.Padding(1);
            this.btnRight.Name = "btnRight";
            this.btnRight.Text = "→";
            this.btnRight.TabIndex = 10;
            this.btnRight.UseVisualStyleBackColor = true;

            this.btnDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDown.Margin = new System.Windows.Forms.Padding(1);
            this.btnDown.Name = "btnDown";
            this.btnDown.Text = "↓";
            this.btnDown.TabIndex = 8;
            this.btnDown.UseVisualStyleBackColor = true;

            //
            // EditContentForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 800);
            this.Controls.Add(this.picEdit);
            this.Controls.Add(this.panelFooter);
            this.Name = "EditContentForm";
            ((System.ComponentModel.ISupportInitialize)(this.picEdit)).EndInit();
            this.panelFooter.ResumeLayout(false);
            this.tlpFooter.ResumeLayout(false);
            this.tlpFooter.PerformLayout();
            this.flpActions.ResumeLayout(false);
            this.tlpPager.ResumeLayout(false);
            this.tlpAdvanced.ResumeLayout(false);
            this.flpRotateZoom.ResumeLayout(false);
            this.flpChecks.ResumeLayout(false);
            this.flpChecks.PerformLayout();
            this.tlpDpad.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.PictureBox picEdit;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.TableLayoutPanel tlpFooter;
        private System.Windows.Forms.FlowLayoutPanel flpActions;
        private System.Windows.Forms.Button btnPresentNow;
        private System.Windows.Forms.Button btnDone;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.TableLayoutPanel tlpPager;
        private System.Windows.Forms.Button btnPrevPageEdit;
        private System.Windows.Forms.Label lblPageIndicator;
        private System.Windows.Forms.Button btnNextPageEdit;
        private System.Windows.Forms.TableLayoutPanel tlpAdvanced;
        private System.Windows.Forms.FlowLayoutPanel flpRotateZoom;
        private System.Windows.Forms.Button btnRotateL;
        private System.Windows.Forms.Button btnRotateR;
        private System.Windows.Forms.Button btnZoomFit;
        private System.Windows.Forms.FlowLayoutPanel flpChecks;
        private System.Windows.Forms.CheckBox chkLiveSync;
        private System.Windows.Forms.CheckBox chkAutoSend;
        private System.Windows.Forms.CheckBox chkEnableAutoScroll;
        private System.Windows.Forms.TableLayoutPanel tlpDpad;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnRight;
    }
}
