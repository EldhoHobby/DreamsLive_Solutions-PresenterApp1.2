namespace DreamsLive_Solutions_PresenterApp1
{
    partial class GalleryForm
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.cmbSubfolders = new System.Windows.Forms.ComboBox();
            this.lblFolder = new System.Windows.Forms.Label();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.lblThumbSize = new System.Windows.Forms.Label();
            this.trackThumbSize = new System.Windows.Forms.TrackBar();
            this.flowThumbs = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlTop.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackThumbSize)).BeginInit();
            this.SuspendLayout();
            //
            // pnlTop
            //
            this.pnlTop.Controls.Add(this.cmbSubfolders);
            this.pnlTop.Controls.Add(this.lblFolder);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(800, 60);
            this.pnlTop.TabIndex = 0;
            //
            // cmbSubfolders
            //
            this.cmbSubfolders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSubfolders.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbSubfolders.FormattingEnabled = true;
            this.cmbSubfolders.Location = new System.Drawing.Point(75, 15);
            this.cmbSubfolders.MaxDropDownItems = 20;
            this.cmbSubfolders.Name = "cmbSubfolders";
            this.cmbSubfolders.Size = new System.Drawing.Size(400, 29);
            this.cmbSubfolders.TabIndex = 1;
            this.cmbSubfolders.SelectedIndexChanged += new System.EventHandler(this.cmbSubfolders_SelectedIndexChanged);
            //
            // lblFolder
            //
            this.lblFolder.AutoSize = true;
            this.lblFolder.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFolder.Location = new System.Drawing.Point(12, 18);
            this.lblFolder.Name = "lblFolder";
            this.lblFolder.Size = new System.Drawing.Size(57, 21);
            this.lblFolder.TabIndex = 0;
            this.lblFolder.Text = "Folder:";
            //
            // pnlBottom
            //
            this.pnlBottom.Controls.Add(this.lblThumbSize);
            this.pnlBottom.Controls.Add(this.trackThumbSize);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 400);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(800, 50);
            this.pnlBottom.TabIndex = 2;
            //
            // lblThumbSize
            //
            this.lblThumbSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblThumbSize.AutoSize = true;
            this.lblThumbSize.Location = new System.Drawing.Point(540, 18);
            this.lblThumbSize.Name = "lblThumbSize";
            this.lblThumbSize.Size = new System.Drawing.Size(85, 13);
            this.lblThumbSize.TabIndex = 1;
            this.lblThumbSize.Text = "Thumbnail Size:";
            //
            // trackThumbSize
            //
            this.trackThumbSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.trackThumbSize.LargeChange = 50;
            this.trackThumbSize.Location = new System.Drawing.Point(631, 10);
            this.trackThumbSize.Maximum = 300;
            this.trackThumbSize.Minimum = 50;
            this.trackThumbSize.Name = "trackThumbSize";
            this.trackThumbSize.Size = new System.Drawing.Size(157, 45);
            this.trackThumbSize.SmallChange = 10;
            this.trackThumbSize.TabIndex = 0;
            this.trackThumbSize.TickFrequency = 50;
            this.trackThumbSize.Value = 150;
            this.trackThumbSize.Scroll += new System.EventHandler(this.trackThumbSize_Scroll);
            //
            // flowThumbs
            //
            this.flowThumbs.AutoScroll = true;
            this.flowThumbs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowThumbs.Location = new System.Drawing.Point(0, 60);
            this.flowThumbs.Name = "flowThumbs";
            this.flowThumbs.Size = new System.Drawing.Size(800, 340);
            this.flowThumbs.TabIndex = 1;
            //
            // GalleryForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.flowThumbs);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlTop);
            // Thumbnails reflow in the FlowLayoutPanel, so the gallery scales freely; this floor
            // just keeps the top folder bar and bottom size slider from overlapping.
            this.MinimumSize = new System.Drawing.Size(440, 320);
            this.Name = "GalleryForm";
            this.Text = "Host Gallery";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GalleryForm_FormClosing);
            this.Load += new System.EventHandler(this.GalleryForm_Load);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackThumbSize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.ComboBox cmbSubfolders;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.TrackBar trackThumbSize;
        private System.Windows.Forms.Label lblThumbSize;
        private System.Windows.Forms.FlowLayoutPanel flowThumbs;
        private System.Windows.Forms.Timer timerDebounce;
    }
}
