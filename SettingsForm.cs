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
            this.Icon = LinearTheme.FormIcon("settings"); // title-bar glyph matching this dialog's purpose

            chkSkipOnePage.Checked = _mainForm.SkipOnePage;
            chkTwoPagePdf.Checked = _mainForm.TwoPagePdf;

            ApplyTheme();
        }

        private void ApplyTheme()
        {
            LinearTheme.SetMode(_mainForm != null ? _mainForm.IsDarkMode : LinearTheme.IsDark);
            LinearTheme.Apply(this);
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
