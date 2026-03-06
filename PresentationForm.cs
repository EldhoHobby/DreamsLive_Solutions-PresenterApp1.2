using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PdfiumViewer;

namespace DreamsLive_Solutions_PresenterApp1
{
    public enum ImageDisplayMode
    {
        Fit,    // Maintain aspect ratio, fit entire image, letter/pillarbox if needed.
        Fill,   // Maintain aspect ratio, fill entire screen, crop if needed.
        Stretch,// Ignore aspect ratio, fill entire screen by stretching/compressing.
        Tile,   // Repeat the image.
        Center  // Display image at original size, centered. Crop if larger than screen.
    }

    public class DoubleBufferedPanel : System.Windows.Forms.Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }
    }

    public class PresentationForm : Form
    {
        private DoubleBufferedPanel displayPanel;
        private Image currentImage = null;
        private Image cachedFullPageImage = null;
        private string cachedPdfPath = null;
        private int cachedPdfPageNum = -1;
        private int cachedManualRotationAngle = 0;

        private RectangleF? initialSourceRegion = null;
        private ImageDisplayMode currentDisplayMode = ImageDisplayMode.Fit;
        private bool receivedRegionIsNormalized = false;
        private string pdfPathForPresentation = null;
        private int pdfPageNumForPresentation = -1;
        private PdfDocument currentPresPdfDocument = null;
        private PointF? laserPointNormalized = null; // Normalized laser position (0-1) relative to display area
        private List<List<PointF>> highlightsNormalized = new List<List<PointF>>();
        private Color currentHighlighterColor = Color.Yellow;

        public PresentationForm(
           string filePathOrPdfPath,
           int pageNumIfApplicable,
           Screen targetScreen,
           RectangleF? initialRegion = null,
           bool isRegionNormalized = false,
           int manualRotationAngle = 0)
        {
            this.pdfPathForPresentation = filePathOrPdfPath;
            this.pdfPageNumForPresentation = pageNumIfApplicable;
            this.initialSourceRegion = initialRegion;
            this.receivedRegionIsNormalized = isRegionNormalized;

            InitializeComponent(filePathOrPdfPath, targetScreen, manualRotationAngle);

            this.KeyDown += PresentationForm_KeyDown;
        }

        private void InitializeComponent(string imagePath, Screen targetScreen, int manualRotationAngle = 0)
        {
            this.displayPanel = new DoubleBufferedPanel();
            this.SuspendLayout();

            this.displayPanel.Dock = DockStyle.Fill;
            this.displayPanel.BackColor = Color.Black;
            this.displayPanel.Paint += new PaintEventHandler(this.displayPanel_Paint);

            // Add Context Menu for exiting
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit Presenter");
            exitMenuItem.Click += (s, args) => this.Close();
            contextMenu.Items.Add(exitMenuItem);
            this.displayPanel.ContextMenuStrip = contextMenu;

            // Only attempt to load content if a path is provided.
            if (!string.IsNullOrEmpty(this.pdfPathForPresentation))
            {
                try
                {
                    UpdateImage(this.pdfPathForPresentation, this.pdfPageNumForPresentation, this.initialSourceRegion, this.receivedRegionIsNormalized, null, null, manualRotationAngle);
                }
                catch (Exception ex)
                {
                    CopyableMessageBox.Show("Error loading file (image or PDF): " + ex.Message, "File Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }
            }

            this.BackColor = Color.Black;
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(targetScreen.Bounds.Width, targetScreen.Bounds.Height);
            this.Controls.Add(this.displayPanel);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "PresentationForm";
            this.Text = "Presentation";
            this.StartPosition = FormStartPosition.Manual;
            this.Bounds = targetScreen.Bounds;
            this.TopMost = true;
            this.KeyPreview = true;
            this.ResumeLayout(false);
            this.Load += (s, e) => {
                this.WindowState = FormWindowState.Normal;
                this.Bounds = targetScreen.Bounds;
                this.WindowState = FormWindowState.Maximized;
                this.Activate();
                this.Focus();
                SetupInitialView();
            };
        }

        private void SetupInitialView()
        {
            if (this.displayPanel != null) this.displayPanel.Invalidate();
        }

        private Image RenderPdfPageWithHighQuality(PdfDocument doc, int pageNum, Rectangle targetBounds, RectangleF? region, bool isNormalized)
        {
            if (doc == null || pageNum < 0 || pageNum >= doc.PageCount) return null;

            SizeF pageSize = doc.PageSizes[pageNum];
            float pageAspectRatio = pageSize.Width / pageSize.Height;

            float targetWidth = targetBounds.Width;
            float targetHeight = targetBounds.Height;

            if (region.HasValue && isNormalized)
            {
                float zoomFactorX = 1.0f / Math.Max(0.01f, region.Value.Width);
                float zoomFactorY = 1.0f / Math.Max(0.01f, region.Value.Height);
                float zoomFactor = Math.Max(zoomFactorX, zoomFactorY);

                targetWidth *= zoomFactor;
                targetHeight *= zoomFactor;
            }

            float minDpi = 200.0f;
            float minWidth = (pageSize.Width / 72.0f) * minDpi;
            float minHeight = (pageSize.Height / 72.0f) * minDpi;

            if (targetWidth < minWidth) targetWidth = minWidth;
            if (targetHeight < minHeight) targetHeight = minHeight;

            if (targetWidth / targetHeight > pageAspectRatio)
            {
                targetWidth = targetHeight * pageAspectRatio;
            }
            else
            {
                targetHeight = targetWidth / pageAspectRatio;
            }

            const float MaxDimension = 4096f;
            if (targetWidth > MaxDimension || targetHeight > MaxDimension)
            {
                float scale = MaxDimension / Math.Max(targetWidth, targetHeight);
                targetWidth *= scale;
                targetHeight *= scale;
            }

            int finalWidth = (int)Math.Max(1, targetWidth);
            int finalHeight = (int)Math.Max(1, targetHeight);

            float renderDpi = 300.0f;

            return doc.Render(
                pageNum,
                finalWidth,
                finalHeight,
                renderDpi,
                renderDpi,
                PdfRenderFlags.CorrectFromDpi | PdfRenderFlags.Annotations
            );
        }

        private void PresentationForm_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Escape) this.Close(); }

        private void displayPanel_Paint(object sender, PaintEventArgs e)
        {
            if (this.currentImage == null) { e.Graphics.Clear(Color.Black); return; }
            e.Graphics.Clear(Color.Black);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            RectangleF actualSourceRegion;
            Image imageToDraw = this.currentImage;

            if (this.initialSourceRegion.HasValue)
            {
                if (this.receivedRegionIsNormalized)
                {
                    actualSourceRegion = new RectangleF(
                        this.initialSourceRegion.Value.X * imageToDraw.Width,
                        this.initialSourceRegion.Value.Y * imageToDraw.Height,
                        this.initialSourceRegion.Value.Width * imageToDraw.Width,
                        this.initialSourceRegion.Value.Height * imageToDraw.Height);
                }
                else
                {
                    actualSourceRegion = this.initialSourceRegion.Value;
                }
            }
            else
            {
                actualSourceRegion = new RectangleF(0, 0, imageToDraw.Width, imageToDraw.Height);
            }

            if (actualSourceRegion.Width <= 0 || actualSourceRegion.Height <= 0) return;

            RectangleF destRect = CalculateDisplayRectangle(
                new SizeF(actualSourceRegion.Width, actualSourceRegion.Height),
                this.displayPanel.ClientRectangle,
                this.currentDisplayMode);

            RectangleF visibleSrcRect = actualSourceRegion;
            RectangleF visibleDestRect = destRect;

            if (this.currentDisplayMode == ImageDisplayMode.Tile)
            {
                using (System.Drawing.TextureBrush textureBrush = new System.Drawing.TextureBrush(imageToDraw, System.Drawing.Drawing2D.WrapMode.Tile))
                {
                    e.Graphics.FillRectangle(textureBrush, this.displayPanel.ClientRectangle);
                }
                visibleSrcRect = new RectangleF(0, 0, imageToDraw.Width, imageToDraw.Height);
                visibleDestRect = this.displayPanel.ClientRectangle;
            }
            else if (this.currentDisplayMode == ImageDisplayMode.Fill)
            {
                float panelAspect = (float)this.displayPanel.ClientRectangle.Width / this.displayPanel.ClientRectangle.Height;
                float imageAspect = actualSourceRegion.Width / actualSourceRegion.Height;
                RectangleF srcFillRect;

                if (imageAspect > panelAspect)
                {
                    float scaledHeight = actualSourceRegion.Height;
                    float scaledWidth = scaledHeight * panelAspect;
                    srcFillRect = new RectangleF(
                        actualSourceRegion.X + (actualSourceRegion.Width - scaledWidth) / 2f,
                        actualSourceRegion.Y,
                        scaledWidth,
                        scaledHeight);
                }
                else
                {
                    float scaledWidth = actualSourceRegion.Width;
                    float scaledHeight = scaledWidth / panelAspect;
                    srcFillRect = new RectangleF(
                        actualSourceRegion.X,
                        actualSourceRegion.Y + (actualSourceRegion.Height - scaledHeight) / 2f,
                        scaledWidth,
                        scaledHeight);
                }
                srcFillRect.Intersect(actualSourceRegion);
                visibleSrcRect = srcFillRect;
                visibleDestRect = this.displayPanel.ClientRectangle;

                if (srcFillRect.Width > 0 && srcFillRect.Height > 0)
                {
                    e.Graphics.DrawImage(imageToDraw, visibleDestRect, srcFillRect, GraphicsUnit.Pixel);
                }
            }
            else if (!destRect.IsEmpty && destRect.Width > 0 && destRect.Height > 0)
            {
                e.Graphics.DrawImage(imageToDraw, destRect, actualSourceRegion, GraphicsUnit.Pixel);
                visibleSrcRect = actualSourceRegion;
                visibleDestRect = destRect;
            }

            if (!visibleDestRect.IsEmpty && visibleDestRect.Width > 0 && visibleDestRect.Height > 0)
            {
                PointF MapDocToScreen(PointF normDocPoint)
                {
                    float docPixelX = normDocPoint.X * imageToDraw.Width;
                    float docPixelY = normDocPoint.Y * imageToDraw.Height;

                    float rx = (docPixelX - visibleSrcRect.X) / visibleSrcRect.Width;
                    float ry = (docPixelY - visibleSrcRect.Y) / visibleSrcRect.Height;

                    return new PointF(
                        visibleDestRect.X + rx * visibleDestRect.Width,
                        visibleDestRect.Y + ry * visibleDestRect.Height
                    );
                }

                float effectiveScale = visibleDestRect.Width / visibleSrcRect.Width;

                if (this.highlightsNormalized != null && this.highlightsNormalized.Count > 0)
                {
                    foreach (var stroke in this.highlightsNormalized)
                    {
                        if (stroke.Count < 1) continue;
                        PointF[] points = stroke.Select(p => MapDocToScreen(p)).ToArray();
                        MainForm.DrawHighlightStroke(e.Graphics, points, 10f * effectiveScale, this.currentHighlighterColor);
                    }
                }

                if (this.laserPointNormalized != null)
                {
                    PointF center = MapDocToScreen(this.laserPointNormalized.Value);
                    MainForm.DrawLaserPointer(e.Graphics, center, 8f);
                }
            }
        }

        private RectangleF CalculateDisplayRectangle(SizeF contentSize, Rectangle panelBounds, ImageDisplayMode mode)
        {
            if (contentSize.Width <= 0 || contentSize.Height <= 0 || panelBounds.Width == 0 || panelBounds.Height == 0)
                return RectangleF.Empty;

            float imageWidth = contentSize.Width;
            float imageHeight = contentSize.Height;
            float panelWidth = panelBounds.Width;
            float panelHeight = panelBounds.Height;
            float x = 0, y = 0, w = 0, h = 0;

            switch (mode)
            {
                case ImageDisplayMode.Fit:
                    float ratioFit = Math.Min(panelWidth / imageWidth, panelHeight / imageHeight);
                    w = imageWidth * ratioFit;
                    h = imageHeight * ratioFit;
                    x = (panelWidth - w) / 2f;
                    y = (panelHeight - h) / 2f;
                    break;
                case ImageDisplayMode.Fill:
                    return panelBounds;
                case ImageDisplayMode.Stretch:
                    w = panelWidth;
                    h = panelHeight;
                    x = 0;
                    y = 0;
                    break;
                case ImageDisplayMode.Center:
                    w = imageWidth;
                    h = imageHeight;
                    x = (panelWidth - w) / 2f;
                    y = (panelHeight - h) / 2f;
                    break;
                case ImageDisplayMode.Tile:
                    return panelBounds;
            }
            return new RectangleF(x, y, w, h);
        }

        public void UpdateImage(
           string filePathOrPdfPath,
           int pageNumIfApplicable,
           RectangleF? initialRegion = null,
           bool isRegionNormalized = false,
           ImageDisplayMode? modeToApply = null,
           Bitmap stitchedImage = null,
           int manualRotationAngle = 0)
        {
            this.pdfPathForPresentation = filePathOrPdfPath;
            this.pdfPageNumForPresentation = pageNumIfApplicable;

            try
            {
                if (stitchedImage != null)
                {
                    if (this.currentImage != null && this.currentImage != this.cachedFullPageImage) this.currentImage.Dispose();
                    this.currentImage = (Image)stitchedImage.Clone();
                    this.cachedPdfPath = null;
                }
                else if (this.pdfPathForPresentation != null && this.pdfPageNumForPresentation >= 0)
                {
                    bool needsReRender = (this.pdfPathForPresentation != this.cachedPdfPath ||
                                         this.pdfPageNumForPresentation != this.cachedPdfPageNum ||
                                         this.cachedFullPageImage == null ||
                                         manualRotationAngle != this.cachedManualRotationAngle);

                    if (needsReRender)
                    {
                        if (this.pdfPathForPresentation != this.cachedPdfPath)
                        {
                            if (this.currentPresPdfDocument != null) this.currentPresPdfDocument.Dispose();
                            this.currentPresPdfDocument = PdfDocument.Load(this.pdfPathForPresentation);
                        }

                        if (this.pdfPageNumForPresentation >= 0 && this.pdfPageNumForPresentation < this.currentPresPdfDocument.PageCount)
                        {
                            if (this.cachedFullPageImage != null) this.cachedFullPageImage.Dispose();

                            this.cachedFullPageImage = RenderPdfPageWithHighQuality(
                                this.currentPresPdfDocument,
                                this.pdfPageNumForPresentation,
                                Screen.FromControl(this).Bounds,
                                null,
                                false
                            );

                            if (this.cachedFullPageImage != null && manualRotationAngle != 0)
                            {
                                ImageUtils.ApplyRotation(this.cachedFullPageImage, manualRotationAngle);
                            }

                            this.cachedPdfPath = this.pdfPathForPresentation;
                            this.cachedPdfPageNum = this.pdfPageNumForPresentation;
                            this.cachedManualRotationAngle = manualRotationAngle;
                        }
                    }

                    if (this.currentImage != null && this.currentImage != this.cachedFullPageImage) this.currentImage.Dispose();
                    this.currentImage = this.cachedFullPageImage;
                }
                else if (File.Exists(this.pdfPathForPresentation))
                {
                    if (this.currentPresPdfDocument != null) { this.currentPresPdfDocument.Dispose(); this.currentPresPdfDocument = null; }
                    if (this.currentImage != null && this.currentImage != this.cachedFullPageImage) this.currentImage.Dispose();
                    if (this.cachedFullPageImage != null) { this.cachedFullPageImage.Dispose(); this.cachedFullPageImage = null; }

                    this.currentImage = ImageUtils.LoadImage(this.pdfPathForPresentation);
                    if (this.currentImage != null && manualRotationAngle != 0)
                    {
                        ImageUtils.ApplyRotation(this.currentImage, manualRotationAngle);
                    }
                    this.cachedPdfPath = null;
                    this.cachedPdfPageNum = -1;
                }
                else
                {
                    throw new FileNotFoundException("The specified image or PDF file was not found.", this.pdfPathForPresentation);
                }
            }
            catch (OutOfMemoryException oomEx)
            {
                CopyableMessageBox.Show("Error loading file (image or PDF) for update: Out of memory.\n" + oomEx.Message, "File Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (this.currentImage != null && this.currentImage != this.cachedFullPageImage) { this.currentImage.Dispose(); this.currentImage = null; }
            }
            catch (Exception ex)
            {
                CopyableMessageBox.Show("Error loading file (image or PDF) for update: " + ex.Message, "File Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (this.currentImage != null && this.currentImage != this.cachedFullPageImage) { this.currentImage.Dispose(); this.currentImage = null; }
            }

            this.receivedRegionIsNormalized = isRegionNormalized;
            this.initialSourceRegion = initialRegion;

            if (modeToApply.HasValue)
            {
                this.currentDisplayMode = modeToApply.Value;
                UpdateControlsForDisplayMode(this.currentDisplayMode);
            }

            SetupInitialView();
            this.Activate();
        }

        private void UpdateControlsForDisplayMode(ImageDisplayMode mode)
        {
            if (this.displayPanel != null) this.displayPanel.Cursor = Cursors.Default;
        }

        public void ClearDisplay()
        {
            if (this.currentImage != null && this.currentImage != this.cachedFullPageImage)
            {
                this.currentImage.Dispose();
            }
            this.currentImage = null;

            if (this.currentPresPdfDocument != null)
            {
                this.currentPresPdfDocument.Dispose();
                this.currentPresPdfDocument = null;
            }

            this.initialSourceRegion = null;
            this.receivedRegionIsNormalized = false;

            if (this.displayPanel != null)
            {
                this.displayPanel.Invalidate();
            }
        }

        public void UpdateHighlights(List<List<PointF>> highlights, Color color)
        {
            this.highlightsNormalized = highlights.Select(stroke => stroke.ToList()).ToList();
            this.currentHighlighterColor = color;
            if (this.displayPanel != null)
            {
                this.displayPanel.Invalidate();
            }
        }

        public void SetDisplayMode(ImageDisplayMode newMode)
        {
            this.currentDisplayMode = newMode;
            UpdateControlsForDisplayMode(this.currentDisplayMode);
            if (this.IsHandleCreated) { SetupInitialView(); }
        }

        public void UpdateLaserPointer(PointF? normalizedPoint)
        {
            this.laserPointNormalized = normalizedPoint;
            if (this.displayPanel != null)
            {
                this.displayPanel.Invalidate();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.currentPresPdfDocument != null)
                {
                    this.currentPresPdfDocument.Dispose();
                    this.currentPresPdfDocument = null;
                }

                if (this.currentImage != null && this.currentImage != this.cachedFullPageImage)
                {
                    this.currentImage.Dispose();
                }
                if (this.cachedFullPageImage != null)
                {
                    this.cachedFullPageImage.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
