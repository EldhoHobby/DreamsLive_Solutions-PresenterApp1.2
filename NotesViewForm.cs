using System;
using System.Drawing;
using System.Windows.Forms;

namespace DreamsLive_Solutions_PresenterApp1
{
    public class NotesViewForm : Form
    {
        private RichTextBox _rtbNotes;
        private Panel _pnlContainer;

        public NotesViewForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this._pnlContainer = new Panel();
            this._rtbNotes = new RichTextBox();
            this.SuspendLayout();

            // Panel Container
            this._pnlContainer.Dock = DockStyle.Fill;
            this._pnlContainer.BackColor = Color.Black;
            this._pnlContainer.Padding = new Padding(40);
            this._pnlContainer.Controls.Add(this._rtbNotes);

            // RichTextBox Notes
            this._rtbNotes.BackColor = Color.Black;
            this._rtbNotes.BorderStyle = BorderStyle.None;
            this._rtbNotes.Dock = DockStyle.Fill;
            this._rtbNotes.ForeColor = Color.Yellow;
            this._rtbNotes.ReadOnly = true;
            this._rtbNotes.ScrollBars = RichTextBoxScrollBars.Vertical;
            this._rtbNotes.Text = "";
            this._rtbNotes.Font = new Font("Segoe UI", 36F, FontStyle.Regular);

            // NotesViewForm
            this.BackColor = Color.Black;
            this.ClientSize = new Size(800, 600);
            this.Controls.Add(this._pnlContainer);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "NotesViewForm";
            this.Text = "Presenter Notes";
            this.TopMost = true;
            this.ShowInTaskbar = false;

            this.ResumeLayout(false);
        }

        public void UpdateNotes(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateNotes(text)));
                return;
            }

            _rtbNotes.Text = text;
            UpdateFontSize();
        }

        private void UpdateFontSize()
        {
            string text = _rtbNotes.Text;
            if (string.IsNullOrWhiteSpace(text)) return;

            // Logic for dynamic scaling:
            // Small text: boost font to 72pt+
            // Big writeup: scale down to 24pt

            float fontSize = 72f;

            if (text.Length > 500) fontSize = 24f;
            else if (text.Length > 200) fontSize = 36f;
            else if (text.Length > 100) fontSize = 48f;
            else if (text.Length > 50) fontSize = 60f;

            _rtbNotes.Font = new Font("Segoe UI", fontSize, FontStyle.Regular);
            _rtbNotes.SelectionStart = 0;
            _rtbNotes.ScrollToCaret();
        }

        public void AdjustFontSize(int delta)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => AdjustFontSize(delta)));
                return;
            }

            float newSize = _rtbNotes.Font.Size + delta;
            if (newSize < 12) newSize = 12;
            if (newSize > 120) newSize = 120;

            _rtbNotes.Font = new Font(_rtbNotes.Font.FontFamily, newSize, _rtbNotes.Font.Style);
        }

        public void ScrollNotes(int direction)
        {
             if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ScrollNotes(direction)));
                return;
            }

             // direction: 1 = down, -1 = up
             // Using SendMessage for smooth scroll if needed, but for now simple scroll
             const int WM_VSCROLL = 0x115;
             const int SB_LINEUP = 0;
             const int SB_LINEDOWN = 1;

             IntPtr msg = (direction > 0) ? (IntPtr)SB_LINEDOWN : (IntPtr)SB_LINEUP;
             NativeMethods.SendMessage(_rtbNotes.Handle, WM_VSCROLL, msg, IntPtr.Zero);
        }
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
