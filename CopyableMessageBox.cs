using System;
using System.Drawing;
using System.Windows.Forms;

namespace DreamsLive_Solutions_PresenterApp1
{
    public partial class CopyableMessageBox : Form
    {
        private CopyableMessageBox(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            InitializeComponent();
            this.Text = title;
            this.txtMessage.Text = message;
            SetupButtons(buttons);
            SetupIcon(icon);
            // Theme AFTER the buttons are created so LinearTheme styles them too. If applied
            // first, the dynamically-added buttons keep default colors and inherit the form's
            // light ForeColor over a light visual-style button — making the text unreadable
            // (invisible) in dark mode. Theming here gives them readable ink-on-surface text.
            LinearTheme.Apply(this);
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
