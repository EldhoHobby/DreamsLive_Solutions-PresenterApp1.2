using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq; 
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;          // For JsonSerializer (System.Text.Json)
using System.Diagnostics; // Added for Debug.WriteLine
using PdfiumViewer;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DreamsLive_Solutions_PresenterApp1
{
    public partial class MainForm
    {
        private void chkAutoStagePreview_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkAutoStagePreview.Checked)
            {
                btnStageContent_Click(this, EventArgs.Empty);
            }
        }

        private void SyncStagedSelectionToMain()
        {
            if (this.picPreview.Image == null || !this.stagedContentRegion.HasValue) return;

            RectangleF normRegion;
            if (this.stagedContentIsNormalized)
            {
                normRegion = this.stagedContentRegion.Value;
            }
            else
            {
                // Convert pixel region to normalized
                float imgW = this.picPreview.Image.Width;
                float imgH = this.picPreview.Image.Height;
                normRegion = new RectangleF(
                    this.stagedContentRegion.Value.X / imgW,
                    this.stagedContentRegion.Value.Y / imgH,
                    this.stagedContentRegion.Value.Width / imgW,
                    this.stagedContentRegion.Value.Height / imgH
                );
            }

            // Convert normalized to source image pixel coords
            float sourceImgW = this.picPreview.Image.Width;
            float sourceImgH = this.picPreview.Image.Height;
            RectangleF pixelRegionOnSource = new RectangleF(
                normRegion.X * sourceImgW,
                normRegion.Y * sourceImgH,
                normRegion.Width * sourceImgW,
                normRegion.Height * sourceImgH
            );

            this.selectionRectangle = ConvertOriginalImageRectToPreviewRect(pixelRegionOnSource);
            UpdateSelectionSizeLabel();
            this.picPreview.Invalidate();

            // Also update the saved selection for this image
            if (this.selectedImagePath != null)
            {
                List<ImageSelectionData> selections = LoadSelections();
                selections.RemoveAll(s => s.ImagePath.Equals(this.selectedImagePath, StringComparison.OrdinalIgnoreCase));
                selections.Add(new ImageSelectionData(this.selectedImagePath, pixelRegionOnSource));
                SaveSelections(selections);
            }
        }

        public void btnStageContent_Click(object sender, EventArgs e)
        {
            ClearHighlights();
            this.previousSelectionRectangle = Rectangle.Empty;
            this.stagedSelectionRectangle = Rectangle.Empty;

            if (this.stagedStitchedImage != null)
            {
                this.stagedStitchedImage.Dispose();
                this.stagedStitchedImage = null;
            }

            if (string.IsNullOrEmpty(this.selectedImagePath))
            {
                ShowInfoMessage("Please load an image or PDF first.");
                return;
            }
            // picPreview.Image should be loaded if selectedImagePath is valid, via ProcessNewImage
            if (this.picPreview.Image == null)
            {
                ShowWarningMessage("Main preview content is not loaded correctly.");
                return;
            }

            this.stagedContentPath = this.selectedImagePath;
            bool currentMainContentIsPdf = (this.currentPdfDocument != null &&
                                            !string.IsNullOrEmpty(this.selectedImagePath) &&
                                            this.selectedImagePath.ToLowerInvariant().EndsWith(".pdf"));
            this.stagedContentPageNum = currentMainContentIsPdf ? this.currentPageNumber : -1;

            // First, get the selected region in terms of pixels on picPreview.Image
            // GetSelectedRegionInImageCoordinates already accounts for picPreview's SizeMode.Zoom
            RectangleF? regionOnPicPreviewImage = GetSelectedRegionInImageCoordinates();

            if (regionOnPicPreviewImage.HasValue)
            {
                if (currentMainContentIsPdf)
                {
                    // For PDF, picPreview.Image is the rendered page. We need to normalize coordinates from this.
                    // regionOnPicPreviewImage.Value contains pixel coordinates on this.picPreview.Image.
                    RectangleF pixelRegionOnRenderedPdf = regionOnPicPreviewImage.Value;
                    float previewImageWidth = this.picPreview.Image.Width;
                    float previewImageHeight = this.picPreview.Image.Height;

                    if (previewImageWidth > 0 && previewImageHeight > 0)
                    {
                        float normX = pixelRegionOnRenderedPdf.X / previewImageWidth;
                        float normY = pixelRegionOnRenderedPdf.Y / previewImageHeight;
                        float normW = pixelRegionOnRenderedPdf.Width / previewImageWidth;
                        float normH = pixelRegionOnRenderedPdf.Height / previewImageHeight;

                        // Clamp normalized coordinates
                        normX = Math.Max(0f, Math.Min(1f, normX));
                        normY = Math.Max(0f, Math.Min(1f, normY));
                        normW = Math.Max(0f, Math.Min(1f - normX, normW));
                        normH = Math.Max(0f, Math.Min(1f - normY, normH));

                        if (normW > 0.000001F && normH > 0.000001F)
                        {
                            this.stagedContentRegion = new RectangleF(normX, normY, normW, normH);
                            this.stagedContentIsNormalized = true;
                        }
                        else
                        {
                            this.stagedContentRegion = null;
                            this.stagedContentIsNormalized = false;
                        }
                    }
                    else // picPreview.Image has no size, cannot normalize
                    {
                        this.stagedContentRegion = null;
                        this.stagedContentIsNormalized = false;
                    }
                }
                else // Standard image
                {
                    // For standard images, GetSelectedRegionInImageCoordinates() ALREADY returns
                    // coordinates relative to the original image. So, no further normalization needed.
                    this.stagedContentRegion = regionOnPicPreviewImage;
                    this.stagedContentIsNormalized = false;
                }
            }
            else // No region selected on picPreview (GetSelectedRegionInImageCoordinates returned null)
            {
                this.stagedContentRegion = null;
                this.stagedContentIsNormalized = false;
            }

            this.stagedContentRotationAngle = this.currentManualRotationAngle;
            this.stagedSelectionRectangle = this.selectionRectangle;

            // Render this staged content to the secondary preview
            RenderContentToPictureBox(
                this.picSecondaryPreview,
                this.stagedContentPath,
                this.stagedContentPageNum,
                this.stagedContentRegion,
                this.stagedContentIsNormalized,
                this.stagedContentRotationAngle
            );

            this.isSecondaryPreviewPopulated = (this.picSecondaryPreview.Image != null);
            // if (this.btnClearPresenterDisplay != null) // Replaced by UpdateButtonAppearanceAndState
            // {
            //     this.btnClearPresenterDisplay.Enabled = this.isSecondaryPreviewPopulated;
            // }
            UpdateButtonAppearanceAndState();
            UpdateButtonEnableStates();

            UpdateSecondaryPreviewBorderColor();



            // Auto-Update Main Presentation (if linked)
            if (this.chkLinkLocalPreviewToPresenter != null && this.chkLinkLocalPreviewToPresenter.Checked)
            {
                if (this.isSecondaryPreviewPopulated)
                {
                    UpdateMainPresentation(
                        this.stagedContentPath,
                        this.stagedContentPageNum,
                        this.stagedContentRegion,
                        this.stagedContentIsNormalized,
                        this.stagedStitchedImage,
                        this.stagedContentRotationAngle
                    );
                }
                else
                {
                    UpdateMainPresentation(null, -1, null, false, null, 0); // Clear the main presentation
                    // isPresenterShowingLiveContent will be set to false within UpdateMainPresentation if path is null
                }
            }
            // If not auto-linking, staging content means it's ready but not necessarily live yet.
            // The border color update will reflect this (Green).
            // If it *was* auto-linked, UpdateMainPresentation would have set isPresenterShowingLiveContent and liveContent fields.
            // So, just ensure border color is updated based on the outcome.
            UpdateSecondaryPreviewBorderColor();
            this.picPreview.Invalidate();
        }

        // Add this private method to MainForm.cs
        // Assumes 'using System.Drawing;', 'using System.Windows.Forms;', 
        // 'using PdfiumViewer;', 'using System.IO;' are present.
        private void RenderContentToPictureBox(
            PictureBox targetBox,
            string contentPath,
            int pageNumIfPdf, // 0-indexed for PDF, -1 for image
            RectangleF? region, // Can be null (full content), pixel-based, or normalized
            bool isRegionNormalized,
            int manualRotationAngle = 0)
        {
            if (targetBox == null || string.IsNullOrEmpty(contentPath))
            {
                if (targetBox?.Image != null) { targetBox.Image.Dispose(); targetBox.Image = null; }
                return;
            }

            Bitmap sourceBitmap = null; // The full image or full rendered PDF page
            PdfDocument tempPdfDoc = null;

            try
            {
                // --- 1. Load Source Bitmap ---
                if (Path.GetExtension(contentPath).ToLowerInvariant() == ".pdf" && pageNumIfPdf >= 0)
                {
                    PdfDocument docToUse;
                    if (this.currentPdfDocument != null && this.selectedImagePath == contentPath)
                    {
                        docToUse = this.currentPdfDocument;
                    }
                    else
                    {
                        tempPdfDoc = PdfDocument.Load(contentPath);
                        docToUse = tempPdfDoc;
                    }

                    if (pageNumIfPdf >= 0 && pageNumIfPdf < docToUse.PageCount)
                    {
                        float previewRenderDpi = 150f; // Performance: Lower DPI for previews
                        if (docToUse == this.currentPdfDocument)
                            // Reuse the page already rendered for the preview — no second
                            // Pdfium render on the UI thread when staging right after a switch.
                            sourceBitmap = GetRenderedPdfPage(pageNumIfPdf, previewRenderDpi, true);
                        else
                            sourceBitmap = (Bitmap)docToUse.Render(pageNumIfPdf, previewRenderDpi, previewRenderDpi, PdfRenderFlags.Annotations | PdfRenderFlags.LcdText | PdfRenderFlags.CorrectFromDpi);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("pageNumIfPdf", "Page number is out of range.");
                    }
                }
                else if (File.Exists(contentPath)) // Standard image
                {
                    sourceBitmap = (Bitmap)ImageUtils.LoadImage(contentPath);
                }
                else
                {
                    throw new FileNotFoundException("Content file not found.", contentPath);
                }

                if (sourceBitmap == null)
                {
                    // Clear stale master so laser/highlight don't render against a previous image
                    if (this.stagedMasterImage != null) { this.stagedMasterImage.Dispose(); this.stagedMasterImage = null; }
                    return;
                }

                // Apply manual rotation to the source content (Image or PDF render)
                if (manualRotationAngle != 0)
                {
                    ImageUtils.ApplyRotation(sourceBitmap, manualRotationAngle);
                }

                // --- 2. Determine actualSrcRect (in pixels on sourceBitmap) ---
                RectangleF actualSrcRect;
                if (region.HasValue)
                {
                    if (isRegionNormalized)
                    {
                        if (sourceBitmap.Width > 0 && sourceBitmap.Height > 0)
                        {
                            RectangleF normRegion = region.Value;
                            actualSrcRect = new RectangleF(
                                normRegion.X * sourceBitmap.Width,
                                normRegion.Y * sourceBitmap.Height,
                                normRegion.Width * sourceBitmap.Width,
                                normRegion.Height * sourceBitmap.Height
                            );
                        }
                        else
                        {
                            actualSrcRect = new RectangleF(0, 0, sourceBitmap.Width, sourceBitmap.Height);
                        }
                    }
                    else
                    {
                        actualSrcRect = region.Value;
                    }
                }
                else
                {
                    actualSrcRect = new RectangleF(0, 0, sourceBitmap.Width, sourceBitmap.Height);
                }

                // Removed clamping to allow regions outside the image (black bars) as per requirement
                actualSrcRect.Width = Math.Max(0.001F, actualSrcRect.Width);
                actualSrcRect.Height = Math.Max(0.001F, actualSrcRect.Height);

                Bitmap finalBitmapForTarget = CreateFittedBitmap(sourceBitmap, targetBox.ClientSize, targetBox.BackColor, actualSrcRect);

                if (targetBox == this.picSecondaryPreview)
                {
                    if (this.stagedMasterImage != null) this.stagedMasterImage.Dispose();
                    this.stagedMasterImage = (Bitmap)sourceBitmap.Clone();
                }

                if (targetBox.Image != null)
                {
                    targetBox.Image.Dispose();
                }
                targetBox.Image = finalBitmapForTarget;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RenderContentToPictureBox for {contentPath}: {ex.Message}");
                if (targetBox.Image != null) { targetBox.Image.Dispose(); targetBox.Image = null; }
            }
            finally
            {
                if (sourceBitmap != null)
                {
                    sourceBitmap.Dispose();
                }
                if (tempPdfDoc != null)
                {
                    tempPdfDoc.Dispose();
                }
            }
        }

        private void chkLinkLocalPreviewToPresenter_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkLinkLocalPreviewToPresenter.Checked && this.isSecondaryPreviewPopulated)
            {
                // If the checkbox is checked and there's content ready in the secondary preview,
                // update the main presentation to show it.
                UpdateMainPresentation(
                    this.stagedContentPath,
                    this.stagedContentPageNum,
                    this.stagedContentRegion,
                    this.stagedContentIsNormalized,
                    this.stagedStitchedImage,
                    this.stagedContentRotationAngle
                );
            }
            // If unchecked, no immediate action is taken on the main presentation.
            // The link is simply broken for future automatic updates from btnStageContent.
            UpdateButtonEnableStates(); // Update btnPushToPresenter state
        }

        // New name and logic for the method in MainForm.cs

        private bool AreRegionsEqual(RectangleF? region1, RectangleF? region2)
        {
            if (region1.HasValue != region2.HasValue) return false; // One is null, the other isn't
            if (!region1.HasValue) return true; // Both are null

            // Both have values, compare them (allowing for minor float inaccuracies if needed, but direct for now)
            // For simplicity, using direct comparison. For more robust float comparison, an epsilon would be used.
            return region1.Value.X == region2.Value.X &&
                   region1.Value.Y == region2.Value.Y &&
                   region1.Value.Width == region2.Value.Width &&
                   region1.Value.Height == region2.Value.Height;
        }

        private void UpdateSecondaryPreviewBorderColor()
        {
            // This assumes 'panelSecondaryPreviewBorder' is the Panel wrapping picSecondaryPreview
            Control borderControl = this.Controls.Find("panelSecondaryPreviewBorder", true).FirstOrDefault();
            if (borderControl == null || !(borderControl is Panel panelSecondaryPreviewBorder))
            {
                // If the panel isn't found (e.g., not yet created or named differently), try picSecondaryPreview itself.
                // This won't show a border but avoids crashing. Proper setup is in the next step.
                if (this.picSecondaryPreview != null)
                {
                    // No direct border color for PictureBox, perhaps change its BackColor if no image?
                    // For now, do nothing if panel not found, will be handled in panel creation step.
                }
                return;
            }

            // If presenter window is closed, border should always be Gray
            if (this.activePresentationForm == null || this.activePresentationForm.IsDisposed)
            {
                panelSecondaryPreviewBorder.BackColor = Constants.BorderColorDefault; // Gray
                return;
            }

            // Presenter window is open, proceed with previous logic
            if (!isSecondaryPreviewPopulated)
            {
                panelSecondaryPreviewBorder.BackColor = Constants.BorderColorDefault; // Gray
            }
            else // Content is staged
            {
                bool liveContentMatchesStaged =
                    isPresenterShowingLiveContent && // Presenter must be actively showing "live" content
                    this.liveContentPath == this.stagedContentPath &&
                    this.liveContentPageNum == this.stagedContentPageNum &&
                    this.liveContentIsNormalized == this.stagedContentIsNormalized &&
                    this.liveContentIsStitched == (this.stagedStitchedImage != null) &&
                    this.liveContentRotationAngle == this.stagedContentRotationAngle &&
                    AreRegionsEqual(this.liveContentRegion, this.stagedContentRegion);

                if (liveContentMatchesStaged)
                {
                    panelSecondaryPreviewBorder.BackColor = Constants.BorderColorLive; // Red
                }
                else
                {
                    panelSecondaryPreviewBorder.BackColor = Constants.BorderColorStagedNotLive; // Green
                }
            }
        }

        // Event Handlers for picSecondaryPreview interactivity
        private void picSecondaryPreview_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.picSecondaryPreview.Image == null) return;

            if (this.highlighterActive && e.Button == MouseButtons.Left)
            {
                this.isHighlighting = true;
                this.highlightsNormalized.Add(new List<PointF>());
                // Add first point
                this.highlightsNormalized.Last().Add(MapControlToDocNormalized(e.Location));
                this.picSecondaryPreview.Invalidate();
                NotifyPresenterOfHighlights();
            }
            else if ((e.Button == MouseButtons.Right || e.Button == MouseButtons.Left) &&
                     !(this.chkLaserPointer != null && this.chkLaserPointer.Checked))
            {
                this.isPanningSecondaryPreview = true;
                this.secondaryPreviewLastMousePosition = e.Location;
                this.picSecondaryPreview.Cursor = Cursors.SizeAll;
            }
        }

        private void picSecondaryPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isHighlighting && this.highlighterActive && this.picSecondaryPreview.Image != null)
            {
                this.highlightsNormalized.Last().Add(MapControlToDocNormalized(e.Location));
                this.picSecondaryPreview.Invalidate();
                NotifyPresenterOfHighlights();
            }

            if (this.isPanningSecondaryPreview && this.picSecondaryPreview.Image != null)
            {
                float dx = e.Location.X - this.secondaryPreviewLastMousePosition.X;
                float dy = e.Location.Y - this.secondaryPreviewLastMousePosition.Y;

                this.secondaryPreviewPan.X -= dx / this.secondaryPreviewZoom;
                this.secondaryPreviewPan.Y -= dy / this.secondaryPreviewZoom;

                this.secondaryPreviewLastMousePosition = e.Location;
                this.picSecondaryPreview.Invalidate();
            }

            if (this.chkLaserPointer != null && this.chkLaserPointer.Checked && this.picSecondaryPreview.Image != null)
            {
                this.laserPointNormalized = MapControlToDocNormalized(e.Location);
                this.picSecondaryPreview.Invalidate();
                NotifyPresenterOfLaserPoint();
            }
            else if (this.laserPointNormalized != null)
            {
                this.laserPointNormalized = null;
                this.picSecondaryPreview.Invalidate();
                NotifyPresenterOfLaserPoint();
            }
        }

        private void picSecondaryPreview_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.isPanningSecondaryPreview && (e.Button == MouseButtons.Right || e.Button == MouseButtons.Left))
            {
                this.isPanningSecondaryPreview = false;
                this.picSecondaryPreview.Cursor = Cursors.Default;
                UpdateStagedContentRegionFromInteraction();
                this.picSecondaryPreview.Invalidate();
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                this.isHighlighting = false;
                this.picSecondaryPreview.Invalidate();
            }
        }

        private void picSecondaryPreview_MouseWheel(object sender, MouseEventArgs e)
        {
            if (this.picSecondaryPreview.Image == null) return;

            float oldZoom = this.secondaryPreviewZoom;
            float newZoom;

            if (e.Delta > 0)
                newZoom = oldZoom * 1.25f; // Zoom in
            else
                newZoom = oldZoom / 1.25f; // Zoom out

            // Clamp zoom factor
            newZoom = Math.Max(0.1f, Math.Min(newZoom, 10.0f)); // Example limits: 0.1x to 10x

            if (Math.Abs(newZoom - oldZoom) < 0.001f) return; // No significant change

            // Get mouse position relative to the control
            PointF mousePos = e.Location;

            // Calculate image coordinates under the mouse before zoom
            // This requires knowing the current displayed rectangle of the image content
            // For simplicity, we'll zoom towards the center of the view if the content is smaller than the view,
            // or towards the mouse pointer if content is larger/zoomed.

            // Transform mouse pointer coordinates from control space to image space (pre-zoom)
            // This is a simplified version. A more accurate one would consider the current pan and zoom.
            // (mousePos.X - pan.X*oldZoom) / oldZoom = imageX
            // (mousePos.Y - pan.Y*oldZoom) / oldZoom = imageY

            // For a simpler zoom-towards-mouse:
            // Calculate the point in the *unzoomed* image that is currently under the mouse cursor.
            // This needs to account for the current pan.
            // The 'secondaryPreviewPan' is in original image coordinates.
            // The image displayed in picSecondaryPreview is a *portion* of the original,
            // potentially scaled to fit the PictureBox.

            // Let's adjust pan to keep the point under the mouse stationary.
            // PointF imagePointBeforeZoom = new PointF(
            // (mousePos.X - (this.picSecondaryPreview.ClientSize.Width / 2f + this.secondaryPreviewPan.X * oldZoom)) / oldZoom,
            // (mousePos.Y - (this.picSecondaryPreview.ClientSize.Height / 2f + this.secondaryPreviewPan.Y * oldZoom)) / oldZoom
            // );

            // Simpler approach: Adjust pan based on zoom change around mouse point
            // This logic ensures that the point under the cursor stays under the cursor after zooming.
            // It transforms the mouse position to "world" coordinates (image coordinates before current pan/zoom),
            // then calculates where the new top-left of the view (pan point) should be to keep that world point
            // at the same screen position after zoom.

            // Convert mouse screen coords to coords on the (potentially panned & zoomed) image content
            // This needs a robust transformation. The current secondaryPreviewPan is in original image pixels.
            // The rendering in picSecondaryPreview_Paint will need to consider this.

            // Let's simplify: The zoom operation will adjust the view into the original image.
            // The pan is relative to the top-left of the original image.
            // When zooming, we want the point under the mouse to remain the same.

            // Point in original image pixels that is currently under the mouse
            // This is complex because RenderContentToPictureBox fits a region.
            // For now, let's do a simpler zoom towards center, then refine if needed.
            // Or, more simply, the zoom changes how much of the original image is seen.
            // The pan changes *which* part of the original image is seen.

            // --- Zoom towards mouse logic ---
            PointF mouseRelativeToControl = this.picSecondaryPreview.PointToClient(Cursor.Position);

            // Point in the content (picSecondaryPreview.Image) that is currently under the mouse
            float contentMouseX = this.secondaryPreviewPan.X + mouseRelativeToControl.X / oldZoom;
            float contentMouseY = this.secondaryPreviewPan.Y + mouseRelativeToControl.Y / oldZoom;

            // Update zoom
            this.secondaryPreviewZoom = newZoom;

            // Calculate new pan so the contentMouse point remains under mouseRelativeToControl
            this.secondaryPreviewPan.X = contentMouseX - mouseRelativeToControl.X / this.secondaryPreviewZoom;
            this.secondaryPreviewPan.Y = contentMouseY - mouseRelativeToControl.Y / this.secondaryPreviewZoom;

            // Boundary checks for pan
            if (this.picSecondaryPreview.Image != null)
            {
                float contentWidth = this.picSecondaryPreview.Image.Width;
                float contentHeight = this.picSecondaryPreview.Image.Height;

                float viewWidthInContentCoords = this.picSecondaryPreview.ClientSize.Width / this.secondaryPreviewZoom;
                float viewHeightInContentCoords = this.picSecondaryPreview.ClientSize.Height / this.secondaryPreviewZoom;

                // Clamp Pan X
                if (viewWidthInContentCoords >= contentWidth) // Content is narrower than or fits the view
                {
                    this.secondaryPreviewPan.X = (contentWidth - viewWidthInContentCoords) / 2; // Center
                }
                else // Content is wider than the view (zoomed in)
                {
                    this.secondaryPreviewPan.X = Math.Max(0, Math.Min(this.secondaryPreviewPan.X, contentWidth - viewWidthInContentCoords));
                }

                // Clamp Pan Y
                if (viewHeightInContentCoords >= contentHeight) // Content is shorter than or fits the view
                {
                    this.secondaryPreviewPan.Y = (contentHeight - viewHeightInContentCoords) / 2; // Center
                }
                else // Content is taller than the view (zoomed in)
                {
                    this.secondaryPreviewPan.Y = Math.Max(0, Math.Min(this.secondaryPreviewPan.Y, contentHeight - viewHeightInContentCoords));
                }
            }

            this.picSecondaryPreview.Invalidate(); // Request repaint
            UpdateStagedContentRegionFromInteraction(); // Update staged region after zoom
        }

        private void picSecondaryPreview_Paint(object sender, PaintEventArgs e)
        {
            if (sender == this.picPreview) // From original Paint handler for picPreview selection
            {
                if (this.selectionRectangle.Width > 0 && this.selectionRectangle.Height > 0)
                {
                    using (Pen selectionPen = new Pen(Color.Red, 2))
                    {
                        e.Graphics.DrawRectangle(selectionPen, this.selectionRectangle);
                    }
                }
            }
            else if (sender == this.picSecondaryPreview) // Unified Painting for interactive picSecondaryPreview
            {
                if (this.picSecondaryPreview.Image == null && this.stagedMasterImage == null && this.stagedStitchedImage == null)
                {
                    e.Graphics.Clear(this.picSecondaryPreview.BackColor);
                    return;
                }

                e.Graphics.Clear(this.picSecondaryPreview.BackColor);

                // --- 1. Draw Base Image ---
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                // Priority: Stitched Image > Master Image > Fitted Image
                Image baseImage = this.stagedStitchedImage ?? this.stagedMasterImage ?? this.picSecondaryPreview.Image;

                if (baseImage != null)
                {
                    if (baseImage == this.picSecondaryPreview.Image)
                    {
                        // Use legacy pan/zoom logic for the fitted image
                        float viewWidthUnzoomed = this.picSecondaryPreview.ClientSize.Width / this.secondaryPreviewZoom;
                        float viewHeightUnzoomed = this.picSecondaryPreview.ClientSize.Height / this.secondaryPreviewZoom;
                        RectangleF srcRect = new RectangleF(this.secondaryPreviewPan.X, this.secondaryPreviewPan.Y, viewWidthUnzoomed, viewHeightUnzoomed);
                        RectangleF destRect = new RectangleF(0, 0, this.picSecondaryPreview.ClientSize.Width, this.picSecondaryPreview.ClientSize.Height);
                        if (viewWidthUnzoomed > 0 && viewHeightUnzoomed > 0)
                            e.Graphics.DrawImage(baseImage, destRect, srcRect, GraphicsUnit.Pixel);
                    }
                    else
                    {
                        // Map control viewport to normalized doc coordinates to use high-res master/stitched image
                        PointF topLeftNorm = MapControlToDocNormalized(new Point(0, 0));
                        PointF bottomRightNorm = MapControlToDocNormalized(new Point(this.picSecondaryPreview.ClientSize.Width, this.picSecondaryPreview.ClientSize.Height));

                        RectangleF srcRectNorm = new RectangleF(
                            topLeftNorm.X, topLeftNorm.Y,
                            bottomRightNorm.X - topLeftNorm.X,
                            bottomRightNorm.Y - topLeftNorm.Y
                        );

                        RectangleF srcRectPixels = new RectangleF(
                            srcRectNorm.X * baseImage.Width,
                            srcRectNorm.Y * baseImage.Height,
                            srcRectNorm.Width * baseImage.Width,
                            srcRectNorm.Height * baseImage.Height
                        );

                        RectangleF destRect = new RectangleF(0, 0, this.picSecondaryPreview.ClientSize.Width, this.picSecondaryPreview.ClientSize.Height);
                        e.Graphics.DrawImage(baseImage, destRect, srcRectPixels, GraphicsUnit.Pixel);
                    }
                }

                // --- 2. Draw Annotations (Highlighter & Laser) ---
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                if (this.highlightsNormalized != null && this.highlightsNormalized.Count > 0)
                {
                    foreach (var stroke in this.highlightsNormalized)
                    {
                        if (stroke.Count < 1) continue;
                        PointF[] points = stroke.Select(p => MapDocNormalizedToControl(p)).ToArray();
                        DrawHighlightStroke(e.Graphics, points, 20f * this.secondaryPreviewZoom, this.highlighterColor);
                    }
                }

                if (this.laserPointNormalized != null)
                {
                    PointF center = MapDocNormalizedToControl(this.laserPointNormalized.Value);
                    DrawLaserPointer(e.Graphics, center, 5f);
                }
            }
        }

        // Helper to update stagedContentRegion based on pan/zoom of picSecondaryPreview
        private void UpdateStagedContentRegionFromInteraction()
        {
            if (this.picSecondaryPreview.Image == null || stagedContentPath == null) return;

            bool isPdf = Path.GetExtension(stagedContentPath).ToLowerInvariant() == ".pdf";
            // Check if the STAGED content represents the full page. 
            // This is true if stagedContentRegion is currently null (meaning the last completed operation resulted in a full page view).
            bool currentStagedViewIsFullPage = !this.stagedContentRegion.HasValue;

            if (isPdf && currentStagedViewIsFullPage)
            {
                // Get current bitmap dimensions (it shows the fitted full page)
                float currentImageBitmapWidth = this.picSecondaryPreview.Image.Width;
                float currentImageBitmapHeight = this.picSecondaryPreview.Image.Height;

                if (currentImageBitmapWidth > 0 && currentImageBitmapHeight > 0) // Ensure bitmap is valid
                {
                    // Calculate the viewport the user *wants* to see on the current bitmap, based on interactive zoom
                    float desiredViewportWidthOnBitmap = this.picSecondaryPreview.ClientSize.Width / this.secondaryPreviewZoom;
                    float desiredViewportHeightOnBitmap = this.picSecondaryPreview.ClientSize.Height / this.secondaryPreviewZoom;

                    // Condition for "Zoomed Out Beyond Fit" - user wants to see area >= current bitmap
                    // Use a small tolerance (e.g., 1.5f pixels for ClientSize vs Image size, and some margin for desiredViewport)
                    bool wantsFullPageOrMore =
                        (desiredViewportWidthOnBitmap >= currentImageBitmapWidth - 1.5f &&
                         desiredViewportHeightOnBitmap >= currentImageBitmapHeight - 1.5f); // Simplified: if desired view is >= current view in both dimensions

                    if (wantsFullPageOrMore)
                    {
                        Debug.WriteLine("PDF Full Zoom-Out Override: Explicitly setting stagedContentRegion to null and returning.");
                        this.stagedContentRegion = null;
                        this.stagedContentIsNormalized = false;

                        // Perform the rendering and UI updates directly and EXIT the method
                        RenderContentToPictureBox(
                            this.picSecondaryPreview,
                            this.stagedContentPath,
                            this.stagedContentPageNum,
                            null, // Force full page render
                            false
                        );
                        this.secondaryPreviewPan = PointF.Empty;
                        this.secondaryPreviewZoom = 1.0f;
                        this.picSecondaryPreview.Invalidate();

                        if (this.chkLinkLocalPreviewToPresenter.Checked)
                        {
                        UpdateMainPresentation(this.stagedContentPath, this.stagedContentPageNum, null, false, null, 0);
                        }
                        UpdateButtonEnableStates();
                        UpdateSecondaryPreviewBorderColor();
                        return; // *** CRITICAL: Exit early ***
                    }
                }
            }

            // If the PDF full zoom-out override was not taken, proceed with normal logic.
            // `this.picSecondaryPreview.Image` is the current base image.
            // Its dimensions are clientWidth, clientHeight because RenderContentToPictureBox makes it so.
            float currentImageWidth = this.picSecondaryPreview.Image.Width;  // Should be picSecondaryPreview.ClientSize.Width
            float currentImageHeight = this.picSecondaryPreview.Image.Height; // Should be picSecondaryPreview.ClientSize.Height

            if (currentImageWidth <= 0 || currentImageHeight <= 0) return; // Defensive check, though covered by early check too

            // This is the view into `this.picSecondaryPreview.Image` based on interactive pan/zoom
            float visibleRegionX_on_currentImage = this.secondaryPreviewPan.X;
            float visibleRegionY_on_currentImage = this.secondaryPreviewPan.Y;
            float visibleRegionWidth_on_currentImage = this.picSecondaryPreview.ClientSize.Width / this.secondaryPreviewZoom;
            float visibleRegionHeight_on_currentImage = this.picSecondaryPreview.ClientSize.Height / this.secondaryPreviewZoom;

            RectangleF newRegionInOriginalFileCoords;

            // Get dimensions of the *original* file/PDF page
            float actualOriginalFileWidth = 0;
            float actualOriginalFileHeight = 0;
            // isPdf is defined at the top of the method.

            try
            {
                if (this.stagedMasterImage != null)
                {
                    actualOriginalFileWidth = this.stagedMasterImage.Width;
                    actualOriginalFileHeight = this.stagedMasterImage.Height;
                }
                else if (isPdf && stagedContentPageNum >= 0)
                {
                    // For the 'else' block (full page view), we MUST use the actual rendered dimensions
                    // as the basis for fitScale_full and subsequent clamping, because RenderContentToPictureBox
                    // will use a sourceBitmap of these dimensions.
                    // This ensures consistency.
                    PdfDocument docToUse = null;
                    PdfDocument tempDoc = null;
                    if (this.currentPdfDocument != null && this.selectedImagePath == stagedContentPath)
                    {
                        docToUse = this.currentPdfDocument;
                    }
                    else
                    {
                        tempDoc = PdfDocument.Load(stagedContentPath);
                        docToUse = tempDoc;
                    }

                    try
                    {
                        if (stagedContentPageNum < docToUse.PageCount)
                        {
                            float previewRenderDpi = 150f; // Consistent with RenderContentToPictureBox
                            using (var tempBitmap = (Bitmap)docToUse.Render(stagedContentPageNum, previewRenderDpi, previewRenderDpi, true))
                            {
                                actualOriginalFileWidth = tempBitmap.Width;
                                actualOriginalFileHeight = tempBitmap.Height;
                            }
                            Debug.WriteLine($"UCSRI (Full Page Path): PDF actual rendered dims for calculation: {actualOriginalFileWidth}x{actualOriginalFileHeight}");
                        }
                        else { Debug.WriteLine("UCSRI: Invalid page num"); return; }
                    }
                    finally
                    {
                        if (tempDoc != null) tempDoc.Dispose();
                    }
                }
                else if (!isPdf) // Standard image
                {
                    using (var img = Image.FromFile(this.stagedContentPath))
                    {
                        actualOriginalFileWidth = img.Width;
                        actualOriginalFileHeight = img.Height;
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine("Error getting original file dimensions: " + ex.Message); return; }

            if (actualOriginalFileWidth <= 0 || actualOriginalFileHeight <= 0) { Debug.WriteLine("Original file dimensions are zero or invalid."); return; }


            if (this.stagedContentRegion.HasValue && !this.stagedContentIsNormalized) // Initial region was specific pixels on original
            {
                RectangleF initialPixelRegionOnOriginal = this.stagedContentRegion.Value;
                // Ensure initialPixelRegionOnOriginal has positive dimensions before division
                if (initialPixelRegionOnOriginal.Width <= 0 || initialPixelRegionOnOriginal.Height <= 0) { Debug.WriteLine("Initial pixel region has zero dimension."); return; }

                float fitScale = Math.Min(currentImageWidth / initialPixelRegionOnOriginal.Width, currentImageHeight / initialPixelRegionOnOriginal.Height);

                float displayedInitialWidth = initialPixelRegionOnOriginal.Width * fitScale;
                float displayedInitialHeight = initialPixelRegionOnOriginal.Height * fitScale;
                float offsetX = (currentImageWidth - displayedInitialWidth) / 2f;
                float offsetY = (currentImageHeight - displayedInitialHeight) / 2f;

                float visibleX_on_displayedInitial = visibleRegionX_on_currentImage - offsetX;
                float visibleY_on_displayedInitial = visibleRegionY_on_currentImage - offsetY;

                newRegionInOriginalFileCoords = new RectangleF(
                    initialPixelRegionOnOriginal.X + (visibleX_on_displayedInitial / fitScale),
                    initialPixelRegionOnOriginal.Y + (visibleY_on_displayedInitial / fitScale),
                    visibleRegionWidth_on_currentImage / fitScale,
                    visibleRegionHeight_on_currentImage / fitScale
                );
                // stagedContentIsNormalized remains false
            }
            else if (this.stagedContentRegion.HasValue && this.stagedContentIsNormalized) // Initial region was normalized
            {
                RectangleF initialNormalizedRegion = this.stagedContentRegion.Value;
                // Ensure initialNormalizedRegion has positive dimensions before use
                if (initialNormalizedRegion.Width <= 0 || initialNormalizedRegion.Height <= 0) { Debug.WriteLine("Initial normalized region has zero dimension."); return; }

                float initialPixelRegionX = initialNormalizedRegion.X * actualOriginalFileWidth;
                float initialPixelRegionY = initialNormalizedRegion.Y * actualOriginalFileHeight;
                float initialPixelRegionWidth = initialNormalizedRegion.Width * actualOriginalFileWidth;
                float initialPixelRegionHeight = initialNormalizedRegion.Height * actualOriginalFileHeight;
                Debug.WriteLineIf(isPdf && this.stagedContentIsNormalized, $"PDF NormToPixel: InitialPixelRegion {initialPixelRegionX}x{initialPixelRegionY} {initialPixelRegionWidth}x{initialPixelRegionHeight}");


                if (initialPixelRegionWidth <= 0 || initialPixelRegionHeight <= 0) { Debug.WriteLine("Initial pixel region from normalized has zero dimension."); return; }

                float fitScale = Math.Min(currentImageWidth / initialPixelRegionWidth, currentImageHeight / initialPixelRegionHeight);
                if (fitScale <= 0) { Debug.WriteLine("fitScale is zero or negative in normalized case."); return; }
                Debug.WriteLineIf(isPdf && this.stagedContentIsNormalized, $"PDF NormToPixel: fitScale={fitScale}");


                float displayedInitialWidth = initialPixelRegionWidth * fitScale;
                float displayedInitialHeight = initialPixelRegionHeight * fitScale;
                float offsetX = (currentImageWidth - displayedInitialWidth) / 2f;
                float offsetY = (currentImageHeight - displayedInitialHeight) / 2f;

                float visibleX_on_displayedInitial = visibleRegionX_on_currentImage - offsetX;
                float visibleY_on_displayedInitial = visibleRegionY_on_currentImage - offsetY;

                float newPixelX = initialPixelRegionX + (visibleX_on_displayedInitial / fitScale);
                float newPixelY = initialPixelRegionY + (visibleY_on_displayedInitial / fitScale);
                float newPixelWidth = visibleRegionWidth_on_currentImage / fitScale;
                float newPixelHeight = visibleRegionHeight_on_currentImage / fitScale;
                Debug.WriteLineIf(isPdf && this.stagedContentIsNormalized, $"PDF NormToPixel: newPixelRegion (before norm) {newPixelX}x{newPixelY} {newPixelWidth}x{newPixelHeight}");


                newRegionInOriginalFileCoords = new RectangleF(
                    newPixelX / actualOriginalFileWidth,
                    newPixelY / actualOriginalFileHeight,
                    newPixelWidth / actualOriginalFileWidth,
                    newPixelHeight / actualOriginalFileHeight
                );
                Debug.WriteLineIf(isPdf && this.stagedContentIsNormalized, $"PDF NormToPixel: newRegionInOriginalFileCoords (normalized) {newRegionInOriginalFileCoords}");
                // stagedContentIsNormalized remains true
            }
            else // No initial region (stagedContentRegion was null)
            {
                if (actualOriginalFileWidth <= 0 || actualOriginalFileHeight <= 0) { Debug.WriteLine("Actual original file dimensions zero in 'no initial region' case."); return; }
                float fitScale_full = Math.Min(currentImageWidth / actualOriginalFileWidth, currentImageHeight / actualOriginalFileHeight);
                if (fitScale_full <= 0) { Debug.WriteLine("fitScale_full is zero or negative."); return; } // Avoid division by zero

                float displayedFullContentWidth = actualOriginalFileWidth * fitScale_full;
                float displayedFullContentHeight = actualOriginalFileHeight * fitScale_full;
                float offsetX_full = (currentImageWidth - displayedFullContentWidth) / 2f;
                float offsetY_full = (currentImageHeight - displayedFullContentHeight) / 2f;

                float visibleX_on_displayedFull = visibleRegionX_on_currentImage - offsetX_full;
                float visibleY_on_displayedFull = visibleRegionY_on_currentImage - offsetY_full;

                newRegionInOriginalFileCoords = new RectangleF(
                    visibleX_on_displayedFull / fitScale_full,
                    visibleY_on_displayedFull / fitScale_full,
                    visibleRegionWidth_on_currentImage / fitScale_full,
                    visibleRegionHeight_on_currentImage / fitScale_full
                );
                this.stagedContentIsNormalized = false;
            }

            // Clamp the newRegionInOriginalFileCoords
            Debug.WriteLineIf(isPdf, $"PDF PreClamp Region: {newRegionInOriginalFileCoords}, IsNormalized: {this.stagedContentIsNormalized}");
            if (this.stagedContentIsNormalized)
            {
                float minNormDim = 0.0001f; // Minimum normalized dimension
                newRegionInOriginalFileCoords.X = Math.Max(0f, newRegionInOriginalFileCoords.X);
                newRegionInOriginalFileCoords.Y = Math.Max(0f, newRegionInOriginalFileCoords.Y);

                // Clamp X and Y to allow for minimum dimension
                newRegionInOriginalFileCoords.X = Math.Min(newRegionInOriginalFileCoords.X, 1f - minNormDim);
                newRegionInOriginalFileCoords.Y = Math.Min(newRegionInOriginalFileCoords.Y, 1f - minNormDim);

                newRegionInOriginalFileCoords.Width = Math.Max(minNormDim, newRegionInOriginalFileCoords.Width);
                newRegionInOriginalFileCoords.Height = Math.Max(minNormDim, newRegionInOriginalFileCoords.Height);

                // Clamp width and height against the (1.0 - X) and (1.0 - Y) boundaries
                newRegionInOriginalFileCoords.Width = Math.Min(newRegionInOriginalFileCoords.Width, 1f - newRegionInOriginalFileCoords.X);
                newRegionInOriginalFileCoords.Height = Math.Min(newRegionInOriginalFileCoords.Height, 1f - newRegionInOriginalFileCoords.Y);

                // Ensure minimum dimensions again after all clamping
                newRegionInOriginalFileCoords.Width = Math.Max(minNormDim, newRegionInOriginalFileCoords.Width);
                newRegionInOriginalFileCoords.Height = Math.Max(minNormDim, newRegionInOriginalFileCoords.Height);
            }
            else // Pixel coordinates - Revised clamping to preserve aspect ratio
            {
                float minPixelDim = 0.1f;
                float targetAR = (this.picSecondaryPreview.ClientSize.Height == 0) ? 1.0f : (float)this.picSecondaryPreview.ClientSize.Width / this.picSecondaryPreview.ClientSize.Height;

                Debug.WriteLine($"Pixel Clamping - Before: X={newRegionInOriginalFileCoords.X:F2} Y={newRegionInOriginalFileCoords.Y:F2} W={newRegionInOriginalFileCoords.Width:F2} H={newRegionInOriginalFileCoords.Height:F2} TargetAR={targetAR:F2} OriginalFileDims={actualOriginalFileWidth:F2}x{actualOriginalFileHeight:F2}");

                // Ensure initial dimensions are positive
                newRegionInOriginalFileCoords.Width = Math.Max(minPixelDim, newRegionInOriginalFileCoords.Width);
                newRegionInOriginalFileCoords.Height = Math.Max(minPixelDim, newRegionInOriginalFileCoords.Height);

                // Adjust to target aspect ratio first, preferring to keep area or one dimension.
                // If current aspect ratio is wider than target, reduce width or increase height.
                // If current aspect ratio is narrower than target, increase width or reduce height.
                // Let's adjust height based on width and targetAR first.
                newRegionInOriginalFileCoords.Height = newRegionInOriginalFileCoords.Width / targetAR;

                // If this new height makes the region exceed original file height, then width must be adjusted based on max height.
                if (newRegionInOriginalFileCoords.Height > actualOriginalFileHeight)
                {
                    newRegionInOriginalFileCoords.Height = actualOriginalFileHeight;
                    newRegionInOriginalFileCoords.Width = newRegionInOriginalFileCoords.Height * targetAR;
                }
                // If this new width (either original or adjusted) makes the region exceed original file width, adjust again.
                if (newRegionInOriginalFileCoords.Width > actualOriginalFileWidth)
                {
                    newRegionInOriginalFileCoords.Width = actualOriginalFileWidth;
                    newRegionInOriginalFileCoords.Height = newRegionInOriginalFileCoords.Width / targetAR;
                }

                // Ensure dimensions are still at least minPixelDim after aspect ratio adjustments and potential shrinking
                newRegionInOriginalFileCoords.Width = Math.Max(minPixelDim, newRegionInOriginalFileCoords.Width);
                newRegionInOriginalFileCoords.Height = Math.Max(minPixelDim, newRegionInOriginalFileCoords.Height);

                // Now clamp X and Y position
                newRegionInOriginalFileCoords.X = Math.Max(0f, newRegionInOriginalFileCoords.X);
                newRegionInOriginalFileCoords.X = Math.Min(newRegionInOriginalFileCoords.X, actualOriginalFileWidth - newRegionInOriginalFileCoords.Width);

                newRegionInOriginalFileCoords.Y = Math.Max(0f, newRegionInOriginalFileCoords.Y);
                newRegionInOriginalFileCoords.Y = Math.Min(newRegionInOriginalFileCoords.Y, actualOriginalFileHeight - newRegionInOriginalFileCoords.Height);

                // Final check on X, Y (could become negative if width/height were clamped to full original size and X/Y were initially off)
                newRegionInOriginalFileCoords.X = Math.Max(0f, newRegionInOriginalFileCoords.X);
                newRegionInOriginalFileCoords.Y = Math.Max(0f, newRegionInOriginalFileCoords.Y);

                Debug.WriteLine($"Pixel Clamping - After: X={newRegionInOriginalFileCoords.X:F2} Y={newRegionInOriginalFileCoords.Y:F2} W={newRegionInOriginalFileCoords.Width:F2} H={newRegionInOriginalFileCoords.Height:F2}");
            }

            this.stagedContentRegion = newRegionInOriginalFileCoords;

            RenderContentToPictureBox(
                this.picSecondaryPreview,
                this.stagedContentPath,
                this.stagedContentPageNum,
                this.stagedContentRegion,
                this.stagedContentIsNormalized,
                this.stagedContentRotationAngle
            );

            this.secondaryPreviewPan = PointF.Empty;
            this.secondaryPreviewZoom = 1.0f;
            this.picSecondaryPreview.Invalidate();

            UpdateButtonEnableStates();
            UpdateSecondaryPreviewBorderColor();

            // If linked, push the changes to the main presenter
            if (this.chkLinkLocalPreviewToPresenter.Checked)
            {
                UpdateMainPresentation(
                    this.stagedContentPath,
                    this.stagedContentPageNum,
                    this.stagedContentRegion,
                    this.stagedContentIsNormalized,
                    null,
                    this.stagedContentRotationAngle
                );
            }
        }

        private void picSecondaryPreview_MouseEnter(object sender, EventArgs e)
        {
            if (this.picSecondaryPreview.Image != null && !isPanningSecondaryPreview)
            {
                this.picSecondaryPreview.Cursor = Cursors.Hand; // Open hand cursor
            }
        }

        private void picSecondaryPreview_MouseLeave(object sender, EventArgs e)
        {
            if (!isPanningSecondaryPreview) // Don't change if currently panning (though mouse leave might interrupt pan)
            {
                this.picSecondaryPreview.Cursor = Cursors.Default;
            }

            if (this.laserPointNormalized != null)
            {
                this.laserPointNormalized = null;
                this.picSecondaryPreview.Invalidate();
                NotifyPresenterOfLaserPoint();
            }
        }

        private PointF MapControlToDocNormalized(Point eLocation)
        {
            float fittedX = (eLocation.X / this.secondaryPreviewZoom) + this.secondaryPreviewPan.X;
            float fittedY = (eLocation.Y / this.secondaryPreviewZoom) + this.secondaryPreviewPan.Y;

            RectangleF contentRect = GetSecondaryPreviewContentRect();
            if (contentRect.Width == 0 || contentRect.Height == 0) return PointF.Empty;

            float relX = (fittedX - contentRect.X) / contentRect.Width;
            float relY = (fittedY - contentRect.Y) / contentRect.Height;

            // relX, relY are 0..1 relative to the STAGED content (selection or full page)
            if (this.stagedStitchedImage != null)
            {
                // Stitched image is treated as the full document
                return new PointF(relX, relY);
            }

            if (this.stagedContentRegion.HasValue)
            {
                RectangleF region = this.stagedContentRegion.Value;
                if (this.stagedContentIsNormalized)
                {
                    return new PointF(region.X + relX * region.Width, region.Y + relY * region.Height);
                }
                else
                {
                    float fullWidth = this.stagedMasterImage?.Width ?? 1;
                    float fullHeight = this.stagedMasterImage?.Height ?? 1;
                    return new PointF(
                        (region.X + relX * region.Width) / fullWidth,
                        (region.Y + relY * region.Height) / fullHeight
                    );
                }
            }

            return new PointF(relX, relY);
        }

        private PointF MapDocNormalizedToControl(PointF normDocPoint)
        {
            float relX = normDocPoint.X;
            float relY = normDocPoint.Y;

            if (this.stagedStitchedImage == null && this.stagedContentRegion.HasValue)
            {
                RectangleF region = this.stagedContentRegion.Value;
                if (this.stagedContentIsNormalized)
                {
                    relX = (normDocPoint.X - region.X) / (region.Width != 0 ? region.Width : 1);
                    relY = (normDocPoint.Y - region.Y) / (region.Height != 0 ? region.Height : 1);
                }
                else
                {
                    float fullWidth = this.stagedMasterImage?.Width ?? 1;
                    float fullHeight = this.stagedMasterImage?.Height ?? 1;
                    relX = (normDocPoint.X * fullWidth - region.X) / (region.Width != 0 ? region.Width : 1);
                    relY = (normDocPoint.Y * fullHeight - region.Y) / (region.Height != 0 ? region.Height : 1);
                }
            }

            RectangleF contentRect = GetSecondaryPreviewContentRect();
            float fittedX = relX * contentRect.Width + contentRect.X;
            float fittedY = relY * contentRect.Height + contentRect.Y;

            return new PointF(
                (fittedX - this.secondaryPreviewPan.X) * this.secondaryPreviewZoom,
                (fittedY - this.secondaryPreviewPan.Y) * this.secondaryPreviewZoom
            );
        }

        private RectangleF GetSecondaryPreviewContentRect()
        {
            if (this.picSecondaryPreview.Image == null) return RectangleF.Empty;

            float boxWidth = this.picSecondaryPreview.ClientSize.Width;
            float boxHeight = this.picSecondaryPreview.ClientSize.Height;

            if (boxWidth <= 0 || boxHeight <= 0) return RectangleF.Empty;

            float contentWidth, contentHeight;

            if (this.stagedStitchedImage != null)
            {
                contentWidth = this.stagedStitchedImage.Width;
                contentHeight = this.stagedStitchedImage.Height;
            }
            else if (this.stagedContentRegion.HasValue)
            {
                RectangleF region = this.stagedContentRegion.Value;
                if (this.stagedContentIsNormalized)
                {
                    float baseW = this.stagedMasterImage?.Width ?? 1;
                    float baseH = this.stagedMasterImage?.Height ?? 1;
                    contentWidth = region.Width * baseW;
                    contentHeight = region.Height * baseH;
                }
                else
                {
                    contentWidth = region.Width;
                    contentHeight = region.Height;
                }
            }
            else
            {
                contentWidth = this.stagedMasterImage?.Width ?? 1;
                contentHeight = this.stagedMasterImage?.Height ?? 1;
            }

            if (contentWidth <= 0 || contentHeight <= 0) return RectangleF.Empty;

            float ratio = Math.Min(boxWidth / contentWidth, boxHeight / contentHeight);
            float w = contentWidth * ratio;
            float h = contentHeight * ratio;
            float x = (boxWidth - w) / 2f;
            float y = (boxHeight - h) / 2f;

            return new RectangleF(x, y, w, h);
        }

        private Bitmap CreateFittedBitmap(Image sourceImage, Size targetSize, Color backColor, RectangleF? sourceRegion = null)
        {
            if (sourceImage == null || targetSize.Width <= 0 || targetSize.Height <= 0) return null;

            RectangleF actualSrcRect = sourceRegion ?? new RectangleF(0, 0, sourceImage.Width, sourceImage.Height);

            // Ensure dimensions are positive. Negative X/Y is allowed for padding.
            actualSrcRect.Width = Math.Max(0.001F, actualSrcRect.Width);
            actualSrcRect.Height = Math.Max(0.001F, actualSrcRect.Height);

            Bitmap fittedBitmap = new Bitmap(targetSize.Width, targetSize.Height);
            using (Graphics g = Graphics.FromImage(fittedBitmap))
            {
                g.Clear(backColor);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear; // Performance: Faster interpolation

                float targetWidth = targetSize.Width;
                float targetHeight = targetSize.Height;

                float zoomToFit = Math.Min(targetWidth / actualSrcRect.Width, targetHeight / actualSrcRect.Height);

                float scaledWidth = actualSrcRect.Width * zoomToFit;
                float scaledHeight = actualSrcRect.Height * zoomToFit;

                float destX = (targetWidth - scaledWidth) / 2.0F;
                float destY = (targetHeight - scaledHeight) / 2.0F;

                RectangleF destRect = new RectangleF(destX, destY, scaledWidth, scaledHeight);
                g.DrawImage(sourceImage, destRect, actualSrcRect, GraphicsUnit.Pixel);
            }
            return fittedBitmap;
        }

        public RectangleF? GetStagedSelectionNormalized()
        {
            if (stagedSelectionRectangle.IsEmpty) return null;
            return GetSelectedRegionNormalized(stagedSelectionRectangle);
        }

    }
}
