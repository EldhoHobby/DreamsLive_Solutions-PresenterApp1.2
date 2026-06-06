// Version DreamsLive_Solutions-PresenterApp1.2_V1.2.0.3 (0)
// stable build_03 28 2026
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
        private bool isDarkMode = true; // Added for theme switching - default to the modern dark look
        private bool isPresenterBlackedOut = false; // Added for blackout toggle
        private bool hasAlwaysOnTopBeenAutoChecked = false;

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



































        public class DisplayItem
        {
            public string Name { get; set; }
            public Screen DisplayScreen { get; set; }
            public override string ToString() => Name; // Concise way to write the ToString
        }












        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadSettings();
            PopulateDisplayComboBox();
            UpdateSecondaryPreviewAspectRatio(); // Call after populating and selecting a default display
            DisplayConnectionInfo();
        }









        public bool IsPdfPrevButtonEnabled
        {
            get { return this.btnPrevPage != null && this.btnPrevPage.Enabled; }
        }

        public bool IsPdfNextButtonEnabled
        {
            get { return this.btnNextPage != null && this.btnNextPage.Enabled; }
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
                this.stagedStitchedImage.Dispose();
                this.stagedStitchedImage = null;
            }
            if (this.stagedMasterImage != null)
            {
                this.stagedMasterImage.Dispose();
                this.stagedMasterImage = null;
            }

            base.OnFormClosing(e);
        }































    }

}