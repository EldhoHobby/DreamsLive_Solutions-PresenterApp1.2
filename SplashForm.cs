using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DreamsLive_Solutions_PresenterApp1
{
    // Code-painted splash: dark dotted backdrop + brand logo + animated lavender bar.
    public class SplashForm : Form
    {
        private readonly Image _logo;
        private readonly Timer _timer;
        private float _phase;

        public SplashForm()
        {
            try { _logo = Properties.Resources.DreamsLiveSolutions_Logo1; }
            catch { _logo = null; }

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(600, 380);
            this.BackColor = Color.FromArgb(8, 8, 10);
            this.DoubleBuffered = true;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Text = "DreamsLive Solutions";

            _timer = new Timer { Interval = 30 };
            _timer.Tick += (s, e) =>
            {
                _phase += 0.014f;
                if (_phase > 1f) _phase -= 1f;
                Invalidate();
            };
            _timer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            var r = this.ClientRectangle;

            // Dark base.
            using (var b = new SolidBrush(Color.FromArgb(8, 8, 10)))
                g.FillRectangle(b, r);

            // Soft lavender glow behind the logo.
            using (var path = new GraphicsPath())
            {
                var glow = new Rectangle(r.Width / 2 - 280, r.Height / 2 - 210, 560, 420);
                path.AddEllipse(glow);
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = Color.FromArgb(46, 94, 106, 210);
                    pgb.SurroundColors = new[] { Color.FromArgb(0, 8, 8, 10) };
                    g.FillPath(pgb, path);
                }
            }

            // Faint dotted grid.
            const int gap = 26;
            using (var dot = new SolidBrush(Color.FromArgb(16, 150, 165, 210)))
                for (int y = gap; y < r.Height; y += gap)
                    for (int x = gap; x < r.Width; x += gap)
                        g.FillEllipse(dot, x, y, 2, 2);

            // Brand logo, centered (slightly above middle to leave room for the tagline).
            int contentBottom = r.Height / 2;
            if (_logo != null && _logo.Width > 0 && _logo.Height > 0)
            {
                int lw = (int)(r.Width * 0.64f);
                int lh = (int)Math.Round(_logo.Height * (lw / (float)_logo.Width));
                int maxH = (int)(r.Height * 0.36f);
                if (lh > maxH)
                {
                    lh = maxH;
                    lw = (int)Math.Round(_logo.Width * (lh / (float)_logo.Height));
                }
                int lx = r.X + (r.Width - lw) / 2;
                int ly = r.Y + (r.Height - lh) / 2 - 14;
                g.DrawImage(_logo, new Rectangle(lx, ly, lw, lh));
                contentBottom = ly + lh;
            }

            // Tagline.
            using (var fTag = new Font("Segoe UI", 10f, FontStyle.Regular))
            {
                const string tag = "Where Ideas Go Live.";
                var ts = TextRenderer.MeasureText(g, tag, fTag);
                TextRenderer.DrawText(g, tag, fTag,
                    new Point(r.X + (r.Width - ts.Width) / 2, contentBottom + 12),
                    Color.FromArgb(150, 160, 175), TextFormatFlags.NoPrefix);
            }

            // Indeterminate lavender progress bar along the bottom edge.
            int barH = 3, barY = r.Bottom - barH;
            using (var track = new SolidBrush(Color.FromArgb(38, 255, 255, 255)))
                g.FillRectangle(track, r.X, barY, r.Width, barH);

            int seg = Math.Max(60, (int)(r.Width * 0.32f));
            int x2 = (int)(_phase * (r.Width + seg)) - seg;
            using (var lg = new LinearGradientBrush(
                       new Rectangle(x2, barY, seg, barH),
                       Color.FromArgb(0x5E, 0x6A, 0xD2),
                       Color.FromArgb(0x82, 0x8F, 0xFF),
                       LinearGradientMode.Horizontal))
                g.FillRectangle(lg, x2, barY, seg, barH);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_timer != null) { _timer.Stop(); _timer.Dispose(); }
            base.OnFormClosed(e);
        }
    }
}
