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
            this.linearHeader = new DreamsLive_Solutions_PresenterApp1.LinearHeaderControl();
            this.label1 = new System.Windows.Forms.Label();
            this.panelSecondaryPreviewBorder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSecondaryPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.linearHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // tnBrowse
            // 
            this.tnBrowse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.tnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.tnBrowse.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.tnBrowse.Location = new System.Drawing.Point(754, 137);
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
            this.lblImagePath.Location = new System.Drawing.Point(835, 142);
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
            this.cmbDisplays.Location = new System.Drawing.Point(456, 134);
            this.cmbDisplays.Name = "cmbDisplays";
            this.cmbDisplays.Size = new System.Drawing.Size(146, 21);
            this.cmbDisplays.TabIndex = 2;
            // 
            // btnStageContent
            // 
            this.btnStageContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(106)))), ((int)(((byte)(210)))));
            this.btnStageContent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStageContent.ForeColor = System.Drawing.Color.White;
            this.btnStageContent.Location = new System.Drawing.Point(612, 384);
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
            this.btnPrevPage.Location = new System.Drawing.Point(1219, 137);
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
            this.btnNextPage.Location = new System.Drawing.Point(1289, 137);
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
            this.txtCurrentPageNum.Location = new System.Drawing.Point(1219, 117);
            this.txtCurrentPageNum.Name = "txtCurrentPageNum";
            this.txtCurrentPageNum.Size = new System.Drawing.Size(63, 20);
            this.txtCurrentPageNum.TabIndex = 7;
            // 
            // lblTotalPages
            // 
            this.lblTotalPages.AutoSize = true;
            this.lblTotalPages.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.lblTotalPages.Location = new System.Drawing.Point(1288, 120);
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
            this.btnPushToPresenter.Location = new System.Drawing.Point(612, 444);
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
            this.chkLinkLocalPreviewToPresenter.Location = new System.Drawing.Point(614, 480);
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
            this.btnClearPresenterDisplay.Location = new System.Drawing.Point(612, 210);
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
            this.btnToggleTheme.Location = new System.Drawing.Point(1252, 61);
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
            this.btnCloseLivePresenter.Location = new System.Drawing.Point(612, 168);
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
            this.lblConnectionInfo.Location = new System.Drawing.Point(11, 605);
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
            this.cmbDisplayMode.Location = new System.Drawing.Point(284, 134);
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
            this.lblWebServerUrl.Location = new System.Drawing.Point(9, 634);
            this.lblWebServerUrl.Name = "lblWebServerUrl";
            this.lblWebServerUrl.Size = new System.Drawing.Size(115, 13);
            this.lblWebServerUrl.TabIndex = 18;
            this.lblWebServerUrl.Text = "Web Server: Starting...";
            // 
            // panelSecondaryPreviewBorder
            // 
            this.panelSecondaryPreviewBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(8)))), ((int)(((byte)(10)))));
            this.panelSecondaryPreviewBorder.Controls.Add(this.picSecondaryPreview);
            this.panelSecondaryPreviewBorder.Location = new System.Drawing.Point(4, 164);
            this.panelSecondaryPreviewBorder.Name = "panelSecondaryPreviewBorder";
            this.panelSecondaryPreviewBorder.Padding = new System.Windows.Forms.Padding(2);
            this.panelSecondaryPreviewBorder.Size = new System.Drawing.Size(600, 425);
            this.panelSecondaryPreviewBorder.TabIndex = 9;
            // 
            // picSecondaryPreview
            // 
            this.picSecondaryPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.picSecondaryPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picSecondaryPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picSecondaryPreview.Location = new System.Drawing.Point(2, 2);
            this.picSecondaryPreview.Name = "picSecondaryPreview";
            this.picSecondaryPreview.Size = new System.Drawing.Size(596, 421);
            this.picSecondaryPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picSecondaryPreview.TabIndex = 0;
            this.picSecondaryPreview.TabStop = false;
            // 
            // chkAlwaysOnTop
            // 
            this.chkAlwaysOnTop.AutoSize = true;
            this.chkAlwaysOnTop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkAlwaysOnTop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.chkAlwaysOnTop.Location = new System.Drawing.Point(1255, 88);
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
            this.lblMessage.Location = new System.Drawing.Point(7, 406);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(0, 13);
            this.lblMessage.TabIndex = 20;
            // 
            // btnUp
            // 
            this.btnUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.btnUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnUp.Location = new System.Drawing.Point(1053, 590);
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
            this.btnDown.Location = new System.Drawing.Point(1053, 620);
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
            this.btnLeft.Location = new System.Drawing.Point(1023, 605);
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
            this.btnRight.Location = new System.Drawing.Point(1083, 605);
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
            this.btnSettings.Location = new System.Drawing.Point(1044, 61);
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
            this.lblMoveStep.Location = new System.Drawing.Point(754, 611);
            this.lblMoveStep.Name = "lblMoveStep";
            this.lblMoveStep.Size = new System.Drawing.Size(62, 13);
            this.lblMoveStep.TabIndex = 29;
            this.lblMoveStep.Text = "Move Step:";
            // 
            // txtMoveStep
            // 
            this.txtMoveStep.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.txtMoveStep.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.txtMoveStep.Location = new System.Drawing.Point(817, 608);
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
            this.btnHelp.Location = new System.Drawing.Point(1148, 61);
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
            this.chkEnableScroll.Location = new System.Drawing.Point(1241, 600);
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
            this.chkAutoStagePreview.Location = new System.Drawing.Point(621, 420);
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
            this.lblSelectionSize.Location = new System.Drawing.Point(751, 593);
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
            this.chkLaserPointer.Location = new System.Drawing.Point(636, 293);
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
            this.btnHighlighter.Location = new System.Drawing.Point(635, 316);
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
            this.btnEditContent.Location = new System.Drawing.Point(612, 346);
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
            this.btnMessageOkay.Location = new System.Drawing.Point(1277, 624);
            this.btnMessageOkay.Name = "btnMessageOkay";
            this.btnMessageOkay.Size = new System.Drawing.Size(75, 23);
            this.btnMessageOkay.TabIndex = 21;
            this.btnMessageOkay.Text = "Okay";
            this.btnMessageOkay.UseVisualStyleBackColor = false;
            // 
            // lblPreviewLabel
            // 
            this.lblPreviewLabel.AutoSize = true;
            this.lblPreviewLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.lblPreviewLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.lblPreviewLabel.Location = new System.Drawing.Point(6, 166);
            this.lblPreviewLabel.Name = "lblPreviewLabel";
            this.lblPreviewLabel.Size = new System.Drawing.Size(45, 13);
            this.lblPreviewLabel.TabIndex = 50;
            this.lblPreviewLabel.Text = "Preview";
            // 
            // lblStagedLabel
            // 
            this.lblStagedLabel.AutoSize = true;
            this.lblStagedLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.lblStagedLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.lblStagedLabel.Location = new System.Drawing.Point(756, 164);
            this.lblStagedLabel.Name = "lblStagedLabel";
            this.lblStagedLabel.Size = new System.Drawing.Size(41, 13);
            this.lblStagedLabel.TabIndex = 51;
            this.lblStagedLabel.Text = "Staged";
            // 
            // picPreview
            // 
            this.picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreview.Location = new System.Drawing.Point(754, 162);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(600, 425);
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
            this.btnSetDatabaseFolder.Location = new System.Drawing.Point(250, 600);
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
            this.lblDatabaseFolderPath.Location = new System.Drawing.Point(376, 605);
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
            this.btnSnip.Location = new System.Drawing.Point(754, 79);
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
            this.btnOpenGallery.Location = new System.Drawing.Point(754, 108);
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
            this.btnAddToDatabase.Location = new System.Drawing.Point(860, 108);
            this.btnAddToDatabase.Name = "btnAddToDatabase";
            this.btnAddToDatabase.Size = new System.Drawing.Size(120, 23);
            this.btnAddToDatabase.TabIndex = 54;
            this.btnAddToDatabase.Text = "Add to Database";
            this.btnAddToDatabase.UseVisualStyleBackColor = false;
            this.btnAddToDatabase.Click += new System.EventHandler(this.btnAddToDatabase_Click);
            // 
            // linearHeader
            // 
            this.linearHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.linearHeader.Controls.Add(this.label1);
            this.linearHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.linearHeader.IsLive = false;
            this.linearHeader.Location = new System.Drawing.Point(0, 0);
            this.linearHeader.Name = "linearHeader";
            this.linearHeader.Size = new System.Drawing.Size(1359, 56);
            this.linearHeader.TabIndex = 55;
            this.linearHeader.Tag = "chrome";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Variable Small Semibol", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.label1.Location = new System.Drawing.Point(246, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(166, 21);
            this.label1.TabIndex = 35;
            this.label1.Text = "Where Ideas Go Live.";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(8)))), ((int)(((byte)(10)))));
            this.ClientSize = new System.Drawing.Size(1359, 656);
            this.Controls.Add(this.btnMessageOkay);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.chkAlwaysOnTop);
            this.Controls.Add(this.lblWebServerUrl);
            this.Controls.Add(this.cmbDisplayMode);
            this.Controls.Add(this.lblConnectionInfo);
            this.Controls.Add(this.btnCloseLivePresenter);
            this.Controls.Add(this.btnToggleTheme);
            this.Controls.Add(this.btnClearPresenterDisplay);
            this.Controls.Add(this.chkLinkLocalPreviewToPresenter);
            this.Controls.Add(this.btnPushToPresenter);
            this.Controls.Add(this.panelSecondaryPreviewBorder);
            this.Controls.Add(this.lblTotalPages);
            this.Controls.Add(this.txtCurrentPageNum);
            this.Controls.Add(this.btnNextPage);
            this.Controls.Add(this.btnPrevPage);
            this.Controls.Add(this.btnStageContent);
            this.Controls.Add(this.cmbDisplays);
            this.Controls.Add(this.lblImagePath);
            this.Controls.Add(this.tnBrowse);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.chkEnableScroll);
            this.Controls.Add(this.chkAutoStagePreview);
            this.Controls.Add(this.btnUp);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.btnLeft);
            this.Controls.Add(this.btnRight);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.lblMoveStep);
            this.Controls.Add(this.txtMoveStep);
            this.Controls.Add(this.lblSelectionSize);
            this.Controls.Add(this.chkLaserPointer);
            this.Controls.Add(this.lblPreviewLabel);
            this.Controls.Add(this.lblStagedLabel);
            this.Controls.Add(this.btnHighlighter);
            this.Controls.Add(this.btnEditContent);
            this.Controls.Add(this.picPreview);
            this.Controls.Add(this.btnSetDatabaseFolder);
            this.Controls.Add(this.lblDatabaseFolderPath);
            this.Controls.Add(this.btnSnip);
            this.Controls.Add(this.btnOpenGallery);
            this.Controls.Add(this.btnAddToDatabase);
            this.Controls.Add(this.linearHeader);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.Name = "MainForm";
            this.Text = "Presenter App";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panelSecondaryPreviewBorder.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picSecondaryPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.linearHeader.ResumeLayout(false);
            this.linearHeader.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private LinearHeaderControl linearHeader;
        private System.Windows.Forms.Label label1;
    }
}
