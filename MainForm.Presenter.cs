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
        // Ensure this method exists and is subscribed to the correct button's Click event.
        // (Formerly btnSendToSecondaryPreview, now assumed to be btnPushToPresenter)
        public void btnPushToPresenter_Click(object sender, EventArgs e)
        {
            if (!this.isSecondaryPreviewPopulated || (string.IsNullOrEmpty(this.stagedContentPath) && this.stagedStitchedImage == null))
            {
                ShowInfoMessage("There is no content staged in the secondary preview to present.");
                return;
            }

            UpdateMainPresentation(
                this.stagedContentPath,
                this.stagedContentPageNum,
                this.stagedContentRegion,
                this.stagedContentIsNormalized,
                this.stagedStitchedImage,
                this.stagedContentRotationAngle
            );
        }

        // Ensure this method is named btnClearPresenterDisplay_Click
        // and subscribed to the btnClearPresenterDisplay.Click event.
        public void btnClearPresenterDisplay_Click(object sender, EventArgs e)
        {
            if (this.activePresentationForm != null && !this.activePresentationForm.IsDisposed && isSecondaryPreviewPopulated)
            {
                isPresenterBlackedOut = !isPresenterBlackedOut; // Toggle state

                if (isPresenterBlackedOut)
                {
                    this.activePresentationForm.ClearDisplay();
                    this.isPresenterShowingLiveContent = false; // Blacked out, so not showing live content
                }
                else
                {
                    // Restore the image using staged content
                    ImageDisplayMode displayMode = GetCurrentSelectedDisplayMode();
                    this.activePresentationForm.UpdateImage(
                        this.stagedContentPath,
                        this.stagedContentPageNum,
                        this.stagedContentRegion,
                        this.stagedContentIsNormalized,
                        displayMode,
                        this.stagedStitchedImage,
                        this.stagedContentRotationAngle);
                    this.isPresenterShowingLiveContent = true; // Restored from staged, so it's live
                    // Update live trackers to match staged, as this is what's now live
                    this.liveContentPath = this.stagedContentPath;
                    this.liveContentPageNum = this.stagedContentPageNum;
                    this.liveContentRegion = this.stagedContentRegion;
                    this.liveContentIsNormalized = this.stagedContentIsNormalized;
                    this.liveContentIsStitched = (this.stagedStitchedImage != null);
                }
                UpdateButtonAppearanceAndState(); // Update text and color for btnClearPresenterDisplay
                UpdateSecondaryPreviewBorderColor(); // Update border color
            }
            // If no active presentation or no content staged, the button should be disabled by UpdateButtonAppearanceAndState(),
            // so this click handler might not even be reached in those states.
        }

        private void UpdateMainPresentation(string path, int pageNum, RectangleF? region, bool isNormalized, Bitmap stitchedImage = null, int rotationAngle = 0)
        {
            // 1. Handle Clearing
            if (string.IsNullOrEmpty(path) && stitchedImage == null)
            {
                if (this.activePresentationForm != null && !this.activePresentationForm.IsDisposed)
                {
                    this.activePresentationForm.ClearDisplay();
                    this.isPresenterBlackedOut = false;

                    this.isPresenterShowingLiveContent = false; // No longer showing live content
                    this.liveContentIsStitched = false;
                }
                // Ensure button states are updated after clearing
                UpdateButtonAppearanceAndState();
                UpdateButtonEnableStates();

                UpdateSecondaryPreviewBorderColor(); // Update border
                return;
            }

            // 2. Ensure presenter is open and ready.
            // EnsurePresenterIsOpenAndReady also handles getting the targetScreen and current displayMode,
            // and updates button states.
            PresentationForm presenter = EnsurePresenterIsOpenAndReady();
            if (presenter == null)
            {
                // If EnsurePresenterIsOpenAndReady returns null, it means setup failed (e.g., no display selected).
                // It would have shown a message. Button states are updated within it.
                return;
            }


            // 3. Update content on the (now guaranteed to be open) presenter form.
            // The display mode from cmbDisplays is already applied by EnsurePresenterIsOpenAndReady
            // if the form was newly created, or re-applied if it was already open.
            ImageDisplayMode currentSelectedMode = GetCurrentSelectedDisplayMode(); // Get the most current mode
            presenter.UpdateImage(path, pageNum, region, isNormalized, currentSelectedMode, stitchedImage, rotationAngle);

            this.isPresenterBlackedOut = false; // Showing content means it's not blacked out.

            // Update live content trackers since UpdateMainPresentation can be called directly (e.g. by auto-link)
            this.liveContentPath = path;
            this.liveContentPageNum = pageNum;
            this.liveContentRegion = region;
            this.liveContentIsNormalized = isNormalized;
            this.liveContentIsStitched = (stitchedImage != null);
            this.liveContentRotationAngle = rotationAngle;
            this.isPresenterShowingLiveContent = true;


            // Button states are updated within EnsurePresenterIsOpenAndReady.
            // Called them again here for good measure, though likely redundant if EnsurePresenterIsOpenAndReady covers all paths.
            UpdateButtonAppearanceAndState();
            UpdateButtonEnableStates();

            UpdateSecondaryPreviewBorderColor(); // Update border

        }

        private void UpdateMainPresentation(Bitmap stitchedImage)
        {
            UpdateMainPresentation(null, -1, null, false, stitchedImage, 0);
        }

        // Add this private method to MainForm.cs
        private PresentationForm EnsurePresenterIsOpenAndReady()
        {
            if (this.cmbDisplays.SelectedItem == null)
            {
                ShowWarningMessage("Please select a display for the presentation.");
                return null;
            }
            DisplayItem selectedDisplayItem = this.cmbDisplays.SelectedItem as DisplayItem;
            if (selectedDisplayItem == null || selectedDisplayItem.DisplayScreen == null)
            {
                ShowErrorMessage("Invalid display selection.");
                return null;
            }
            Screen targetScreen = selectedDisplayItem.DisplayScreen;
            ImageDisplayMode displayMode = GetCurrentSelectedDisplayMode(); // Uses existing helper

            // Ensure that if activePresentationForm is not null but is disposed, we treat it as null.
            if (this.activePresentationForm != null && this.activePresentationForm.IsDisposed)
            {
                this.activePresentationForm = null;
            }

            if (this.activePresentationForm == null) // Only check for null now
            {
                // Path, pageNum, region, isNormalized will be set by the caller via UpdateImage
                this.activePresentationForm = new PresentationForm(null, -1, targetScreen, null, false, 0);
                this.activePresentationForm.FormClosed += (s, args) =>
                {
                    if (s == this.activePresentationForm)
                    {
                        this.activePresentationForm = null;
                        this.isPresenterBlackedOut = false;

                        this.isPresenterShowingLiveContent = false; // Form closed, so not showing live
                        UpdateButtonAppearanceAndState();
                        UpdateButtonEnableStates();
                        UpdateSecondaryPreviewBorderColor(); // Update border

                    }
                };
                this.activePresentationForm.SetDisplayMode(displayMode); // Set initial mode
                this.activePresentationForm.Show();
                this.isPresenterBlackedOut = false; // Reset blackout state

                if (!hasAlwaysOnTopBeenAutoChecked)
                {
                    chkAlwaysOnTop.Checked = true;
                }
            }
            else
            {
                // If already open, ensure it's on the correct screen and update its display mode if changed
                if (this.activePresentationForm.Bounds.Location != targetScreen.Bounds.Location ||
                    this.activePresentationForm.ClientSize != targetScreen.Bounds.Size)
                {
                    // Basic check; more robust screen check might be needed if multi-monitor setup changes dynamically
                    // For simplicity, we might just re-apply bounds or let user manage this.
                    // If we want to move it:
                    // this.activePresentationForm.WindowState = FormWindowState.Normal; // Required before changing bounds sometimes
                    // this.activePresentationForm.Bounds = targetScreen.Bounds;
                    // this.activePresentationForm.WindowState = FormWindowState.Maximized;
                }
                this.activePresentationForm.SetDisplayMode(displayMode); // Ensure current display mode is applied
                if (this.activePresentationForm.WindowState == FormWindowState.Minimized)
                {
                    this.activePresentationForm.WindowState = FormWindowState.Normal;
                }
                this.activePresentationForm.Activate();
            }


            UpdateButtonAppearanceAndState();
            UpdateButtonEnableStates();
            return this.activePresentationForm;
        }

        private void UpdateButtonEnableStates()
        {
            if (this.btnPushToPresenter != null && this.chkLinkLocalPreviewToPresenter != null)
            {

                bool stagedContentIsCurrentlyLive =
                    this.isPresenterShowingLiveContent &&
                    this.liveContentPath == this.stagedContentPath &&
                    this.liveContentPageNum == this.stagedContentPageNum &&
                    this.liveContentRotationAngle == this.stagedContentRotationAngle &&
                    AreRegionsEqual(this.liveContentRegion, this.stagedContentRegion) &&
                    this.liveContentIsNormalized == this.stagedContentIsNormalized;

                bool enablePushToPresenter = !this.chkLinkLocalPreviewToPresenter.Checked &&
                                             this.isSecondaryPreviewPopulated &&
                                             !stagedContentIsCurrentlyLive;
                // Only enable if content is staged, auto-link is off, AND staged content is not already live.


                this.btnPushToPresenter.Enabled = enablePushToPresenter;
            }

            if (this.btnCloseLivePresenter != null)
            {
                this.btnCloseLivePresenter.Enabled = (this.activePresentationForm != null && !this.activePresentationForm.IsDisposed);
            }

            // Drive the header "LIVE" indicator: pulsing red while the presenter window
            // is open, steady dim-gray otherwise.
            if (this.linearHeader != null)
            {
                this.linearHeader.IsLive = (this.activePresentationForm != null && !this.activePresentationForm.IsDisposed);
            }
            // This method can be expanded later to include other button states if needed.
        }

        public void btnCloseLivePresenter_Click(object sender, EventArgs e)
        {
            if (this.activePresentationForm != null && !this.activePresentationForm.IsDisposed)
            {
                this.activePresentationForm.Close(); // This will trigger the FormClosed event
                // The FormClosed event handler in EnsurePresenterIsOpenAndReady already calls:
                // - UpdateButtonAppearanceAndState()
                // - UpdateButtonEnableStates()
                // - UpdateSecondaryPreviewBorderColor()
                // - Sets isPresenterShowingLiveContent = false

                // Explicitly set border to Gray as per specific requirement for this button,
                // potentially overriding what UpdateSecondaryPreviewBorderColor might do based on other states
                // if it runs after this due to event order.
                // However, FormClosed should set isPresenterShowingLiveContent = false, and if
                // isSecondaryPreviewPopulated is also false, border will be Gray.
                // If isSecondaryPreviewPopulated is true, border will be Green.
                // To FORCE gray:
                Control borderControl = this.Controls.Find("panelSecondaryPreviewBorder", true).FirstOrDefault();
                if (borderControl is Panel panelSecondaryPreviewBorder)
                {
                    panelSecondaryPreviewBorder.BackColor = Constants.BorderColorDefault;
                }
            }
        }

    }
}
