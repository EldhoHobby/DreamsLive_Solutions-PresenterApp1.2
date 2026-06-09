using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace DreamsLive_Solutions_PresenterApp1
{
    // Modern code-painted splash: embedded brand artwork + animated lavender bar.
    public class SplashForm : Form
    {
        private readonly Image _bg;
        private readonly Timer _timer;
        private float _phase;

        public SplashForm()
        {
            _bg = LoadAsset("Resources.splash_bg.png");

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = _bg != null ? _bg.Size : new Size(600, 423);
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

        private static Image LoadAsset(string endsWith)
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                foreach (var n in asm.GetManifestResourceNames())
                    if (n.EndsWith(endsWith, StringComparison.OrdinalIgnoreCase))
                        using (var st = asm.GetManifestResourceStream(n))
                            if (st != null) return Image.FromStream(st);
            }
            catch { }
            return null;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            var r = this.ClientRectangle;

            if (_bg != null)
                g.DrawImage(_bg, r);
            else
                using (var b = new SolidBrush(Color.FromArgb(8, 8, 10)))
                    g.FillRectangle(b, r);

            // Indeterminate lavender progress bar along the bottom edge.
            int barH = 3, barY = r.Bottom - barH;
            using (var track = new SolidBrush(Color.FromArgb(38, 255, 255, 255)))
                g.FillRectangle(track, r.X, barY, r.Width, barH);

            int seg = Math.Max(60, (int)(r.Width * 0.32f));
            int x = (int)(_phase * (r.Width + seg)) - seg;
            using (var lg = new LinearGradientBrush(
                       new Rectangle(x, barY, seg, barH),
                       Color.FromArgb(0x5E, 0x6A, 0xD2),
                       Color.FromArgb(0x82, 0x8F, 0xFF),
                       LinearGradientMode.Horizontal))
                g.FillRectangle(lg, x, barY, seg, barH);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_timer != null) { _timer.Stop(); _timer.Dispose(); }
            base.OnFormClosed(e);
        }
    }
}
