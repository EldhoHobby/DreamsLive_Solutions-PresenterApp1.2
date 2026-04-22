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

            PopulateScreens();
            ApplyTheme();
        }

        private void PopulateScreens()
        {
            cmbAudienceScreen.Items.Clear();
            cmbNotesScreen.Items.Clear();

            Screen[] allScreens = Screen.AllScreens;
            for (int i = 0; i < allScreens.Length; i++)
            {
                string desc = $"Display {i + 1} ({allScreens[i].Bounds.Width}x{allScreens[i].Bounds.Height})";
                if (allScreens[i].Primary) desc += " [Primary]";

                cmbAudienceScreen.Items.Add(desc);
                cmbNotesScreen.Items.Add(desc);
            }

            if (allScreens.Length > _mainForm.AudienceScreenIndex)
                cmbAudienceScreen.SelectedIndex = _mainForm.AudienceScreenIndex;
            else if (allScreens.Length > 0)
                cmbAudienceScreen.SelectedIndex = 0;

            if (allScreens.Length > _mainForm.NotesScreenIndex)
                cmbNotesScreen.SelectedIndex = _mainForm.NotesScreenIndex;
            else if (allScreens.Length > 0)
                cmbNotesScreen.SelectedIndex = 0;
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
            _mainForm.AudienceScreenIndex = cmbAudienceScreen.SelectedIndex;
            _mainForm.NotesScreenIndex = cmbNotesScreen.SelectedIndex;
            _mainForm.SaveSettings();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnResyncDisplays_Click(object sender, EventArgs e)
        {
            PopulateScreens();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
