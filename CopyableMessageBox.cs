using System;
using System.Drawing;
using System.Windows.Forms;

namespace DreamsLive_Solutions_PresenterApp1
{
    public partial class CopyableMessageBox : Form
    {
        private Panel pnlTitleBar;
        private Label lblFormTitle;
        private Button btnAppClose;

        private CopyableMessageBox(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            InitializeComponent();
            this.lblFormTitle.Text = title;
            this.txtMessage.Text = message;
            SetupButtons(buttons);
            SetupIcon(icon);
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

            txtMessage.BackColor = backColor;
            txtMessage.ForeColor = foreColor;

            foreach (Control c in flpButtons.Controls)
            {
                if (c is Button btn)
                {
                    btn.BackColor = Color.FromArgb(45, 45, 48);
                    btn.ForeColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 60);

                    // Highlight positive actions
                    if (btn.DialogResult == DialogResult.OK || btn.DialogResult == DialogResult.Yes)
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

        protected override void WndProc(ref Message m)
        {
            ModernUIHelper.HandleResize(ref m, this);
            base.WndProc(ref m);
        }

        public static DialogResult Show(string message) => Show(null, message, "", MessageBoxButtons.OK, MessageBoxIcon.None);
        public static DialogResult Show(string message, string title) => Show(null, message, title, MessageBoxButtons.OK, MessageBoxIcon.None);
        public static DialogResult Show(string message, string title, MessageBoxButtons buttons) => Show(null, message, title, buttons, MessageBoxIcon.None);
        public static DialogResult Show(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon) => Show(null, message, title, buttons, icon);

        public static DialogResult Show(IWin32Window owner, string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            // If owner is null, try to find the MainForm to ensure it shows on the correct monitor
            if (owner == null)
            {
                foreach (Form openForm in Application.OpenForms)
                {
                    if (openForm is MainForm)
                    {
                        owner = openForm;
                        break;
                    }
                }
            }

            using (var form = new CopyableMessageBox(message, title, buttons, icon))
            {
                // Always set TopMost to true as requested, to stay above even other TopMost windows
                form.TopMost = true;

                // Set start position based on owner availability
                if (owner != null)
                {
                    form.StartPosition = FormStartPosition.CenterParent;
                }
                else
                {
                    form.StartPosition = FormStartPosition.CenterScreen;
                }

                using (Graphics g = form.CreateGraphics())
                {
                    SizeF size = g.MeasureString(message, form.txtMessage.Font, form.txtMessage.Width);
                    int newHeight = (int)Math.Ceiling(size.Height) + form.flpButtons.Height + 40;
                    if (newHeight > form.Height)
                    {
                        form.Height = Math.Min(newHeight, Screen.FromControl(form).WorkingArea.Height - 100);
                    }
                }
                return owner == null ? form.ShowDialog() : form.ShowDialog(owner);
            }
        }
    }
}
