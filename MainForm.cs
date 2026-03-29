// Version DreamsLive_Solutions-PresenterApp1.2_V1.2.0.2 (0)
// stable build_03 15 2026
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
    public partial class MainForm : Form
    {
        private HttpWebServer _httpWebServer;
        private GalleryForm _galleryForm;
        private System.Windows.Forms.Timer _statusUpdateTimer;
        private System.Windows.Forms.Timer _autoStageDebounceTimer;
        private FileSystemWatcher _dbWatcher;
        private long _galleryVersion = 0;

        // Fields to track what's "live" on the presenter
        private string liveContentPath = null;
        private int liveContentPageNum = -1;
        private RectangleF? liveContentRegion = null;
        private bool liveContentIsNormalized = false;
        private bool liveContentIsStitched = false;
        private int liveContentRotationAngle = 0;
        private bool isPresenterShowingLiveContent = false; // True if presenter is showing content matching liveContent fields

        private string selectedImagePath = null;
        private PresentationForm activePresentationForm = null;
        private Rectangle selectionRectangle = Rectangle.Empty;
        private Rectangle stagedSelectionRectangle = Rectangle.Empty;
        private Rectangle previousSelectionRectangle = Rectangle.Empty;
        private Point selectionStartPoint = Point.Empty;
        private Point moveStartOffset = Point.Empty;
        private bool isSelecting = false;
        private bool isMoving = false;

        private PdfDocument currentPdfDocument = null;
        private int currentPageNumber = 0; // Internally 0-indexed for PdfiumPage.Render
        private int totalPdfPages = 0;
        private int currentManualRotationAngle = 0; // 0, 90, 180, 270

        // Fields for Secondary Preview
        private string stagedContentPath = null;            // Path to the original image/PDF for what's in secondary preview
        private int stagedContentPageNum = -1;             // Page number if PDF, -1 if image, for secondary preview content
        private RectangleF? stagedContentRegion = null;     // Region in original image/PDF page coordinates (can be pixel or normalized)
        private bool stagedContentIsNormalized = false;     // Flag if stagedContentRegion is normalized
        private int stagedContentRotationAngle = 0;
        private Bitmap stagedStitchedImage = null;          // High-res stitched bitmap if stitching is active
        private Bitmap stagedMasterImage = null;            // High-res master bitmap for non-stitched content
        private bool isSecondaryPreviewPopulated = false;   // True if secondary preview has content
        private bool isDarkMode = false; // Added for theme switching
        private bool isPresenterBlackedOut = false; // Added for blackout toggle
        private bool hasAlwaysOnTopBeenAutoChecked = false;
        private bool isDisplayingStitchInMainPreview = false;
        private Image currentPdfPageImage = null;

        // New PDF Navigation Settings - Reset to false on startup
        private bool skipOnePage = false;
        private bool twoPagePdf = false;

        // Fields for picSecondaryPreview interactivity
        private PointF secondaryPreviewPan = PointF.Empty;
        private float secondaryPreviewZoom = 1.0f;
        private Point secondaryPreviewLastMousePosition = Point.Empty;
        private bool isPanningSecondaryPreview = false;
        private PointF? laserPointNormalized = null; // Normalized laser position (0-1) relative to picSecondaryPreview
        private List<List<PointF>> highlightsNormalized = new List<List<PointF>>();
        private bool highlighterActive = false;
        private Color highlighterColor = Color.Yellow;
        private bool isHighlighting = false;

        public string DatabaseFolderPath { get; private set; }
        public string SelectedImagePath => selectedImagePath;
        public int CurrentPageNumber => currentPageNumber;
        public int GetCurrentManualRotationAngle() => currentManualRotationAngle;
        public bool PresenterDisplayIsBlack => isPresenterBlackedOut;
        public int SelectionWidth => selectionRectangle.Width;
        public int SelectionHeight => selectionRectangle.Height;

        public bool IsAutoSendEnabled() => chkLinkLocalPreviewToPresenter.Checked;
        public void SetAutoSendEnabled(bool enabled) => chkLinkLocalPreviewToPresenter.Checked = enabled;

        public bool SkipOnePage { get => skipOnePage; set => skipOnePage = value; }
        public bool TwoPagePdf { get => twoPagePdf; set => twoPagePdf = value; }
        public bool EnableAutoScroll { get => chkEnableScroll.Checked; set => chkEnableScroll.Checked = value; }
        public bool AutoStagePreview { get => chkAutoStagePreview.Checked; set => chkAutoStagePreview.Checked = value; }
        public string MoveStepText { get => txtMoveStep.Text; set => txtMoveStep.Text = value; }
        public bool IsDarkMode => isDarkMode;
        public long GalleryVersion => _galleryVersion;

        public MainForm()
        {
            InitializeComponent();
            this.picSecondaryPreview.SizeMode = PictureBoxSizeMode.Normal;

            this.Text = $"Presenter App V{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} {GetActivationStatus()}";

            // Set the initial TopMost state based on the checkbox's default value
            this.TopMost = chkAlwaysOnTop.Checked;

            if (!this.DesignMode)
            {
                _httpWebServer = new HttpWebServer(this);
                _httpWebServer.Start();

                // Update the text of the designer-created lblWebServerUrl
                if (this.lblWebServerUrl != null)
                {
                    if (_httpWebServer.IsRunning)
                    {
                        this.lblWebServerUrl.Text = "Web Server: " + _httpWebServer.ServerUrl;
                        this.lblWebServerUrl.ForeColor = this.isDarkMode ? Color.White : SystemColors.ControlText;
                    }
                    else
                    {
                        this.lblWebServerUrl.Text = "Web Server: FAILED TO START";
                        this.lblWebServerUrl.ForeColor = Color.Red;
                    }
                }
            }

            // Initialize theme
            ApplyTheme();
            // Subscribe to picPreview mouse events
            if (this.picPreview != null) // Ensure picPreview is not null
            {
                this.picPreview.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picPreview_MouseDown);
                this.picPreview.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picPreview_MouseMove);
                this.picPreview.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picPreview_MouseUp);
                this.picPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.picPreview_Paint);
                this.picPreview.KeyDown += new System.Windows.Forms.KeyEventHandler(this.picPreview_KeyDown);

                // Add for Drag and Drop
                this.picPreview.AllowDrop = true;
                this.picPreview.DragEnter += new DragEventHandler(picPreview_DragEnter);
                this.picPreview.DragDrop += new DragEventHandler(picPreview_DragDrop);
            }

            // Initialize DisplayMode ComboBox
            if (this.cmbDisplayMode != null)
            {
                this.cmbDisplayMode.SelectedItem = "Fit"; // Default selection
            }

            if (this.chkAlwaysOnTop != null)
            {
                this.chkAlwaysOnTop.CheckedChanged += new System.EventHandler(this.chkAlwaysOnTop_CheckedChanged);
            }

            if (this.chkAutoStagePreview != null)
            {
                this.chkAutoStagePreview.CheckedChanged += new System.EventHandler(this.chkAutoStagePreview_CheckedChanged);
            }

            if (this.btnMessageOkay != null)
            {
                this.btnMessageOkay.Click += (s, e) => ClearMessage();
            }

            if (!this.DesignMode)
            {
                this.lblMessage.Visible = false;
                this.btnMessageOkay.Visible = false;
            }

            // Initialize PDF navigation controls state
            if (this.btnPrevPage != null) { this.btnPrevPage.Visible = false; this.btnPrevPage.Enabled = false; }
            if (this.btnNextPage != null) { this.btnNextPage.Visible = false; this.btnNextPage.Enabled = false; }
            if (this.txtCurrentPageNum != null) { this.txtCurrentPageNum.Visible = false; this.txtCurrentPageNum.Enabled = false; }
            if (this.lblTotalPages != null) { this.lblTotalPages.Visible = false; /* Enabled is not typically set for Label text */ }

            // Assuming an optional static label named "lblPageStatic" might exist for "Page:" text
            Control foundLblPageStatic = this.Controls.Find("lblPageStatic", true).FirstOrDefault();
            if (foundLblPageStatic != null) { foundLblPageStatic.Visible = false; }

            // Subscribe to PDF navigation control events
            if (this.btnPrevPage != null) this.btnPrevPage.Click += new System.EventHandler(this.btnPrevPage_Click);
            if (this.btnNextPage != null) this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            if (this.txtCurrentPageNum != null) this.txtCurrentPageNum.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCurrentPageNum_KeyDown);

            // Initialize Secondary Preview UI controls
            // Removed old control initializations for chkPresentFromSecondary and btnClearSecondaryPreview

            // Initialize new/renamed Secondary Preview UI controls
            if (this.chkLinkLocalPreviewToPresenter != null) // New name
            {
                this.chkLinkLocalPreviewToPresenter.Checked = false;
            }
            if (this.chkLinkLocalPreviewToPresenter != null) // New name for control
            {
                this.chkLinkLocalPreviewToPresenter.Checked = false;
                // Ensure correct event subscription using remove-then-add pattern
                this.chkLinkLocalPreviewToPresenter.CheckedChanged -= this.chkLinkLocalPreviewToPresenter_CheckedChanged;
                this.chkLinkLocalPreviewToPresenter.CheckedChanged += new System.EventHandler(this.chkLinkLocalPreviewToPresenter_CheckedChanged);
            }

            if (this.btnClearPresenterDisplay != null) // New name for control
            {
                this.btnClearPresenterDisplay.Enabled = false;
                // Ensure correct event subscription using remove-then-add pattern
                this.btnClearPresenterDisplay.Click -= this.btnClearPresenterDisplay_Click;
                this.btnClearPresenterDisplay.Click += new System.EventHandler(this.btnClearPresenterDisplay_Click);
            }

            // Assuming btnSendToSecondaryPreview (control) was renamed to btnPushToPresenter (control) by the user in the designer.
            // The handler btnSendToSecondaryPreview_Click was programmatically renamed to btnPushToPresenter_Click.
            // Remove any potential old subscription if it references the old control name.
            // The line 'if (this.btnSendToSecondaryPreview != null) ...' is removed as btnSendToSecondaryPreview is obsolete.

            if (this.btnPushToPresenter != null) // New name for control
            {
                // Ensure correct event subscription using remove-then-add pattern
                this.btnPushToPresenter.Click -= this.btnPushToPresenter_Click;
                this.btnPushToPresenter.Click += new System.EventHandler(this.btnPushToPresenter_Click);
            }

            if (this.btnUp != null) this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            if (this.btnDown != null) this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            if (this.btnLeft != null) this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            if (this.btnRight != null) this.btnRight.Click += new System.EventHandler(this.btnRight_Click);

            // Assuming btnEditContent is already defined in Designer or I'll add it
            Control foundBtnEditContent = this.Controls.Find("btnEditContent", true).FirstOrDefault();
            if (foundBtnEditContent is Button btnEditContent)
            {
                btnEditContent.Click += btnEditContent_Click;
            }

            // Note: Subscription for btnStageContent (formerly btnStartPresentation) is assumed
            // to be handled by the designer if the control was renamed and the method name
            // (btnStageContent_Click) was already in place or updated in Designer.cs.
            // No explicit add/remove for btnStageContent.Click here.

            // Initial button state and text
            this.isPresenterBlackedOut = false;
            // Text is set by UpdateButtonAppearanceAndState
            // if (this.btnClearPresenterDisplay != null)
            // {
            //     this.btnClearPresenterDisplay.Text = "Blackout Presenter";
            // }


            // Programmatic creation of btnCloseLivePresenter removed.
            // It's now expected to be created by MainForm.Designer.cs InitializeComponent().

            // Add event handlers for picSecondaryPreview
            if (this.picSecondaryPreview != null)
            {
                this.picSecondaryPreview.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picSecondaryPreview_MouseDown);
                this.picSecondaryPreview.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picSecondaryPreview_MouseMove);
                this.picSecondaryPreview.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picSecondaryPreview_MouseUp);
                this.picSecondaryPreview.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.picSecondaryPreview_MouseWheel);
                this.picSecondaryPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.picSecondaryPreview_Paint); // Add Paint event
                this.picSecondaryPreview.MouseEnter += new System.EventHandler(this.picSecondaryPreview_MouseEnter);
                this.picSecondaryPreview.MouseLeave += new System.EventHandler(this.picSecondaryPreview_MouseLeave);
            }

            if (this.panelSecondaryPreviewBorder != null)
            {
                this.panelSecondaryPreviewBorder.BackColor = Constants.BorderColorDefault;
            }
            UpdateButtonAppearanceAndState(); // Set initial appearance and state for btnClearPresenterDisplay
            UpdateButtonEnableStates(); // Set initial state for btnPushToPresenter and btnCloseLivePresenter
            UpdateSecondaryPreviewBorderColor(); // Set initial border color

            _statusUpdateTimer = new System.Windows.Forms.Timer();
            _statusUpdateTimer.Interval = 1000; // 1 second
            _statusUpdateTimer.Tick += new EventHandler(StatusUpdateTimer_Tick);
            _statusUpdateTimer.Start();

            _autoStageDebounceTimer = new System.Windows.Forms.Timer();
            _autoStageDebounceTimer.Interval = 250;
            _autoStageDebounceTimer.Tick += (s, e) => {
                _autoStageDebounceTimer.Stop();
                btnStageContent_Click(this, EventArgs.Empty);
            };

            this.lblPreviewLabel.BringToFront();
            this.lblStagedLabel.BringToFront();
        }

        private void StatusUpdateTimer_Tick(object sender, EventArgs e)
        {
            var usageManager = new UsageManager();
            if (usageManager.HasExceededUsageLimit() && !usageManager.IsInGracePeriod())
            {
                _statusUpdateTimer.Stop();
                CopyableMessageBox.Show(this, "The 5-minute grace period has expired. The application will now exit.", "Grace Period Over", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
                return;
            }
            this.Text = $"Presenter App V{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} {GetActivationStatus()}";
        }

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

        private void chkAutoStagePreview_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkAutoStagePreview.Checked)
            {
                btnStageContent_Click(this, EventArgs.Empty);
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

        // Add this method to MainForm.cs
        private void RenderPdfPageToPreview(int pageIndex)
        {
            if (this.currentPdfDocument == null || this.currentPdfDocument.PageCount == 0)
            {
                if (this.currentPdfPageImage != null)
                {
                    Image temp = this.currentPdfPageImage;
                    this.currentPdfPageImage = null;
                    temp.Dispose();
                }
                if (this.picPreview.Image != null)
                {
                    Image temp = this.picPreview.Image;
                    this.picPreview.Image = null;
                    temp.Dispose();
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

                if (this.currentPdfPageImage != null)
                {
                    Image temp = this.currentPdfPageImage;
                    this.currentPdfPageImage = null;
                    temp.Dispose();
                }
                this.currentPdfPageImage = renderedPageImage;

                if (!isDisplayingStitchInMainPreview)
                {
                    if (this.picPreview.Image != null && this.picPreview.Image != this.currentPdfPageImage)
                    {
                        Image temp = this.picPreview.Image;
                        this.picPreview.Image = null;
                        temp.Dispose();
                    }
                    this.picPreview.Image = this.currentPdfPageImage; // Display the rendered page
                }

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
            if (!isDisplayingStitchInMainPreview && this.selectionRectangle.Width > 0 && this.selectionRectangle.Height > 0)
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
            Image img = (isDisplayingStitchInMainPreview && currentPdfPageImage != null) ? currentPdfPageImage : picPreview.Image;
            if (img == null)
                return RectangleF.Empty;

            float picBoxWidth = picPreview.ClientSize.Width;
            float picBoxHeight = picPreview.ClientSize.Height;
            float imgWidth = img.Width;
            float imgHeight = img.Height;

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


        private string GetAppDataFolderPath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appSpecificFolder = Path.Combine(appDataPath, "DreamsLivePresenterApp");
            Directory.CreateDirectory(appSpecificFolder);
            return appSpecificFolder;
        }

        private string GetSelectionsFilePath()
        {
            return Path.Combine(GetAppDataFolderPath(), "selections.json");
        }

        private string GetSettingsFilePath()
        {
            return Path.Combine(GetAppDataFolderPath(), "settings.json");
        }

        private void LoadSettings()
        {
            string filePath = GetSettingsFilePath();
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (settings != null)
                    {
                        if (settings.ContainsKey("DatabaseFolderPath"))
                        {
                            DatabaseFolderPath = settings["DatabaseFolderPath"];
                            if (!string.IsNullOrEmpty(DatabaseFolderPath))
                            {
                                lblDatabaseFolderPath.Text = "Database Folder: " + DatabaseFolderPath;
                                SetupDatabaseWatcher();
                            }
                            else
                            {
                                lblDatabaseFolderPath.Text = "Database Folder: Not Selected";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading settings: {ex.Message}");
                }
            }
            else
            {
                lblDatabaseFolderPath.Text = "Database Folder: Not Selected";
            }
        }

        private void SetupDatabaseWatcher()
        {
            if (_dbWatcher != null)
            {
                _dbWatcher.EnableRaisingEvents = false;
                _dbWatcher.Dispose();
                _dbWatcher = null;
            }

            if (!string.IsNullOrEmpty(DatabaseFolderPath) && Directory.Exists(DatabaseFolderPath))
            {
                _dbWatcher = new FileSystemWatcher(DatabaseFolderPath);
                _dbWatcher.IncludeSubdirectories = true;
                _dbWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

                FileSystemEventHandler handler = (s, e) => {
                    Interlocked.Increment(ref _galleryVersion);
                };

                _dbWatcher.Created += handler;
                _dbWatcher.Deleted += handler;
                _dbWatcher.Renamed += (s, e) => Interlocked.Increment(ref _galleryVersion);

                _dbWatcher.EnableRaisingEvents = true;
                _galleryVersion++; // Trigger initial load
            }
        }

        public void SaveSettings()
        {
            string filePath = GetSettingsFilePath();
            try
            {
                var settings = new Dictionary<string, string>
                {
                    { "DatabaseFolderPath", DatabaseFolderPath },
                    { "SkipOnePage", skipOnePage.ToString() },
                    { "TwoPagePdf", twoPagePdf.ToString() }
                };
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private void btnSetDatabaseFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select the Database Folder for Media Files";
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    DatabaseFolderPath = fbd.SelectedPath;
                    lblDatabaseFolderPath.Text = "Database Folder: " + DatabaseFolderPath;
                    SetupDatabaseWatcher();
                    SaveSettings();
                }
            }
        }

        public List<string> GetDatabaseSubfolders()
        {
            List<string> subfolders = new List<string>();
            if (string.IsNullOrEmpty(DatabaseFolderPath) || !Directory.Exists(DatabaseFolderPath))
                return subfolders;

            try
            {
                subfolders.AddRange(Directory.GetDirectories(DatabaseFolderPath, "*", SearchOption.AllDirectories)
                    .Select(d => d.Substring(DatabaseFolderPath.Length).TrimStart(Path.DirectorySeparatorChar)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting subfolders: {ex.Message}");
            }
            return subfolders;
        }

        public List<DatabaseFileInfo> GetDatabaseMediaFiles(string subfolder = "")
        {
            List<DatabaseFileInfo> files = new List<DatabaseFileInfo>();
            if (string.IsNullOrEmpty(DatabaseFolderPath) || !Directory.Exists(DatabaseFolderPath))
                return files;

            try
            {
                string targetPath = string.IsNullOrEmpty(subfolder) ? DatabaseFolderPath : Path.Combine(DatabaseFolderPath, subfolder);
                if (!Directory.Exists(targetPath)) return files;

                // Requirement: all files in folder without extension filtering, support non-media with icons
                var allFiles = Directory.EnumerateFiles(targetPath, "*.*", SearchOption.TopDirectoryOnly);

                foreach (var file in allFiles)
                {
                    files.Add(new DatabaseFileInfo(file, DatabaseFolderPath));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting media files: {ex.Message}");
            }
            return files;
        }

        public void OpenMediaFile(string relativePath, bool updateStaging = true)
        {
            if (string.IsNullOrEmpty(DatabaseFolderPath)) return;
            string fullPath = Path.Combine(DatabaseFolderPath, relativePath);
            if (File.Exists(fullPath))
            {
                this.Invoke((Action)(() => ProcessNewImage(fullPath, updateStaging)));
            }
        }


        private List<ImageSelectionData> LoadSelections()
        {
            string filePath = GetSelectionsFilePath();
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    if (string.IsNullOrWhiteSpace(json)) return new List<ImageSelectionData>();
                    return JsonConvert.DeserializeObject<List<ImageSelectionData>>(json) ?? new List<ImageSelectionData>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading selections: {ex.Message}");
                    // Optionally, handle corrupted file (e.g., backup and create new)
                }
            }
            return new List<ImageSelectionData>();
        }

        private void SaveSelections(List<ImageSelectionData> selections)
        {
            string filePath = GetSelectionsFilePath();
            try
            {
                string json = JsonConvert.SerializeObject(selections, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving selections: {ex.Message}");
                // Optionally, inform the user
            }
        }

        // In MainForm.cs
        private Rectangle ConvertOriginalImageRectToPreviewRect(RectangleF originalImageRect)
        {
            Image img = (isDisplayingStitchInMainPreview && currentPdfPageImage != null) ? currentPdfPageImage : picPreview.Image;
            if (this.picPreview == null || img == null || originalImageRect.IsEmpty ||
                this.picPreview.ClientSize.Width <= 0 || this.picPreview.ClientSize.Height <= 0)
            {
                return Rectangle.Empty;
            }

            float originalImageWidth = img.Width;
            float originalImageHeight = img.Height;

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

        private void SaveCurrentSelection()
        {
            if (!this.selectionRectangle.IsEmpty && this.selectedImagePath != null)
            {
                RectangleF? imageCoordsSelection = GetSelectedRegionInImageCoordinates();
                if (imageCoordsSelection.HasValue && imageCoordsSelection.Value.Width > 0 && imageCoordsSelection.Value.Height > 0)
                {
                    List<ImageSelectionData> selections = LoadSelections();
                    selections.RemoveAll(s => s.ImagePath.Equals(this.selectedImagePath, StringComparison.OrdinalIgnoreCase));
                    selections.Add(new ImageSelectionData(this.selectedImagePath, imageCoordsSelection.Value));
                    SaveSelections(selections);
                }
            }
            else if (this.selectionRectangle.IsEmpty && this.selectedImagePath != null)
            {
                List<ImageSelectionData> selections = LoadSelections();
                int removedCount = selections.RemoveAll(s => s.ImagePath.Equals(this.selectedImagePath, StringComparison.OrdinalIgnoreCase));
                if (removedCount > 0)
                {
                    SaveSelections(selections);
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
                if (isDisplayingStitchInMainPreview)
                {
                    isDisplayingStitchInMainPreview = false;
                    RenderPdfPageToPreview(currentPageNumber);
                }

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

        private void HandlePageStitching(Point newLocation, bool isMovingDown)
        {
            if (this.currentPdfDocument == null) return;
            ClearHighlights();
            Rectangle proposedRect = new Rectangle(newLocation, selectionRectangle.Size);
            RectangleF? normalizedRegion = GetSelectedRegionNormalized(proposedRect);

            if (!normalizedRegion.HasValue) return;

            RectangleF currentRect = normalizedRegion.Value;
            Bitmap stitched = null;

            if (isMovingDown)
            {
                // Check if we are stitching between columns or between pages
                float hCenter = selectionRectangle.X + (selectionRectangle.Width / 2f);
                RectangleF displayedImageRect = GetDisplayedImageRect();
                float pageHCenter = displayedImageRect.Left + (displayedImageRect.Width / 2f);
                bool isLeftSide = hCenter < pageHCenter;

                if (twoPagePdf && isLeftSide)
                {
                    // Stitching Left Column to Right Column
                    // Normalize horizontal coordinates relative to left/right halves
                    float leftHalfXStart = 0f;
                    float leftHalfXEnd = 0.5f;
                    float rightHalfXStart = 0.5f;

                    float visibleHeightOnLeft = Math.Max(0, 1.0f - currentRect.Top);
                    float overflowHeight = currentRect.Height - visibleHeightOnLeft;

                    if (overflowHeight > 0)
                    {
                        RectangleF leftSelection = new RectangleF(currentRect.X, currentRect.Y, currentRect.Width, visibleHeightOnLeft);
                        RectangleF rightSelection = new RectangleF(currentRect.X + 0.5f, 0, currentRect.Width, overflowHeight);
                        stitched = RenderStitchedPdfPages(currentPageNumber, currentPageNumber, leftSelection, rightSelection);
                    }
                }
                else
                {
                    // Stitching between pages
                    int skip = (skipOnePage && currentPageNumber < totalPdfPages - 2) ? 2 : 1;
                    float sourceX = currentRect.X;
                    if (twoPagePdf) sourceX = currentRect.X - 0.5f; // Selection was on right side, so it should be on left side of next page

                    float visibleHeightOnPage1 = Math.Max(0, 1.0f - currentRect.Top);
                    float overflowHeight = currentRect.Height - visibleHeightOnPage1;

                    if (overflowHeight > 0)
                    {
                        RectangleF page1Selection = new RectangleF(currentRect.X, currentRect.Y, currentRect.Width, visibleHeightOnPage1);
                        RectangleF page2Selection = new RectangleF(sourceX, 0, currentRect.Width, overflowHeight);
                        stitched = RenderStitchedPdfPages(currentPageNumber, currentPageNumber + skip, page1Selection, page2Selection);
                    }
                }
            }
            else // Moving Up
            {
                float hCenter = selectionRectangle.X + (selectionRectangle.Width / 2f);
                RectangleF displayedImageRect = GetDisplayedImageRect();
                float pageHCenter = displayedImageRect.Left + (displayedImageRect.Width / 2f);
                bool isRightSide = hCenter >= pageHCenter;

                if (twoPagePdf && isRightSide)
                {
                    // Stitching Right Column to Left Column
                    float visibleHeightOnRight = Math.Max(0, currentRect.Bottom);
                    float overflowHeight = currentRect.Height - visibleHeightOnRight;

                    if (overflowHeight > 0)
                    {
                        RectangleF leftSelection = new RectangleF(currentRect.X - 0.5f, 1.0f - overflowHeight, currentRect.Width, overflowHeight);
                        RectangleF rightSelection = new RectangleF(currentRect.X, 0, currentRect.Width, visibleHeightOnRight);
                        stitched = RenderStitchedPdfPages(currentPageNumber, currentPageNumber, leftSelection, rightSelection);
                    }
                }
                else
                {
                    // Stitching between pages
                    int skip = (skipOnePage && currentPageNumber > 1) ? 2 : 1;
                    float sourceX = currentRect.X;
                    if (twoPagePdf) sourceX = currentRect.X + 0.5f; // Selection was on left side, so it should be on right side of prev page

                    float overflowHeight = Math.Max(0, -currentRect.Top);
                    float visibleHeightOnPage2 = currentRect.Height - overflowHeight;

                    if (overflowHeight > 0)
                    {
                        RectangleF page1Selection = new RectangleF(sourceX, 1.0f - overflowHeight, currentRect.Width, overflowHeight);
                        RectangleF page2Selection = new RectangleF(currentRect.X, 0, currentRect.Width, visibleHeightOnPage2);
                        stitched = RenderStitchedPdfPages(currentPageNumber - skip, currentPageNumber, page1Selection, page2Selection);
                    }
                }
            }

            if (stitched != null)
            {
                this.secondaryPreviewPan = PointF.Empty;
                this.secondaryPreviewZoom = 1.0f;

                if (this.stagedStitchedImage != null)
                {
                    Image temp = this.stagedStitchedImage;
                    this.stagedStitchedImage = null;
                    temp.Dispose();
                }
                this.stagedStitchedImage = stitched;
                if (this.stagedMasterImage != null)
                {
                    Image temp = this.stagedMasterImage;
                    this.stagedMasterImage = null;
                    temp.Dispose();
                }

                Bitmap fitted = CreateFittedBitmap(this.stagedStitchedImage, picSecondaryPreview.ClientSize, picSecondaryPreview.BackColor);

                if (picSecondaryPreview.Image != null) picSecondaryPreview.Image.Dispose();
                picSecondaryPreview.Image = fitted;
                isSecondaryPreviewPopulated = true;

                // Requirement 3: Show stitched view in Main Preview too
                if (picPreview.Image != null && picPreview.Image != currentPdfPageImage)
                {
                    Image temp = picPreview.Image;
                    picPreview.Image = null;
                    temp.Dispose();
                }

                picPreview.Image = (Image)stitched.Clone();
                isDisplayingStitchInMainPreview = true;

                if (chkLinkLocalPreviewToPresenter.Checked)
                {
                    UpdateMainPresentation(null, -1, null, false, this.stagedStitchedImage, 0);
                }
                UpdateSecondaryPreviewBorderColor();
                UpdateButtonEnableStates();
            }
        }

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

                // Handle PDF page transitions with gradual stitching
                if (currentPdfDocument != null && yDirection != 0)
                {
                    bool isCrossingDown = yDirection > 0 && newLocation.Y > (int)displayedImageRect.Bottom;
                    bool isCrossingUp = yDirection < 0 && newLocation.Y + selectionRectangle.Height < (int)displayedImageRect.Top;

                    bool startStitchingDown = yDirection > 0 && newLocation.Y + selectionRectangle.Height > (int)displayedImageRect.Bottom;
                    bool startStitchingUp = yDirection < 0 && newLocation.Y < (int)displayedImageRect.Top;

                    float hCenter = selectionRectangle.X + (selectionRectangle.Width / 2f);
                    float pageHCenter = displayedImageRect.Left + (displayedImageRect.Width / 2f);
                    bool isLeftSide = hCenter < pageHCenter;

                    if (isCrossingDown)
                    {
                        if (twoPagePdf && isLeftSide)
                        {
                            // Snap from Left column to Right column
                            newLocation.X = (int)displayedImageRect.Right - selectionRectangle.Width;
                            newLocation.Y = (int)displayedImageRect.Top;
                            isDisplayingStitchInMainPreview = false;
                            RenderPdfPageToPreview(currentPageNumber);
                        }
                        else if (currentPageNumber < totalPdfPages - 1)
                        {
                            // Snap to next page
                            int skip = (skipOnePage && currentPageNumber < totalPdfPages - 2) ? 2 : 1;
                            isDisplayingStitchInMainPreview = false;
                            GoToPage(currentPageNumber + skip, true);
                            displayedImageRect = GetDisplayedImageRect();
                            newLocation.Y = (int)displayedImageRect.Top;
                            if (twoPagePdf) newLocation.X = (int)displayedImageRect.Left;
                        }
                    }
                    else if (isCrossingUp)
                    {
                        if (twoPagePdf && !isLeftSide)
                        {
                            // Snap from Right column to Left column
                            newLocation.X = (int)displayedImageRect.Left;
                            newLocation.Y = (int)displayedImageRect.Bottom - selectionRectangle.Height;
                            isDisplayingStitchInMainPreview = false;
                            RenderPdfPageToPreview(currentPageNumber);
                        }
                        else if (currentPageNumber > 0)
                        {
                            // Snap to previous page
                            int skip = (skipOnePage && currentPageNumber > 1) ? 2 : 1;
                            isDisplayingStitchInMainPreview = false;
                            GoToPage(currentPageNumber - skip, true);
                            displayedImageRect = GetDisplayedImageRect();
                            newLocation.Y = (int)displayedImageRect.Bottom - selectionRectangle.Height;
                            if (twoPagePdf) newLocation.X = (int)displayedImageRect.Right - selectionRectangle.Width;
                        }
                    }
                    else if (startStitchingDown || startStitchingUp)
                    {
                        // Check if transition is possible
                        bool canTransition = false;
                        if (yDirection > 0)
                        {
                            canTransition = (twoPagePdf && isLeftSide) || (currentPageNumber < totalPdfPages - 1);
                        }
                        else
                        {
                            canTransition = (twoPagePdf && !isLeftSide) || (currentPageNumber > 0);
                        }

                        if (canTransition)
                        {
                            HandlePageStitching(newLocation, yDirection > 0);
                        }
                    }
                    else if (isDisplayingStitchInMainPreview)
                    {
                        // We were stitching but now we are back fully on one page
                        isDisplayingStitchInMainPreview = false;
                        RenderPdfPageToPreview(currentPageNumber);
                    }
                }

                if (!isDisplayingStitchInMainPreview)
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

        public class DisplayItem
        {
            public string Name { get; set; }
            public Screen DisplayScreen { get; set; }
            public override string ToString() => Name; // Concise way to write the ToString
        }

        // In MainForm.cs
        public void ProcessUploadedFile(string tempPath, string originalFileName, bool updateStaging = true)
        {
            string fileExtension = Path.GetExtension(originalFileName).ToLowerInvariant();
            string newTempPath = Path.ChangeExtension(tempPath, fileExtension);

            try
            {
                if (File.Exists(newTempPath))
                {
                    File.Delete(newTempPath);
                }
                File.Move(tempPath, newTempPath);

                ProcessNewImage(newTempPath, updateStaging);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error processing uploaded file: {ex.Message}");
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
                if (File.Exists(newTempPath))
                {
                    File.Delete(newTempPath);
                }
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
                Image temp = this.picPreview.Image;
                this.picPreview.Image = null;
                if (temp != this.currentPdfPageImage) temp.Dispose();
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
                        PdfDocument temp = this.currentPdfDocument;
                        this.currentPdfDocument = null;
                        temp.Dispose();
                    }
                    this.currentPdfDocument = PdfDocument.Load(pdfPath);
                }

                if (this.currentPdfPageImage != null)
                {
                    Image temp = this.currentPdfPageImage;
                    this.currentPdfPageImage = null;
                    temp.Dispose();
                }
                this.isDisplayingStitchInMainPreview = false;
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
                PdfDocument temp = this.currentPdfDocument;
                this.currentPdfDocument = null;
                temp.Dispose();
                this.totalPdfPages = 0;
                this.currentPageNumber = 0;
            }
            if (this.currentPdfPageImage != null)
            {
                Image temp = this.currentPdfPageImage;
                this.currentPdfPageImage = null;
                temp.Dispose();
            }
            this.isDisplayingStitchInMainPreview = false;
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

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadSettings();
            PopulateDisplayComboBox();
            UpdateSecondaryPreviewAspectRatio(); // Call after populating and selecting a default display
            DisplayConnectionInfo();
        }

        private void DisplayConnectionInfo()
        {
            // This is now handled by the Label added in the constructor
        }

        public Image GetPreviewImage()
        {
            return picPreview.Image;
        }

        public Image GetSecondaryPreviewImage()
        {
            return picSecondaryPreview.Image;
        }

        public int GetCurrentPdfPage()
        {
            return currentPageNumber;
        }

        public int GetTotalPdfPages()
        {
            return totalPdfPages;
        }

        public Image RenderCurrentPdfPage(int dpi)
        {
            if (this.currentPdfDocument == null) return null;
            try
            {
                return this.currentPdfDocument.Render(this.currentPageNumber, dpi, dpi, PdfRenderFlags.Annotations | PdfRenderFlags.LcdText | PdfRenderFlags.CorrectFromDpi);
            }
            catch { return null; }
        }

        public void RemoteCrop(float x, float y, float w, float h)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => RemoteCrop(x, y, w, h)));
                return;
            }

            if (isDisplayingStitchInMainPreview)
            {
                isDisplayingStitchInMainPreview = false;
                RenderPdfPageToPreview(currentPageNumber);
            }

            if (string.IsNullOrEmpty(selectedImagePath)) return;

            // Prevent crop update if dimensions are zero (initialization artifacts)
            if (w <= 0 || h <= 0) return;

            // Update staged content region
            this.stagedContentPath = selectedImagePath;
            this.stagedContentPageNum = (this.currentPdfDocument != null) ? this.currentPageNumber : -1;
            this.stagedContentRegion = new RectangleF(x, y, w, h);
            this.stagedContentIsNormalized = true;
            this.stagedContentRotationAngle = this.currentManualRotationAngle;

            // Render to secondary preview
            RenderContentToPictureBox(
                this.picSecondaryPreview,
                this.stagedContentPath,
                this.stagedContentPageNum,
                this.stagedContentRegion,
                this.stagedContentIsNormalized,
                this.stagedContentRotationAngle
            );

            this.isSecondaryPreviewPopulated = (this.picSecondaryPreview.Image != null);
            this.secondaryPreviewPan = PointF.Empty;
            this.secondaryPreviewZoom = 1.0f;
            this.picSecondaryPreview.Invalidate();

            // Sync back to main preview
            SyncStagedSelectionToMain();
            this.previousSelectionRectangle = Rectangle.Empty;
            this.stagedSelectionRectangle = this.selectionRectangle;

            UpdateButtonAppearanceAndState();
            UpdateButtonEnableStates();
            UpdateSecondaryPreviewBorderColor();
            this.picPreview.Invalidate();

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
                        this.currentManualRotationAngle
                    );
                }
            }
        }

        private void SyncStagedSelectionToMain()
        {
            Image img = (isDisplayingStitchInMainPreview && currentPdfPageImage != null) ? currentPdfPageImage : picPreview.Image;
            if (img == null || !this.stagedContentRegion.HasValue) return;

            RectangleF normRegion;
            if (this.stagedContentIsNormalized)
            {
                normRegion = this.stagedContentRegion.Value;
            }
            else
            {
                // Convert pixel region to normalized
                float imgW = img.Width;
                float imgH = img.Height;
                normRegion = new RectangleF(
                    this.stagedContentRegion.Value.X / imgW,
                    this.stagedContentRegion.Value.Y / imgH,
                    this.stagedContentRegion.Value.Width / imgW,
                    this.stagedContentRegion.Value.Height / imgH
                );
            }

            // Convert normalized to source image pixel coords
            float sourceImgW = img.Width;
            float sourceImgH = img.Height;
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

        public bool IsPdfPrevButtonEnabled
        {
            get { return this.btnPrevPage != null && this.btnPrevPage.Enabled; }
        }

        public bool IsPdfNextButtonEnabled
        {
            get { return this.btnNextPage != null && this.btnNextPage.Enabled; }
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

        public RectangleF? GetCurrentSelectionNormalized()
        {
            if (selectionRectangle.IsEmpty) return null;
            return GetSelectedRegionNormalized(selectionRectangle);
        }

        public void btnStageContent_Click(object sender, EventArgs e)
        {
            if (isDisplayingStitchInMainPreview && stagedStitchedImage != null)
            {
                // Already staged during transition movement
                this.stagedSelectionRectangle = this.selectionRectangle;
                return;
            }

            if (string.IsNullOrEmpty(this.selectedImagePath))
            {
                ShowInfoMessage("Please load an image or PDF first.");
                return;
            }

            ClearHighlights();
            this.previousSelectionRectangle = Rectangle.Empty;
            this.stagedSelectionRectangle = Rectangle.Empty;

            if (this.stagedStitchedImage != null)
            {
                this.stagedStitchedImage.Dispose();
                this.stagedStitchedImage = null;
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
            GoToPage(this.currentPageNumber - 1, false);
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            GoToPage(this.currentPageNumber + 1, false);
        }

        public void NextPage()
        {
            GoToPage(this.currentPageNumber + 1, false);
        }

        public void PreviousPage()
        {
            GoToPage(this.currentPageNumber - 1, false);
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
                            GoToPage(desiredPageIndex, false);
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

                if (sourceBitmap == null) return;

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
        private RectangleF? GetSelectedRegionInImageCoordinates(Rectangle rect)
        {
            try
            {
                Image img = (isDisplayingStitchInMainPreview && currentPdfPageImage != null) ? currentPdfPageImage : picPreview.Image;
                if (img == null || rect.IsEmpty || rect.Width <= 0 || rect.Height <= 0)
                {
                    return null; // No image or no valid selection
                }

                // Original image dimensions
                float originalImageWidth = img.Width;
                float originalImageHeight = img.Height;

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
            catch (ArgumentException)
            {
                // Image might have been disposed or is otherwise invalid (e.g. during a fast transition)
                return null;
            }
        }

        private RectangleF? GetSelectedRegionNormalized(Rectangle rect)
        {
            RectangleF? pixelRegion = GetSelectedRegionInImageCoordinates(rect);
            Image img = (isDisplayingStitchInMainPreview && currentPdfPageImage != null) ? currentPdfPageImage : picPreview.Image;
            if (!pixelRegion.HasValue || img == null)
            {
                return null;
            }

            float imgWidth = img.Width;
            float imgHeight = img.Height;

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
            // This method can be expanded later to include other button states if needed.
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
                bool liveContentMatchesStaged;
                if (this.stagedStitchedImage != null)
                {
                    liveContentMatchesStaged = isPresenterShowingLiveContent && liveContentIsStitched;
                    // For simplicity, we assume if they are both stitched, they match during the transition
                }
                else
                {
                    liveContentMatchesStaged =
                        isPresenterShowingLiveContent && // Presenter must be actively showing "live" content
                        this.liveContentPath == this.stagedContentPath &&
                        this.liveContentPageNum == this.stagedContentPageNum &&
                        this.liveContentIsNormalized == this.stagedContentIsNormalized &&
                        !this.liveContentIsStitched &&
                        this.liveContentRotationAngle == this.stagedContentRotationAngle &&
                        AreRegionsEqual(this.liveContentRegion, this.stagedContentRegion);
                }

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

        // Theme switching logic
        private void ApplyTheme()
        {
            Color backColor;
            Color foreColor;
            Color buttonBackColor;
            Color buttonForeColor;
            Color textBoxBackColor;
            Color textBoxForeColor;

            if (isDarkMode)
            {
                // Dark Theme
                backColor = Color.FromArgb(45, 45, 48);
                foreColor = Color.White;
                buttonBackColor = Color.FromArgb(63, 63, 70);
                buttonForeColor = Color.White;
                textBoxBackColor = Color.FromArgb(30, 30, 30);
                textBoxForeColor = Color.White;
            }
            else
            {
                // Light Theme (default)
                backColor = SystemColors.Control;
                foreColor = SystemColors.ControlText;
                buttonBackColor = SystemColors.Control;
                buttonForeColor = SystemColors.ControlText;
                textBoxBackColor = SystemColors.Window;
                textBoxForeColor = SystemColors.WindowText;
            }

            this.BackColor = backColor;
            this.ForeColor = foreColor;

            // Apply to all controls on the form
            foreach (Control control in this.Controls)
            {
                ApplyThemeToControl(control, backColor, foreColor, buttonBackColor, buttonForeColor, textBoxBackColor, textBoxForeColor);
            }
            UpdateButtonAppearanceAndState(); // Re-apply specific button style after general theme application
        }

        private void ApplyThemeToControl(Control control, Color backColor, Color foreColor, Color buttonBackColor, Color buttonForeColor, Color textBoxBackColor, Color textBoxForeColor)
        {
            if (control == this.lblWebServerUrl && _httpWebServer != null && !_httpWebServer.IsRunning)
            {
                control.BackColor = backColor;
                control.ForeColor = Color.Red;
            }
            else
            {
                control.BackColor = backColor;
                control.ForeColor = foreColor;
            }

            if (control is Button button) // Use pattern matching
            {
                if (button == this.btnClearPresenterDisplay)
                {
                    // Specific styling for btnClearPresenterDisplay is handled by UpdateButtonAppearanceAndState,
                    // which is called after ApplyTheme finishes iterating.
                    // So, we skip applying generic button theme here for this specific button.
                }
                else if (button == this.btnHighlighter && this.highlighterActive)
                {
                    // Keep highlighter button yellow if active
                    button.BackColor = Color.Yellow;
                    button.ForeColor = Color.Black;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderColor = isDarkMode ? Color.DarkGray : SystemColors.ControlDark;
                }
                else
                {
                    // Apply generic button theme to other buttons
                    button.BackColor = buttonBackColor;
                    button.ForeColor = buttonForeColor;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderColor = isDarkMode ? Color.DarkGray : SystemColors.ControlDark;
                }
            }
            else if (control is TextBox)
            {
                var textBox = (TextBox)control;
                textBox.BackColor = textBoxBackColor;
                textBox.ForeColor = textBoxForeColor;
                textBox.BorderStyle = BorderStyle.FixedSingle; // Ensure border is visible
            }
            else if (control is ComboBox)
            {
                var comboBox = (ComboBox)control;
                comboBox.BackColor = textBoxBackColor; // Use TextBox colors for ComboBox
                comboBox.ForeColor = textBoxForeColor;
                // ComboBox style is harder to customize fully without custom drawing
            }
            else if (control is CheckBox)
            {
                // CheckBoxes use the parent's BackColor for their background area typically
                control.ForeColor = foreColor; // Text color
            }
            else if (control is Label)
            {
                // Labels are transparent by default, their BackColor refers to their own background if not transparent.
                // If you want their background to match the form, ensure their BackColor is set to the form's backColor
                // or they are set to transparent and the container has the right color.
                // For simplicity, we just set ForeColor. If labels have their own opaque background, set it.
                control.ForeColor = foreColor;
            }
            else if (control is PictureBox)
            {
                // PictureBoxes usually have their own content (Image).
                // Their BackColor is visible if no image or image has transparency.
                control.BackColor = isDarkMode ? Color.FromArgb(50, 50, 53) : SystemColors.ControlLight; // Slightly different shade for pic box background
            }
            // Add more control types if needed (e.g., GroupBox, Panel, etc.)

            // Recursively apply to child controls if the control is a container
            if (control.HasChildren)
            {
                foreach (Control childControl in control.Controls)
                {
                    ApplyThemeToControl(childControl, backColor, foreColor, buttonBackColor, buttonForeColor, textBoxBackColor, textBoxForeColor);
                }
            }
        }

        private void ToggleTheme()
        {
            isDarkMode = !isDarkMode;
            ApplyTheme();
        }

        private void btnToggleTheme_Click(object sender, EventArgs e)
        {
            ToggleTheme();
        }


        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return null;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult result = MessageBox.Show(this, "Are you sure you want to close the application?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            _httpWebServer?.Stop();

            if (_dbWatcher != null)
            {
                _dbWatcher.EnableRaisingEvents = false;
                _dbWatcher.Dispose();
                _dbWatcher = null;
            }

            if (this.stagedStitchedImage != null)
            {
                Image temp = this.stagedStitchedImage;
                this.stagedStitchedImage = null;
                temp.Dispose();
            }
            if (this.currentPdfPageImage != null)
            {
                Image temp = this.currentPdfPageImage;
                this.currentPdfPageImage = null;
                temp.Dispose();
            }
            if (this.stagedMasterImage != null)
            {
                Image temp = this.stagedMasterImage;
                this.stagedMasterImage = null;
                temp.Dispose();
            }

            base.OnFormClosing(e);
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
            else if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Left)
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

        private void UpdateSelectionSizeLabel()
        {
            if (this.lblSelectionSize != null)
            {
                this.lblSelectionSize.Text = $"Width: {this.selectionRectangle.Width}, Height: {this.selectionRectangle.Height}";
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
                // During transitions, we don't have a reliable single-page normalized coordinate
                // So we return the relative position within the stitched area itself
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

        private void NotifyPresenterOfLaserPoint()
        {
            if (this.activePresentationForm != null && !this.activePresentationForm.IsDisposed)
            {
                float docRadius = 5f;
                RectangleF contentRect = GetSecondaryPreviewContentRect();
                Image baseImage = (Image)this.stagedStitchedImage ?? this.stagedMasterImage;

                if (contentRect.Width > 0 && baseImage != null)
                {
                    float previewToDocRatio = baseImage.Width / contentRect.Width;
                    docRadius = 5f * previewToDocRatio;
                }
                this.activePresentationForm.UpdateLaserPointer(this.laserPointNormalized, docRadius);
            }
        }

        private void NotifyPresenterOfHighlights()
        {
            if (this.activePresentationForm != null && !this.activePresentationForm.IsDisposed)
            {
                float docWidth = 20f;
                RectangleF contentRect = GetSecondaryPreviewContentRect();
                Image baseImage = (Image)this.stagedStitchedImage ?? this.stagedMasterImage;

                if (contentRect.Width > 0 && baseImage != null)
                {
                    float previewToDocRatio = baseImage.Width / contentRect.Width;
                    docWidth = 20f * previewToDocRatio;
                }
                this.activePresentationForm.UpdateHighlights(this.highlightsNormalized, this.highlighterColor, docWidth);
            }
        }

        private void ClearHighlights()
        {
            this.highlightsNormalized.Clear();
            this.picSecondaryPreview.Invalidate();
            NotifyPresenterOfHighlights();
        }

        private void btnHighlighter_Click(object sender, EventArgs e)
        {
            this.highlighterActive = !this.highlighterActive;

            if (this.highlighterActive)
            {
                this.btnHighlighter.BackColor = Color.Yellow;
                this.btnHighlighter.ForeColor = Color.Black;
            }
            else
            {
                // Clear highlights when disabling
                ClearHighlights();
                this.btnHighlighter.BackColor = isDarkMode ? Color.FromArgb(63, 63, 70) : SystemColors.Control;
                this.btnHighlighter.ForeColor = isDarkMode ? Color.White : SystemColors.ControlText;
            }
        }

        public static void DrawHighlightStroke(Graphics g, PointF[] points, float width, Color? strokeColor = null)
        {
            if (points == null || points.Length == 0) return;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            Color color = strokeColor ?? Color.Yellow;

            // Multiple layers for "glow" effect
            int layers = 3;
            for (int i = layers; i >= 0; i--)
            {
                float currentWidth = width + i * (width * 0.25f);
                int alpha = (i == 0) ? 120 : (int)(50 / (i + 0.5f));
                using (Pen highlightPen = new Pen(Color.FromArgb(alpha, color), currentWidth))
                {
                    highlightPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    highlightPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    highlightPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                    if (points.Length == 1)
                    {
                        using (Brush b = new SolidBrush(Color.FromArgb(alpha, color)))
                        {
                            g.FillEllipse(b, points[0].X - currentWidth / 2f, points[0].Y - currentWidth / 2f, currentWidth, currentWidth);
                        }
                    }
                    else
                    {
                        g.DrawLines(highlightPen, points);
                    }
                }
            }
        }

        public static void DrawLaserPointer(Graphics g, PointF center, float radius)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            // Outer glow
            for (int i = 8; i > 0; i--)
            {
                float r = radius + i * 1.5f;
                int alpha = (int)(60 / (i * 0.8f + 1));
                using (Brush b = new SolidBrush(Color.FromArgb(alpha, Color.Red)))
                {
                    g.FillEllipse(b, center.X - r, center.Y - r, r * 2, r * 2);
                }
            }

            // Inner core glow
            using (Brush b = new SolidBrush(Color.FromArgb(150, Color.White)))
            {
                g.FillEllipse(b, center.X - radius * 0.6f, center.Y - radius * 0.6f, radius * 1.2f, radius * 1.2f);
            }

            // Core
            using (Brush b = new SolidBrush(Color.Red))
            {
                g.FillEllipse(b, center.X - radius * 0.4f, center.Y - radius * 0.4f, radius * 0.8f, radius * 0.8f);
            }
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

        private void btnHelp_Click(object sender, EventArgs e)
        {
            using (ActivationForm activationForm = new ActivationForm())
            {
                activationForm.ShowDialog(this);
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

        public RectangleF? GetStagedSelectionNormalized()
        {
            if (stagedSelectionRectangle.IsEmpty) return null;
            return GetSelectedRegionNormalized(stagedSelectionRectangle);
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

                    string targetDir = Path.GetFullPath(Path.Combine(DatabaseFolderPath, targetSub));
                    // Security check: Ensure target is within DatabaseFolderPath
                    if (!targetDir.StartsWith(Path.GetFullPath(DatabaseFolderPath), StringComparison.OrdinalIgnoreCase))
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