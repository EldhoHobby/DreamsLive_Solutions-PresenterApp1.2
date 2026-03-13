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
            Color backColor = Color.FromArgb(28, 28, 28);
            Color titleBarColor = Color.FromArgb(20, 20, 20);
            Color foreColor = Color.FromArgb(240, 240, 240);
            Color accentColor = Color.FromArgb(0, 120, 215);

            this.BackColor = backColor;
            this.ForeColor = foreColor;
            pnlTitleBar.BackColor = titleBarColor;
            lblFormTitle.ForeColor = foreColor;
            btnAppClose.BackColor = titleBarColor;
            btnAppClose.FlatAppearance.MouseOverBackColor = Color.Red;

            foreach (Control c in this.Controls)
            {
                if (c == pnlTitleBar) continue;
                c.ForeColor = foreColor;
                if (c is Button btn)
                {
                    btn.BackColor = Color.FromArgb(45, 45, 48);
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    if (btn == btnSave)
                    {
                        btn.BackColor = accentColor;
                    }
                    ModernUIHelper.ApplyRoundedCorners(btn, 6);
                }
            }
            ModernUIHelper.ApplyRoundedCorners(this, 15);
        }

        private void pnlTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ModernUIHelper.DragForm(this.Handle);
            }
        }

        private void btnAppClose_Click(object sender, EventArgs e)
        {
            this.Close();
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
