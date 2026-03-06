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
        // this.zoomSlider = new TrackBar(); // Removed
        this.SuspendLayout();

        this.displayPanel.Dock = DockStyle.Fill;
        this.displayPanel.BackColor = Color.Black;
        this.displayPanel.Paint += new PaintEventHandler(this.displayPanel_Paint);

        // Add Context Menu for exiting
        ContextMenuStrip contextMenu = new ContextMenuStrip();
        ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit Presenter");
        exitMenuItem.Click += (s, args) => this.Close();
        contextMenu.Items.Add(exitMenuItem);
        this.displayPanel.ContextMenuStrip = contextMenu; // Assign to panel, or Form itself

        // Only attempt to load content if a path is provided.
        // If pdfPathForPresentation is null/empty, currentImage will remain null, which is handled by Paint.
        if (!string.IsNullOrEmpty(this.pdfPathForPresentation))
        {
            try
            {
                if (this.pdfPathForPresentation != null && this.pdfPageNumForPresentation >= 0) // Redundant null check for path, but fine


                {
                    if (this.currentPresPdfDocument != null)
                    {
                        this.currentPresPdfDocument.Dispose();
                    }
                    this.currentPresPdfDocument = PdfDocument.Load(this.pdfPathForPresentation);

                    if (this.pdfPageNumForPresentation >= 0 && this.pdfPageNumForPresentation < this.currentPresPdfDocument.PageCount)
                    {
                        if (this.currentImage != null) { this.currentImage.Dispose(); }
                        this.currentImage = RenderPdfPageWithHighQuality(
                            this.currentPresPdfDocument,
                            this.pdfPageNumForPresentation,
                            targetScreen.Bounds,
                            this.initialSourceRegion,
                            this.receivedRegionIsNormalized
                        );
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("pdfPageNumForPresentation", "Page number is out of range for the PDF document.");
                    }
                }
                else if (File.Exists(this.pdfPathForPresentation))
                {
                    if (this.currentPresPdfDocument != null) { this.currentPresPdfDocument.Dispose(); this.currentPresPdfDocument = null; }
                    if (this.currentImage != null) { this.currentImage.Dispose(); }
                    this.currentImage = ImageUtils.LoadImage(this.pdfPathForPresentation);
                    if (this.currentImage != null && manualRotationAngle != 0)
                    {
                        ImageUtils.ApplyRotation(this.currentImage, manualRotationAngle);
                    }
                }
                else
                {
                    throw new FileNotFoundException("The specified image or PDF file was not found.", this.pdfPathForPresentation);
                }
            }
            catch (OutOfMemoryException oomEx)
            {
                    CopyableMessageBox.Show("Error loading file (image or PDF): Out of memory. The file might be too large or corrupted.\n" + oomEx.Message, "File Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (this.currentImage != null) { this.currentImage.Dispose(); this.currentImage = null; }
                if (this.currentPresPdfDocument != null) { this.currentPresPdfDocument.Dispose(); this.currentPresPdfDocument = null; }
                this.Close();
                return;
            }
            catch (Exception ex)
            {

                CopyableMessageBox.Show("Error loading file (image or PDF): " + ex.Message, "File Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (this.currentImage != null) { this.currentImage.Dispose(); this.currentImage = null; }
                if (this.currentPresPdfDocument != null) { this.currentPresPdfDocument.Dispose(); this.currentPresPdfDocument = null; }
                this.Close();
                return;
            } // End of try-catch

        } // End of if (!string.IsNullOrEmpty(this.pdfPathForPresentation))

        // this.zoomSlider.Dock = DockStyle.Bottom; this.zoomSlider.Minimum = 10; this.zoomSlider.Maximum = 500; this.zoomSlider.Value = 100; this.zoomSlider.TickFrequency = 10; // Removed
        // this.zoomSlider.Scroll += new EventHandler(this.zoomSlider_Scroll); // Removed

        this.BackColor = Color.Black; this.AutoScaleDimensions = new SizeF(6F, 13F); this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(targetScreen.Bounds.Width, targetScreen.Bounds.Height);
        this.Controls.Add(this.displayPanel); // this.Controls.Add(this.zoomSlider); // zoomSlider removed from controls
        this.FormBorderStyle = FormBorderStyle.None; this.Name = "PresentationForm"; this.Text = "Presentation";
        this.StartPosition = FormStartPosition.Manual; this.Bounds = targetScreen.Bounds; this.TopMost = true; this.KeyPreview = true;
        this.ResumeLayout(false);
        this.Load += (s, e) => {
            this.WindowState = FormWindowState.Normal; this.Bounds = targetScreen.Bounds; this.WindowState = FormWindowState.Maximized;
            this.Activate(); this.Focus(); SetupInitialView();
        };
    }

    private void SetupInitialView()
    {
        // Simplified: No zoom/pan, just ensure displayPanel is invalidated to trigger paint with currentDisplayMode
        if (this.displayPanel != null) this.displayPanel.Invalidate();
    }

    private Image RenderPdfPageWithHighQuality(PdfDocument doc, int pageNum, Rectangle targetBounds, RectangleF? region, bool isNormalized)
    {
        if (doc == null || pageNum < 0 || pageNum >= doc.PageCount) return null;

        SizeF pageSize = doc.PageSizes[pageNum];
        float pageAspectRatio = pageSize.Width / pageSize.Height;

        // Base target size: Start with the target screen/panel bounds
        float targetWidth = targetBounds.Width;
        float targetHeight = targetBounds.Height;

        // If a region is specified, we want that region to be sharp when scaled up.
        // We increase the overall page render size so the sub-region has sufficient resolution.
        if (region.HasValue && isNormalized)
        {
            float zoomFactorX = 1.0f / Math.Max(0.01f, region.Value.Width);
            float zoomFactorY = 1.0f / Math.Max(0.01f, region.Value.Height);
            float zoomFactor = Math.Max(zoomFactorX, zoomFactorY);

            targetWidth *= zoomFactor;
            targetHeight *= zoomFactor;
        }

        // Ensure we have at least a decent base resolution (e.g. roughly 200-300 DPI equivalent)
        // for full page views on high-res monitors.
        float minDpi = 200.0f;
        float minWidth = (pageSize.Width / 72.0f) * minDpi;
        float minHeight = (pageSize.Height / 72.0f) * minDpi;

        if (targetWidth < minWidth) targetWidth = minWidth;
        if (targetHeight < minHeight) targetHeight = minHeight;

        // Maintain aspect ratio based on the largest calculated dimension
        if (targetWidth / targetHeight > pageAspectRatio)
        {
            targetWidth = targetHeight * pageAspectRatio;
        }
        else
        {
            targetHeight = targetWidth / pageAspectRatio;
        }

        // Safety Cap: Avoid OutOfMemoryException with extremely high resolutions.
        // 4096 is a common limit for textures and fits well within memory for most systems.
        const float MaxDimension = 4096f;
        if (targetWidth > MaxDimension || targetHeight > MaxDimension)
        {
            float scale = MaxDimension / Math.Max(targetWidth, targetHeight);
            targetWidth *= scale;
            targetHeight *= scale;
        }

        int finalWidth = (int)Math.Max(1, targetWidth);
        int finalHeight = (int)Math.Max(1, targetHeight);

        // Rendering at a higher DPI (300) helps with text and vector clarity
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
        Image imageToDraw = this.currentImage; // Use the main image by default

        // Determine the source region from the full image based on initialSourceRegion
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
            // Removed clamping to allow regions outside the image (black bars) as per requirement
        }
        else
        {
            actualSourceRegion = new RectangleF(0, 0, imageToDraw.Width, imageToDraw.Height);
        }

        if (actualSourceRegion.Width <= 0 || actualSourceRegion.Height <= 0) return; // Nothing to draw from source

        RectangleF destRect = CalculateDisplayRectangle(
            new SizeF(actualSourceRegion.Width, actualSourceRegion.Height), // Pass the size of the region to be displayed
            this.displayPanel.ClientRectangle,
            this.currentDisplayMode);

        RectangleF visibleSrcRect = actualSourceRegion;
        RectangleF visibleDestRect = destRect;

        if (this.currentDisplayMode == ImageDisplayMode.Tile)
        {
            // Tiling the original image, not just a region. If region tiling is needed, this needs adjustment.
            using (System.Drawing.TextureBrush textureBrush = new System.Drawing.TextureBrush(imageToDraw, System.Drawing.Drawing2D.WrapMode.Tile))
            {
                e.Graphics.FillRectangle(textureBrush, this.displayPanel.ClientRectangle);
            }
            visibleSrcRect = new RectangleF(0, 0, imageToDraw.Width, imageToDraw.Height);
            visibleDestRect = this.displayPanel.ClientRectangle;
        }
        else if (this.currentDisplayMode == ImageDisplayMode.Fill)
        {
            // For Fill, destRect is the panel itself. We need to calculate a new src for DrawImage.
            float panelAspect = (float)this.displayPanel.ClientRectangle.Width / this.displayPanel.ClientRectangle.Height;
            float imageAspect = actualSourceRegion.Width / actualSourceRegion.Height;
            RectangleF srcFillRect;

            if (imageAspect > panelAspect) // Image is wider than panel (letterbox in Fit) -> Fill means crop sides
            {
                float scaledHeight = actualSourceRegion.Height;
                float scaledWidth = scaledHeight * panelAspect;
                srcFillRect = new RectangleF(
                    actualSourceRegion.X + (actualSourceRegion.Width - scaledWidth) / 2f,
                    actualSourceRegion.Y,
                    scaledWidth,
                    scaledHeight);
            }
            else // Image is taller than panel (pillarbox in Fit) -> Fill means crop top/bottom
            {
                float scaledWidth = actualSourceRegion.Width;
                float scaledHeight = scaledWidth / panelAspect;
                srcFillRect = new RectangleF(
                    actualSourceRegion.X,
                    actualSourceRegion.Y + (actualSourceRegion.Height - scaledHeight) / 2f,
                    scaledWidth,
                    scaledHeight);
            }
            // Clamp srcFillRect to actualSourceRegion bounds (already effectively done by basing on actualSourceRegion)
            srcFillRect.Intersect(actualSourceRegion); // Ensure it doesn't go outside the intended source
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
            // Helper to map normalized doc coordinates to screen coordinates
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
                MainForm.DrawLaserPointer(e.Graphics, center, 8f); // Constant screen size
            }
        }
    }

    // Updated Helper method for calculating display rectangle
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
                // For Fill, the destination is the entire panel.
                // The source rectangle is calculated in Paint to achieve the fill effect.
                // This method returns the destination for DrawImage, which is panelBounds for Fill.
                return panelBounds;
            case ImageDisplayMode.Stretch:
                w = panelWidth;
                h = panelHeight;
                x = 0;
                y = 0;
                break;
            case ImageDisplayMode.Center:
                w = imageWidth; // Display at original size of the content (or region)
                h = imageHeight;
                x = (panelWidth - w) / 2f;
                y = (panelHeight - h) / 2f;
                break;
            case ImageDisplayMode.Tile:
                // Tile mode fills the entire panel, handled directly in Paint.
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
        if (this.currentImage != null)
        {
            this.currentImage.Dispose();
            this.currentImage = null;
        }

        this.pdfPathForPresentation = filePathOrPdfPath;
        this.pdfPageNumForPresentation = pageNumIfApplicable;

        try
        {
            if (stitchedImage != null)
            {
                this.currentImage = (Image)stitchedImage.Clone();
            }
            else if (this.pdfPathForPresentation != null && this.pdfPageNumForPresentation >= 0)
            {
                if (this.currentPresPdfDocument != null)
                {
                    this.currentPresPdfDocument.Dispose();
                }
                this.currentPresPdfDocument = PdfDocument.Load(this.pdfPathForPresentation);

                if (this.pdfPageNumForPresentation >= 0 && this.pdfPageNumForPresentation < this.currentPresPdfDocument.PageCount)
                {
                    Rectangle targetBounds = this.displayPanel.ClientSize.Width > 0 ?
                        new Rectangle(0, 0, this.displayPanel.ClientSize.Width, this.displayPanel.ClientSize.Height) :
                        Screen.FromControl(this).Bounds;

                    this.currentImage = RenderPdfPageWithHighQuality(
                        this.currentPresPdfDocument,
                        this.pdfPageNumForPresentation,
                        targetBounds,
                        initialRegion,
                        isRegionNormalized
                    );
                }
                else
                {
                    throw new ArgumentOutOfRangeException("pageNumIfApplicable", "Page number is out of range for the PDF document.");
                }
            }
            else if (File.Exists(this.pdfPathForPresentation))
            {
                if (this.currentPresPdfDocument != null) { this.currentPresPdfDocument.Dispose(); this.currentPresPdfDocument = null; }
                this.currentImage = ImageUtils.LoadImage(this.pdfPathForPresentation);
                if (this.currentImage != null && manualRotationAngle != 0)
                {
                    ImageUtils.ApplyRotation(this.currentImage, manualRotationAngle);
                }
            }
            else
            {
                throw new FileNotFoundException("The specified image or PDF file was not found.", this.pdfPathForPresentation);
            }
        }
        catch (OutOfMemoryException oomEx)
        {
            CopyableMessageBox.Show("Error loading file (image or PDF) for update: Out of memory.\n" + oomEx.Message, "File Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (this.currentImage != null) { this.currentImage.Dispose(); this.currentImage = null; }
            if (this.currentPresPdfDocument != null) { this.currentPresPdfDocument.Dispose(); this.currentPresPdfDocument = null; }
        }
        catch (Exception ex)
        {
            CopyableMessageBox.Show("Error loading file (image or PDF) for update: " + ex.Message, "File Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (this.currentImage != null) { this.currentImage.Dispose(); this.currentImage = null; }
            if (this.currentPresPdfDocument != null) { this.currentPresPdfDocument.Dispose(); this.currentPresPdfDocument = null; }
        }

        this.receivedRegionIsNormalized = isRegionNormalized;
        this.initialSourceRegion = initialRegion;

        if (modeToApply.HasValue)
        {
            this.currentDisplayMode = modeToApply.Value;
            UpdateControlsForDisplayMode(this.currentDisplayMode);
        }

        SetupInitialView(); // This will just invalidate the panel
        this.Activate();
    }

    private void UpdateControlsForDisplayMode(ImageDisplayMode mode)
    {
        // This method is now much simpler as zoomSlider and isPanningAllowed are removed.
        // It could potentially be removed if no other mode-specific UI changes are needed.
        // For now, just ensuring cursor is default.
        if (this.displayPanel != null) this.displayPanel.Cursor = Cursors.Default;

        // Panning is no longer a concept here, so isPanningAllowed logic is removed.
        // Zoom slider is removed, so no enabling/disabling logic needed for it.
    }

    // Add this public method to PresentationForm.cs
    public void ClearDisplay()
    {
        // Dispose and nullify the current display image
        if (this.currentImage != null)
        {
            this.currentImage.Dispose();
            this.currentImage = null;
        }

        // Dispose and nullify the current PDF document, if one was loaded
        if (this.currentPresPdfDocument != null)
        {
            this.currentPresPdfDocument.Dispose();
            this.currentPresPdfDocument = null;
        }

        // Reset zoom and pan to default states // Removed: currentZoom, currentPan no longer exist
        // this.currentZoom = 1.0f;
        // this.currentPan = PointF.Empty;

        // Reset active region tracking for Fit mode // Removed: activeRegion fields no longer exist
        // this.isRegionActiveForFitMode = false;
        // this.activeRegionOriginalWidth = 0f;
        // this.activeRegionOriginalHeight = 0f;

        // Reset initial source region
        this.initialSourceRegion = null;
        this.receivedRegionIsNormalized = false; // Also reset this flag

        // If there's a zoom slider, reset its visual state as well // Removed zoomSlider logic

        // Invalidate the display panel to trigger a repaint
        // The Paint event should handle currentImage == null by clearing to black.
        if (this.displayPanel != null)
        {
            this.displayPanel.Invalidate();
        }
    }

    public void UpdateHighlights(List<List<PointF>> highlights, Color color)
    {
        // Deep copy the highlights list
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

            if (this.currentImage != null)
            {
                this.currentImage.Dispose();
                this.currentImage = null;
            }

        }
        base.Dispose(disposing);
    }
}
}
