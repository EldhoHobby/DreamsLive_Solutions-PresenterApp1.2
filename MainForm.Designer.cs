namespace DreamsLive_Solutions_PresenterApp1
{
    partial class MainForm
    {
        // ---------------------------------------------------------------------
        // DESIGNER vs RUNTIME APPEARANCE
        // The Visual Studio designer renders ONLY the controls/properties defined
        // in InitializeComponent() below. It does not run MainForm_Load or any
        // theming code, so the design surface intentionally looks different from
        // the running app. At runtime the following is applied on top:
        //   * ApplyTheme() -> LinearTheme.Apply(this): recolors/restyles all
        //     controls (colors, fonts, flat/rounded buttons) for light/dark mode.
        //   * LinearTheme.InstallHeader(...): creates and adds a header Panel in
        //     code (not present in this designer file).
        // Use this designer for LAYOUT (control positions/sizes); the themed skin
        // is layered on at runtime by design. See LinearTheme.cs / MainForm.Theme.cs.
        // ---------------------------------------------------------------------

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tnBrowse = new System.Windows.Forms.Button();
            this.lblImagePath = new System.Windows.Forms.Label();
            this.cmbDisplays = new System.Windows.Forms.ComboBox();
            this.btnStageContent = new System.Windows.Forms.Button();
            this.btnPrevPage = new System.Windows.Forms.Button();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.txtCurrentPageNum = new System.Windows.Forms.TextBox();
            this.lblTotalPages = new System.Windows.Forms.Label();
            this.btnPushToPresenter = new System.Windows.Forms.Button();
            this.chkLinkLocalPreviewToPresenter = new System.Windows.Forms.CheckBox();
            this.btnClearPresenterDisplay = new System.Windows.Forms.Button();
            this.btnToggleTheme = new System.Windows.Forms.Button();
            this.btnCloseLivePresenter = new System.Windows.Forms.Button();
            this.lblConnectionInfo = new System.Windows.Forms.Label();
            this.cmbDisplayMode = new System.Windows.Forms.ComboBox();
            this.lblWebServerUrl = new System.Windows.Forms.Label();
            this.panelSecondaryPreviewBorder = new System.Windows.Forms.Panel();
            this.picSecondaryPreview = new System.Windows.Forms.PictureBox();
            this.liveIndicator = new DreamsLive_Solutions_PresenterApp1.LiveIndicatorControl();
            this.chkAlwaysOnTop = new System.Windows.Forms.CheckBox();
            this.lblMessage = new System.Windows.Forms.Label();
            this.btnUp = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.lblMoveStep = new System.Windows.Forms.Label();
            this.txtMoveStep = new System.Windows.Forms.TextBox();
            this.btnHelp = new System.Windows.Forms.Button();
            this.chkEnableScroll = new System.Windows.Forms.CheckBox();
            this.chkAutoStagePreview = new System.Windows.Forms.CheckBox();
            this.lblSelectionSize = new System.Windows.Forms.Label();
            this.chkLaserPointer = new System.Windows.Forms.CheckBox();
            this.btnHighlighter = new System.Windows.Forms.Button();
            this.btnEditContent = new System.Windows.Forms.Button();
            this.btnMessageOkay = new System.Windows.Forms.Button();
            this.lblPreviewLabel = new System.Windows.Forms.Label();
            this.lblStagedLabel = new System.Windows.Forms.Label();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.btnSetDatabaseFolder = new System.Windows.Forms.Button();
            this.lblDatabaseFolderPath = new System.Windows.Forms.Label();
            this.btnSnip = new System.Windows.Forms.Button();
            this.btnOpenGallery = new System.Windows.Forms.Button();
            this.btnAddToDatabase = new System.Windows.Forms.Button();
            this.logoBox = new System.Windows.Forms.PictureBox();
            this.tableCenter = new System.Windows.Forms.TableLayoutPanel();
            this.paneLocal = new System.Windows.Forms.TableLayoutPanel();
            this.flowMid = new System.Windows.Forms.FlowLayoutPanel();
            this.paneProgram = new System.Windows.Forms.TableLayoutPanel();
            this.panelBottom = new System.Windows.Forms.TableLayoutPanel();
            this.tableDocks = new System.Windows.Forms.TableLayoutPanel();
            this.cardSource = new System.Windows.Forms.TableLayoutPanel();
            this.lblHdrSource = new System.Windows.Forms.Label();
            this.flowSource = new System.Windows.Forms.FlowLayoutPanel();
            this.cardOutput = new System.Windows.Forms.TableLayoutPanel();
            this.lblHdrOutput = new System.Windows.Forms.Label();
            this.flowOutput = new System.Windows.Forms.FlowLayoutPanel();
            this.cardTools = new System.Windows.Forms.TableLayoutPanel();
            this.lblHdrTools = new System.Windows.Forms.Label();
            this.flowTools = new System.Windows.Forms.FlowLayoutPanel();
            this.cardPosition = new System.Windows.Forms.TableLayoutPanel();
            this.lblHdrPosition = new System.Windows.Forms.Label();
            this.flowPosition = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlNudge = new System.Windows.Forms.Panel();
            this.tableStatus = new System.Windows.Forms.TableLayoutPanel();
            this.flowStatus = new System.Windows.Forms.FlowLayoutPanel();
            this.panelSecondaryPreviewBorder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSecondaryPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.logoBox)).BeginInit();
            this.tableCenter.SuspendLayout();
            this.paneLocal.SuspendLayout();
            this.flowMid.SuspendLayout();
            this.paneProgram.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.tableDocks.SuspendLayout();
            this.cardSource.SuspendLayout();
            this.flowSource.SuspendLayout();
            this.cardOutput.SuspendLayout();
            this.flowOutput.SuspendLayout();
            this.cardTools.SuspendLayout();
            this.flowTools.SuspendLayout();
            this.cardPosition.SuspendLayout();
            this.flowPosition.SuspendLayout();
            this.pnlNudge.SuspendLayout();
            this.tableStatus.SuspendLayout();
            this.flowStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // tnBrowse
            // 
            this.tnBrowse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.tnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.tnBrowse.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.tnBrowse.Location = new System.Drawing.Point(3, 3);
            this.tnBrowse.Name = "tnBrowse";
            this.tnBrowse.Size = new System.Drawing.Size(75, 23);
            this.tnBrowse.TabIndex = 0;
            this.tnBrowse.Text = "Browse...";
            this.tnBrowse.UseVisualStyleBackColor = false;
            this.tnBrowse.Click += new System.EventHandler(this.tnBrowse_Click);
            // 
            // lblImagePath
            // 
            this.lblImagePath.AutoSize = true;
            this.lblImagePath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.lblImagePath.Location = new System.Drawing.Point(129, 29);
            this.lblImagePath.Name = "lblImagePath";
            this.lblImagePath.Size = new System.Drawing.Size(113, 13);
            this.lblImagePath.TabIndex = 1;
            this.lblImagePath.Text = "Selected Image: None";
            // 
            // cmbDisplays
            // 
            this.cmbDisplays.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.cmbDisplays.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDisplays.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbDisplays.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.cmbDisplays.FormattingEnabled = true;
            this.cmbDisplays.Location = new System.Drawing.Point(3, 3);
            this.cmbDisplays.Name = "cmbDisplays";
            this.cmbDisplays.Size = new System.Drawing.Size(146, 21);
            this.cmbDisplays.TabIndex = 2;
            // 
            // btnStageContent
            // 
            this.btnStageContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(106)))), ((int)(((byte)(210)))));
            this.btnStageContent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStageContent.ForeColor = System.Drawing.Color.White;
            this.btnStageContent.Location = new System.Drawing.Point(3, 45);
            this.btnStageContent.Name = "btnStageContent";
            this.btnStageContent.Size = new System.Drawing.Size(136, 36);
            this.btnStageContent.TabIndex = 3;
            this.btnStageContent.Text = "Stage Preview";
            this.btnStageContent.UseVisualStyleBackColor = false;
            this.btnStageContent.Click += new System.EventHandler(this.btnStageContent_Click);
            // 
            // btnPrevPage
            // 
            this.btnPrevPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnPrevPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevPage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnPrevPage.Location = new System.Drawing.Point(3, 75);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new System.Drawing.Size(65, 23);
            this.btnPrevPage.TabIndex = 5;
            this.btnPrevPage.Text = "Prev Page";
            this.btnPrevPage.UseVisualStyleBackColor = false;
            // 
            // btnNextPage
            // 
            this.btnNextPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnNextPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNextPage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnNextPage.Location = new System.Drawing.Point(215, 75);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(65, 23);
            this.btnNextPage.TabIndex = 6;
            this.btnNextPage.Text = "Next Page";
            this.btnNextPage.UseVisualStyleBackColor = false;
            // 
            // txtCurrentPageNum
            // 
            this.txtCurrentPageNum.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.txtCurrentPageNum.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.txtCurrentPageNum.Location = new System.Drawing.Point(74, 75);
            this.txtCurrentPageNum.Name = "txtCurrentPageNum";
            this.txtCurrentPageNum.Size = new System.Drawing.Size(63, 20);
            this.txtCurrentPageNum.TabIndex = 7;
            // 
            // lblTotalPages
            // 
            this.lblTotalPages.AutoSize = true;
            this.lblTotalPages.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.lblTotalPages.Location = new System.Drawing.Point(143, 72);
            this.lblTotalPages.Name = "lblTotalPages";
            this.lblTotalPages.Size = new System.Drawing.Size(66, 13);
            this.lblTotalPages.TabIndex = 8;
            this.lblTotalPages.Text = "/TotalPages";
            // 
            // btnPushToPresenter
            // 
            this.btnPushToPresenter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(106)))), ((int)(((byte)(210)))));
            this.btnPushToPresenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPushToPresenter.ForeColor = System.Drawing.Color.White;
            this.btnPushToPresenter.Location = new System.Drawing.Point(3, 110);
            this.btnPushToPresenter.Name = "btnPushToPresenter";
            this.btnPushToPresenter.Size = new System.Drawing.Size(136, 36);
            this.btnPushToPresenter.TabIndex = 10;
            this.btnPushToPresenter.Text = "Go Live";
            this.btnPushToPresenter.UseVisualStyleBackColor = false;
            // 
            // chkLinkLocalPreviewToPresenter
            // 
            this.chkLinkLocalPreviewToPresenter.AutoSize = true;
            this.chkLinkLocalPreviewToPresenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkLinkLocalPreviewToPresenter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.chkLinkLocalPreviewToPresenter.Location = new System.Drawing.Point(3, 152);
            this.chkLinkLocalPreviewToPresenter.Name = "chkLinkLocalPreviewToPresenter";
            this.chkLinkLocalPreviewToPresenter.Size = new System.Drawing.Size(133, 17);
            this.chkLinkLocalPreviewToPresenter.TabIndex = 11;
            this.chkLinkLocalPreviewToPresenter.Text = "Auto-Send to Presenter";
            this.chkLinkLocalPreviewToPresenter.UseVisualStyleBackColor = true;
            // 
            // btnClearPresenterDisplay
            // 
            this.btnClearPresenterDisplay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnClearPresenterDisplay.Enabled = false;
            this.btnClearPresenterDisplay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearPresenterDisplay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnClearPresenterDisplay.Location = new System.Drawing.Point(3, 175);
            this.btnClearPresenterDisplay.Name = "btnClearPresenterDisplay";
            this.btnClearPresenterDisplay.Size = new System.Drawing.Size(136, 36);
            this.btnClearPresenterDisplay.TabIndex = 12;
            this.btnClearPresenterDisplay.Text = "Blank Presenter";
            this.btnClearPresenterDisplay.UseVisualStyleBackColor = false;
            // 
            // btnToggleTheme
            // 
            this.btnToggleTheme.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnToggleTheme.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleTheme.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnToggleTheme.Location = new System.Drawing.Point(215, 30);
            this.btnToggleTheme.Name = "btnToggleTheme";
            this.btnToggleTheme.Size = new System.Drawing.Size(100, 23);
            this.btnToggleTheme.TabIndex = 14;
            this.btnToggleTheme.Text = "Toggle Theme";
            this.btnToggleTheme.UseVisualStyleBackColor = false;
            this.btnToggleTheme.Click += new System.EventHandler(this.btnToggleTheme_Click);
            // 
            // btnCloseLivePresenter
            // 
            this.btnCloseLivePresenter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnCloseLivePresenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCloseLivePresenter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnCloseLivePresenter.Location = new System.Drawing.Point(3, 217);
            this.btnCloseLivePresenter.Name = "btnCloseLivePresenter";
            this.btnCloseLivePresenter.Size = new System.Drawing.Size(136, 36);
            this.btnCloseLivePresenter.TabIndex = 15;
            this.btnCloseLivePresenter.Text = "Close Live";
            this.btnCloseLivePresenter.UseVisualStyleBackColor = false;
            this.btnCloseLivePresenter.Click += new System.EventHandler(this.btnCloseLivePresenter_Click);
            // 
            // lblConnectionInfo
            // 
            this.lblConnectionInfo.AutoSize = true;
            this.lblConnectionInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.lblConnectionInfo.Location = new System.Drawing.Point(3, 0);
            this.lblConnectionInfo.Name = "lblConnectionInfo";
            this.lblConnectionInfo.Size = new System.Drawing.Size(126, 13);
            this.lblConnectionInfo.TabIndex = 16;
            this.lblConnectionInfo.Text = "IP Address Appears Here";
            // 
            // cmbDisplayMode
            // 
            this.cmbDisplayMode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.cmbDisplayMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDisplayMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbDisplayMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.cmbDisplayMode.FormattingEnabled = true;
            this.cmbDisplayMode.Items.AddRange(new object[] {
            "Fit",
            "Fill",
            "Stretch",
            "Tile",
            "Center"});
            this.cmbDisplayMode.Location = new System.Drawing.Point(155, 3);
            this.cmbDisplayMode.Name = "cmbDisplayMode";
            this.cmbDisplayMode.Size = new System.Drawing.Size(146, 21);
            this.cmbDisplayMode.TabIndex = 17;
            this.cmbDisplayMode.SelectedIndexChanged += new System.EventHandler(this.cmbDisplayMode_SelectedIndexChanged);
            // 
            // lblWebServerUrl
            // 
            this.lblWebServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblWebServerUrl.AutoSize = true;
            this.lblWebServerUrl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(143)))), ((int)(((byte)(152)))));
            this.lblWebServerUrl.Location = new System.Drawing.Point(135, 16);
            this.lblWebServerUrl.Name = "lblWebServerUrl";
            this.lblWebServerUrl.Size = new System.Drawing.Size(115, 13);
            this.lblWebServerUrl.TabIndex = 18;
            this.lblWebServerUrl.Text = "Web Server: Starting...";
            // 
            // panelSecondaryPreviewBorder
            // 
            this.panelSecondaryPreviewBorder.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.panelSecondaryPreviewBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(8)))), ((int)(((byte)(10)))));
            this.panelSecondaryPreviewBorder.Controls.Add(this.picSecondaryPreview);
            this.panelSecondaryPreviewBorder.Location = new System.Drawing.Point(3, 30);
            this.panelSecondaryPreviewBorder.Name = "panelSecondaryPreviewBorder";
            this.panelSecondaryPreviewBorder.Padding = new System.Windows.Forms.Padding(2);
            this.panelSecondaryPreviewBorder.Size = new System.Drawing.Size(595, 425);
            this.panelSecondaryPreviewBorder.TabIndex = 9;
            // 
            // picSecondaryPreview
            // 
            this.picSecondaryPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.picSecondaryPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picSecondaryPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picSecondaryPreview.Location = new System.Drawing.Point(2, 2);
            this.picSecondaryPreview.Name = "picSecondaryPreview";
            this.picSecondaryPreview.Size = new System.Drawing.Size(591, 421);
            this.picSecondaryPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picSecondaryPreview.TabIndex = 0;
            this.picSecondaryPreview.TabStop = false;
            // 
            // liveIndicator
            // 
            this.liveIndicator.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.liveIndicator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(17)))), ((int)(((byte)(20)))));
            this.liveIndicator.IsLive = false;
            this.liveIndicator.Location = new System.Drawing.Point(3, 34);
            this.liveIndicator.Name = "liveIndicator";
            this.liveIndicator.Size = new System.Drawing.Size(160, 28);
            this.liveIndicator.TabIndex = 0;
            // 
            // chkAlwaysOnTop
            // 
            this.chkAlwaysOnTop.AutoSize = true;
            this.chkAlwaysOnTop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkAlwaysOnTop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.chkAlwaysOnTop.Location = new System.Drawing.Point(3, 59);
            this.chkAlwaysOnTop.Name = "chkAlwaysOnTop";
            this.chkAlwaysOnTop.Size = new System.Drawing.Size(93, 17);
            this.chkAlwaysOnTop.TabIndex = 19;
            this.chkAlwaysOnTop.Text = "Always on Top";
            this.chkAlwaysOnTop.UseVisualStyleBackColor = true;
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.lblMessage.Location = new System.Drawing.Point(515, 0);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(0, 13);
            this.lblMessage.TabIndex = 20;
            // 
            // btnUp
            // 
            this.btnUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnUp.Location = new System.Drawing.Point(34, 2);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(30, 30);
            this.btnUp.TabIndex = 25;
            this.btnUp.Text = "↑";
            this.btnUp.UseVisualStyleBackColor = false;
            // 
            // btnDown
            // 
            this.btnDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDown.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnDown.Location = new System.Drawing.Point(34, 34);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(30, 30);
            this.btnDown.TabIndex = 26;
            this.btnDown.Text = "↓";
            this.btnDown.UseVisualStyleBackColor = false;
            // 
            // btnLeft
            // 
            this.btnLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLeft.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnLeft.Location = new System.Drawing.Point(2, 34);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(30, 30);
            this.btnLeft.TabIndex = 27;
            this.btnLeft.Text = "←";
            this.btnLeft.UseVisualStyleBackColor = false;
            // 
            // btnRight
            // 
            this.btnRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRight.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnRight.Location = new System.Drawing.Point(66, 34);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(30, 30);
            this.btnRight.TabIndex = 28;
            this.btnRight.Text = "→";
            this.btnRight.UseVisualStyleBackColor = false;
            // 
            // btnSettings
            // 
            this.btnSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnSettings.Location = new System.Drawing.Point(3, 30);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(100, 23);
            this.btnSettings.TabIndex = 43;
            this.btnSettings.Text = "Scroll Settings";
            this.btnSettings.UseVisualStyleBackColor = false;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // lblMoveStep
            // 
            this.lblMoveStep.AutoSize = true;
            this.lblMoveStep.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.lblMoveStep.Location = new System.Drawing.Point(209, 0);
            this.lblMoveStep.Name = "lblMoveStep";
            this.lblMoveStep.Size = new System.Drawing.Size(62, 13);
            this.lblMoveStep.TabIndex = 29;
            this.lblMoveStep.Text = "Move Step:";
            // 
            // txtMoveStep
            // 
            this.txtMoveStep.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.txtMoveStep.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.txtMoveStep.Location = new System.Drawing.Point(277, 3);
            this.txtMoveStep.Name = "txtMoveStep";
            this.txtMoveStep.Size = new System.Drawing.Size(40, 20);
            this.txtMoveStep.TabIndex = 30;
            this.txtMoveStep.Text = "50";
            // 
            // btnHelp
            // 
            this.btnHelp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnHelp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHelp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnHelp.Location = new System.Drawing.Point(109, 30);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(100, 23);
            this.btnHelp.TabIndex = 22;
            this.btnHelp.Text = "Help";
            this.btnHelp.UseVisualStyleBackColor = false;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // chkEnableScroll
            // 
            this.chkEnableScroll.AutoSize = true;
            this.chkEnableScroll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkEnableScroll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.chkEnableScroll.Location = new System.Drawing.Point(191, 3);
            this.chkEnableScroll.Name = "chkEnableScroll";
            this.chkEnableScroll.Size = new System.Drawing.Size(110, 17);
            this.chkEnableScroll.TabIndex = 23;
            this.chkEnableScroll.Text = "Enable Auto Scroll";
            this.chkEnableScroll.UseVisualStyleBackColor = true;
            // 
            // chkAutoStagePreview
            // 
            this.chkAutoStagePreview.AutoSize = true;
            this.chkAutoStagePreview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkAutoStagePreview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.chkAutoStagePreview.Location = new System.Drawing.Point(3, 87);
            this.chkAutoStagePreview.Name = "chkAutoStagePreview";
            this.chkAutoStagePreview.Size = new System.Drawing.Size(117, 17);
            this.chkAutoStagePreview.TabIndex = 24;
            this.chkAutoStagePreview.Text = "Auto Stage Preview";
            this.chkAutoStagePreview.UseVisualStyleBackColor = true;
            // 
            // lblSelectionSize
            // 
            this.lblSelectionSize.AutoSize = true;
            this.lblSelectionSize.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.lblSelectionSize.Location = new System.Drawing.Point(107, 0);
            this.lblSelectionSize.Name = "lblSelectionSize";
            this.lblSelectionSize.Size = new System.Drawing.Size(96, 13);
            this.lblSelectionSize.TabIndex = 31;
            this.lblSelectionSize.Text = "Width: 0, Height: 0";
            // 
            // chkLaserPointer
            // 
            this.chkLaserPointer.AutoSize = true;
            this.chkLaserPointer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkLaserPointer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.chkLaserPointer.Location = new System.Drawing.Point(3, 3);
            this.chkLaserPointer.Name = "chkLaserPointer";
            this.chkLaserPointer.Size = new System.Drawing.Size(85, 17);
            this.chkLaserPointer.TabIndex = 32;
            this.chkLaserPointer.Text = "Laser Pointer";
            this.chkLaserPointer.UseVisualStyleBackColor = true;
            // 
            // btnHighlighter
            // 
            this.btnHighlighter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnHighlighter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHighlighter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnHighlighter.Location = new System.Drawing.Point(94, 3);
            this.btnHighlighter.Name = "btnHighlighter";
            this.btnHighlighter.Size = new System.Drawing.Size(91, 25);
            this.btnHighlighter.TabIndex = 33;
            this.btnHighlighter.Text = "Highlighter";
            this.btnHighlighter.UseVisualStyleBackColor = false;
            this.btnHighlighter.Click += new System.EventHandler(this.btnHighlighter_Click);
            // 
            // btnEditContent
            // 
            this.btnEditContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnEditContent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditContent.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnEditContent.Location = new System.Drawing.Point(3, 3);
            this.btnEditContent.Name = "btnEditContent";
            this.btnEditContent.Size = new System.Drawing.Size(136, 36);
            this.btnEditContent.TabIndex = 38;
            this.btnEditContent.Text = "Edit / Crop Preview";
            this.btnEditContent.UseVisualStyleBackColor = false;
            // 
            // btnMessageOkay
            // 
            this.btnMessageOkay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnMessageOkay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMessageOkay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnMessageOkay.Location = new System.Drawing.Point(521, 3);
            this.btnMessageOkay.Name = "btnMessageOkay";
            this.btnMessageOkay.Size = new System.Drawing.Size(75, 23);
            this.btnMessageOkay.TabIndex = 21;
            this.btnMessageOkay.Text = "Okay";
            this.btnMessageOkay.UseVisualStyleBackColor = false;
            // 
            // lblPreviewLabel
            // 
            this.lblPreviewLabel.BackColor = System.Drawing.Color.Transparent;
            this.lblPreviewLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPreviewLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.lblPreviewLabel.Location = new System.Drawing.Point(3, 0);
            this.lblPreviewLabel.Name = "lblPreviewLabel";
            this.lblPreviewLabel.Size = new System.Drawing.Size(594, 22);
            this.lblPreviewLabel.TabIndex = 50;
            this.lblPreviewLabel.Text = "Local preview";
            this.lblPreviewLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStagedLabel
            // 
            this.lblStagedLabel.BackColor = System.Drawing.Color.Transparent;
            this.lblStagedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStagedLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.lblStagedLabel.Location = new System.Drawing.Point(3, 0);
            this.lblStagedLabel.Name = "lblStagedLabel";
            this.lblStagedLabel.Size = new System.Drawing.Size(595, 22);
            this.lblStagedLabel.TabIndex = 51;
            this.lblStagedLabel.Text = "Program / live";
            this.lblStagedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // picPreview
            // 
            this.picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picPreview.Location = new System.Drawing.Point(3, 25);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(594, 436);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreview.TabIndex = 4;
            this.picPreview.TabStop = false;
            // 
            // btnSetDatabaseFolder
            // 
            this.btnSetDatabaseFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSetDatabaseFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnSetDatabaseFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetDatabaseFolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnSetDatabaseFolder.Location = new System.Drawing.Point(389, 3);
            this.btnSetDatabaseFolder.Name = "btnSetDatabaseFolder";
            this.btnSetDatabaseFolder.Size = new System.Drawing.Size(120, 23);
            this.btnSetDatabaseFolder.TabIndex = 36;
            this.btnSetDatabaseFolder.Text = "Set Database Folder";
            this.btnSetDatabaseFolder.UseVisualStyleBackColor = false;
            this.btnSetDatabaseFolder.Click += new System.EventHandler(this.btnSetDatabaseFolder_Click);
            // 
            // lblDatabaseFolderPath
            // 
            this.lblDatabaseFolderPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblDatabaseFolderPath.AutoSize = true;
            this.lblDatabaseFolderPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.lblDatabaseFolderPath.Location = new System.Drawing.Point(256, 16);
            this.lblDatabaseFolderPath.Name = "lblDatabaseFolderPath";
            this.lblDatabaseFolderPath.Size = new System.Drawing.Size(127, 13);
            this.lblDatabaseFolderPath.TabIndex = 37;
            this.lblDatabaseFolderPath.Text = "Database Folder: Not Set";
            // 
            // btnSnip
            // 
            this.btnSnip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnSnip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSnip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnSnip.Location = new System.Drawing.Point(84, 3);
            this.btnSnip.Name = "btnSnip";
            this.btnSnip.Size = new System.Drawing.Size(75, 23);
            this.btnSnip.TabIndex = 52;
            this.btnSnip.Text = "Snip";
            this.btnSnip.UseVisualStyleBackColor = false;
            this.btnSnip.Click += new System.EventHandler(this.btnSnip_Click);
            // 
            // btnOpenGallery
            // 
            this.btnOpenGallery.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnOpenGallery.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenGallery.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnOpenGallery.Location = new System.Drawing.Point(165, 3);
            this.btnOpenGallery.Name = "btnOpenGallery";
            this.btnOpenGallery.Size = new System.Drawing.Size(100, 23);
            this.btnOpenGallery.TabIndex = 53;
            this.btnOpenGallery.Text = "Open Gallery";
            this.btnOpenGallery.UseVisualStyleBackColor = false;
            this.btnOpenGallery.Click += new System.EventHandler(this.btnOpenGallery_Click);
            // 
            // btnAddToDatabase
            // 
            this.btnAddToDatabase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnAddToDatabase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddToDatabase.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnAddToDatabase.Location = new System.Drawing.Point(3, 32);
            this.btnAddToDatabase.Name = "btnAddToDatabase";
            this.btnAddToDatabase.Size = new System.Drawing.Size(120, 23);
            this.btnAddToDatabase.TabIndex = 54;
            this.btnAddToDatabase.Text = "Add to Database";
            this.btnAddToDatabase.UseVisualStyleBackColor = false;
            this.btnAddToDatabase.Click += new System.EventHandler(this.btnAddToDatabase_Click);
            // 
            // logoBox
            // 
            this.logoBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logoBox.Image = global::DreamsLive_Solutions_PresenterApp1.Properties.Resources.DreamsLiveSolutions_Logo1;
            this.logoBox.Location = new System.Drawing.Point(3, 3);
            this.logoBox.Name = "logoBox";
            this.logoBox.Size = new System.Drawing.Size(126, 22);
            this.logoBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.logoBox.TabIndex = 0;
            this.logoBox.TabStop = false;
            // 
            // tableCenter
            // 
            this.tableCenter.ColumnCount = 3;
            this.tableCenter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableCenter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 146F));
            this.tableCenter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableCenter.Controls.Add(this.paneLocal, 0, 0);
            this.tableCenter.Controls.Add(this.flowMid, 1, 0);
            this.tableCenter.Controls.Add(this.paneProgram, 2, 0);
            this.tableCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableCenter.Location = new System.Drawing.Point(0, 0);
            this.tableCenter.Name = "tableCenter";
            this.tableCenter.RowCount = 1;
            this.tableCenter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableCenter.Size = new System.Drawing.Size(1359, 470);
            this.tableCenter.TabIndex = 0;
            // 
            // paneLocal
            // 
            this.paneLocal.ColumnCount = 1;
            this.paneLocal.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.paneLocal.Controls.Add(this.lblPreviewLabel, 0, 0);
            this.paneLocal.Controls.Add(this.picPreview, 0, 1);
            this.paneLocal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.paneLocal.Location = new System.Drawing.Point(3, 3);
            this.paneLocal.Name = "paneLocal";
            this.paneLocal.RowCount = 2;
            this.paneLocal.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.paneLocal.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.paneLocal.Size = new System.Drawing.Size(600, 464);
            this.paneLocal.TabIndex = 0;
            // 
            // flowMid
            // 
            this.flowMid.Controls.Add(this.btnEditContent);
            this.flowMid.Controls.Add(this.btnStageContent);
            this.flowMid.Controls.Add(this.chkAutoStagePreview);
            this.flowMid.Controls.Add(this.btnPushToPresenter);
            this.flowMid.Controls.Add(this.chkLinkLocalPreviewToPresenter);
            this.flowMid.Controls.Add(this.btnClearPresenterDisplay);
            this.flowMid.Controls.Add(this.btnCloseLivePresenter);
            this.flowMid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowMid.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowMid.Location = new System.Drawing.Point(608, 28);
            this.flowMid.Margin = new System.Windows.Forms.Padding(2, 28, 2, 2);
            this.flowMid.Name = "flowMid";
            this.flowMid.Size = new System.Drawing.Size(142, 440);
            this.flowMid.TabIndex = 1;
            this.flowMid.WrapContents = false;
            // 
            // paneProgram
            // 
            this.paneProgram.ColumnCount = 1;
            this.paneProgram.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.paneProgram.Controls.Add(this.lblStagedLabel, 0, 0);
            this.paneProgram.Controls.Add(this.panelSecondaryPreviewBorder, 0, 1);
            this.paneProgram.Dock = System.Windows.Forms.DockStyle.Fill;
            this.paneProgram.Location = new System.Drawing.Point(755, 3);
            this.paneProgram.Name = "paneProgram";
            this.paneProgram.RowCount = 2;
            this.paneProgram.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.paneProgram.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.paneProgram.Size = new System.Drawing.Size(601, 464);
            this.paneProgram.TabIndex = 2;
            // 
            // panelBottom
            // 
            this.panelBottom.ColumnCount = 1;
            this.panelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panelBottom.Controls.Add(this.tableDocks, 0, 0);
            this.panelBottom.Controls.Add(this.tableStatus, 0, 1);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 470);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.RowCount = 2;
            this.panelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 152F));
            this.panelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            this.panelBottom.Size = new System.Drawing.Size(1359, 208);
            this.panelBottom.TabIndex = 1;
            // 
            // tableDocks
            // 
            this.tableDocks.ColumnCount = 4;
            this.tableDocks.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableDocks.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableDocks.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableDocks.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableDocks.Controls.Add(this.cardSource, 0, 0);
            this.tableDocks.Controls.Add(this.cardOutput, 1, 0);
            this.tableDocks.Controls.Add(this.cardTools, 2, 0);
            this.tableDocks.Controls.Add(this.cardPosition, 3, 0);
            this.tableDocks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableDocks.Location = new System.Drawing.Point(3, 3);
            this.tableDocks.Name = "tableDocks";
            this.tableDocks.RowCount = 1;
            this.tableDocks.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableDocks.Size = new System.Drawing.Size(1353, 146);
            this.tableDocks.TabIndex = 0;
            // 
            // cardSource
            // 
            this.cardSource.ColumnCount = 1;
            this.cardSource.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cardSource.Controls.Add(this.lblHdrSource, 0, 0);
            this.cardSource.Controls.Add(this.flowSource, 0, 1);
            this.cardSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardSource.Location = new System.Drawing.Point(3, 3);
            this.cardSource.Name = "cardSource";
            this.cardSource.RowCount = 2;
            this.cardSource.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.cardSource.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cardSource.Size = new System.Drawing.Size(332, 140);
            this.cardSource.TabIndex = 0;
            // 
            // lblHdrSource
            // 
            this.lblHdrSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHdrSource.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblHdrSource.Location = new System.Drawing.Point(3, 0);
            this.lblHdrSource.Name = "lblHdrSource";
            this.lblHdrSource.Size = new System.Drawing.Size(326, 20);
            this.lblHdrSource.TabIndex = 0;
            this.lblHdrSource.Text = "Source";
            this.lblHdrSource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowSource
            // 
            this.flowSource.Controls.Add(this.tnBrowse);
            this.flowSource.Controls.Add(this.btnSnip);
            this.flowSource.Controls.Add(this.btnOpenGallery);
            this.flowSource.Controls.Add(this.btnAddToDatabase);
            this.flowSource.Controls.Add(this.lblImagePath);
            this.flowSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowSource.Location = new System.Drawing.Point(3, 23);
            this.flowSource.Name = "flowSource";
            this.flowSource.Size = new System.Drawing.Size(326, 114);
            this.flowSource.TabIndex = 1;
            // 
            // cardOutput
            // 
            this.cardOutput.ColumnCount = 1;
            this.cardOutput.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cardOutput.Controls.Add(this.lblHdrOutput, 0, 0);
            this.cardOutput.Controls.Add(this.flowOutput, 0, 1);
            this.cardOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardOutput.Location = new System.Drawing.Point(341, 3);
            this.cardOutput.Name = "cardOutput";
            this.cardOutput.RowCount = 2;
            this.cardOutput.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.cardOutput.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cardOutput.Size = new System.Drawing.Size(332, 140);
            this.cardOutput.TabIndex = 1;
            // 
            // lblHdrOutput
            // 
            this.lblHdrOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHdrOutput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblHdrOutput.Location = new System.Drawing.Point(3, 0);
            this.lblHdrOutput.Name = "lblHdrOutput";
            this.lblHdrOutput.Size = new System.Drawing.Size(326, 20);
            this.lblHdrOutput.TabIndex = 0;
            this.lblHdrOutput.Text = "Display & options";
            this.lblHdrOutput.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowOutput
            // 
            this.flowOutput.Controls.Add(this.cmbDisplays);
            this.flowOutput.Controls.Add(this.cmbDisplayMode);
            this.flowOutput.Controls.Add(this.btnSettings);
            this.flowOutput.Controls.Add(this.btnHelp);
            this.flowOutput.Controls.Add(this.btnToggleTheme);
            this.flowOutput.Controls.Add(this.chkAlwaysOnTop);
            this.flowOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowOutput.Location = new System.Drawing.Point(3, 23);
            this.flowOutput.Name = "flowOutput";
            this.flowOutput.Size = new System.Drawing.Size(326, 114);
            this.flowOutput.TabIndex = 1;
            // 
            // cardTools
            // 
            this.cardTools.ColumnCount = 1;
            this.cardTools.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cardTools.Controls.Add(this.lblHdrTools, 0, 0);
            this.cardTools.Controls.Add(this.flowTools, 0, 1);
            this.cardTools.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardTools.Location = new System.Drawing.Point(679, 3);
            this.cardTools.Name = "cardTools";
            this.cardTools.RowCount = 2;
            this.cardTools.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.cardTools.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cardTools.Size = new System.Drawing.Size(332, 140);
            this.cardTools.TabIndex = 2;
            // 
            // lblHdrTools
            // 
            this.lblHdrTools.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHdrTools.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblHdrTools.Location = new System.Drawing.Point(3, 0);
            this.lblHdrTools.Name = "lblHdrTools";
            this.lblHdrTools.Size = new System.Drawing.Size(326, 20);
            this.lblHdrTools.TabIndex = 0;
            this.lblHdrTools.Text = "Tools";
            this.lblHdrTools.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowTools
            // 
            this.flowTools.Controls.Add(this.chkLaserPointer);
            this.flowTools.Controls.Add(this.btnHighlighter);
            this.flowTools.Controls.Add(this.chkEnableScroll);
            this.flowTools.Controls.Add(this.liveIndicator);
            this.flowTools.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowTools.Location = new System.Drawing.Point(3, 23);
            this.flowTools.Name = "flowTools";
            this.flowTools.Size = new System.Drawing.Size(326, 114);
            this.flowTools.TabIndex = 1;
            // 
            // cardPosition
            // 
            this.cardPosition.ColumnCount = 1;
            this.cardPosition.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cardPosition.Controls.Add(this.lblHdrPosition, 0, 0);
            this.cardPosition.Controls.Add(this.flowPosition, 0, 1);
            this.cardPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardPosition.Location = new System.Drawing.Point(1017, 3);
            this.cardPosition.Name = "cardPosition";
            this.cardPosition.RowCount = 2;
            this.cardPosition.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.cardPosition.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cardPosition.Size = new System.Drawing.Size(333, 140);
            this.cardPosition.TabIndex = 3;
            // 
            // lblHdrPosition
            // 
            this.lblHdrPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHdrPosition.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblHdrPosition.Location = new System.Drawing.Point(3, 0);
            this.lblHdrPosition.Name = "lblHdrPosition";
            this.lblHdrPosition.Size = new System.Drawing.Size(327, 20);
            this.lblHdrPosition.TabIndex = 0;
            this.lblHdrPosition.Text = "Position & pages";
            this.lblHdrPosition.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowPosition
            // 
            this.flowPosition.Controls.Add(this.pnlNudge);
            this.flowPosition.Controls.Add(this.lblSelectionSize);
            this.flowPosition.Controls.Add(this.lblMoveStep);
            this.flowPosition.Controls.Add(this.txtMoveStep);
            this.flowPosition.Controls.Add(this.btnPrevPage);
            this.flowPosition.Controls.Add(this.txtCurrentPageNum);
            this.flowPosition.Controls.Add(this.lblTotalPages);
            this.flowPosition.Controls.Add(this.btnNextPage);
            this.flowPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowPosition.Location = new System.Drawing.Point(3, 23);
            this.flowPosition.Name = "flowPosition";
            this.flowPosition.Size = new System.Drawing.Size(327, 114);
            this.flowPosition.TabIndex = 1;
            // 
            // pnlNudge
            // 
            this.pnlNudge.Controls.Add(this.btnUp);
            this.pnlNudge.Controls.Add(this.btnLeft);
            this.pnlNudge.Controls.Add(this.btnDown);
            this.pnlNudge.Controls.Add(this.btnRight);
            this.pnlNudge.Location = new System.Drawing.Point(3, 3);
            this.pnlNudge.Name = "pnlNudge";
            this.pnlNudge.Size = new System.Drawing.Size(98, 66);
            this.pnlNudge.TabIndex = 0;
            // 
            // tableStatus
            // 
            this.tableStatus.ColumnCount = 2;
            this.tableStatus.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.tableStatus.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableStatus.Controls.Add(this.logoBox, 0, 0);
            this.tableStatus.Controls.Add(this.flowStatus, 1, 0);
            this.tableStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableStatus.Location = new System.Drawing.Point(3, 155);
            this.tableStatus.Name = "tableStatus";
            this.tableStatus.RowCount = 1;
            this.tableStatus.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableStatus.Size = new System.Drawing.Size(1353, 28);
            this.tableStatus.TabIndex = 1;
            // 
            // flowStatus
            // 
            this.flowStatus.Controls.Add(this.lblConnectionInfo);
            this.flowStatus.Controls.Add(this.lblWebServerUrl);
            this.flowStatus.Controls.Add(this.lblDatabaseFolderPath);
            this.flowStatus.Controls.Add(this.btnSetDatabaseFolder);
            this.flowStatus.Controls.Add(this.lblMessage);
            this.flowStatus.Controls.Add(this.btnMessageOkay);
            this.flowStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowStatus.Location = new System.Drawing.Point(135, 3);
            this.flowStatus.Name = "flowStatus";
            this.flowStatus.Size = new System.Drawing.Size(1215, 22);
            this.flowStatus.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(8)))), ((int)(((byte)(10)))));
            this.ClientSize = new System.Drawing.Size(1359, 656);
            this.Controls.Add(this.tableCenter);
            this.Controls.Add(this.panelBottom);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            // Floor that keeps the docked previews + the bottom control cards usable; the
            // TableLayoutPanel/FlowLayoutPanel layout reflows above this and scales up to full screen.
            this.MinimumSize = new System.Drawing.Size(1024, 640);
            this.Name = "MainForm";
            this.Text = "Presenter App";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panelSecondaryPreviewBorder.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picSecondaryPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.logoBox)).EndInit();
            this.tableCenter.ResumeLayout(false);
            this.paneLocal.ResumeLayout(false);
            this.flowMid.ResumeLayout(false);
            this.flowMid.PerformLayout();
            this.paneProgram.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.tableDocks.ResumeLayout(false);
            this.cardSource.ResumeLayout(false);
            this.flowSource.ResumeLayout(false);
            this.flowSource.PerformLayout();
            this.cardOutput.ResumeLayout(false);
            this.flowOutput.ResumeLayout(false);
            this.flowOutput.PerformLayout();
            this.cardTools.ResumeLayout(false);
            this.flowTools.ResumeLayout(false);
            this.flowTools.PerformLayout();
            this.cardPosition.ResumeLayout(false);
            this.flowPosition.ResumeLayout(false);
            this.flowPosition.PerformLayout();
            this.pnlNudge.ResumeLayout(false);
            this.tableStatus.ResumeLayout(false);
            this.flowStatus.ResumeLayout(false);
            this.flowStatus.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button tnBrowse;
        private System.Windows.Forms.Label lblImagePath;
        private System.Windows.Forms.ComboBox cmbDisplays;
        private System.Windows.Forms.Button btnStageContent;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Button btnPrevPage;
        private System.Windows.Forms.Button btnNextPage;
        private System.Windows.Forms.TextBox txtCurrentPageNum;
        private System.Windows.Forms.Label lblTotalPages;
        private System.Windows.Forms.PictureBox picSecondaryPreview;
        private System.Windows.Forms.Button btnPushToPresenter;
        private System.Windows.Forms.CheckBox chkLinkLocalPreviewToPresenter;
        private System.Windows.Forms.Button btnClearPresenterDisplay;
        private System.Windows.Forms.Button btnToggleTheme;
        private System.Windows.Forms.Button btnCloseLivePresenter;
        private System.Windows.Forms.Label lblConnectionInfo;
        private System.Windows.Forms.ComboBox cmbDisplayMode;
        private System.Windows.Forms.Label lblWebServerUrl;
        private System.Windows.Forms.Panel panelSecondaryPreviewBorder;
        private System.Windows.Forms.CheckBox chkAlwaysOnTop;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.CheckBox chkEnableScroll;
        private System.Windows.Forms.CheckBox chkAutoStagePreview;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Label lblMoveStep;
        private System.Windows.Forms.TextBox txtMoveStep;
        private System.Windows.Forms.Label lblSelectionSize;
        private System.Windows.Forms.CheckBox chkLaserPointer;
        private System.Windows.Forms.Button btnHighlighter;
        private System.Windows.Forms.Button btnEditContent;
        private System.Windows.Forms.Button btnMessageOkay;
        private System.Windows.Forms.Label lblPreviewLabel;
        private System.Windows.Forms.Label lblStagedLabel;
        private System.Windows.Forms.Button btnSetDatabaseFolder;
        private System.Windows.Forms.Label lblDatabaseFolderPath;
        private System.Windows.Forms.Button btnSnip;
        private System.Windows.Forms.Button btnOpenGallery;
        private System.Windows.Forms.Button btnAddToDatabase;
        private LiveIndicatorControl liveIndicator;
        private System.Windows.Forms.PictureBox logoBox;
        private System.Windows.Forms.TableLayoutPanel tableCenter;
        private System.Windows.Forms.FlowLayoutPanel flowMid;
        private System.Windows.Forms.TableLayoutPanel panelBottom;
        private System.Windows.Forms.TableLayoutPanel tableDocks;
        private System.Windows.Forms.FlowLayoutPanel flowSource;
        private System.Windows.Forms.FlowLayoutPanel flowOutput;
        private System.Windows.Forms.FlowLayoutPanel flowTools;
        private System.Windows.Forms.FlowLayoutPanel flowPosition;
        private System.Windows.Forms.Panel pnlNudge;
        private System.Windows.Forms.TableLayoutPanel tableStatus;
        private System.Windows.Forms.FlowLayoutPanel flowStatus;
        private System.Windows.Forms.TableLayoutPanel paneLocal;
        private System.Windows.Forms.TableLayoutPanel paneProgram;
        private System.Windows.Forms.TableLayoutPanel cardSource;
        private System.Windows.Forms.TableLayoutPanel cardOutput;
        private System.Windows.Forms.TableLayoutPanel cardTools;
        private System.Windows.Forms.TableLayoutPanel cardPosition;
        private System.Windows.Forms.Label lblHdrSource;
        private System.Windows.Forms.Label lblHdrOutput;
        private System.Windows.Forms.Label lblHdrTools;
        private System.Windows.Forms.Label lblHdrPosition;
    }
}
