using System;
using System.Drawing;
using System.Windows.Forms;

namespace DreamsLive_Solutions_PresenterApp1
{
    /// <summary>
    /// Design-time-visible application header bar.
    ///
    /// It paints the brand mark + "Dreams / Live Solutions" wordmark + tagline (via
    /// <see cref="LinearTheme.PaintHeader"/>) and a right-aligned "LIVE" status indicator
    /// (via <see cref="LinearTheme.PaintLiveIndicator"/>). Because it is a real control
    /// placed on MainForm, the Visual Studio designer renders it, so the offline designer
    /// view matches the running app.
    ///
    /// Set <see cref="IsLive"/> to true while the presenter window is open: the "LIVE"
    /// dot then pulses red (a smooth breathing animation). When false it shows a steady
    /// dim-gray "LIVE".
    ///
    /// It is named "linearHeader" on purpose: <see cref="LinearTheme.InstallHeader"/>
    /// early-returns when a control with that key already exists, so at runtime it will
    /// NOT inject a second header (and will not run its 56px control shift). The 56px of
    /// headroom is instead baked statically into MainForm.Designer.cs.
    /// </summary>
    public class LinearHeaderControl : Panel
    {
        public const int HeaderHeight = 56;

        private bool _isLive;
        private double _phase;
        private float _pulse;
        private readonly Timer _pulseTimer;

        public LinearHeaderControl()
        {
            Name = "linearHeader";
            Dock = DockStyle.Top;
            Height = HeaderHeight;
            Tag = "chrome";
            BackColor = LinearTheme.Current.Surface1;
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw,
                true);

            // ~25 fps breathing animation, only running while live.
            _pulseTimer = new Timer { Interval = 40 };
            _pulseTimer.Tick += PulseTimer_Tick;
        }

        /// <summary>
        /// True when the presenter window is open/live. Drives the pulsing red "LIVE"
        /// indicator; when false the indicator is a steady dim-gray "LIVE".
        /// </summary>
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

        private void PulseTimer_Tick(object sender, EventArgs e)
        {
            _phase += 0.13;
            _pulse = (float)((Math.Sin(_phase) + 1.0) / 2.0);

            // Only the right-hand indicator region needs repainting each frame.
            int w = Math.Min(Width, 280);
            Invalidate(new Rectangle(Width - w, 0, w, Height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                LinearTheme.PaintHeader(
                    e.Graphics, ClientRectangle,
                    "Dreams", "Live Solutions", "Where Ideas Go Live.");
                LinearTheme.PaintLiveIndicator(e.Graphics, ClientRectangle, _isLive, _pulse);
            }
            catch
            {
                // Never let a paint failure break the designer surface or the app.
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
