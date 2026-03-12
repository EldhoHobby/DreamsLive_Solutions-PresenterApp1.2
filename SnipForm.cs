using System;
using System.Drawing;
using System.Windows.Forms;

namespace DreamsLive_Solutions_PresenterApp1
{
    public partial class SnipForm : Form
    {
        private Bitmap fullScreenCapture;
        private Point startPos;
        private Rectangle snipRectangle;
        private bool isSelecting = false;

        public Bitmap SnippedImage { get; private set; }

        public SnipForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Cursor = Cursors.Cross;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.ShowInTaskbar = false;
        }

        private void SnipForm_Load(object sender, EventArgs e)
        {
            // Capture all screens
            Rectangle bounds = SystemInformation.VirtualScreen;
            fullScreenCapture = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(fullScreenCapture))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            }
            this.BackgroundImage = fullScreenCapture;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Dim the background
            using (Brush dimBrush = new SolidBrush(Color.FromArgb(120, Color.Black)))
            {
                e.Graphics.FillRectangle(dimBrush, this.ClientRectangle);
            }

            if (isSelecting && snipRectangle.Width > 0 && snipRectangle.Height > 0)
            {
                // Clear the selection area
                e.Graphics.SetClip(snipRectangle);
                e.Graphics.DrawImage(fullScreenCapture, 0, 0);
                e.Graphics.ResetClip();

                // Draw border
                using (Pen p = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(p, snipRectangle);
                }
            }
        }

        private void SnipForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isSelecting = true;
                startPos = e.Location;
                snipRectangle = new Rectangle(e.Location, Size.Empty);
            }
            else if (e.Button == MouseButtons.Right)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void SnipForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                int x = Math.Min(startPos.X, e.X);
                int y = Math.Min(startPos.Y, e.Y);
                int w = Math.Abs(startPos.X - e.X);
                int h = Math.Abs(startPos.Y - e.Y);
                snipRectangle = new Rectangle(x, y, w, h);
                this.Invalidate();
            }
        }

        private void SnipForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                isSelecting = false;
                if (snipRectangle.Width > 0 && snipRectangle.Height > 0)
                {
                    SnippedImage = new Bitmap(snipRectangle.Width, snipRectangle.Height);
                    using (Graphics g = Graphics.FromImage(SnippedImage))
                    {
                        g.DrawImage(fullScreenCapture, new Rectangle(0, 0, SnippedImage.Width, SnippedImage.Height), snipRectangle, GraphicsUnit.Pixel);
                    }
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        private void SnipForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            fullScreenCapture?.Dispose();
        }
    }
}
