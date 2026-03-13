using System.Drawing;
using System.Windows.Forms;

namespace DreamsLive_Solutions_PresenterApp1
{
    partial class AddToDatabaseForm
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
            this.lblSubfolder = new System.Windows.Forms.Label();
            this.cmbSubfolders = new System.Windows.Forms.ComboBox();
            this.btnToggleNewFolder = new System.Windows.Forms.Button();
            this.lblNewFolder = new System.Windows.Forms.Label();
            this.txtNewFolder = new System.Windows.Forms.TextBox();
            this.lblCustomFile = new System.Windows.Forms.Label();
            this.txtCustomFileName = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblSubfolder
            //
            this.lblSubfolder.AutoSize = true;
            this.lblSubfolder.Location = new System.Drawing.Point(12, 15);
            this.lblSubfolder.Name = "lblSubfolder";
            this.lblSubfolder.Size = new System.Drawing.Size(91, 13);
            this.lblSubfolder.TabIndex = 0;
            this.lblSubfolder.Text = "Target Subfolder:";
            //
            // cmbSubfolders
            //
            this.cmbSubfolders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSubfolders.FormattingEnabled = true;
            this.cmbSubfolders.Location = new System.Drawing.Point(12, 35);
            this.cmbSubfolders.Name = "cmbSubfolders";
            this.cmbSubfolders.Size = new System.Drawing.Size(260, 21);
            this.cmbSubfolders.TabIndex = 1;
            //
            // btnToggleNewFolder
            //
            this.btnToggleNewFolder.Location = new System.Drawing.Point(12, 65);
            this.btnToggleNewFolder.Name = "btnToggleNewFolder";
            this.btnToggleNewFolder.Size = new System.Drawing.Size(120, 25);
            this.btnToggleNewFolder.TabIndex = 2;
            this.btnToggleNewFolder.Text = "+ New Folder";
            this.btnToggleNewFolder.UseVisualStyleBackColor = true;
            this.btnToggleNewFolder.Click += new System.EventHandler(this.btnToggleNewFolder_Click);
            //
            // lblNewFolder
            //
            this.lblNewFolder.AutoSize = true;
            this.lblNewFolder.Location = new System.Drawing.Point(12, 100);
            this.lblNewFolder.Name = "lblNewFolder";
            this.lblNewFolder.Size = new System.Drawing.Size(95, 13);
            this.lblNewFolder.TabIndex = 3;
            this.lblNewFolder.Text = "New Folder Name:";
            this.lblNewFolder.Visible = false;
            //
            // txtNewFolder
            //
            this.txtNewFolder.Location = new System.Drawing.Point(12, 120);
            this.txtNewFolder.Name = "txtNewFolder";
            this.txtNewFolder.Size = new System.Drawing.Size(260, 20);
            this.txtNewFolder.TabIndex = 4;
            this.txtNewFolder.Visible = false;
            //
            // lblCustomFile
            //
            this.lblCustomFile.AutoSize = true;
            this.lblCustomFile.Location = new System.Drawing.Point(12, 150);
            this.lblCustomFile.Name = "lblCustomFile";
            this.lblCustomFile.Size = new System.Drawing.Size(135, 13);
            this.lblCustomFile.TabIndex = 5;
            this.lblCustomFile.Text = "Custom Filename (optional):";
            //
            // txtCustomFileName
            //
            this.txtCustomFileName.Location = new System.Drawing.Point(12, 170);
            this.txtCustomFileName.Name = "txtCustomFileName";
            this.txtCustomFileName.Size = new System.Drawing.Size(260, 20);
            this.txtCustomFileName.TabIndex = 6;
            //
            // btnOK
            //
            this.btnOK.Location = new System.Drawing.Point(116, 210);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 25);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "Add Now";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            //
            // btnCancel
            //
            this.btnCancel.Location = new System.Drawing.Point(197, 210);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 25);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // AddToDatabaseForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 250);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtCustomFileName);
            this.Controls.Add(this.lblCustomFile);
            this.Controls.Add(this.txtNewFolder);
            this.Controls.Add(this.lblNewFolder);
            this.Controls.Add(this.btnToggleNewFolder);
            this.Controls.Add(this.cmbSubfolders);
            this.Controls.Add(this.lblSubfolder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddToDatabaseForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add to Database";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private Label lblSubfolder;
        private ComboBox cmbSubfolders;
        private Button btnToggleNewFolder;
        private Label lblNewFolder;
        private TextBox txtNewFolder;
        private Label lblCustomFile;
        private TextBox txtCustomFileName;
        private Button btnOK;
        private Button btnCancel;
    }
}
