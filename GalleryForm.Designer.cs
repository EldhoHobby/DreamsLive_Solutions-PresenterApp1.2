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
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnAddFile = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblThumbSize = new System.Windows.Forms.Label();
            this.trackBarThumbSize = new System.Windows.Forms.TrackBar();
            this.cmbSubfolders = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarThumbSize)).BeginInit();
            this.SuspendLayout();
            //
            // pnlTop
            //
            this.pnlTop.Controls.Add(this.btnAddFile);
            this.pnlTop.Controls.Add(this.btnRefresh);
            this.pnlTop.Controls.Add(this.lblThumbSize);
            this.pnlTop.Controls.Add(this.trackBarThumbSize);
            this.pnlTop.Controls.Add(this.cmbSubfolders);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
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
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.pnlTop);
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
    }
}
