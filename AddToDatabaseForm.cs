using System;
using System.Collections.Generic;
using System.IO;
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
    }
}
