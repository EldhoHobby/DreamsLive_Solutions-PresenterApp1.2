using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
            var hdr = form.Controls["linearHeader"]; if (hdr != null) hdr.Invalidate();
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
                    if (string.Equals(pic.Name, "picPreview", StringComparison.OrdinalIgnoreCase))
                        AttachPreviewFrame(pic);
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

            DrawButtonContent(g, btn, rect, text);
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

        // ---- Icon + brand graphics -------------------------------------------
        private static readonly Dictionary<string, Image> _imgCache =
            new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        public static readonly Dictionary<string, string> ButtonIcons =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "btnStageContent", "stage" }, { "btnPushToPresenter", "push" },
            { "tnBrowse", "browse" }, { "btnOpenGallery", "gallery" },
            { "btnSettings", "settings" }, { "btnEditContent", "edit" },
            { "btnSnip", "snip" }, { "btnHighlighter", "highlighter" },
            { "btnClearPresenterDisplay", "blackout" }, { "btnCloseLivePresenter", "close" },
            { "btnToggleTheme", "theme" }, { "btnHelp", "help" },
            { "btnAddToDatabase", "adddb" }, { "btnSetDatabaseFolder", "setfolder" },
            { "btnPrevPage", "prev" }, { "btnNextPage", "next" },
            { "btnUp", "up" }, { "btnDown", "down" }, { "btnLeft", "left" }, { "btnRight", "right" },
        };

        private static Image LoadEmbedded(string endsWith)
        {
            if (_imgCache.TryGetValue(endsWith, out var cached)) return cached;
            Image img = null;
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                var name = asm.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith(endsWith, StringComparison.OrdinalIgnoreCase));
                if (name != null)
                    using (var s = asm.GetManifestResourceStream(name))
                        if (s != null) img = Image.FromStream(s);
            }
            catch { img = null; }
            _imgCache[endsWith] = img;
            return img;
        }

        private static Image LoadIcon(string iconName)
        {
            return string.IsNullOrEmpty(iconName) ? null : LoadEmbedded("Resources.icons." + iconName + ".png");
        }

        public static Image BrandMark => LoadEmbedded("Resources.logo_mark.png");

        // Full brand wordmark (transparent PNG) used in the header bar and splash.
        private static Image _brandLogo;
        public static Image BrandLogo
        {
            get
            {
                if (_brandLogo != null) return _brandLogo;
                try { _brandLogo = Properties.Resources.DreamsLiveSolutions_Logo1; }
                catch { _brandLogo = null; }
                return _brandLogo;
            }
        }

        private static void DrawTintedImage(Graphics g, Image img, Rectangle dest, Color color)
        {
            if (img == null) return;
            var cm = new ColorMatrix(new float[][]
            {
                new float[] { 0, 0, 0, 0, 0 },
                new float[] { 0, 0, 0, 0, 0 },
                new float[] { 0, 0, 0, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { color.R / 255f, color.G / 255f, color.B / 255f, 0, 1 },
            });
            using (var ia = new ImageAttributes())
            {
                ia.SetColorMatrix(cm);
                var old = g.InterpolationMode;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, dest, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
                g.InterpolationMode = old;
            }
        }

        private static void DrawButtonContent(Graphics g, Button btn, Rectangle rect, Color text)
        {
            Image icon = (btn.Name != null && ButtonIcons.TryGetValue(btn.Name, out var ic)) ? LoadIcon(ic) : null;
            string label = btn.Text ?? "";
            bool hasText = label.Trim().Length > 1;     // single-glyph labels (arrows) -> icon only
            int iconSize = Math.Min(18, rect.Height - 8);
            if (iconSize < 10) icon = null;

            if (icon != null && hasText)
            {
                Size ts = TextRenderer.MeasureText(g, label, btn.Font,
                    new Size(rect.Width, rect.Height), TextFormatFlags.NoPrefix);
                int gap = 7;
                int groupW = iconSize + gap + ts.Width;
                int startX = rect.X + Math.Max(6, (rect.Width - groupW) / 2);
                int iconY = rect.Y + (rect.Height - iconSize) / 2;
                DrawTintedImage(g, icon, new Rectangle(startX, iconY, iconSize, iconSize), text);
                var textRect = new Rectangle(startX + iconSize + gap, rect.Y,
                    rect.Right - (startX + iconSize + gap), rect.Height);
                TextRenderer.DrawText(g, label, btn.Font, textRect, text,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
            else if (icon != null)
            {
                int iconY = rect.Y + (rect.Height - iconSize) / 2;
                int iconX = rect.X + (rect.Width - iconSize) / 2;
                DrawTintedImage(g, icon, new Rectangle(iconX, iconY, iconSize, iconSize), text);
            }
            else
            {
                TextRenderer.DrawText(g, label, btn.Font, rect, text,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }

        // ---- Elevated preview surface ----------------------------------------
        private static void AttachPreviewFrame(PictureBox pic)
        {
            if (!_wired.Add(pic)) return;
            pic.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = pic.ClientRectangle; r.Width -= 1; r.Height -= 1;
                if (r.Width < 6 || r.Height < 6) return;
                using (var path = RoundedRect(r, 10))
                using (var pen = new Pen(Current.Hairline, 1f))
                    g.DrawPath(pen, path);
                using (var hi = new Pen(Color.FromArgb(26, 255, 255, 255), 1f))
                    g.DrawLine(hi, r.X + 10, r.Y + 1, r.Right - 10, r.Y + 1);
            };
        }

        // ---- Branded header bar ----------------------------------------------
        public static void InstallHeader(Form form, string brandLead, string brandRest, string tagline)
        {
            if (form == null || form.Controls.ContainsKey("linearHeader")) return;
            const int h = 56;
            var existing = form.Controls.Cast<Control>().ToList();
            foreach (var c in existing)
                if ((c.Anchor & AnchorStyles.Bottom) == 0)
                    c.Top += h;
            try { form.Height += h; } catch { }

            var header = new Panel
            {
                Name = "linearHeader",
                Dock = DockStyle.Top,
                Height = h,
                Tag = "chrome",
                BackColor = Current.Surface1
            };
            form.Controls.Add(header);
            header.BringToFront();
            EnableDoubleBuffer(header);
            header.Paint += (s, e) =>
            {
                PaintHeader(e.Graphics, header.ClientRectangle, brandLead, brandRest, tagline);
                PaintLiveIndicator(e.Graphics, header.ClientRectangle, false, 0f);
            };
            form.Resize += (s, e) => header.Invalidate();
            header.Invalidate();
        }

        public static void PaintHeader(Graphics g, Rectangle r, string lead, string rest, string tagline)
        {
            if (r.Width < 2 || r.Height < 2) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using (var br = new LinearGradientBrush(r, Current.Surface1, Current.Canvas, LinearGradientMode.Horizontal))
                g.FillRectangle(br, r);
            using (var pen = new Pen(Current.Hairline, 1f))
                g.DrawLine(pen, r.X, r.Bottom - 1, r.Right, r.Bottom - 1);
            using (var pen = new Pen(Current.Primary, 2f))
                g.DrawLine(pen, r.X, r.Bottom - 2, r.X + 128, r.Bottom - 2);

            // Brand logo (DreamsLiveSolutions_Logo1), scaled to the bar height and kept
            // clear of the right-hand LIVE indicator. Falls back to the wordmark text if
            // the logo image cannot be loaded.
            int pad = 14;
            var brand = BrandLogo;
            if (brand != null && brand.Width > 0 && brand.Height > 0)
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                int logoH = r.Height - 18;
                int logoW = (int)Math.Round(brand.Width * (logoH / (float)brand.Height));
                int maxW = Math.Max(60, r.Width - 240);
                if (logoW > maxW)
                {
                    logoW = maxW;
                    logoH = (int)Math.Round(brand.Height * (logoW / (float)brand.Width));
                }
                int ly = r.Y + (r.Height - logoH) / 2;
                g.DrawImage(brand, new Rectangle(r.X + pad, ly, logoW, logoH));
            }
            else
            {
                using (var fLead = new Font(FontFamily, 14f, FontStyle.Bold))
                using (var fRest = new Font(FontFamily, 14f, FontStyle.Regular))
                {
                    int ty = r.Y + 7;
                    var sz1 = TextRenderer.MeasureText(g, lead, fLead, Size.Empty, TextFormatFlags.NoPrefix);
                    TextRenderer.DrawText(g, lead, fLead, new Point(r.X + pad, ty), Current.Ink, TextFormatFlags.NoPrefix);
                    TextRenderer.DrawText(g, rest, fRest, new Point(r.X + pad + sz1.Width - 2, ty), Current.InkSubtle, TextFormatFlags.NoPrefix);
                    if (!string.IsNullOrEmpty(tagline))
                        using (var fTag = new Font(FontFamily, 8.5f, FontStyle.Regular))
                            TextRenderer.DrawText(g, tagline, fTag, new Point(r.X + pad, r.Y + 31), Current.InkSubtle, TextFormatFlags.NoPrefix);
                }
            }

            // The "LIVE" status indicator is drawn separately by PaintLiveIndicator so it
            // can reflect presenter state (dim gray when idle, pulsing red when live).
        }

        /// <summary>
        /// Draws the right-aligned status pill ("LIVE"). When <paramref name="live"/> is
        /// false it renders as a steady dim-gray indicator; when true it renders red and
        /// uses <paramref name="pulse"/> (0..1, a smooth breathing value) to brighten the
        /// dot and grow a soft glow.
        /// </summary>
        public static void PaintLiveIndicator(Graphics g, Rectangle r, bool live, float pulse)
        {
            if (r.Width < 80 || r.Height < 10) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            const string text = "Presenter Live";
            using (var fp = new Font(FontFamily, 8f, FontStyle.Bold))
            {
                var ts = TextRenderer.MeasureText(g, text, fp, Size.Empty, TextFormatFlags.NoPrefix);
                int ph = 22, pw = ts.Width + 34;
                var pr = new Rectangle(r.Right - pw - 16, r.Y + (r.Height - ph) / 2, pw, ph);
                if (pr.X < r.X + 8) return;

                if (pulse < 0f) pulse = 0f; else if (pulse > 1f) pulse = 1f;

                Color dotColor, textColor, borderColor;
                if (live)
                {
                    Color dimRed = Blend(Current.Danger, Current.Surface1, 0.45f);
                    Color hotRed = Color.FromArgb(0xFF, 0x6B, 0x6B);
                    dotColor = Blend(dimRed, hotRed, pulse);
                    textColor = Blend(Blend(Current.Danger, Current.Ink, 0.15f), hotRed, pulse);
                    borderColor = dotColor;
                }
                else
                {
                    dotColor = Current.InkSubtle;
                    textColor = Current.InkSubtle;
                    borderColor = Current.Hairline;
                }

                using (var path = RoundedRect(pr, ph / 2))
                using (var pen = new Pen(borderColor, 1.4f))
                    g.DrawPath(pen, path);

                int dotD = 6;
                int dotX = pr.X + 11;
                int cy = pr.Y + ph / 2;
                if (live)
                {
                    int gr = (int)(4 + 7 * pulse);
                    int ga = (int)(40 + 130 * pulse);
                    using (var gb = new SolidBrush(Color.FromArgb(ga, dotColor)))
                        g.FillEllipse(gb, dotX - gr, cy - dotD / 2 - gr, dotD + gr * 2, dotD + gr * 2);
                }
                using (var dotb = new SolidBrush(dotColor))
                    g.FillEllipse(dotb, dotX, cy - dotD / 2, dotD, dotD);

                TextRenderer.DrawText(g, text, fp,
                    new Rectangle(pr.X + 20, pr.Y, pr.Width - 20, pr.Height), textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
        }
    }
}
