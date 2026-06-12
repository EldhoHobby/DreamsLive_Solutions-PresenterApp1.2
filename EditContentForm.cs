using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace DreamsLive_Solutions_PresenterApp1
{
    public partial class EditContentForm : Form
    {
        private readonly MainForm _mainForm;
        private Image _originalImage;
        private RectangleF _cropRegionNormalized;
        private RectangleF? _stagedRegionNormalized;
        private float _targetAspectRatio;
        private bool _isAutoSend = false;
        private bool _isLiveSync = false;
        private int _currentRotation = 0;
        private int _lastPageShown = -1;
        private Timer _syncTimer;
        private Timer _outboundSyncTimer;

        // Interaction fields
        private Rectangle _cropRect; // In control coordinates
        private bool _isDragging = false;
        private bool _isResizing = false;
        private string _resizeHandle = "";
        private Point _lastMousePos;

        // Image view (wheel zoom + pan). The canvas draws _displayImage itself so it can be
        // scaled beyond / below the fit size; the crop region stays anchored to the image.
        // _displayImage is an OWNED CLONE of the host preview: the host disposes
        // picPreview.Image on page/file/rotation changes (which the web remote can trigger
        // while this dialog is modal), so painting the host's reference directly would hit
        // a disposed image. _lastHostImageRef is only an identity token for change
        // detection — it may point at a disposed object and must never be dereferenced.
        private Image _displayImage;
        private Image _lastHostImageRef;
        private bool _isPanning = false;
        private float _viewZoom = 1f;                       // 1 = fit-to-window; >1 in; <1 out (smaller than window)
        private System.Drawing.PointF _viewPan = System.Drawing.PointF.Empty;

        private const int HandleSize = 10;

        public EditContentForm(MainForm mainForm, Image sourceImage, RectangleF initialCropNormalized, RectangleF? stagedCropNormalized, float targetAR, int initialRotation)
        {
            _mainForm = mainForm;
            _originalImage = (Image)sourceImage.Clone();
            _cropRegionNormalized = initialCropNormalized;
            _stagedRegionNormalized = stagedCropNormalized;
            _targetAspectRatio = targetAR;
            _currentRotation = initialRotation;

            InitializeComponent();
            LinearTheme.Apply(this);

            // Re-assert the primary-action palette AFTER theming. LinearTheme owner-draws
            // buttons from their BackColor/ForeColor; without this they would inherit the
            // neutral surface color and lose the lavender/green/white styling.
            ApplyActionButtonStyles();

            // The canvas paints the image itself (so the wheel can zoom past / below fit).
            picEdit.Image = null;
            picEdit.BackColor = System.Drawing.Color.Black;

            this.Text = "Edit / Crop Content";
            this.Size = new Size(1000, 800);
            this.MinimumSize = new Size(760, 580);   // keep the stacked footer rows from clipping
            this.StartPosition = FormStartPosition.CenterParent;
            this.DoubleBuffered = true;

            picEdit.MouseMove += PicEdit_MouseMove;
            picEdit.MouseDown += PicEdit_MouseDown;
            picEdit.MouseUp += PicEdit_MouseUp;
            picEdit.MouseWheel += PicEdit_MouseWheel;
            picEdit.Paint += PicEdit_Paint;
            picEdit.Resize += (s, e) => UpdateCropRectFromNormalized();
            this.KeyDown += EditContentForm_KeyDown;
            this.KeyPreview = true;

            chkAutoSend.Checked = _mainForm.IsAutoSendEnabled();
            _isAutoSend = chkAutoSend.Checked;
            chkLiveSync.Checked = false; // Default off to match remote usually
            _isLiveSync = chkLiveSync.Checked;
            chkEnableAutoScroll.Checked = _mainForm.EnableAutoScroll;

            btnUp.Click += (s, e) => MoveCrop(0, -1, false);
            btnDown.Click += (s, e) => MoveCrop(0, 1, false);
            btnLeft.Click += (s, e) => MoveCrop(-1, 0, false);
            btnRight.Click += (s, e) => MoveCrop(1, 0, false);
            chkEnableAutoScroll.CheckedChanged += (s, e) => _mainForm.EnableAutoScroll = chkEnableAutoScroll.Checked;
            btnZoomFit.Click += (s, e) => ZoomToFit();
            btnPrevPageEdit.Click += (s, e) => GoToPdfPage(-1);
            btnNextPageEdit.Click += (s, e) => GoToPdfPage(1);

            AdoptHostPreviewImage();

            UpdateCropRectFromNormalized();
            _lastPageShown = _mainForm.GetCurrentPdfPage();
            UpdatePagerUi();

            _syncTimer = new Timer();
            _syncTimer.Interval = 1000;
            _syncTimer.Tick += SyncTimer_Tick;
            _syncTimer.Start();

            _outboundSyncTimer = new Timer();
            _outboundSyncTimer.Interval = 250;
            _outboundSyncTimer.Tick += (s, e) => {
                _outboundSyncTimer.Stop();
                SyncOutbound();
            };

            this.FormClosing += (s, e) => {
                _syncTimer.Stop();
                _outboundSyncTimer.Stop();
            };
            this.FormClosed += (s, e) => {
                _displayImage?.Dispose();
                _displayImage = null;
                _originalImage?.Dispose();
                _originalImage = null;
            };
        }

        // Clone the host preview so this form owns what it paints. Keeps the previous
        // clone if the host image is briefly unavailable; the next sync tick retries
        // (the host reference token is only advanced on success).
        private void AdoptHostPreviewImage()
        {
            Image src = _mainForm.GetPreviewImage();
            if (src == null)
            {
                _displayImage?.Dispose();
                _displayImage = null;
                _lastHostImageRef = null;
                return;
            }
            try
            {
                Image clone = new Bitmap(src);
                _displayImage?.Dispose();
                _displayImage = clone;
                _lastHostImageRef = src;
            }
            catch (ArgumentException)
            {
                // src was already disposed by the host; keep the previous owned clone.
            }
        }

        private void SyncTimer_Tick(object sender, EventArgs e)
        {
            if (_isDragging || _isResizing || _isPanning) return;

            bool changed = false;

            // Sync Rotation
            int latestRotation = _mainForm.GetCurrentManualRotationAngle();
            if (latestRotation != _currentRotation)
            {
                _currentRotation = latestRotation;
                changed = true;
            }

            // Sync PDF page bookkeeping.
            if (_mainForm.GetTotalPdfPages() > 0)
            {
                int latestPage = _mainForm.GetCurrentPdfPage();
                if (latestPage != _lastPageShown)
                {
                    _lastPageShown = latestPage;
                    changed = true;
                }
            }

            // Adopt the host preview whenever its bitmap was replaced — page switch,
            // rotation, re-render, or a new file, including changes the web remote
            // triggers while this dialog is open.
            if (!ReferenceEquals(_mainForm.GetPreviewImage(), _lastHostImageRef))
            {
                AdoptHostPreviewImage();
                changed = true;
            }

            // Sync Staged (Gray Box)
            var latestStaged = _mainForm.GetStagedSelectionNormalized();
            if (!AreRegionsSimilar(latestStaged, _stagedRegionNormalized))
            {
                _stagedRegionNormalized = latestStaged;
                changed = true;
            }

            UpdatePagerUi();

            if (changed)
            {
                UpdateCropRectFromNormalized();
                picEdit.Invalidate();
            }
        }

        private bool AreRegionsSimilar(RectangleF? r1, RectangleF? r2)
        {
            if (r1.HasValue != r2.HasValue) return false;
            if (!r1.HasValue) return true;
            return Math.Abs(r1.Value.X - r2.Value.X) < 0.001f &&
                   Math.Abs(r1.Value.Y - r2.Value.Y) < 0.001f &&
                   Math.Abs(r1.Value.Width - r2.Value.Width) < 0.001f &&
                   Math.Abs(r1.Value.Height - r2.Value.Height) < 0.001f;
        }

        private void PicEdit_Paint(object sender, PaintEventArgs e)
        {
            if (_displayImage == null) return;

            // Draw the image at its current zoom/pan (the canvas owns the drawing now).
            RectangleF dispRect = GetDisplayedImageRect();
            if (dispRect.Width > 0 && dispRect.Height > 0)
            {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                e.Graphics.DrawImage(_displayImage, dispRect);
            }

            // Draw gray reference box for staged area
            if (_stagedRegionNormalized.HasValue)
            {
                RectangleF displayRect = GetDisplayedImageRect();
                Rectangle stagedRect = new Rectangle(
                    (int)(displayRect.X + _stagedRegionNormalized.Value.X * displayRect.Width),
                    (int)(displayRect.Y + _stagedRegionNormalized.Value.Y * displayRect.Height),
                    (int)(_stagedRegionNormalized.Value.Width * displayRect.Width),
                    (int)(_stagedRegionNormalized.Value.Height * displayRect.Height)
                );

                using (Pen stagedPen = new Pen(Color.FromArgb(180, Color.DimGray), 6))
                {
                    stagedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawRectangle(stagedPen, stagedRect);
                }
            }

            // Draw darkening overlay outside crop area
            using (Region r = new Region(picEdit.ClientRectangle))
            {
                r.Exclude(_cropRect);
                using (Brush b = new SolidBrush(Color.FromArgb(150, Color.Black)))
                {
                    e.Graphics.FillRegion(b, r);
                }
            }

            // Draw crop box
            using (Pen p = new Pen(Color.White, 2))
            {
                e.Graphics.DrawRectangle(p, _cropRect);
            }

            // Draw handles
            DrawHandles(e.Graphics);
        }

        private void DrawHandles(Graphics g)
        {
            using (Brush b = new SolidBrush(Color.White))
            {
                // Corners
                g.FillRectangle(b, _cropRect.Left - HandleSize / 2, _cropRect.Top - HandleSize / 2, HandleSize, HandleSize); // TL
                g.FillRectangle(b, _cropRect.Right - HandleSize / 2, _cropRect.Top - HandleSize / 2, HandleSize, HandleSize); // TR
                g.FillRectangle(b, _cropRect.Left - HandleSize / 2, _cropRect.Bottom - HandleSize / 2, HandleSize, HandleSize); // BL
                g.FillRectangle(b, _cropRect.Right - HandleSize / 2, _cropRect.Bottom - HandleSize / 2, HandleSize, HandleSize); // BR

                // Edges
                g.FillRectangle(b, _cropRect.Left + _cropRect.Width / 2 - HandleSize / 2, _cropRect.Top - HandleSize / 2, HandleSize, HandleSize); // Top
                g.FillRectangle(b, _cropRect.Left + _cropRect.Width / 2 - HandleSize / 2, _cropRect.Bottom - HandleSize / 2, HandleSize, HandleSize); // Bottom
                g.FillRectangle(b, _cropRect.Left - HandleSize / 2, _cropRect.Top + _cropRect.Height / 2 - HandleSize / 2, HandleSize, HandleSize); // Left
                g.FillRectangle(b, _cropRect.Right - HandleSize / 2, _cropRect.Top + _cropRect.Height / 2 - HandleSize / 2, HandleSize, HandleSize); // Right
            }
        }

        private void UpdateCropRectFromNormalized()
        {
            if (picEdit.ClientSize.Width == 0 || picEdit.ClientSize.Height == 0 || _originalImage == null) return;

            RectangleF displayRect = GetDisplayedImageRect();
            _cropRect = new Rectangle(
                (int)(displayRect.X + _cropRegionNormalized.X * displayRect.Width),
                (int)(displayRect.Y + _cropRegionNormalized.Y * displayRect.Height),
                (int)(_cropRegionNormalized.Width * displayRect.Width),
                (int)(_cropRegionNormalized.Height * displayRect.Height)
            );
            picEdit.Invalidate();
        }

        private RectangleF GetDisplayedImageRect()
        {
            if (_displayImage == null) return RectangleF.Empty;
            float imgW = _displayImage.Width;
            float imgH = _displayImage.Height;
            float boxW = picEdit.ClientSize.Width;
            float boxH = picEdit.ClientSize.Height;
            if (imgW <= 0 || imgH <= 0 || boxW <= 0 || boxH <= 0) return RectangleF.Empty;

            // Base fit-to-window, then apply the wheel zoom and the pan offset.
            float fitRatio = Math.Min(boxW / imgW, boxH / imgH);
            float w = imgW * fitRatio * _viewZoom;
            float h = imgH * fitRatio * _viewZoom;
            float x = (boxW - w) / 2f + _viewPan.X;
            float y = (boxH - h) / 2f + _viewPan.Y;
            return new RectangleF(x, y, w, h);
        }

        private void UpdateNormalizedFromCropRect(bool forceSync = false)
        {
            RectangleF displayRect = GetDisplayedImageRect();
            if (displayRect.Width == 0 || displayRect.Height == 0) return;

            _cropRegionNormalized = new RectangleF(
                (_cropRect.X - displayRect.X) / displayRect.Width,
                (_cropRect.Y - displayRect.Y) / displayRect.Height,
                (float)_cropRect.Width / displayRect.Width,
                (float)_cropRect.Height / displayRect.Height
            );

            if (forceSync)
            {
                _outboundSyncTimer.Stop();
                SyncOutbound();
            }
            else if (_isLiveSync)
            {
                _outboundSyncTimer.Stop();
                _outboundSyncTimer.Start();
            }
        }

        private void SyncOutbound()
        {
            _mainForm.RemoteCrop(_cropRegionNormalized.X, _cropRegionNormalized.Y, _cropRegionNormalized.Width, _cropRegionNormalized.Height);
            _stagedRegionNormalized = _cropRegionNormalized;
            picEdit.Invalidate();
        }

        /// <summary>
        /// "Full Zoom Out" — mirrors the web remote: snap the view to the whole image/page,
        /// resetting the crop selection to the entire (uncropped) content at a glance.
        /// </summary>
        private void ZoomToFit()
        {
            if (_displayImage == null) return;
            // Reset the view zoom/pan so the whole page fits the window again. The selection
            // box stays where it is on screen (viewfinder); re-derive what it now covers.
            _viewZoom = 1f;
            _viewPan = System.Drawing.PointF.Empty;
            UpdateNormalizedFromCropRect();
            picEdit.Invalidate();
        }

        // Lavender / green / white action buttons (re-applied after LinearTheme owner-draw).
        private void ApplyActionButtonStyles()
        {
            // Brand accents from the shared palette (same lavender in both modes; the
            // Close button is always white-with-ink regardless of the active mode).
            btnPresentNow.BackColor = LinearTheme.Dark.Primary;
            btnPresentNow.ForeColor = LinearTheme.Dark.OnPrimary;
            btnDone.BackColor = LinearTheme.Dark.Success;
            btnDone.ForeColor = LinearTheme.Dark.OnPrimary;
            btnClose.BackColor = Color.White;
            btnClose.ForeColor = LinearTheme.Light.Ink;
            btnPresentNow.Invalidate();
            btnDone.Invalidate();
            btnClose.Invalidate();
        }

        // PDF pagination from the edit view — drives the host's page, mirroring the web remote.
        private void GoToPdfPage(int delta)
        {
            int total = _mainForm.GetTotalPdfPages();
            if (total <= 0) return;
            int target = _mainForm.GetCurrentPdfPage() + (delta < 0 ? -1 : 1);
            if (target < 0 || target >= total) return;

            if (delta < 0) _mainForm.PreviousPage(); else _mainForm.NextPage();

            // The host re-rendered the page; adopt the new bitmap and keep the crop region.
            AdoptHostPreviewImage();
            _lastPageShown = _mainForm.GetCurrentPdfPage();
            UpdateCropRectFromNormalized();
            UpdatePagerUi();
            picEdit.Invalidate();
            if (_isLiveSync) { _outboundSyncTimer.Stop(); SyncOutbound(); }
        }

        private void UpdatePagerUi()
        {
            int total = _mainForm.GetTotalPdfPages();
            bool isPdf = total > 0;
            if (tlpPager.Visible != isPdf) tlpPager.Visible = isPdf;
            if (!isPdf) return;
            int cur = _mainForm.GetCurrentPdfPage(); // 0-based
            string text = (cur + 1) + "  /  " + total;
            if (lblPageIndicator.Text != text) lblPageIndicator.Text = text;
            btnPrevPageEdit.Enabled = cur > 0;
            btnNextPageEdit.Enabled = cur < total - 1;
        }

        private void PicEdit_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            _resizeHandle = GetHandleAtPoint(e.Location);
            if (!string.IsNullOrEmpty(_resizeHandle))
            {
                _isResizing = true;   // resize the crop box via its handles
            }
            else if (_cropRect.Contains(e.Location))
            {
                _isDragging = true;   // move the crop box (its screen position/size is independent of zoom)
            }
            else
            {
                _isPanning = true;    // drag on the empty area pans the document under the fixed box
                picEdit.Cursor = Cursors.SizeAll;
            }
            _lastMousePos = e.Location;
        }

        private string GetHandleAtPoint(Point p)
        {
            int range = HandleSize + 5;
            if (new Rectangle(_cropRect.Left - range, _cropRect.Top - range, range * 2, range * 2).Contains(p)) return "TL";
            if (new Rectangle(_cropRect.Right - range, _cropRect.Top - range, range * 2, range * 2).Contains(p)) return "TR";
            if (new Rectangle(_cropRect.Left - range, _cropRect.Bottom - range, range * 2, range * 2).Contains(p)) return "BL";
            if (new Rectangle(_cropRect.Right - range, _cropRect.Bottom - range, range * 2, range * 2).Contains(p)) return "BR";

            if (new Rectangle(_cropRect.Left + _cropRect.Width / 2 - range, _cropRect.Top - range, range * 2, range * 2).Contains(p)) return "T";
            if (new Rectangle(_cropRect.Left + _cropRect.Width / 2 - range, _cropRect.Bottom - range, range * 2, range * 2).Contains(p)) return "B";
            if (new Rectangle(_cropRect.Left - range, _cropRect.Top + _cropRect.Height / 2 - range, range * 2, range * 2).Contains(p)) return "L";
            if (new Rectangle(_cropRect.Right - range, _cropRect.Top + _cropRect.Height / 2 - range, range * 2, range * 2).Contains(p)) return "R";

            return "";
        }

        private void PicEdit_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                _viewPan.X += e.X - _lastMousePos.X;
                _viewPan.Y += e.Y - _lastMousePos.Y;
                _lastMousePos = e.Location;
                UpdateNormalizedFromCropRect(); // box stays fixed on screen; re-derive the selection it covers
                picEdit.Invalidate();
                return;
            }
            if (_isDragging)
            {
                int dx = e.X - _lastMousePos.X;
                int dy = e.Y - _lastMousePos.Y;
                _cropRect.Offset(dx, dy);
                _lastMousePos = e.Location;
                UpdateNormalizedFromCropRect(); // Updates normalized region and syncs if _isLiveSync
                picEdit.Invalidate();
            }
            else if (_isResizing)
            {
                ResizeCropRect(e.Location);
                _lastMousePos = e.Location;
                UpdateNormalizedFromCropRect(); // Updates normalized region and syncs if _isLiveSync
                picEdit.Invalidate();
            }
            else
            {
                string handle = GetHandleAtPoint(e.Location);
                if (!string.IsNullOrEmpty(handle))
                {
                    if (handle == "TL" || handle == "BR") picEdit.Cursor = Cursors.SizeNWSE;
                    else if (handle == "TR" || handle == "BL") picEdit.Cursor = Cursors.SizeNESW;
                    else if (handle == "T" || handle == "B") picEdit.Cursor = Cursors.SizeNS;
                    else if (handle == "L" || handle == "R") picEdit.Cursor = Cursors.SizeWE;
                }
                else if (_cropRect.Contains(e.Location))
                {
                    picEdit.Cursor = Cursors.SizeAll;
                }
                else
                {
                    picEdit.Cursor = Cursors.Default;
                }
            }
        }

        private void ResizeCropRect(Point p)
        {
            int left = _cropRect.Left;
            int top = _cropRect.Top;
            int right = _cropRect.Right;
            int bottom = _cropRect.Bottom;

            switch (_resizeHandle)
            {
                case "TL": left = p.X; top = p.Y; break;
                case "TR": right = p.X; top = p.Y; break;
                case "BL": left = p.X; bottom = p.Y; break;
                case "BR": right = p.X; bottom = p.Y; break;
                case "T": top = p.Y; break;
                case "B": bottom = p.Y; break;
                case "L": left = p.X; break;
                case "R": right = p.X; break;
            }

            int w = right - left;
            int h = bottom - top;

            if (_targetAspectRatio > 0)
            {
                // For edge handles, we must adjust the OTHER dimension to maintain aspect ratio
                // For corner handles, we use the logic above
                if (_resizeHandle.Length == 1) // Edge handles T, B, L, R
                {
                    if (_resizeHandle == "T" || _resizeHandle == "B")
                    {
                        w = (int)(h * _targetAspectRatio);
                        left = left + (right - left - w) / 2;
                        right = left + w;
                    }
                    else // L or R
                    {
                        h = (int)(w / _targetAspectRatio);
                        top = top + (bottom - top - h) / 2;
                        bottom = top + h;
                    }
                }
                else // Corner handles TL, TR, BL, BR
                {
                    if (Math.Abs(w) / _targetAspectRatio > Math.Abs(h))
                    {
                        h = (int)(w / _targetAspectRatio);
                    }
                    else
                    {
                        w = (int)(h * _targetAspectRatio);
                    }
                }
            }

            // Re-apply based on handle to keep anchor point (for corner handles)
            // For edge handles, anchor is the opposite edge
            switch (_resizeHandle)
            {
                case "TL": left = right - w; top = bottom - h; break;
                case "TR": right = left + w; top = bottom - h; break;
                case "BL": left = right - w; bottom = top + h; break;
                case "BR": right = left + w; bottom = top + h; break;
                case "T": top = bottom - h; break;
                case "B": bottom = top + h; break;
                case "L": left = right - w; break;
                case "R": right = left + w; break;
            }

            _cropRect = Rectangle.FromLTRB(
                Math.Min(left, right), Math.Min(top, bottom),
                Math.Max(left, right), Math.Max(top, bottom)
            );
        }

        private void PicEdit_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                _isPanning = false;
                picEdit.Cursor = Cursors.Default;
                return;
            }
            _isDragging = false;
            _isResizing = false;
            if (_cropRect.Width < 5 || _cropRect.Height < 5)
            {
                MaximizeCropArea();
            }
            UpdateNormalizedFromCropRect();
        }

        // Mouse wheel zooms the DOCUMENT only. The crop box keeps its absolute screen
        // position/size — it is never modified here; we only re-derive which document region
        // it now covers. Zoom-out may go below fit; zoom is anchored on the cursor.
        private void PicEdit_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_displayImage == null) return;

            RectangleF before = GetDisplayedImageRect();
            if (before.Width <= 0 || before.Height <= 0) return;

            // Fraction of the image under the cursor, kept fixed across the zoom.
            float fx = (e.X - before.X) / before.Width;
            float fy = (e.Y - before.Y) / before.Height;

            float factor = (e.Delta > 0) ? 1.1f : (1f / 1.1f);
            _viewZoom = Math.Max(0.15f, Math.Min(8f, _viewZoom * factor)); // < 1 allowed (smaller than window)

            RectangleF after = GetDisplayedImageRect();
            _viewPan.X += e.X - (after.X + fx * after.Width);
            _viewPan.Y += e.Y - (after.Y + fy * after.Height);

            // Fixed viewfinder: the crop box keeps its absolute screen position/size; only the
            // document scaled. Re-derive which document region the (unmoved) box now frames.
            UpdateNormalizedFromCropRect();
            picEdit.Invalidate();
        }

        private void EditContentForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (_mainForm.EnableAutoScroll)
            {
                bool isShift = e.Shift;
                switch (e.KeyCode)
                {
                    case Keys.Up: MoveCrop(0, -1, isShift); break;
                    case Keys.Down: MoveCrop(0, 1, isShift); break;
                    case Keys.Left: MoveCrop(-1, 0, isShift); break;
                    case Keys.Right: MoveCrop(1, 0, isShift); break;
                    case Keys.PageUp: MoveCrop(0, -1, true); break;
                    case Keys.PageDown: MoveCrop(0, 1, true); break;
                    default: return;
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void MoveCrop(int xDir, int yDir, bool isPageOrShift)
        {
            int step;
            if (isPageOrShift)
            {
                step = (xDir != 0) ? _cropRect.Width : _cropRect.Height;
            }
            else
            {
                // Get the move step from main form
                step = _mainForm.InvokeRequired ? (int)_mainForm.Invoke(new Func<int>(() => GetMainFormMoveStep())) : GetMainFormMoveStep();
            }

            int dx = xDir * step;
            int dy = yDir * step;

            _cropRect.Offset(dx, dy);

            // Clamp to image bounds
            RectangleF displayRect = GetDisplayedImageRect();
            if (_cropRect.X < (int)displayRect.Left) _cropRect.X = (int)displayRect.Left;
            if (_cropRect.Y < (int)displayRect.Top) _cropRect.Y = (int)displayRect.Top;
            if (_cropRect.Right > (int)displayRect.Right) _cropRect.X = (int)displayRect.Right - _cropRect.Width;
            if (_cropRect.Bottom > (int)displayRect.Bottom) _cropRect.Y = (int)displayRect.Bottom - _cropRect.Height;

            UpdateNormalizedFromCropRect();
            picEdit.Invalidate();
        }

        private int GetMainFormMoveStep()
        {
            return _mainForm.GetMoveStep();
        }

        private void btnRotateL_Click(object sender, EventArgs e)
        {
            _mainForm.RotateContent(-1);
            ReloadImage();
        }

        private void btnRotateR_Click(object sender, EventArgs e)
        {
            _mainForm.RotateContent(1);
            ReloadImage();
        }

        private void ReloadImage()
        {
            AdoptHostPreviewImage(); // Use rotated image from host
            MaximizeCropArea();
        }

        private void MaximizeCropArea()
        {
            if (_displayImage == null) return;

            float imgW = _displayImage.Width;
            float imgH = _displayImage.Height;
            float imgAR = imgW / imgH;

            if (_targetAspectRatio > 0)
            {
                if (imgAR > _targetAspectRatio) // Image is wider than target
                {
                    float normW = _targetAspectRatio / imgAR;
                    _cropRegionNormalized = new RectangleF((1f - normW) / 2f, 0f, normW, 1f);
                }
                else // Image is taller than target
                {
                    float normH = imgAR / _targetAspectRatio;
                    _cropRegionNormalized = new RectangleF(0f, (1f - normH) / 2f, 1f, normH);
                }
            }
            else
            {
                _cropRegionNormalized = new RectangleF(0f, 0f, 1f, 1f);
            }

            UpdateCropRectFromNormalized();
            UpdateNormalizedFromCropRect(); // Sync if needed
        }

        private void btnPresentNow_Click(object sender, EventArgs e)
        {
            UpdateNormalizedFromCropRect(forceSync: true);
            _mainForm.btnPushToPresenter_Click(null, EventArgs.Empty);
            // After 'Present Now', the current selection becomes the new 'staged' reference.
            _stagedRegionNormalized = _cropRegionNormalized;
            // We force a refresh of the host's main preview to update the gray box location.
            _mainForm.Invalidate();
            this.picEdit.Invalidate();
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            UpdateNormalizedFromCropRect(forceSync: true);
            _mainForm.btnStageContent_Click(null, EventArgs.Empty);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void chkAutoSend_CheckedChanged(object sender, EventArgs e)
        {
            _isAutoSend = chkAutoSend.Checked;
            _mainForm.SetAutoSendEnabled(_isAutoSend);
        }

        private void chkLiveSync_CheckedChanged(object sender, EventArgs e)
        {
            _isLiveSync = chkLiveSync.Checked;
            if (_isLiveSync) UpdateNormalizedFromCropRect(forceSync: true);
        }
    }
}
