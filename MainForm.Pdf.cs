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
        // Add this method to MainForm.cs
        private void RenderPdfPageToPreview(int pageIndex)
        {
            if (this.currentPdfDocument == null || this.currentPdfDocument.PageCount == 0)
            {
                if (this.picPreview.Image != null)
                {
                    this.picPreview.Image.Dispose();
                    this.picPreview.Image = null;
                }
                this.lblImagePath.Text = "No PDF loaded or PDF is empty.";
                // Ensure PDF controls are correctly disabled/hidden if this state is reached unexpectedly
                if (this.btnPrevPage != null) { this.btnPrevPage.Enabled = false; this.btnPrevPage.Visible = false; }
                if (this.btnNextPage != null) { this.btnNextPage.Enabled = false; this.btnNextPage.Visible = false; }
                if (this.txtCurrentPageNum != null) { this.txtCurrentPageNum.Text = "0"; this.txtCurrentPageNum.Visible = false; }
                if (this.lblTotalPages != null) { this.lblTotalPages.Text = "/ 0"; this.lblTotalPages.Visible = false; }
                Control lblPageStaticCtrl = this.Controls.Find("lblPageStatic", true).FirstOrDefault();
                if (lblPageStaticCtrl != null) { lblPageStaticCtrl.Visible = false; }
                return;
            }

            // Validate pageIndex
            if (pageIndex < 0) pageIndex = 0;
            if (pageIndex >= this.totalPdfPages) pageIndex = this.totalPdfPages - 1;

            this.currentPageNumber = pageIndex; // Update current page number (0-indexed)

            try
            {
                // Render the PDF page to an image.
                // Determine rendering size. For picPreview, we can render at a decent DPI,
                // e.g., 150, and let picPreview's Zoom SizeMode handle display scaling.
                // Alternatively, render to picPreview.ClientSize, but this might be low res if preview box is small.
                // Let's try a fixed DPI approach for better quality potential for the actual presentation source.
                float renderDpi = 150f; // Or 96f for screen DPI, or higher like 300f for more detail.
                                        // PdfiumPage.Render takes width/height OR DPI.
                                        // The PdfDocument.Render(page, width, height, dpix, dpiy, flags) is also an option.
                                        // Let's use PdfDocument.Render(page, dpiX, dpiY) which uses page's natural size at that DPI.

                Image renderedPageImage = this.currentPdfDocument.Render(currentPageNumber, renderDpi, renderDpi, true);
                // The 'true' for forPrinting can sometimes improve rendering quality. Or use PdfRenderFlags.CorrectFromDpi

                if (this.picPreview.Image != null)
                {
                    this.picPreview.Image.Dispose();
                }
                this.picPreview.Image = renderedPageImage; // Display the rendered page

                // Update navigation UI
                if (this.txtCurrentPageNum != null) this.txtCurrentPageNum.Text = (this.currentPageNumber + 1).ToString(); // Display 1-based page number
                if (this.lblTotalPages != null) this.lblTotalPages.Text = string.Format("/ {0}", this.totalPdfPages);

                if (this.btnPrevPage != null) this.btnPrevPage.Enabled = (this.currentPageNumber > 0);
                if (this.btnNextPage != null) this.btnNextPage.Enabled = (this.currentPageNumber < this.totalPdfPages - 1);

                // Ensure controls are visible if they were hidden (e.g. first time loading PDF)
                if (this.btnPrevPage != null) this.btnPrevPage.Visible = true;
                if (this.btnNextPage != null) this.btnNextPage.Visible = true;
                if (this.txtCurrentPageNum != null)
                {
                    this.txtCurrentPageNum.Visible = true;
                    this.txtCurrentPageNum.Enabled = true;
                }
                if (this.lblTotalPages != null) this.lblTotalPages.Visible = true;
                Control foundLblPageStatic = this.Controls.Find("lblPageStatic", true).FirstOrDefault();
                if (foundLblPageStatic != null) { foundLblPageStatic.Visible = true; }


                this.picPreview.Invalidate();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error rendering PDF page " + (pageIndex + 1) + ": " + ex.Message);
                if (this.picPreview.Image != null)
                {
                    this.picPreview.Image.Dispose();
                    this.picPreview.Image = null;
                }
                // Optionally, disable nav controls here too or show error placeholder
            }
        }

        private void HandlePageStitching(Point newLocation, bool isMovingDown)
        {
            ClearHighlights();
            Rectangle proposedRect = new Rectangle(newLocation, selectionRectangle.Size);
            RectangleF? normalizedRegion = GetSelectedRegionNormalized(proposedRect);

            if (!normalizedRegion.HasValue) return;

            RectangleF currentRect = normalizedRegion.Value;
            Bitmap stitched = null;

            if (isMovingDown)
            {
                // Calculations are now in normalized coordinates (0.0 to 1.0)
                float visibleHeightOnPage1 = Math.Max(0, 1.0f - currentRect.Top);
                float overflowHeight = currentRect.Height - visibleHeightOnPage1;

                if (overflowHeight > 0)
                {
                    // Clamp page1Selection to avoid negative or out of bounds values
                    float p1Y = Math.Min(0.999f, Math.Max(0, currentRect.Y));
                    float p1H = Math.Max(0.001f, visibleHeightOnPage1);

                    // Selections are now normalized rectangles
                    RectangleF page1Selection = new RectangleF(currentRect.X, p1Y, currentRect.Width, p1H);
                    RectangleF page2Selection = new RectangleF(currentRect.X, 0, currentRect.Width, overflowHeight);
                    stitched = RenderStitchedPdfPages(currentPageNumber, currentPageNumber + 1, page1Selection, page2Selection);
                }
            }
            else // Moving Up
            {
                // Calculations are now in normalized coordinates (0.0 to 1.0)
                float overflowHeight = Math.Max(0, -currentRect.Top);
                float visibleHeightOnPage2 = currentRect.Height - overflowHeight;

                if (overflowHeight > 0)
                {
                    // Clamp page2Selection to avoid negative or out of bounds values
                    float p2H = Math.Max(0.001f, visibleHeightOnPage2);

                    // Selections are now normalized rectangles
                    RectangleF page1Selection = new RectangleF(currentRect.X, 1.0f - overflowHeight, currentRect.Width, overflowHeight);
                    RectangleF page2Selection = new RectangleF(currentRect.X, 0, currentRect.Width, p2H);
                    stitched = RenderStitchedPdfPages(currentPageNumber - 1, currentPageNumber, page1Selection, page2Selection);
                }
            }

            if (stitched != null)
            {
                this.secondaryPreviewPan = PointF.Empty;
                this.secondaryPreviewZoom = 1.0f;

                if (this.stagedStitchedImage != null)
                {
                    this.stagedStitchedImage.Dispose();
                }
                this.stagedStitchedImage = stitched;
                if (this.stagedMasterImage != null)
                {
                    this.stagedMasterImage.Dispose();
                    this.stagedMasterImage = null;
                }

                Bitmap fitted = CreateFittedBitmap(this.stagedStitchedImage, picSecondaryPreview.ClientSize, picSecondaryPreview.BackColor);

                if (picSecondaryPreview.Image != null) picSecondaryPreview.Image.Dispose();
                picSecondaryPreview.Image = fitted;
                isSecondaryPreviewPopulated = true;

                if (chkLinkLocalPreviewToPresenter.Checked)
                {
                    UpdateMainPresentation(null, -1, null, false, this.stagedStitchedImage, 0);
                }
                UpdateSecondaryPreviewBorderColor();
                UpdateButtonEnableStates();
            }
        }

        // In MainForm.cs
        public void ProcessUploadedFile(string tempPath, string originalFileName, bool updateStaging = true)
        {
            string fileExtension = Path.GetExtension(originalFileName).ToLowerInvariant();
            // Skip rename entirely when there is no extension to add: Path.ChangeExtension with ""
            // appends a trailing dot which Windows resolves to the original path, causing the guard
            // below to delete the source file before File.Move can read it.
            string newTempPath = string.IsNullOrEmpty(fileExtension)
                ? tempPath
                : Path.ChangeExtension(tempPath, fileExtension);

            try
            {
                if (newTempPath != tempPath)
                {
                    if (File.Exists(newTempPath))
                        File.Delete(newTempPath);
                    File.Move(tempPath, newTempPath);
                }

                ProcessNewImage(newTempPath, updateStaging);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error processing uploaded file: {ex.Message}");
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
                if (newTempPath != tempPath && File.Exists(newTempPath))
                    File.Delete(newTempPath);
            }
        }

        public void ProcessNewImage(string imagePath, bool updateStaging = true)
        {
            if (updateStaging)
            {
                ClearHighlights();
            }
            // Reset the main preview area before loading new content.
            this.selectionRectangle = Rectangle.Empty;
            this.previousSelectionRectangle = Rectangle.Empty;
            UpdateSelectionSizeLabel();
            this.isSelecting = false;
            this.currentManualRotationAngle = 0;

            if (updateStaging)
            {
                this.stagedSelectionRectangle = Rectangle.Empty;
            }

            if (this.picPreview.Image != null)
            {
                this.picPreview.Image.Dispose();
                this.picPreview.Image = null;
            }

            // Determine the file type and delegate to the appropriate handler.
            string fileExtension = Path.GetExtension(imagePath).ToLowerInvariant();
            if (fileExtension == ".pdf")
            {
                HandlePdfLoading(imagePath);
            }
            else
            {
                HandleImageLoading(imagePath);
            }

            // After loading, update the UI state.
            UpdateButtonAppearanceAndState();
        }

        private void HandlePdfLoading(string pdfPath)
        {
            try
            {
                // Reuse existing PDF document if path is the same
                if (this.currentPdfDocument == null || this.selectedImagePath != pdfPath)
                {
                    if (this.currentPdfDocument != null)
                    {
                        this.currentPdfDocument.Dispose();
                        this.currentPdfDocument = null;
                    }
                    this.currentPdfDocument = PdfDocument.Load(pdfPath);
                }

                this.selectedImagePath = pdfPath;
                this.lblImagePath.Text = "Selected PDF: " + Path.GetFileName(this.selectedImagePath);

                this.totalPdfPages = this.currentPdfDocument.PageCount;
                this.currentPageNumber = 0;

                // Render the first page and display the PDF navigation controls.
                RenderPdfPageToPreview(this.currentPageNumber);
                SetPdfControlsVisibility(true);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error loading PDF: " + ex.Message);
                ResetToNoFileLoadedState();
            }
        }

        private void CorrectRotation(Image img)
        {
            ImageUtils.CorrectRotation(img);
        }

        public void RotateContent(int direction)
        {
            if (this.picPreview.Image == null) return;

            float oldW = this.picPreview.Image.Width;
            float oldH = this.picPreview.Image.Height;

            int angleChange = (direction > 0) ? 90 : 270;
            this.currentManualRotationAngle = (this.currentManualRotationAngle + angleChange) % 360;

            RotateFlipType rotateType = direction > 0 ? RotateFlipType.Rotate90FlipNone : RotateFlipType.Rotate270FlipNone;
            this.picPreview.Image.RotateFlip(rotateType);

            if (!selectionRectangle.IsEmpty)
            {
                RectangleF? oldRegion = GetSelectedRegionInImageCoordinates();
                if (oldRegion.HasValue)
                {
                    RectangleF newRegion;
                    if (direction > 0) // 90 Deg CW
                    {
                        newRegion = new RectangleF(
                            oldH - (oldRegion.Value.Y + oldRegion.Value.Height),
                            oldRegion.Value.X,
                            oldRegion.Value.Height,
                            oldRegion.Value.Width
                        );
                    }
                    else // 90 Deg CCW
                    {
                        newRegion = new RectangleF(
                            oldRegion.Value.Y,
                            oldW - (oldRegion.Value.X + oldRegion.Value.Width),
                            oldRegion.Value.Height,
                            oldRegion.Value.Width
                        );
                    }
                    this.selectionRectangle = ConvertOriginalImageRectToPreviewRect(newRegion);
                }
            }

            if (isSecondaryPreviewPopulated && this.stagedContentPath == this.selectedImagePath)
            {
                this.stagedContentRotationAngle = this.currentManualRotationAngle;
                btnStageContent_Click(this, EventArgs.Empty);
            }

            this.picPreview.Invalidate();
        }

        private void HandleImageLoading(string imagePath)
        {
            // If a PDF was previously loaded, clean up its resources.
            if (this.currentPdfDocument != null)
            {
                this.currentPdfDocument.Dispose();
                this.currentPdfDocument = null;
                this.totalPdfPages = 0;
                this.currentPageNumber = 0;
            }
            SetPdfControlsVisibility(false);

            try
            {
                this.selectedImagePath = imagePath;
                this.lblImagePath.Text = "Selected Image: " + Path.GetFileName(this.selectedImagePath);

                // Load the image into the preview PictureBox using unified utility.
                this.picPreview.Image = ImageUtils.LoadImage(this.selectedImagePath);

                // Restore any saved selection for this image.
                this.selectionRectangle = Rectangle.Empty;
                var selections = LoadSelections();
                var foundSelection = selections.FirstOrDefault(s => s.ImagePath.Equals(this.selectedImagePath, StringComparison.OrdinalIgnoreCase));

                if (foundSelection != null)
                {
                    RectangleF selectionInOriginalCoords = foundSelection.ToRectangleF();
                    if (selectionInOriginalCoords.Width > 0.0f && selectionInOriginalCoords.Height > 0.0f)
                    {
                        this.selectionRectangle = ConvertOriginalImageRectToPreviewRect(selectionInOriginalCoords);
                    }
                }
                UpdateSelectionSizeLabel();
                this.picPreview.Invalidate();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error loading image: " + ex.Message);
                ResetToNoFileLoadedState();
            }
        }

        /// <summary>
        /// Resets the UI and internal state to reflect that no file is loaded.
        /// </summary>
        private void ResetToNoFileLoadedState()
        {
            this.selectedImagePath = null;
            this.lblImagePath.Text = "Selected File: None";
            if (this.picPreview.Image != null) { this.picPreview.Image.Dispose(); this.picPreview.Image = null; }
            if (this.currentPdfDocument != null) { this.currentPdfDocument.Dispose(); this.currentPdfDocument = null; }
            this.totalPdfPages = 0;
            this.currentPageNumber = 0;
            this.selectionRectangle = Rectangle.Empty;
            this.isSelecting = false;
            SetPdfControlsVisibility(false);
            this.picPreview.Invalidate();
        }

        /// <summary>
        /// Shows or hides the PDF navigation controls.
        /// </summary>
        private void SetPdfControlsVisibility(bool visible)
        {
            if (this.btnPrevPage != null) this.btnPrevPage.Visible = visible;
            if (this.btnNextPage != null) this.btnNextPage.Visible = visible;
            if (this.txtCurrentPageNum != null)
            {
                this.txtCurrentPageNum.Visible = visible;
                this.txtCurrentPageNum.Enabled = visible;
            }
            if (this.lblTotalPages != null) this.lblTotalPages.Visible = visible;

            Control lblPageStaticCtrl = this.Controls.Find("lblPageStatic", true).FirstOrDefault();
            if (lblPageStaticCtrl != null) lblPageStaticCtrl.Visible = visible;
        }

        private void tnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select an Image";
                openFileDialog.Filter = "All Supported Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.pdf|PDF Files (*.pdf)|*.pdf|Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All Files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    ProcessNewImage(openFileDialog.FileName);
                }
            }
        }

        // --- Create a new class file for PresentationForm (e.g., PresentationForm.cs) ---
        // Or add this class within the same file as MainForm, outside the MainForm class.

        // --- Create a new class file for PresentationForm (e.g., PresentationForm.cs) ---
        // Or add this class within the same file as MainForm, outside the MainForm class.

        // Required using statements for PresentationForm:
        // using System.Windows.Forms;
        // using System.Drawing;
        // using System.IO;

        // PDF Navigation Event Handlers
        private void GoToPage(int pageIndex, bool preserveSelection)
        {
            if (this.currentPdfDocument == null) return;

            if (pageIndex >= 0 && pageIndex < this.totalPdfPages)
            {
                if (!preserveSelection)
                {
                    this.selectionRectangle = Rectangle.Empty;
                    this.isSelecting = false;
                }
                this.currentPageNumber = pageIndex;
                RenderPdfPageToPreview(this.currentPageNumber);
            }
        }

        private void btnPrevPage_Click(object sender, EventArgs e)
        {
            GoToPage(this.currentPageNumber - 1, true);
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            GoToPage(this.currentPageNumber + 1, true);
        }

        public void NextPage()
        {
            GoToPage(this.currentPageNumber + 1, true);
        }

        public void PreviousPage()
        {
            GoToPage(this.currentPageNumber - 1, true);
        }

        private void txtCurrentPageNum_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (this.currentPdfDocument == null) return;

                if (int.TryParse(this.txtCurrentPageNum.Text, out int desiredPage))
                {
                    int desiredPageIndex = desiredPage - 1; // Convert 1-based to 0-based
                    if (desiredPageIndex >= 0 && desiredPageIndex < this.totalPdfPages)
                    {
                        if (desiredPageIndex != this.currentPageNumber)
                        {
                            GoToPage(desiredPageIndex, true);
                        }
                    }
                    else
                    {
                        ShowWarningMessage(string.Format("Please enter a page number between 1 and {0}.", this.totalPdfPages));
                        this.txtCurrentPageNum.Text = (this.currentPageNumber + 1).ToString();
                    }
                }
                else
                {
                    ShowWarningMessage("Invalid page number format.");
                    this.txtCurrentPageNum.Text = (this.currentPageNumber + 1).ToString();
                }
                e.SuppressKeyPress = true; // Suppress 'ding' for Enter
            }
        }

        private Bitmap RenderStitchedPdfPages(int page1Index, int page2Index, RectangleF page1SelectionNormalized, RectangleF page2SelectionNormalized)
        {
            if (currentPdfDocument == null) return null;

            try
            {
                float renderDpi = 600f;
                PdfRenderFlags flags = PdfRenderFlags.Annotations | PdfRenderFlags.LcdText | PdfRenderFlags.CorrectFromDpi;
                using (Image page1Image = currentPdfDocument.Render(page1Index, renderDpi, renderDpi, flags))
                using (Image page2Image = currentPdfDocument.Render(page2Index, renderDpi, renderDpi, flags))
                {
                    // De-normalize the selections against the actual rendered page dimensions
                    RectangleF page1SelectionPixels = new RectangleF(
                        page1SelectionNormalized.X * page1Image.Width,
                        page1SelectionNormalized.Y * page1Image.Height,
                        page1SelectionNormalized.Width * page1Image.Width,
                        page1SelectionNormalized.Height * page1Image.Height
                    );

                    RectangleF page2SelectionPixels = new RectangleF(
                        page2SelectionNormalized.X * page2Image.Width,
                        page2SelectionNormalized.Y * page2Image.Height,
                        page2SelectionNormalized.Width * page2Image.Width,
                        page2SelectionNormalized.Height * page2Image.Height
                    );

                    int stitchedWidth = (int)Math.Ceiling(Math.Max(page1SelectionPixels.Width, page2SelectionPixels.Width));
                    int stitchedHeight = (int)Math.Ceiling(page1SelectionPixels.Height + page2SelectionPixels.Height);

                    if (stitchedWidth <= 0 || stitchedHeight <= 0) return null;

                    Bitmap stitchedImage = new Bitmap(stitchedWidth, stitchedHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    stitchedImage.SetResolution(page1Image.HorizontalResolution, page1Image.VerticalResolution);

                    using (Graphics g = Graphics.FromImage(stitchedImage))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                        RectangleF destRect1 = new RectangleF(0, 0, page1SelectionPixels.Width, page1SelectionPixels.Height);
                        g.DrawImage(page1Image, destRect1, page1SelectionPixels, GraphicsUnit.Pixel);

                        RectangleF destRect2 = new RectangleF(0, page1SelectionPixels.Height, page2SelectionPixels.Width, page2SelectionPixels.Height);
                        g.DrawImage(page2Image, destRect2, page2SelectionPixels, GraphicsUnit.Pixel);
                    }

                    return stitchedImage;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in RenderStitchedPdfPages: " + ex.Message);
                return null;
            }
        }

        public void GoToPdfPage(int pageNum)
        {
            if (this.currentPdfDocument == null)
            {
                ShowWarningMessage("No PDF document is currently loaded.");
                return;
            }

            int desiredPageIndex = pageNum - 1; // Convert 1-based to 0-based
            if (desiredPageIndex >= 0 && desiredPageIndex < this.totalPdfPages)
            {
                if (desiredPageIndex != this.currentPageNumber)
                {
                    this.currentPageNumber = desiredPageIndex;
                    RenderPdfPageToPreview(this.currentPageNumber);
                }
            }
            else
            {
                ShowWarningMessage(string.Format("Please enter a page number between 1 and {0}.", this.totalPdfPages));
            }
        }

    }
}
