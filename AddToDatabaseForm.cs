using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DreamsLive_Solutions_PresenterApp1
{
    public partial class AddToDatabaseForm : Form
    {
        private readonly List<string> _existingSubfolders;
        public string SelectedSubfolder { get; private set; }
        public string NewSubfolderName { get; private set; }
        public string CustomFileName { get; private set; }

        public AddToDatabaseForm(List<string> existingSubfolders, string currentFileName)
        {
            InitializeComponent();
            _existingSubfolders = existingSubfolders;
            txtCustomFileName.Text = Path.GetFileNameWithoutExtension(currentFileName);

            cmbSubfolders.Items.Add("(Root)");
            foreach (var folder in _existingSubfolders) cmbSubfolders.Items.Add(folder);
            cmbSubfolders.SelectedIndex = 0;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SelectedSubfolder = cmbSubfolders.SelectedItem?.ToString() == "(Root)" ? "" : cmbSubfolders.SelectedItem?.ToString();
            NewSubfolderName = txtNewFolder.Text.Trim();
            CustomFileName = txtCustomFileName.Text.Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnToggleNewFolder_Click(object sender, EventArgs e)
        {
            lblNewFolder.Visible = !lblNewFolder.Visible;
            txtNewFolder.Visible = !txtNewFolder.Visible;
            if (txtNewFolder.Visible)
            {
                btnToggleNewFolder.Text = "Cancel New Folder";
                txtNewFolder.Focus();
            }
            else
            {
                btnToggleNewFolder.Text = "+ New Folder";
                txtNewFolder.Clear();
            }
        }

        // Minimal Designer-like initialization
        private void InitializeComponent()
        {
            this.lblSubfolder = new Label { Text = "Target Subfolder:", Location = new Point(12, 15), AutoSize = true };
            this.cmbSubfolders = new ComboBox { Location = new Point(12, 35), Size = new Size(260, 21), DropDownStyle = ComboBoxStyle.DropDownList };
            this.btnToggleNewFolder = new Button { Text = "+ New Folder", Location = new Point(12, 65), Size = new Size(120, 25) };
            this.lblNewFolder = new Label { Text = "New Folder Name:", Location = new Point(12, 100), AutoSize = true, Visible = false };
            this.txtNewFolder = new TextBox { Location = new Point(12, 120), Size = new Size(260, 20), Visible = false };
            this.lblCustomFile = new Label { Text = "Custom Filename (optional):", Location = new Point(12, 150), AutoSize = true };
            this.txtCustomFileName = new TextBox { Location = new Point(12, 170), Size = new Size(260, 20) };
            this.btnOK = new Button { Text = "Add Now", Location = new Point(116, 210), Size = new Size(75, 25) };
            this.btnCancel = new Button { Text = "Cancel", Location = new Point(197, 210), Size = new Size(75, 25) };

            this.ClientSize = new Size(284, 250);
            this.Controls.AddRange(new Control[] { lblSubfolder, cmbSubfolders, btnToggleNewFolder, lblNewFolder, txtNewFolder, lblCustomFile, txtCustomFileName, btnOK, btnCancel });
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Add to Database";

            this.btnOK.Click += btnOK_Click;
            this.btnCancel.Click += btnCancel_Click;
            this.btnToggleNewFolder.Click += btnToggleNewFolder_Click;
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
