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
        private float _targetAspectRatio;
        private bool _isAutoSend = false;
        private bool _isLiveSync = false;
        private int _currentRotation = 0;

        // Interaction fields
        private Rectangle _cropRect; // In control coordinates
        private bool _isDragging = false;
        private bool _isResizing = false;
        private string _resizeHandle = "";
        private Point _lastMousePos;

        private const int HandleSize = 10;

        public EditContentForm(MainForm mainForm, Image sourceImage, RectangleF initialCropNormalized, float targetAR, int initialRotation)
        {
            _mainForm = mainForm;
            _originalImage = (Image)sourceImage.Clone();
            _cropRegionNormalized = initialCropNormalized;
            _targetAspectRatio = targetAR;
            _currentRotation = initialRotation;

            InitializeComponent();

            this.Text = "Edit / Crop Content";
            this.Size = new Size(1000, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.DoubleBuffered = true;

            picEdit.MouseMove += PicEdit_MouseMove;
            picEdit.MouseDown += PicEdit_MouseDown;
            picEdit.MouseUp += PicEdit_MouseUp;
            picEdit.Paint += PicEdit_Paint;
            picEdit.Resize += (s, e) => UpdateCropRectFromNormalized();

            chkAutoSend.Checked = _mainForm.IsAutoSendEnabled();
            _isAutoSend = chkAutoSend.Checked;
            chkLiveSync.Checked = false; // Default off to match remote usually
            _isLiveSync = chkLiveSync.Checked;

            picEdit.Image = _mainForm.GetPreviewImage();

            UpdateCropRectFromNormalized();
        }

        private void PicEdit_Paint(object sender, PaintEventArgs e)
        {
            if (picEdit.Image == null) return;

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
                g.FillRectangle(b, _cropRect.Left - HandleSize / 2, _cropRect.Top - HandleSize / 2, HandleSize, HandleSize); // TL
                g.FillRectangle(b, _cropRect.Right - HandleSize / 2, _cropRect.Top - HandleSize / 2, HandleSize, HandleSize); // TR
                g.FillRectangle(b, _cropRect.Left - HandleSize / 2, _cropRect.Bottom - HandleSize / 2, HandleSize, HandleSize); // BL
                g.FillRectangle(b, _cropRect.Right - HandleSize / 2, _cropRect.Bottom - HandleSize / 2, HandleSize, HandleSize); // BR
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
            float imgW = picEdit.Image.Width;
            float imgH = picEdit.Image.Height;
            float boxW = picEdit.ClientSize.Width;
            float boxH = picEdit.ClientSize.Height;

            float ratio = Math.Min(boxW / imgW, boxH / imgH);
            float w = imgW * ratio;
            float h = imgH * ratio;
            return new RectangleF((boxW - w) / 2, (boxH - h) / 2, w, h);
        }

        private void UpdateNormalizedFromCropRect(bool forceSync = false)
        {
            RectangleF displayRect = GetDisplayedImageRect();
            _cropRegionNormalized = new RectangleF(
                (_cropRect.X - displayRect.X) / displayRect.Width,
                (_cropRect.Y - displayRect.Y) / displayRect.Height,
                (float)_cropRect.Width / displayRect.Width,
                (float)_cropRect.Height / displayRect.Height
            );

            if (_isLiveSync || forceSync)
            {
                _mainForm.RemoteCrop(_cropRegionNormalized.X, _cropRegionNormalized.Y, _cropRegionNormalized.Width, _cropRegionNormalized.Height);
            }
        }

        private void PicEdit_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            _resizeHandle = GetHandleAtPoint(e.Location);
            if (!string.IsNullOrEmpty(_resizeHandle))
            {
                _isResizing = true;
            }
            else if (_cropRect.Contains(e.Location))
            {
                _isDragging = true;
            }
            _lastMousePos = e.Location;
        }

        private string GetHandleAtPoint(Point p)
        {
            if (new Rectangle(_cropRect.Left - HandleSize, _cropRect.Top - HandleSize, HandleSize * 2, HandleSize * 2).Contains(p)) return "TL";
            if (new Rectangle(_cropRect.Right - HandleSize, _cropRect.Top - HandleSize, HandleSize * 2, HandleSize * 2).Contains(p)) return "TR";
            if (new Rectangle(_cropRect.Left - HandleSize, _cropRect.Bottom - HandleSize, HandleSize * 2, HandleSize * 2).Contains(p)) return "BL";
            if (new Rectangle(_cropRect.Right - HandleSize, _cropRect.Bottom - HandleSize, HandleSize * 2, HandleSize * 2).Contains(p)) return "BR";
            return "";
        }

        private void PicEdit_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                int dx = e.X - _lastMousePos.X;
                int dy = e.Y - _lastMousePos.Y;
                _cropRect.Offset(dx, dy);
                picEdit.Invalidate();
                _lastMousePos = e.Location;
            }
            else if (_isResizing)
            {
                ResizeCropRect(e.Location);
                picEdit.Invalidate();
                _lastMousePos = e.Location;
            }
            else
            {
                string handle = GetHandleAtPoint(e.Location);
                if (!string.IsNullOrEmpty(handle))
                {
                    if (handle == "TL" || handle == "BR") picEdit.Cursor = Cursors.SizeNWSE;
                    else picEdit.Cursor = Cursors.SizeNESW;
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
            }

            int w = right - left;
            int h = bottom - top;

            if (_targetAspectRatio > 0)
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

            // Re-apply based on handle to keep anchor point
            switch (_resizeHandle)
            {
                case "TL": left = right - w; top = bottom - h; break;
                case "TR": right = left + w; top = bottom - h; break;
                case "BL": left = right - w; bottom = top + h; break;
                case "BR": right = left + w; bottom = top + h; break;
            }

            _cropRect = Rectangle.FromLTRB(
                Math.Min(left, right), Math.Min(top, bottom),
                Math.Max(left, right), Math.Max(top, bottom)
            );
        }

        private void PicEdit_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            _isResizing = false;
            UpdateNormalizedFromCropRect();
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
            picEdit.Image = _mainForm.GetPreviewImage(); // Use rotated image from host
            UpdateCropRectFromNormalized();
        }

        private void btnPresentNow_Click(object sender, EventArgs e)
        {
            UpdateNormalizedFromCropRect(forceSync: true);
            _mainForm.btnPushToPresenter_Click(null, EventArgs.Empty);
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
