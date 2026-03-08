using System;
using System.Drawing;
using System.Windows.Forms;

namespace DreamsLive_Solutions_PresenterApp1
{
    public partial class SettingsForm : Form
    {
        private readonly MainForm _mainForm;

        public SettingsForm(MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();

            chkSkipOnePage.Checked = _mainForm.SkipOnePage;
            chkTwoPagePdf.Checked = _mainForm.TwoPagePdf;

            ApplyTheme();
        }

        private void ApplyTheme()
        {
            // Simple theme application or match MainForm's logic if needed
            // For now, standard colors
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;
            foreach (Control c in this.Controls)
            {
                c.ForeColor = Color.White;
                if (c is Button btn)
                {
                    btn.BackColor = Color.FromArgb(63, 63, 70);
                    btn.FlatStyle = FlatStyle.Flat;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _mainForm.SkipOnePage = chkSkipOnePage.Checked;
            _mainForm.TwoPagePdf = chkTwoPagePdf.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
