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
        private void UpdateSecondaryPreviewAspectRatio()
        {
            // Define max dimensions for the preview panel on the MainForm at the start of the method
            // These values might need to be adjusted based on your MainForm layout
            const int maxPreviewPanelWidth = 600;
            const int maxPreviewPanelHeight = 425;

            if (cmbDisplays.SelectedItem is DisplayItem selectedDisplayItem && selectedDisplayItem.DisplayScreen != null)
            {
                Screen presenterScreen = selectedDisplayItem.DisplayScreen;
                float presenterAspectRatio = (float)presenterScreen.Bounds.Width / presenterScreen.Bounds.Height;

                int newWidth = maxPreviewPanelWidth;
                int newHeight = (int)(newWidth / presenterAspectRatio);

                if (newHeight > maxPreviewPanelHeight)
                {
                    newHeight = maxPreviewPanelHeight;
                    newWidth = (int)(newHeight * presenterAspectRatio);
                }

                // Ensure minimum size if aspect ratio is extreme or calculations result in zero/small values
                newWidth = Math.Max(50, newWidth); // Min width of 50
                newHeight = Math.Max(50, Math.Max(1, newHeight)); // Min height of 50, ensure at least 1 if aspect ratio is extreme

                Control borderControl = this.Controls.Find("panelSecondaryPreviewBorder", true).FirstOrDefault();
                if (borderControl is Panel panelSecondaryPreviewBorder)
                {
                    // The panel size includes its own padding (which acts as the border thickness)
                    // So the target size for the panel should be newWidth + panel's horizontal padding, newHeight + panel's vertical padding
                    // If panelSecondaryPreviewBorder.Padding is (2,2,2,2), then total horizontal padding is 4.
                    int panelPaddingHorizontal = panelSecondaryPreviewBorder.Padding.Horizontal;
                    int panelPaddingVertical = panelSecondaryPreviewBorder.Padding.Vertical;

                    panelSecondaryPreviewBorder.Size = new Size(newWidth + panelPaddingHorizontal, newHeight + panelPaddingVertical);
                    // picSecondaryPreview inside will fill this, its ClientSize will be newWidth x newHeight
                }
            }
            else
            {
                // Default size if no display selected or error
                Control borderControl = this.Controls.Find("panelSecondaryPreviewBorder", true).FirstOrDefault();
                if (borderControl is Panel panelSecondaryPreviewBorder)
                {
                    // Revert to a default size, e.g., the one from the designer or a fixed default
                    // For now, let's use the max dimensions as a default fallback if no screen.
                    int panelPaddingHorizontal = panelSecondaryPreviewBorder.Padding.Horizontal;
                    int panelPaddingVertical = panelSecondaryPreviewBorder.Padding.Vertical;
                    panelSecondaryPreviewBorder.Size = new Size(maxPreviewPanelWidth + panelPaddingHorizontal, maxPreviewPanelHeight + panelPaddingVertical);
                }
            }
        }

        private void cmbDisplayMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.activePresentationForm != null && !this.activePresentationForm.IsDisposed)
            {
                string selectedModeString = this.cmbDisplayMode.SelectedItem as string;
                if (selectedModeString != null)
                {
                    try
                    {
                        ImageDisplayMode selectedMode = (ImageDisplayMode)Enum.Parse(typeof(ImageDisplayMode), selectedModeString);
                        this.activePresentationForm.SetDisplayMode(selectedMode);
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine("Error parsing display mode: " + ex.Message);
                    }
                }
            }
        }

        private void picPreview_Paint(object sender, PaintEventArgs e)
        {
            // Draw staged selection as a persistent gray reference
            if (!this.stagedSelectionRectangle.IsEmpty)
            {
                using (Pen stagedPen = new Pen(Color.FromArgb(180, Color.DimGray), 6))
                {
                    stagedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawRectangle(stagedPen, this.stagedSelectionRectangle);
                }
            }

            // Draw previous selection as a gray reference while selecting
            if (this.isSelecting && !this.previousSelectionRectangle.IsEmpty)
            {
                using (Pen refPen = new Pen(Color.FromArgb(200, Color.DimGray), 6))
                {
                    refPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    e.Graphics.DrawRectangle(refPen, this.previousSelectionRectangle);
                }
            }

            // Check if there is a selection rectangle to draw
            if (this.selectionRectangle.Width > 0 && this.selectionRectangle.Height > 0)
            {
                // Draw the selection rectangle using a red pen
                // You can choose a different color or style for the pen
                using (Pen selectionPen = new Pen(Color.Red, 2)) // Using a pen of width 2 for better visibility
                {
                    e.Graphics.DrawRectangle(selectionPen, this.selectionRectangle);
                }
            }
        }

        private RectangleF GetDisplayedImageRect()
        {
            if (picPreview.Image == null)
                return RectangleF.Empty;

            float picBoxWidth = picPreview.ClientSize.Width;
            float picBoxHeight = picPreview.ClientSize.Height;
            float imgWidth = picPreview.Image.Width;
            float imgHeight = picPreview.Image.Height;

            float picBoxAspectRatio = picBoxWidth / picBoxHeight;
            float imgAspectRatio = imgWidth / imgHeight;

            float displayedWidth = picBoxWidth;
            float displayedHeight = picBoxHeight;
            float x = 0;
            float y = 0;

            if (picBoxAspectRatio > imgAspectRatio) // Letterbox (horizontal bars)
            {
                displayedHeight = picBoxHeight;
                displayedWidth = displayedHeight * imgAspectRatio;
                x = (picBoxWidth - displayedWidth) / 2;
            }
            else // Pillarbox (vertical bars)
            {
                displayedWidth = picBoxWidth;
                displayedHeight = displayedWidth / imgAspectRatio;
                y = (picBoxHeight - displayedHeight) / 2;
            }

            return new RectangleF(x, y, displayedWidth, displayedHeight);
        }

        // This method should be added to the MainForm class.
        private RectangleF? GetSelectedRegionInImageCoordinates()
        {
            if (this.selectionRectangle.IsEmpty || this.selectionRectangle.Width <= 0 || this.selectionRectangle.Height <= 0)
            {
                return null;
            }
            return GetSelectedRegionInImageCoordinates(this.selectionRectangle);
        }

        // Place this method within the MainForm class
        private float GetTargetAspectRatio()
        {
            if (this.cmbDisplays.SelectedItem is DisplayItem selectedDisplayItem && selectedDisplayItem.DisplayScreen != null)
            {
                Screen targetScreen = selectedDisplayItem.DisplayScreen;
                // Ensure width and height are positive to avoid division by zero or meaningless aspect ratio
                if (targetScreen.Bounds.Width > 0 && targetScreen.Bounds.Height > 0)
                {
                    return (float)targetScreen.Bounds.Width / (float)targetScreen.Bounds.Height;
                }
            }
            return 0.0f; // Fallback: no constraint or error (e.g. no display selected, or a display with zero width/height). Using 0.0f for clarity.
        }

        // In MainForm.cs
        private Rectangle ConvertOriginalImageRectToPreviewRect(RectangleF originalImageRect)
        {
            if (this.picPreview == null || this.picPreview.Image == null || originalImageRect.IsEmpty ||
                this.picPreview.ClientSize.Width <= 0 || this.picPreview.ClientSize.Height <= 0)
            {
                return Rectangle.Empty;
            }

            float originalImageWidth = this.picPreview.Image.Width;
            float originalImageHeight = this.picPreview.Image.Height;

            // Ensure original image has valid dimensions
            if (originalImageWidth <= 0.0F || originalImageHeight <= 0.0F) return Rectangle.Empty; // Using 0.0F

            float picBoxWidth = this.picPreview.ClientSize.Width;
            float picBoxHeight = this.picPreview.ClientSize.Height;

            // Calculate the aspect ratios
            float imageAspectRatio = originalImageWidth / originalImageHeight;
            float picBoxAspectRatio = picBoxWidth / picBoxHeight;

            float displayedImageWidth;
            float displayedImageHeight;

            // Determine the dimensions of the image as it's displayed within the PictureBox (SizeMode.Zoom)
            if (picBoxAspectRatio > imageAspectRatio) // PictureBox is wider than image (letterboxing)
            {
                displayedImageHeight = picBoxHeight;
                displayedImageWidth = displayedImageHeight * imageAspectRatio;
            }
            else // PictureBox is taller than image (pillarboxing) or aspect ratios are the same
            {
                displayedImageWidth = picBoxWidth;
                displayedImageHeight = displayedImageWidth / imageAspectRatio;
            }

            // If the displayed image area is less than 1 pixel in any dimension, it's too small to work with.
            if (displayedImageWidth < 1.0F || displayedImageHeight < 1.0F) return Rectangle.Empty; // Using 1.0F

            // Calculate the offsets for the displayed image within the PictureBox (due to centering)
            float offsetX = (picBoxWidth - displayedImageWidth) / 2.0F; // Using 2.0F
            float offsetY = (picBoxHeight - displayedImageHeight) / 2.0F; // Using 2.0F

            // Calculate scale factors from original image to displayed image
            float scaleToPreviewX = displayedImageWidth / originalImageWidth;
            float scaleToPreviewY = displayedImageHeight / originalImageHeight;

            // Convert the original image rectangle coordinates to ideal preview coordinates (float)
            float idealPreviewX = originalImageRect.X * scaleToPreviewX + offsetX;
            float idealPreviewY = originalImageRect.Y * scaleToPreviewY + offsetY;
            float idealPreviewWidth = originalImageRect.Width * scaleToPreviewX;
            float idealPreviewHeight = originalImageRect.Height * scaleToPreviewY;

            // Round to integer coordinates for the calculated rectangle
            int calcPreviewX = (int)Math.Round(idealPreviewX);
            int calcPreviewY = (int)Math.Round(idealPreviewY);
            int calcPreviewWidth = (int)Math.Round(idealPreviewWidth);
            int calcPreviewHeight = (int)Math.Round(idealPreviewHeight);

            // Ensure that if the original rectangle had positive dimensions, the calculated one is at least 1x1.
            if (calcPreviewWidth <= 0 && originalImageRect.Width > 0.0F) calcPreviewWidth = 1; // Using 0.0F
            if (calcPreviewHeight <= 0 && originalImageRect.Height > 0.0F) calcPreviewHeight = 1; // Using 0.0F

            if (calcPreviewWidth <= 0 || calcPreviewHeight <= 0) return Rectangle.Empty;

            Rectangle calculatedIntRect = new Rectangle(calcPreviewX, calcPreviewY, calcPreviewWidth, calcPreviewHeight);

            Rectangle actualDisplayedIntArea = new Rectangle(
                (int)Math.Round(offsetX),
                (int)Math.Round(offsetY),
                (int)Math.Round(displayedImageWidth),
                (int)Math.Round(displayedImageHeight)
            );

            Rectangle finalRect = Rectangle.Intersect(calculatedIntRect, actualDisplayedIntArea);

            if (finalRect.Width < 1 || finalRect.Height < 1)
            {
                return Rectangle.Empty;
            }

            return finalRect;
        }

        private void picPreview_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (this.isMoving)
                {
                    this.isMoving = false;
                    this.picPreview.Cursor = Cursors.Default;
                    SaveCurrentSelection();
                    this.picPreview.Invalidate();
                }
                else if (this.isSelecting)
                {
                    this.isSelecting = false;

                    float targetAspectRatio = GetTargetAspectRatio();
                    int currentX = e.X;
                    int currentY = e.Y;

                    int dx = currentX - this.selectionStartPoint.X;
                    int dy = currentY - this.selectionStartPoint.Y;

                    if (dx == 0 && dy == 0) // Click without drag
                    {
                        this.selectionRectangle = Rectangle.Empty;
                        UpdateSelectionSizeLabel();
                        SaveCurrentSelection();
                        this.picPreview.Invalidate();
                        return;
                    }

                    int newWidthAbs = Math.Abs(dx);
                    int newHeightAbs = Math.Abs(dy);

                    int constrainedWidth;
                    int constrainedHeight;

                    if (targetAspectRatio > 0.0f)
                    {
                        if ((float)newWidthAbs / targetAspectRatio >= (float)newHeightAbs)
                        {
                            constrainedWidth = newWidthAbs;
                            constrainedHeight = (int)Math.Round((float)constrainedWidth / targetAspectRatio);
                        }
                        else
                        {
                            constrainedHeight = newHeightAbs;
                            constrainedWidth = (int)Math.Round((float)constrainedHeight * targetAspectRatio);
                        }

                        if (constrainedWidth == 0 && newWidthAbs > 0) constrainedWidth = 1;
                        if (constrainedHeight == 0 && newHeightAbs > 0) constrainedHeight = 1;

                        if (constrainedWidth == 1 && newWidthAbs > 0 && targetAspectRatio > 0.0f) constrainedHeight = Math.Max(1, (int)Math.Round(1.0f / targetAspectRatio));
                        if (constrainedHeight == 1 && newHeightAbs > 0 && targetAspectRatio > 0.0f) constrainedWidth = Math.Max(1, (int)Math.Round(1.0f * targetAspectRatio));
                    }
                    else
                    {
                        constrainedWidth = newWidthAbs;
                        constrainedHeight = newHeightAbs;
                    }

                    if (constrainedWidth <= 0 || constrainedHeight <= 0)
                    {
                        this.selectionRectangle = Rectangle.Empty;
                    }
                    else
                    {
                        int finalX = (dx > 0) ? this.selectionStartPoint.X : this.selectionStartPoint.X - constrainedWidth;
                        int finalY = (dy > 0) ? this.selectionStartPoint.Y : this.selectionStartPoint.Y - constrainedHeight;
                        this.selectionRectangle = new Rectangle(finalX, finalY, constrainedWidth, constrainedHeight);
                    }
                    UpdateSelectionSizeLabel();

                    SaveCurrentSelection();
                    this.picPreview.Invalidate();

                    if (this.chkAutoStagePreview != null && this.chkAutoStagePreview.Checked)
                    {
                        _autoStageDebounceTimer.Stop();
                        btnStageContent_Click(this, EventArgs.Empty);
                    }
                }
            }
        }

        private void picPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isMoving)
            {
                int newX = e.X - moveStartOffset.X;
                int newY = e.Y - moveStartOffset.Y;

                RectangleF displayedImageRect = GetDisplayedImageRect();

                // Clamp position to displayed image area
                if (newX < (int)displayedImageRect.Left) newX = (int)displayedImageRect.Left;
                if (newY < (int)displayedImageRect.Top) newY = (int)displayedImageRect.Top;
                if (newX + selectionRectangle.Width > (int)displayedImageRect.Right) newX = (int)displayedImageRect.Right - selectionRectangle.Width;
                if (newY + selectionRectangle.Height > (int)displayedImageRect.Bottom) newY = (int)displayedImageRect.Bottom - selectionRectangle.Height;

                this.selectionRectangle.Location = new Point(newX, newY);
                UpdateSelectionSizeLabel();
                this.picPreview.Invalidate();

                if (this.chkAutoStagePreview != null && this.chkAutoStagePreview.Checked)
                {
                    _autoStageDebounceTimer.Stop();
                    _autoStageDebounceTimer.Start();
                }
            }
            else if (this.isSelecting)
            {
                float targetAspectRatio = GetTargetAspectRatio();

                int currentX = e.X;
                int currentY = e.Y;

                int dx = currentX - this.selectionStartPoint.X;
                int dy = currentY - this.selectionStartPoint.Y;

                int newWidthAbs = Math.Abs(dx);
                int newHeightAbs = Math.Abs(dy);

                int constrainedWidth;
                int constrainedHeight;

                if (targetAspectRatio > 0.0f) // Apply constraint if aspect ratio is valid
                {
                    // Determine dominant dimension for applying constraint
                    if (newWidthAbs == 0 && newHeightAbs == 0) // No movement yet
                    {
                        constrainedWidth = 0;
                        constrainedHeight = 0;
                    }
                    else if ((float)newWidthAbs / targetAspectRatio >= (float)newHeightAbs) // Width is dominant or equal constraint priority
                    {
                        constrainedWidth = newWidthAbs;
                        constrainedHeight = (int)Math.Round((float)constrainedWidth / targetAspectRatio);
                    }
                    else // Height is dominant constraint priority
                    {
                        constrainedHeight = newHeightAbs;
                        constrainedWidth = (int)Math.Round((float)constrainedHeight * targetAspectRatio);
                    }

                    // Ensure dimensions are at least 1 if the original drag was non-zero and constraint made them zero
                    if (constrainedWidth == 0 && newWidthAbs > 0) constrainedWidth = 1;
                    if (constrainedHeight == 0 && newHeightAbs > 0) constrainedHeight = 1;

                    // If one dimension became 1 due to the above, re-calculate the other to maintain aspect ratio, ensuring it's also at least 1
                    if (constrainedWidth == 1 && newWidthAbs > 0 && targetAspectRatio > 0.0f) constrainedHeight = Math.Max(1, (int)Math.Round(1.0f / targetAspectRatio));
                    if (constrainedHeight == 1 && newHeightAbs > 0 && targetAspectRatio > 0.0f) constrainedWidth = Math.Max(1, (int)Math.Round(1.0f * targetAspectRatio));
                }
                else // No aspect ratio constraint
                {
                    constrainedWidth = newWidthAbs;
                    constrainedHeight = newHeightAbs;
                }

                int finalX = (dx > 0) ? this.selectionStartPoint.X : this.selectionStartPoint.X - constrainedWidth;
                int finalY = (dy > 0) ? this.selectionStartPoint.Y : this.selectionStartPoint.Y - constrainedHeight;

                this.selectionRectangle = new Rectangle(finalX, finalY, constrainedWidth, constrainedHeight);
                UpdateSelectionSizeLabel();
                this.picPreview.Invalidate();

                if (this.chkAutoStagePreview != null && this.chkAutoStagePreview.Checked)
                {
                    _autoStageDebounceTimer.Stop();
                    _autoStageDebounceTimer.Start();
                }
            }
            else
            {
                // Update cursor if hovering over selection
                if (!selectionRectangle.IsEmpty && selectionRectangle.Contains(e.Location))
                {
                    this.picPreview.Cursor = Cursors.SizeAll;
                }
                else
                {
                    this.picPreview.Cursor = Cursors.Default;
                }
            }
        }

        private void picPreview_MouseDown(object sender, MouseEventArgs e)
        {
            // Check if the left mouse button was pressed
            if (e.Button == MouseButtons.Left)
            {
                // Allow picPreview to receive keyboard events
                this.picPreview.Focus();

                // If inside existing selection, move it
                if (!selectionRectangle.IsEmpty && selectionRectangle.Contains(e.Location))
                {
                    this.isMoving = true;
                    this.moveStartOffset = new Point(e.X - selectionRectangle.X, e.Y - selectionRectangle.Y);
                }
                else
                {
                    // Otherwise, start a new selection
                    this.selectionStartPoint = e.Location;
                    this.isSelecting = true;
                    // Keep track of the current selection as a reference before starting a new one
                    this.previousSelectionRectangle = this.selectionRectangle;
                    this.selectionRectangle = new Rectangle(e.Location, Size.Empty);
                    UpdateSelectionSizeLabel();
                }
            }
        }

        public int GetMoveStep()
        {
            if (int.TryParse(txtMoveStep.Text, out int step) && step > 0)
            {
                return step;
            }
            return 5; // Default value
        }

        private void picPreview_KeyDown(object sender, KeyEventArgs e)
        {
            if (chkEnableScroll.Checked && !selectionRectangle.IsEmpty)
            {
                bool isShift = e.Shift;
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        MoveSelection(0, -1, isShift);
                        break;
                    case Keys.Down:
                        MoveSelection(0, 1, isShift);
                        break;
                    case Keys.Left:
                        MoveSelection(-1, 0, isShift);
                        break;
                    case Keys.Right:
                        MoveSelection(1, 0, isShift);
                        break;
                    case Keys.PageUp:
                        MoveSelection(0, -1, true);
                        break;
                    case Keys.PageDown:
                        MoveSelection(0, 1, true);
                        break;
                    default:
                        return;
                }
                e.Handled = true;
            }
        }

        private void btnUp_Click(object sender, EventArgs e) => MoveSelection(0, -1, false);

        private void btnDown_Click(object sender, EventArgs e) => MoveSelection(0, 1, false);

        private void btnLeft_Click(object sender, EventArgs e) => MoveSelection(-1, 0, false);

        private void btnRight_Click(object sender, EventArgs e) => MoveSelection(1, 0, false);

        public void MoveSelection(int xDirection, int yDirection, bool isPageOrShift)
        {
            if (chkEnableScroll.Checked && !selectionRectangle.IsEmpty)
            {
                int moveStep;
                if (isPageOrShift)
                {
                    moveStep = (xDirection != 0) ? selectionRectangle.Width : selectionRectangle.Height;
                }
                else
                {
                    moveStep = GetMoveStep();
                }

                int xOffset = xDirection * moveStep;
                int yOffset = yDirection * moveStep;

                Point newLocation = new Point(selectionRectangle.Location.X + xOffset, selectionRectangle.Location.Y + yOffset);
                RectangleF displayedImageRect = GetDisplayedImageRect();
                bool pageChanged = false;

                bool alreadyAtBottom = selectionRectangle.Bottom >= (int)displayedImageRect.Bottom - 10;
                bool alreadyAtTop = selectionRectangle.Top <= (int)displayedImageRect.Top + 10;

                // Handle PDF page transitions
                if (currentPdfDocument != null)
                {
                    if (yDirection > 0 && (newLocation.Y + selectionRectangle.Height > (int)displayedImageRect.Bottom))
                    {
                        // Use a more reliable side detection based on current selection's horizontal center
                        float hCenter = selectionRectangle.X + (selectionRectangle.Width / 2f);
                        float pageHCenter = displayedImageRect.Left + (displayedImageRect.Width / 2f);
                        bool isLeftSide = hCenter < pageHCenter;

                        if (twoPagePdf && isLeftSide)
                        {
                            if (alreadyAtBottom)
                            {
                                // Move from left half to right half of same page
                                newLocation.X = (int)displayedImageRect.Right - selectionRectangle.Width;
                                newLocation.Y = (int)displayedImageRect.Top;
                                pageChanged = true;
                            }
                        }
                        else if (currentPageNumber < totalPdfPages - 1)
                        {
                            if (alreadyAtBottom)
                            {
                                int skip = (skipOnePage && currentPageNumber < totalPdfPages - 2) ? 2 : 1;
                                int nextPageIndex = Math.Min(totalPdfPages - 1, currentPageNumber + skip);

                                if (chkAutoStagePreview.Checked && nextPageIndex == currentPageNumber + 1)
                                {
                                    HandlePageStitching(newLocation, isMovingDown: true);
                                }

                                GoToPage(nextPageIndex, true);
                                displayedImageRect = GetDisplayedImageRect();
                                newLocation.Y = (int)displayedImageRect.Top;
                                if (twoPagePdf) newLocation.X = (int)displayedImageRect.Left;
                                pageChanged = true;
                            }
                        }
                    }
                    else if (yDirection < 0 && (newLocation.Y < (int)displayedImageRect.Top))
                    {
                        float hCenter = selectionRectangle.X + (selectionRectangle.Width / 2f);
                        float pageHCenter = displayedImageRect.Left + (displayedImageRect.Width / 2f);
                        bool isRightSide = hCenter >= pageHCenter;

                        if (twoPagePdf && isRightSide)
                        {
                            if (alreadyAtTop)
                            {
                                // Move from right half to left half of same page
                                newLocation.X = (int)displayedImageRect.Left;
                                newLocation.Y = (int)displayedImageRect.Bottom - selectionRectangle.Height;
                                pageChanged = true;
                            }
                        }
                        else if (currentPageNumber > 0)
                        {
                            if (alreadyAtTop)
                            {
                                int skip = (skipOnePage && currentPageNumber > 1) ? 2 : 1;
                                int prevPageIndex = Math.Max(0, currentPageNumber - skip);

                                if (chkAutoStagePreview.Checked && prevPageIndex == currentPageNumber - 1)
                                {
                                    HandlePageStitching(newLocation, isMovingDown: false);
                                }

                                GoToPage(prevPageIndex, true);
                                displayedImageRect = GetDisplayedImageRect();
                                newLocation.Y = (int)displayedImageRect.Bottom - selectionRectangle.Height;
                                if (twoPagePdf) newLocation.X = (int)displayedImageRect.Right - selectionRectangle.Width;
                                pageChanged = true;
                            }
                        }
                    }
                }

                if (!pageChanged)
                {
                    if (newLocation.X < (int)displayedImageRect.Left) newLocation.X = (int)displayedImageRect.Left;
                    if (newLocation.Y < (int)displayedImageRect.Top) newLocation.Y = (int)displayedImageRect.Top;
                    if (newLocation.X + selectionRectangle.Width > (int)displayedImageRect.Right) newLocation.X = (int)displayedImageRect.Right - selectionRectangle.Width;
                    if (newLocation.Y + selectionRectangle.Height > (int)displayedImageRect.Bottom) newLocation.Y = (int)displayedImageRect.Bottom - selectionRectangle.Height;
                }

                selectionRectangle.Location = newLocation;
                UpdateSelectionSizeLabel();
                picPreview.Invalidate();

                // Requirement: Show pre-selection (gray box) when moving via Auto Scroll
                if (chkAutoStagePreview.Checked)
                {
                    btnStageContent_Click(this, EventArgs.Empty);
                }
            }
        }

        // Add these event handler methods to MainForm.cs:
        private void picPreview_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Check if any of the files are of common image types (optional, but good UX)
                // For simplicity, we'll allow any file drop and let ProcessNewImage handle errors.
                e.Effect = DragDropEffects.Copy; // Show copy cursor
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void picPreview_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    // Process the first file. Could be enhanced to handle multiple, or filter for images.
                    string imagePath = files[0];
                    // Potentially add a filter here to check file extension if desired, e.g.:
                    // string ext = Path.GetExtension(imagePath).ToLowerInvariant();
                    // if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp")
                    // {
                    //    ProcessNewImage(imagePath);
                    // }
                    // else { /* MessageBox.Show("Unsupported file type."); */ }
                    ProcessNewImage(imagePath); // Process the first file dropped
                }
            }
        }

        private void CmbDisplays_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSecondaryPreviewAspectRatio();
            // Also, changing display might affect target aspect ratio for picPreview selection
            // and potentially what's considered "live" if a presentation is active.
            // For now, just updating the preview size.
            // If a presentation is active on a screen that's no longer selected, behavior might be undefined
            // or should be handled (e.g., by closing the presentation or prompting user).
            // The current GetTargetAspectRatio() for picPreview selection already uses cmbDisplays.SelectedItem.
            this.picPreview.Invalidate(); // Redraw selection rectangle with new aspect ratio

            // If live content is showing, and the selected display for that live content changes,
            // the definition of "live" might need re-evaluation or the presenter might need to be moved/resized.
            // This is complex. For now, focus is on preview size.
            // However, if the presenter form is open, its display mode might need re-application
            // or it might need to be moved if the user expects it to follow this combobox.
            // The EnsurePresenterIsOpenAndReady method already handles screen changes if called.
            // If auto-link is on, changing display could trigger an update if presenter is already open.
            if (chkLinkLocalPreviewToPresenter.Checked && activePresentationForm != null && !activePresentationForm.IsDisposed)
            {
                UpdateMainPresentation(
                       this.stagedContentPath,
                       this.stagedContentPageNum,
                       this.stagedContentRegion,
                       this.stagedContentIsNormalized,
                       this.stagedStitchedImage
                   );
            }
            UpdateSecondaryPreviewBorderColor(); // Border color might change if "live" status changes
            UpdateButtonEnableStates(); // Button states can also change
        }

        private void PopulateDisplayComboBox()
        {
            cmbDisplays.Items.Clear();
            Screen[] allScreens = Screen.AllScreens;

            if (allScreens.Length == 0)
            {
                cmbDisplays.Items.Add("No displays found");
                cmbDisplays.Enabled = false;
                btnStageContent.Enabled = false; // Renamed // Disable start if no displays
                return;
            }

            for (int i = 0; i < allScreens.Length; i++)
            {
                Screen screen = allScreens[i];
                string displayName = $"Display {i + 1}: {screen.Bounds.Width}x{screen.Bounds.Height}";
                if (screen.Primary)
                {
                    displayName += " (Primary)";
                }

                // Add a custom object to the ComboBox
                cmbDisplays.Items.Add(new DisplayItem { Name = displayName, DisplayScreen = screen });
            }

            // Select a default display
            if (cmbDisplays.Items.Count > 0)
            {
                // Try to select the first non-primary screen if more than one screen exists
                int defaultIndex = 0;
                if (allScreens.Length > 1)
                {
                    for (int i = 0; i < allScreens.Length; i++)
                    {
                        if (!allScreens[i].Primary)
                        {
                            defaultIndex = i;
                            break;
                        }
                    }
                }
                cmbDisplays.SelectedIndex = defaultIndex;
            }
            else // Should be caught by allScreens.Length == 0, but as a safeguard
            {
                cmbDisplays.Items.Add("No displays configured");
                cmbDisplays.Enabled = false;
                btnStageContent.Enabled = false; // Renamed
            }
        }

        public RectangleF? GetCurrentSelectionNormalized()
        {
            if (selectionRectangle.IsEmpty) return null;
            return GetSelectedRegionNormalized(selectionRectangle);
        }

        private ImageDisplayMode GetCurrentSelectedDisplayMode()
        {
            ImageDisplayMode mode = ImageDisplayMode.Fit; // Default
            string modeString = this.cmbDisplayMode.SelectedItem as string;
            if (!string.IsNullOrEmpty(modeString))
            {
                try
                {
                    mode = (ImageDisplayMode)Enum.Parse(typeof(ImageDisplayMode), modeString);
                }
                catch (ArgumentException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error parsing display mode in GetCurrentSelectedDisplayMode: {ex.Message}");
                }
            }
            return mode;
        }

        private RectangleF? GetSelectedRegionInImageCoordinates(Rectangle rect)
        {
            if (this.picPreview.Image == null || rect.IsEmpty || rect.Width <= 0 || rect.Height <= 0)
            {
                return null; // No image or no valid selection
            }

            // Original image dimensions
            float originalImageWidth = this.picPreview.Image.Width;
            float originalImageHeight = this.picPreview.Image.Height;

            // PictureBox client dimensions
            float picBoxWidth = this.picPreview.ClientSize.Width;
            float picBoxHeight = this.picPreview.ClientSize.Height;

            // Calculate the scale factor and the size of the image as displayed in the PictureBox (due to Zoom mode)
            float imageAspectRatio = originalImageWidth / originalImageHeight;
            float picBoxAspectRatio = picBoxWidth / picBoxHeight;

            float displayedImageWidth = originalImageWidth;
            float displayedImageHeight = originalImageHeight;

            if (picBoxAspectRatio > imageAspectRatio) // PictureBox is wider than image (letterboxing)
            {
                // Image height fills the PictureBox height, width is scaled proportionally
                displayedImageHeight = picBoxHeight;
                displayedImageWidth = displayedImageHeight * imageAspectRatio;
            }
            else // PictureBox is taller than image (pillarboxing) or aspect ratios are the same
            {
                // Image width fills the PictureBox width, height is scaled proportionally
                displayedImageWidth = picBoxWidth;
                displayedImageHeight = displayedImageWidth / imageAspectRatio;
            }

            // Calculate the offsets for the displayed image within the PictureBox (due to centering)
            float offsetX = (picBoxWidth - displayedImageWidth) / 2f;
            float offsetY = (picBoxHeight - displayedImageHeight) / 2f;

            // Adjust the selection coordinates to be relative to the top-left of the displayed image
            float selectedX_relativeToDisplayedImage = rect.X - offsetX;
            float selectedY_relativeToDisplayedImage = rect.Y - offsetY;

            // Now, scale these coordinates back to the original image dimensions
            float scaleToOriginalX = originalImageWidth / displayedImageWidth;
            float scaleToOriginalY = originalImageHeight / displayedImageHeight;

            float finalX = selectedX_relativeToDisplayedImage * scaleToOriginalX;
            float finalY = selectedY_relativeToDisplayedImage * scaleToOriginalY;
            float finalWidth = rect.Width * scaleToOriginalX;
            float finalHeight = rect.Height * scaleToOriginalY;

            return new RectangleF(finalX, finalY, finalWidth, finalHeight);
        }

        private RectangleF? GetSelectedRegionNormalized(Rectangle rect)
        {
            RectangleF? pixelRegion = GetSelectedRegionInImageCoordinates(rect);
            if (!pixelRegion.HasValue || picPreview.Image == null)
            {
                return null;
            }

            float imgWidth = picPreview.Image.Width;
            float imgHeight = picPreview.Image.Height;

            if (imgWidth <= 0 || imgHeight <= 0)
            {
                return null;
            }

            RectangleF region = pixelRegion.Value;
            return new RectangleF(
                region.X / imgWidth,
                region.Y / imgHeight,
                region.Width / imgWidth,
                region.Height / imgHeight
            );
        }

        private void UpdateSelectionSizeLabel()
        {
            if (this.lblSelectionSize != null)
            {
                this.lblSelectionSize.Text = $"Width: {this.selectionRectangle.Width}, Height: {this.selectionRectangle.Height}";
            }
        }

    }
}
