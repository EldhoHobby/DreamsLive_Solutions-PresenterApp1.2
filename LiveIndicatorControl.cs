using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DreamsLive_Solutions_PresenterApp1
{
    /// <summary>
    /// Small rounded "LIVE" chip shown in the corner of the Program/Live preview.
    /// Steady dim-gray when idle; pulses red (smooth breathing) when <see cref="IsLive"/>
    /// is true. Reuses <see cref="LinearTheme.PaintLiveIndicator"/> for the pill.
    /// </summary>
    public class LiveIndicatorControl : Panel
    {
        private bool _isLive;
        private double _phase;
        private float _pulse;
        private readonly Timer _pulseTimer;

        public LiveIndicatorControl()
        {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw,
                true);
            Size = new Size(94, 28);
            BackColor = Color.FromArgb(16, 17, 20);

            _pulseTimer = new Timer { Interval = 40 };
            _pulseTimer.Tick += (s, e) =>
            {
                _phase += 0.13;
                _pulse = (float)((Math.Sin(_phase) + 1.0) / 2.0);
                Invalidate();
            };
        }

        public bool IsLive
        {
            get { return _isLive; }
            set
            {
                if (_isLive == value) return;
                _isLive = value;
                if (_isLive)
                {
                    _phase = 0;
                    _pulse = 1f;
                    if (!DesignMode) _pulseTimer.Start();
                }
                else
                {
                    _pulseTimer.Stop();
                    _pulse = 0f;
                }
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            try
            {
                using (var p = LinearTheme.RoundedRect(new Rectangle(0, 0, Width, Height), Height / 2))
                    this.Region = new Region(p);
            }
            catch { }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(Color.FromArgb(214, 14, 15, 18)))
                    g.FillRectangle(b, ClientRectangle);
                LinearTheme.PaintLiveIndicator(g, ClientRectangle, _isLive, _pulse);
            }
            catch
            {
                base.OnPaint(e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _pulseTimer != null)
            {
                _pulseTimer.Stop();
                _pulseTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
