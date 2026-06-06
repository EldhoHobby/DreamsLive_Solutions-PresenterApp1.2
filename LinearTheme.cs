using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DreamsLive_Solutions_PresenterApp1
{
    /// <summary>
    /// Linear-inspired theming engine. Re-skins stock WinForms controls at runtime
    /// (dark canvas + lavender accent + hairline surfaces + rounded buttons) without
    /// requiring any changes to the Designer files.
    /// </summary>
    public static class LinearTheme
    {
        // ---- Palette ----------------------------------------------------------
        public sealed class Palette
        {
            public bool IsDark;
            public Color Canvas;        // page background
            public Color Surface1;      // cards / inputs
            public Color Surface2;      // featured / hovered
            public Color Surface3;      // sub-nav / menus
            public Color Hairline;      // 1px borders
            public Color HairlineStrong;
            public Color Ink;           // primary text
            public Color InkMuted;      // secondary text
            public Color InkSubtle;     // tertiary text
            public Color Primary;       // lavender accent
            public Color PrimaryHover;
            public Color PrimaryFocus;
            public Color OnPrimary;     // text on lavender
            public Color Success;
            public Color Danger;
        }

        // Linear marketing dark surface (canvas #010102) + lavender #5e6ad2.
        public static readonly Palette Dark = new Palette
        {
            IsDark = true,
            Canvas = Color.FromArgb(0x08, 0x08, 0x0A),     // near-black with faint blue tint
            Surface1 = Color.FromArgb(0x14, 0x15, 0x18),
            Surface2 = Color.FromArgb(0x1B, 0x1C, 0x20),
            Surface3 = Color.FromArgb(0x23, 0x25, 0x2A),
            Hairline = Color.FromArgb(0x23, 0x25, 0x2A),
            HairlineStrong = Color.FromArgb(0x33, 0x36, 0x3D),
            Ink = Color.FromArgb(0xF7, 0xF8, 0xF8),
            InkMuted = Color.FromArgb(0xD0, 0xD6, 0xE0),
            InkSubtle = Color.FromArgb(0x8A, 0x8F, 0x98),
            Primary = Color.FromArgb(0x5E, 0x6A, 0xD2),
            PrimaryHover = Color.FromArgb(0x82, 0x8F, 0xFF),
            PrimaryFocus = Color.FromArgb(0x5E, 0x69, 0xD1),
            OnPrimary = Color.White,
            Success = Color.FromArgb(0x27, 0xA6, 0x44),
            Danger = Color.FromArgb(0xD2, 0x5E, 0x5E)
        };

        // Complementary light theme (same lavender accent on a near-white canvas).
        public static readonly Palette Light = new Palette
        {
            IsDark = false,
            Canvas = Color.FromArgb(0xFB, 0xFB, 0xFC),
            Surface1 = Color.FromArgb(0xFF, 0xFF, 0xFF),
            Surface2 = Color.FromArgb(0xF2, 0xF3, 0xF6),
            Surface3 = Color.FromArgb(0xEA, 0xEC, 0xF0),
            Hairline = Color.FromArgb(0xE3, 0xE4, 0xE8),
            HairlineStrong = Color.FromArgb(0xCE, 0xD0, 0xD7),
            Ink = Color.FromArgb(0x1C, 0x1D, 0x21),
            InkMuted = Color.FromArgb(0x44, 0x47, 0x4F),
            InkSubtle = Color.FromArgb(0x6E, 0x72, 0x7C),
            Primary = Color.FromArgb(0x5E, 0x6A, 0xD2),
            PrimaryHover = Color.FromArgb(0x4C, 0x57, 0xC0),
            PrimaryFocus = Color.FromArgb(0x5E, 0x69, 0xD1),
            OnPrimary = Color.White,
            Success = Color.FromArgb(0x1F, 0x9D, 0x3A),
            Danger = Color.FromArgb(0xC0, 0x3A, 0x3A)
        };

        public static Palette Current { get; private set; } = Dark;
        public static bool IsDark => Current.IsDark;

        public static void SetMode(bool dark) => Current = dark ? Dark : Light;

        // Buttons that carry the scarce lavender accent (primary CTAs).
        public static readonly HashSet<string> PrimaryButtonNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "btnPushToPresenter", "btnStageContent"
        };

        // Buttons whose colors are owned by application state logic — skip role colors,
        // but still owner-draw them (they read their own BackColor/ForeColor).
        public static readonly HashSet<string> StateOwnedButtonNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "btnClearPresenterDisplay", "btnHighlighter"
        };

        public const int ButtonRadius = 8;
        public const int PanelRadius = 12;
        public const int InputRadius = 6;

        // ---- Font resolution --------------------------------------------------
        private static string _family;
        private static readonly object _familyLock = new object();

        public static string FontFamily
        {
            get
            {
                if (_family != null) return _family;
                lock (_familyLock)
                {
                    if (_family != null) return _family;
                    string[] prefs = { "Inter", "Segoe UI Variable Text", "Segoe UI", "SF Pro Display" };
                    var installed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    using (var ifc = new InstalledFontCollection())
                        foreach (var f in ifc.Families) installed.Add(f.Name);
                    _family = prefs.FirstOrDefault(installed.Contains) ?? "Segoe UI";
                }
                return _family;
            }
        }

        private static Font Resolve(Font src)
        {
            if (src == null) return null;
            if (string.Equals(src.FontFamily.Name, FontFamily, StringComparison.OrdinalIgnoreCase))
                return src;
            try { return new Font(FontFamily, src.Size, src.Style, src.Unit); }
            catch { return src; }
        }

        // ---- Hover / press tracking ------------------------------------------
        private static readonly HashSet<Control> _hovered = new HashSet<Control>();
        private static readonly HashSet<Control> _pressed = new HashSet<Control>();
        private static readonly HashSet<Control> _wired = new HashSet<Control>();

        // ---- Public entry points ---------------------------------------------
        public static void Apply(Form form)
        {
            if (form == null) return;
            form.SuspendLayout();
            form.BackColor = Current.Canvas;
            form.ForeColor = Current.Ink;
            try { form.Font = Resolve(form.Font); } catch { }
            foreach (Control c in form.Controls) Style(c);
            UseDarkTitleBar(form, Current.IsDark);
            form.ResumeLayout();
            form.Invalidate(true);
        }

        public static void ApplyToContainer(Control root)
        {
            if (root == null) return;
            foreach (Control c in root.Controls) Style(c);
        }

        private static void Style(Control c)
        {
            try { c.Font = Resolve(c.Font); } catch { }

            switch (c)
            {
                case Button btn:
                    StyleButton(btn);
                    break;
                case CheckBox chk:
                    chk.BackColor = c.Parent != null ? c.Parent.BackColor : Current.Canvas;
                    chk.ForeColor = Current.Ink;
                    chk.FlatStyle = FlatStyle.Flat;
                    chk.FlatAppearance.BorderColor = Current.Hairline;
                    break;
                case RadioButton rb:
                    rb.BackColor = c.Parent != null ? c.Parent.BackColor : Current.Canvas;
                    rb.ForeColor = Current.Ink;
                    break;
                case TextBox tb:
                    tb.BackColor = Current.Surface1;
                    tb.ForeColor = Current.Ink;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case ComboBox cb:
                    cb.BackColor = Current.Surface1;
                    cb.ForeColor = Current.Ink;
                    cb.FlatStyle = FlatStyle.Flat;
                    break;
                case ListBox lb:
                    lb.BackColor = Current.Surface1;
                    lb.ForeColor = Current.Ink;
                    lb.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case LinkLabel link:
                    link.BackColor = Color.Transparent;
                    link.LinkColor = Current.Primary;
                    link.ActiveLinkColor = Current.PrimaryHover;
                    link.VisitedLinkColor = Current.Primary;
                    break;
                case Label lbl:
                    lbl.BackColor = Color.Transparent;
                    lbl.ForeColor = Current.Ink;
                    break;
                case Panel pnl:
                    // Leave PictureBox-wrapping / preview panels alone if tagged "preview".
                    if (!IsPreviewSurface(pnl))
                        pnl.BackColor = Current.Canvas;
                    break;
                case GroupBox grp:
                    grp.BackColor = c.Parent != null ? c.Parent.BackColor : Current.Canvas;
                    grp.ForeColor = Current.InkMuted;
                    break;
                case PictureBox pic:
                    if (pic.Image == null)
                        pic.BackColor = Current.Surface1;
                    break;
                case NumericUpDown num:
                    num.BackColor = Current.Surface1;
                    num.ForeColor = Current.Ink;
                    break;
                case MenuStrip ms:
                    ms.BackColor = Current.Surface3;
                    ms.ForeColor = Current.Ink;
                    break;
                default:
                    // Generic containers / unknown controls inherit canvas.
                    if (c.HasChildren) c.BackColor = Current.Canvas;
                    break;
            }

            if (c.HasChildren)
                foreach (Control child in c.Controls) Style(child);
        }

        private static bool IsPreviewSurface(Control c)
        {
            return c.Tag is string s && s.IndexOf("preview", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // ---- Button owner-draw ------------------------------------------------
        private static void StyleButton(Button btn)
        {
            EnableDoubleBuffer(btn);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btn.UseVisualStyleBackColor = false;

            // Assign role colors unless the app owns this button's state colors.
            if (!StateOwnedButtonNames.Contains(btn.Name))
            {
                if (PrimaryButtonNames.Contains(btn.Name))
                {
                    btn.BackColor = Current.Primary;
                    btn.ForeColor = Current.OnPrimary;
                }
                else
                {
                    btn.BackColor = Current.Surface1;
                    btn.ForeColor = Current.Ink;
                }
            }

            if (_wired.Add(btn))
            {
                btn.Paint += Button_Paint;
                btn.MouseEnter += (s, e) => { _hovered.Add(btn); btn.Invalidate(); };
                btn.MouseLeave += (s, e) => { _hovered.Remove(btn); _pressed.Remove(btn); btn.Invalidate(); };
                btn.MouseDown += (s, e) => { _pressed.Add(btn); btn.Invalidate(); };
                btn.MouseUp += (s, e) => { _pressed.Remove(btn); btn.Invalidate(); };
                btn.EnabledChanged += (s, e) => btn.Invalidate();
                btn.BackColorChanged += (s, e) => btn.Invalidate();
            }
            btn.Invalidate();
        }

        private static void Button_Paint(object sender, PaintEventArgs e)
        {
            var btn = (Button)sender;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = btn.ClientRectangle;
            Color parentBg = btn.Parent != null ? btn.Parent.BackColor : Current.Canvas;

            bool isPrimary = PrimaryButtonNames.Contains(btn.Name);
            bool hovered = _hovered.Contains(btn);
            bool pressed = _pressed.Contains(btn);

            Color fill = btn.BackColor;
            Color text = btn.ForeColor;
            Color border = isPrimary ? fill : Current.Hairline;

            if (!btn.Enabled)
            {
                fill = Blend(fill, parentBg, 0.55f);
                text = Current.InkSubtle;
                border = Current.Hairline;
            }
            else if (pressed)
            {
                fill = isPrimary ? Current.PrimaryFocus : Blend(fill, Current.Ink, 0.08f);
            }
            else if (hovered)
            {
                fill = isPrimary ? Current.PrimaryHover : Blend(fill, Current.Ink, 0.06f);
                if (!isPrimary) border = Current.HairlineStrong;
            }

            // Erase the square corners with the parent background.
            using (var pb = new SolidBrush(parentBg))
                g.FillRectangle(pb, rect);

            var r = rect;
            r.Width -= 1; r.Height -= 1;
            using (var path = RoundedRect(r, ButtonRadius))
            {
                using (var fb = new SolidBrush(fill)) g.FillPath(fb, path);
                using (var pen = new Pen(border, 1f)) g.DrawPath(pen, path);
            }

            // Focus ring (keyboard focus) — 1px lavender inset.
            if (btn.Focused && btn.Enabled)
            {
                var fr = rect; fr.Inflate(-2, -2);
                using (var path = RoundedRect(fr, ButtonRadius - 2))
                using (var pen = new Pen(Current.PrimaryFocus, 1.5f))
                    g.DrawPath(pen, path);
            }

            TextRenderer.DrawText(g, btn.Text, btn.Font, rect, text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        // ---- Helpers ----------------------------------------------------------
        public static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = Math.Max(1, radius * 2);
            d = Math.Min(d, Math.Min(r.Width, r.Height));
            var path = new GraphicsPath();
            if (d <= 1)
            {
                path.AddRectangle(r);
                path.CloseFigure();
                return path;
            }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static Color Blend(Color a, Color b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }

        private static void EnableDoubleBuffer(Control c)
        {
            try
            {
                typeof(Control)
                    .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.SetValue(c, true, null);
                typeof(Control)
                    .GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(c, new object[]
                    {
                        ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                        ControlStyles.UserPaint | ControlStyles.ResizeRedraw,
                        true
                    });
            }
            catch { /* best effort */ }
        }

        // ---- Dark title bar (Windows 10 1809+) -------------------------------
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public static void UseDarkTitleBar(Form form, bool dark)
        {
            if (form == null || !form.IsHandleCreated) { TryDeferTitleBar(form, dark); return; }
            try
            {
                int useImmersiveDark = dark ? 1 : 0;
                // 20 = DWMWA_USE_IMMERSIVE_DARK_MODE (1903+); 19 = pre-1903 build.
                if (DwmSetWindowAttribute(form.Handle, 20, ref useImmersiveDark, sizeof(int)) != 0)
                    DwmSetWindowAttribute(form.Handle, 19, ref useImmersiveDark, sizeof(int));
            }
            catch { /* unsupported OS — ignore */ }
        }

        private static void TryDeferTitleBar(Form form, bool dark)
        {
            if (form == null) return;
            void Handler(object s, EventArgs e)
            {
                form.HandleCreated -= Handler;
                UseDarkTitleBar(form, dark);
            }
            form.HandleCreated += Handler;
        }
    }
}
