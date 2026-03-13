namespace DreamsLive_Solutions_PresenterApp1
{
    partial class GalleryForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.pnlTitleBar = new System.Windows.Forms.Panel();
            this.btnAppClose = new System.Windows.Forms.Button();
            this.lblFormTitle = new System.Windows.Forms.Label();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnAddFile = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblThumbSize = new System.Windows.Forms.Label();
            this.trackBarThumbSize = new System.Windows.Forms.TrackBar();
            this.cmbSubfolders = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlTitleBar.SuspendLayout();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarThumbSize)).BeginInit();
            this.SuspendLayout();
            //
            // pnlTitleBar
            //
            this.pnlTitleBar.Controls.Add(this.lblFormTitle);
            this.pnlTitleBar.Controls.Add(this.btnAppClose);
            this.pnlTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Location = new System.Drawing.Point(0, 0);
            this.pnlTitleBar.Name = "pnlTitleBar";
            this.pnlTitleBar.Size = new System.Drawing.Size(800, 32);
            this.pnlTitleBar.TabIndex = 62;
            this.pnlTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlTitleBar_MouseDown);
            //
            // btnAppClose
            //
            this.btnAppClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnAppClose.FlatAppearance.BorderSize = 0;
            this.btnAppClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAppClose.Font = new System.Drawing.Font("Segoe MDL2 Assets", 10F);
            this.btnAppClose.Location = new System.Drawing.Point(755, 0);
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
            this.lblFormTitle.Size = new System.Drawing.Size(43, 15);
            this.lblFormTitle.TabIndex = 1;
            this.lblFormTitle.Text = "Gallery";
            this.lblFormTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlTitleBar_MouseDown);
            //
            // pnlTop
            //
            this.pnlTop.Controls.Add(this.btnAddFile);
            this.pnlTop.Controls.Add(this.btnRefresh);
            this.pnlTop.Controls.Add(this.lblThumbSize);
            this.pnlTop.Controls.Add(this.trackBarThumbSize);
            this.pnlTop.Controls.Add(this.cmbSubfolders);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 32);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(800, 60);
            this.pnlTop.TabIndex = 0;
            //
            // btnAddFile
            //
            this.btnAddFile.Location = new System.Drawing.Point(670, 15);
            this.btnAddFile.Name = "btnAddFile";
            this.btnAddFile.Size = new System.Drawing.Size(118, 23);
            this.btnAddFile.TabIndex = 4;
            this.btnAddFile.Text = "+ Add to Database";
            this.btnAddFile.UseVisualStyleBackColor = true;
            this.btnAddFile.Click += new System.EventHandler(this.btnAddFile_Click);
            //
            // btnRefresh
            //
            this.btnRefresh.Location = new System.Drawing.Point(589, 15);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            //
            // lblThumbSize
            //
            this.lblThumbSize.AutoSize = true;
            this.lblThumbSize.Location = new System.Drawing.Point(350, 20);
            this.lblThumbSize.Name = "lblThumbSize";
            this.lblThumbSize.Size = new System.Drawing.Size(30, 13);
            this.lblThumbSize.TabIndex = 2;
            this.lblThumbSize.Text = "Size:";
            //
            // trackBarThumbSize
            //
            this.trackBarThumbSize.Location = new System.Drawing.Point(380, 12);
            this.trackBarThumbSize.Maximum = 300;
            this.trackBarThumbSize.Minimum = 50;
            this.trackBarThumbSize.Name = "trackBarThumbSize";
            this.trackBarThumbSize.Size = new System.Drawing.Size(200, 45);
            this.trackBarThumbSize.TabIndex = 1;
            this.trackBarThumbSize.Value = 100;
            this.trackBarThumbSize.Scroll += new System.EventHandler(this.trackBarThumbSize_Scroll);
            //
            // cmbSubfolders
            //
            this.cmbSubfolders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSubfolders.FormattingEnabled = true;
            this.cmbSubfolders.Location = new System.Drawing.Point(12, 17);
            this.cmbSubfolders.Name = "cmbSubfolders";
            this.cmbSubfolders.Size = new System.Drawing.Size(320, 21);
            this.cmbSubfolders.TabIndex = 0;
            this.cmbSubfolders.SelectedIndexChanged += new System.EventHandler(this.cmbSubfolders_SelectedIndexChanged);
            //
            // flowLayoutPanel1
            //
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 60);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(10);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(800, 540);
            this.flowLayoutPanel1.TabIndex = 1;
            //
            // GalleryForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.pnlTitleBar);
            this.Name = "GalleryForm";
            this.Text = "Gallery";
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarThumbSize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.ComboBox cmbSubfolders;
        private System.Windows.Forms.TrackBar trackBarThumbSize;
        private System.Windows.Forms.Label lblThumbSize;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnAddFile;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Button btnAppClose;
        private System.Windows.Forms.Label lblFormTitle;
    }
}
