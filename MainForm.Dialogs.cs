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
        private void btnSettings_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm(this))
            {
                if (settingsForm.ShowDialog(this) == DialogResult.OK)
                {
                    SaveSettings();
                }
            }
        }

        // Ensure this method exists and is subscribed to the correct button's Click event.
        // (Formerly btnStartPresentation, now assumed to be btnStageContent)
        private void btnEditContent_Click(object sender, EventArgs e)
        {
            if (this.picPreview.Image == null)
            {
                ShowInfoMessage("Please load an image or PDF first.");
                return;
            }

            float targetAR = GetTargetAspectRatio();
            RectangleF initialCrop;

            // Try to use staged region if it matches current path/page
            bool useStaged = isSecondaryPreviewPopulated &&
                             stagedContentPath == selectedImagePath &&
                             stagedContentPageNum == (currentPdfDocument != null ? currentPageNumber : -1);

            if (useStaged && stagedContentRegion.HasValue)
            {
                if (stagedContentIsNormalized)
                {
                    initialCrop = stagedContentRegion.Value;
                }
                else
                {
                    // Convert pixel region to normalized
                    float imgW = picPreview.Image.Width;
                    float imgH = picPreview.Image.Height;
                    initialCrop = new RectangleF(
                        stagedContentRegion.Value.X / imgW,
                        stagedContentRegion.Value.Y / imgH,
                        stagedContentRegion.Value.Width / imgW,
                        stagedContentRegion.Value.Height / imgH
                    );
                }
            }
            else // Calculate maximized crop area based on target aspect ratio
            {
                if (targetAR > 0)
                {
                    float imgW = this.picPreview.Image.Width;
                    float imgH = this.picPreview.Image.Height;
                    float imgAR = imgW / imgH;

                    if (imgAR > targetAR) // Image is wider than target
                    {
                        float normW = targetAR / imgAR;
                        initialCrop = new RectangleF((1f - normW) / 2f, 0f, normW, 1f);
                    }
                    else // Image is taller than target
                    {
                        float normH = imgAR / targetAR;
                        initialCrop = new RectangleF(0f, (1f - normH) / 2f, 1f, normH);
                    }
                }
                else
                {
                    initialCrop = new RectangleF(0f, 0f, 1f, 1f);
                }
            }

            RectangleF? stagedCrop = GetStagedSelectionNormalized();
            using (var editForm = new EditContentForm(this, this.picPreview.Image, initialCrop, stagedCrop, targetAR, this.currentManualRotationAngle))
            {
                editForm.ShowDialog(this);
            }
        }

        // Add this private method to MainForm.cs
        private void UpdateButtonAppearanceAndState()
        {
            if (this.btnEditContent != null)
            {
                this.btnEditContent.Enabled = (this.picPreview.Image != null);
            }

            if (this.btnAddToDatabase != null)
            {
                bool isLoaded = !string.IsNullOrEmpty(selectedImagePath);
                bool isInDatabase = isLoaded && !string.IsNullOrEmpty(DatabaseFolderPath) &&
                                   selectedImagePath.StartsWith(DatabaseFolderPath, StringComparison.OrdinalIgnoreCase);
                this.btnAddToDatabase.Enabled = isLoaded && !isInDatabase;
            }

            if (this.btnClearPresenterDisplay == null) return;

            bool canControlPresenter = this.activePresentationForm != null &&
                                       !this.activePresentationForm.IsDisposed &&
                                       this.isSecondaryPreviewPopulated;

            this.btnClearPresenterDisplay.Enabled = canControlPresenter;

            if (!canControlPresenter)
            {
                // If disabled, revert to theme-default appearance for a disabled button
                // The ApplyThemeToControl method will handle disabled appearance based on theme.
                // We just need to ensure it's not stuck on "active" colors if it becomes disabled.
                Color defaultBackColor = isDarkMode ? Constants.DarkTheme_BlackoutButton_Normal_Back : Constants.LightTheme_BlackoutButton_Normal_Back;
                Color defaultForeColor = isDarkMode ? Constants.DarkTheme_BlackoutButton_Normal_Fore : Constants.LightTheme_BlackoutButton_Normal_Fore;

                // ApplyThemeToControl handles general theming; for specific state when disabled,
                // it might be simpler to set generic disabled look or ensure ApplyThemeToControl does this.
                // For now, let's assume the general ApplyThemeToControl called elsewhere will fix it if it's disabled.
                // Or, explicitly set a "disabled" look based on theme here.
                // To ensure it resets from "active" state if it becomes disabled:
                this.btnClearPresenterDisplay.Text = "Blackout Presenter"; // Default text when disabled or inactive
                this.btnClearPresenterDisplay.BackColor = defaultBackColor; // Reset to normal theme color
                this.btnClearPresenterDisplay.ForeColor = defaultForeColor;
                // The OS usually handles the visual "disabled" state (graying out)
                return;
            }

            // If it's enabled, style based on isPresenterBlackedOut and current theme
            if (isPresenterBlackedOut)
            {
                this.btnClearPresenterDisplay.Text = "Restore Presenter";
                this.btnClearPresenterDisplay.BackColor = isDarkMode ? Constants.DarkTheme_BlackoutButton_Active_Back : Constants.LightTheme_BlackoutButton_Active_Back;
                this.btnClearPresenterDisplay.ForeColor = isDarkMode ? Constants.DarkTheme_BlackoutButton_Active_Fore : Constants.LightTheme_BlackoutButton_Active_Fore;
            }
            else
            {
                this.btnClearPresenterDisplay.Text = "Blackout Presenter";
                this.btnClearPresenterDisplay.BackColor = isDarkMode ? Constants.DarkTheme_BlackoutButton_Normal_Back : Constants.LightTheme_BlackoutButton_Normal_Back;
                this.btnClearPresenterDisplay.ForeColor = isDarkMode ? Constants.DarkTheme_BlackoutButton_Normal_Fore : Constants.LightTheme_BlackoutButton_Normal_Fore;
            }
        }

        private void chkAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = chkAlwaysOnTop.Checked;
            if (chkAlwaysOnTop.Checked)
            {
                hasAlwaysOnTopBeenAutoChecked = true;
            }
            if (_galleryForm != null && !_galleryForm.IsDisposed)
            {
                _galleryForm.TopMost = this.TopMost;
            }
        }

        public void ShowMessage(string message, string type)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowMessage(message, type)));
                return;
            }

            lblMessage.Text = message;
            switch (type.ToLower())
            {
                case "error":
                    lblMessage.ForeColor = Color.Red;
                    break;
                case "warning":
                    lblMessage.ForeColor = Color.Orange;
                    break;
                default:
                    lblMessage.ForeColor = Color.Blue;
                    break;
            }
            lblMessage.Visible = true;
            btnMessageOkay.Visible = true;
        }

        public void ClearMessage()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ClearMessage));
                return;
            }

            lblMessage.Visible = false;
            btnMessageOkay.Visible = false;
            lblMessage.Text = "";
        }

        private void ShowErrorMessage(string message) => ShowMessage(message, "error");

        private void ShowWarningMessage(string message) => ShowMessage(message, "warning");

        private void ShowInfoMessage(string message) => ShowMessage(message, "info");

        private void btnHelp_Click(object sender, EventArgs e)
        {
            using (ActivationForm activationForm = new ActivationForm())
            {
                activationForm.ShowDialog(this);
            }
        }

        private string GetActivationStatus()
        {
            return ActivationStatusHelper.GetActivationStatusString(forTitleBar: true);
        }

        private void btnOpenGallery_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(DatabaseFolderPath))
            {
                ShowWarningMessage("Please set the Database Folder first.");
                return;
            }

            if (_galleryForm == null || _galleryForm.IsDisposed)
            {
                _galleryForm = new GalleryForm(this);
                _galleryForm.Owner = this;
                _galleryForm.TopMost = this.TopMost;
                _galleryForm.Show();
            }
            else
            {
                _galleryForm.BringToFront();
                if (_galleryForm.WindowState == FormWindowState.Minimized)
                    _galleryForm.WindowState = FormWindowState.Normal;
            }
        }

        private void btnAddToDatabase_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedImagePath) || !File.Exists(selectedImagePath))
            {
                ShowWarningMessage("No file loaded to add to database.");
                return;
            }

            if (string.IsNullOrEmpty(DatabaseFolderPath))
            {
                ShowWarningMessage("Please set the Database Folder first.");
                return;
            }

            var subfolders = GetDatabaseSubfolders();
            using (var addForm = new AddToDatabaseForm(subfolders, selectedImagePath))
            {
                if (addForm.ShowDialog(this) == DialogResult.OK)
                {
                    string targetSub = addForm.SelectedSubfolder;
                    if (!string.IsNullOrEmpty(addForm.NewSubfolderName))
                    {
                        // Sanitize new folder name
                        string sanitizedNewFolder = string.Join("_", addForm.NewSubfolderName.Split(Path.GetInvalidFileNameChars()));
                        targetSub = string.IsNullOrEmpty(targetSub) ? sanitizedNewFolder : Path.Combine(targetSub, sanitizedNewFolder);
                    }

                    // Security check: ensure the target stays within DatabaseFolderPath. An empty
                    // targetSub means the root; otherwise it must resolve inside the root (the
                    // shared helper rejects `..` and sibling-prefix escapes).
                    string targetDir = string.IsNullOrEmpty(targetSub)
                        ? Path.GetFullPath(DatabaseFolderPath)
                        : ResolveWithinDatabase(targetSub);
                    if (targetDir == null)
                    {
                        ShowErrorMessage("Invalid target path.");
                        return;
                    }

                    Directory.CreateDirectory(targetDir);

                    string ext = Path.GetExtension(selectedImagePath);
                    string finalName = addForm.CustomFileName;
                    if (string.IsNullOrEmpty(finalName))
                    {
                        finalName = Path.GetFileNameWithoutExtension(selectedImagePath);
                    }
                    else
                    {
                        // Sanitize custom filename and prevent double extension
                        finalName = string.Join("_", finalName.Split(Path.GetInvalidFileNameChars()));
                        if (finalName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                        {
                            finalName = finalName.Substring(0, finalName.Length - ext.Length);
                        }
                    }
                    string destPath = Path.Combine(targetDir, finalName + ext);

                    // Handle filename collision
                    if (File.Exists(destPath))
                    {
                        int counter = 1;
                        while (File.Exists(destPath))
                        {
                            destPath = Path.Combine(targetDir, $"{finalName}_{counter}{ext}");
                            counter++;
                        }
                    }

                    try
                    {
                        File.Copy(selectedImagePath, destPath);
                        ProcessNewImage(destPath);
                        ShowInfoMessage($"Added to database: {Path.GetFileName(destPath)}");
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage($"Error copying file: {ex.Message}");
                    }
                }
            }
        }

        private async void btnSnip_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            await Task.Delay(500); // Give time for minimize animation

            using (SnipForm snipForm = new SnipForm())
            {
                if (snipForm.ShowDialog() == DialogResult.OK && snipForm.SnippedImage != null)
                {
                    string tempPath = Path.Combine(Path.GetTempPath(), $"snip_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                    snipForm.SnippedImage.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);

                    ProcessNewImage(tempPath);

                    // Select entire image for main preview
                    if (this.picPreview.Image != null)
                    {
                        RectangleF fullRect = new RectangleF(0, 0, this.picPreview.Image.Width, this.picPreview.Image.Height);
                        this.selectionRectangle = ConvertOriginalImageRectToPreviewRect(fullRect);
                    }
                }
            }

            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

    }
}
