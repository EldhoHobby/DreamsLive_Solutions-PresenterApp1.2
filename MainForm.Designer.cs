namespace DreamsLive_Solutions_PresenterApp1
{
    partial class MainForm
    {
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
            this.components = new System.ComponentModel.Container();
            this.pnlSideBar = new System.Windows.Forms.Panel();
            this.pnlTitleBar = new System.Windows.Forms.Panel();
            this.btnAppClose = new System.Windows.Forms.Button();
            this.btnAppMinimize = new System.Windows.Forms.Button();
            this.lblAppTitle = new System.Windows.Forms.Label();
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.picSecondaryPreview = new System.Windows.Forms.PictureBox();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSetDatabaseFolder = new System.Windows.Forms.Button();
            this.lblDatabaseFolderPath = new System.Windows.Forms.Label();
            this.btnOpenGallery = new System.Windows.Forms.Button();
            this.btnSnip = new System.Windows.Forms.Button();
            this.pnlSideBar.SuspendLayout();
            this.pnlTitleBar.SuspendLayout();
            this.panelSecondaryPreviewBorder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSecondaryPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // tnBrowse
            // 
            this.tnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.tnBrowse.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tnBrowse.Location = new System.Drawing.Point(690, 81);
            this.tnBrowse.Name = "tnBrowse";
            this.tnBrowse.Size = new System.Drawing.Size(80, 25);
            this.tnBrowse.TabIndex = 0;
            this.tnBrowse.Text = "Browse...";
            this.tnBrowse.UseVisualStyleBackColor = true;
            this.tnBrowse.Click += new System.EventHandler(this.tnBrowse_Click);
            // 
            // btnSnip
            //
            this.btnSnip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSnip.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSnip.Location = new System.Drawing.Point(600, 81);
            this.btnSnip.Name = "btnSnip";
            this.btnSnip.Size = new System.Drawing.Size(80, 25);
            this.btnSnip.TabIndex = 52;
            this.btnSnip.Text = "Snip";
            this.btnSnip.UseVisualStyleBackColor = true;
            this.btnSnip.Click += new System.EventHandler(this.btnSnip_Click);
            //
            // lblImagePath
            // 
            this.lblImagePath.AutoSize = true;
            this.lblImagePath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblImagePath.Location = new System.Drawing.Point(780, 86);
            this.lblImagePath.Name = "lblImagePath";
            this.lblImagePath.Size = new System.Drawing.Size(124, 15);
            this.lblImagePath.TabIndex = 1;
            this.lblImagePath.Text = "Selected Image: None";
            // 
            // cmbDisplays
            // 
            this.cmbDisplays.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDisplays.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbDisplays.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbDisplays.FormattingEnabled = true;
            this.cmbDisplays.Location = new System.Drawing.Point(440, 81);
            this.cmbDisplays.Name = "cmbDisplays";
            this.cmbDisplays.Size = new System.Drawing.Size(146, 23);
            this.cmbDisplays.TabIndex = 2;
            this.cmbDisplays.SelectedIndexChanged += new System.EventHandler(this.CmbDisplays_SelectedIndexChanged);
            // 
            // btnStageContent
            // 
            this.btnStageContent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStageContent.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnStageContent.Location = new System.Drawing.Point(608, 310);
            this.btnStageContent.Name = "btnStageContent";
            this.btnStageContent.Size = new System.Drawing.Size(142, 45);
            this.btnStageContent.TabIndex = 3;
            this.btnStageContent.Text = "Stage Preview";
            this.btnStageContent.UseVisualStyleBackColor = true;
            this.btnStageContent.Click += new System.EventHandler(this.btnStageContent_Click);
            // 
            // btnPrevPage
            // 
            this.btnPrevPage.Location = new System.Drawing.Point(1219, 81);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new System.Drawing.Size(65, 23);
            this.btnPrevPage.TabIndex = 5;
            this.btnPrevPage.Text = "Prev Page";
            this.btnPrevPage.UseVisualStyleBackColor = true;
            // 
            // btnNextPage
            // 
            this.btnNextPage.Location = new System.Drawing.Point(1289, 81);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(65, 23);
            this.btnNextPage.TabIndex = 6;
            this.btnNextPage.Text = "Next Page";
            this.btnNextPage.UseVisualStyleBackColor = true;
            // 
            // txtCurrentPageNum
            // 
            this.txtCurrentPageNum.Location = new System.Drawing.Point(1219, 61);
            this.txtCurrentPageNum.Name = "txtCurrentPageNum";
            this.txtCurrentPageNum.Size = new System.Drawing.Size(63, 20);
            this.txtCurrentPageNum.TabIndex = 7;
            // 
            // lblTotalPages
            // 
            this.lblTotalPages.AutoSize = true;
            this.lblTotalPages.Location = new System.Drawing.Point(1288, 64);
            this.lblTotalPages.Name = "lblTotalPages";
            this.lblTotalPages.Size = new System.Drawing.Size(66, 13);
            this.lblTotalPages.TabIndex = 8;
            this.lblTotalPages.Text = "/TotalPages";
            // 
            // btnPushToPresenter
            // 
            this.btnPushToPresenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPushToPresenter.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.btnPushToPresenter.Location = new System.Drawing.Point(608, 385);
            this.btnPushToPresenter.Name = "btnPushToPresenter";
            this.btnPushToPresenter.Size = new System.Drawing.Size(142, 50);
            this.btnPushToPresenter.TabIndex = 10;
            this.btnPushToPresenter.Text = "Go Live";
            this.btnPushToPresenter.UseVisualStyleBackColor = true;
            // 
            // chkLinkLocalPreviewToPresenter
            // 
            this.chkLinkLocalPreviewToPresenter.AutoSize = true;
            this.chkLinkLocalPreviewToPresenter.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.chkLinkLocalPreviewToPresenter.Location = new System.Drawing.Point(602, 440);
            this.chkLinkLocalPreviewToPresenter.Name = "chkLinkLocalPreviewToPresenter";
            this.chkLinkLocalPreviewToPresenter.Size = new System.Drawing.Size(155, 19);
            this.chkLinkLocalPreviewToPresenter.TabIndex = 11;
            this.chkLinkLocalPreviewToPresenter.Text = "Auto-Send to Presenter";
            this.chkLinkLocalPreviewToPresenter.UseVisualStyleBackColor = true;
            // 
            // btnClearPresenterDisplay
            // 
            this.btnClearPresenterDisplay.Enabled = false;
            this.btnClearPresenterDisplay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearPresenterDisplay.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnClearPresenterDisplay.Location = new System.Drawing.Point(608, 155);
            this.btnClearPresenterDisplay.Name = "btnClearPresenterDisplay";
            this.btnClearPresenterDisplay.Size = new System.Drawing.Size(142, 40);
            this.btnClearPresenterDisplay.TabIndex = 12;
            this.btnClearPresenterDisplay.Text = "Blank Presenter";
            this.btnClearPresenterDisplay.UseVisualStyleBackColor = true;
            // 
            // btnToggleTheme
            // 
            this.btnToggleTheme.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleTheme.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnToggleTheme.Location = new System.Drawing.Point(1252, 5);
            this.btnToggleTheme.Name = "btnToggleTheme";
            this.btnToggleTheme.Size = new System.Drawing.Size(100, 23);
            this.btnToggleTheme.TabIndex = 14;
            this.btnToggleTheme.Text = "Toggle Theme";
            this.btnToggleTheme.UseVisualStyleBackColor = true;
            this.btnToggleTheme.Click += new System.EventHandler(this.btnToggleTheme_Click);
            // 
            // btnCloseLivePresenter
            // 
            this.btnCloseLivePresenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCloseLivePresenter.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnCloseLivePresenter.Location = new System.Drawing.Point(608, 110);
            this.btnCloseLivePresenter.Name = "btnCloseLivePresenter";
            this.btnCloseLivePresenter.Size = new System.Drawing.Size(142, 40);
            this.btnCloseLivePresenter.TabIndex = 15;
            this.btnCloseLivePresenter.Text = "Close Live";
            this.btnCloseLivePresenter.UseVisualStyleBackColor = true;
            this.btnCloseLivePresenter.Click += new System.EventHandler(this.btnCloseLivePresenter_Click);
            // 
            // lblConnectionInfo
            // 
            this.lblConnectionInfo.AutoSize = true;
            this.lblConnectionInfo.Location = new System.Drawing.Point(11, 549);
            this.lblConnectionInfo.Name = "lblConnectionInfo";
            this.lblConnectionInfo.Size = new System.Drawing.Size(126, 13);
            this.lblConnectionInfo.TabIndex = 16;
            this.lblConnectionInfo.Text = "IP Address Appears Here";
            // 
            // cmbDisplayMode
            // 
            this.cmbDisplayMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDisplayMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbDisplayMode.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbDisplayMode.FormattingEnabled = true;
            this.cmbDisplayMode.Items.AddRange(new object[] {
            "Fit",
            "Fill",
            "Stretch",
            "Tile",
            "Center"});
            this.cmbDisplayMode.Location = new System.Drawing.Point(284, 81);
            this.cmbDisplayMode.Name = "cmbDisplayMode";
            this.cmbDisplayMode.Size = new System.Drawing.Size(146, 23);
            this.cmbDisplayMode.TabIndex = 17;
            this.cmbDisplayMode.SelectedIndexChanged += new System.EventHandler(this.cmbDisplayMode_SelectedIndexChanged);
            // 
            // lblWebServerUrl
            // 
            this.lblWebServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblWebServerUrl.AutoSize = true;
            this.lblWebServerUrl.Location = new System.Drawing.Point(9, 578);
            this.lblWebServerUrl.Name = "lblWebServerUrl";
            this.lblWebServerUrl.Size = new System.Drawing.Size(115, 13);
            this.lblWebServerUrl.TabIndex = 18;
            this.lblWebServerUrl.Text = "Web Server: Starting...";
            // 
            // panelSecondaryPreviewBorder
            // 
            this.panelSecondaryPreviewBorder.Controls.Add(this.picSecondaryPreview);
            this.panelSecondaryPreviewBorder.Location = new System.Drawing.Point(54, 108);
            this.panelSecondaryPreviewBorder.Name = "panelSecondaryPreviewBorder";
            this.panelSecondaryPreviewBorder.Padding = new System.Windows.Forms.Padding(2);
            this.panelSecondaryPreviewBorder.Size = new System.Drawing.Size(600, 425);
            this.panelSecondaryPreviewBorder.TabIndex = 9;
            // 
            // chkAlwaysOnTop
            // 
            this.chkAlwaysOnTop.AutoSize = true;
            this.chkAlwaysOnTop.Checked = true;
            this.chkAlwaysOnTop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAlwaysOnTop.Location = new System.Drawing.Point(1255, 32);
            this.chkAlwaysOnTop.Name = "chkAlwaysOnTop";
            this.chkAlwaysOnTop.Size = new System.Drawing.Size(96, 17);
            this.chkAlwaysOnTop.TabIndex = 19;
            this.chkAlwaysOnTop.Text = "Always on Top";
            this.chkAlwaysOnTop.UseVisualStyleBackColor = true;
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(7, 350);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(0, 13);
            this.lblMessage.TabIndex = 20;
            // 
            // btnUp
            // 
            this.btnUp.Location = new System.Drawing.Point(1053, 534);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(30, 30);
            this.btnUp.TabIndex = 25;
            this.btnUp.Text = "↑";
            this.btnUp.UseVisualStyleBackColor = true;
            // 
            // btnDown
            // 
            this.btnDown.Location = new System.Drawing.Point(1053, 564);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(30, 30);
            this.btnDown.TabIndex = 26;
            this.btnDown.Text = "↓";
            this.btnDown.UseVisualStyleBackColor = true;
            // 
            // btnLeft
            // 
            this.btnLeft.Location = new System.Drawing.Point(1023, 549);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(30, 30);
            this.btnLeft.TabIndex = 27;
            this.btnLeft.Text = "←";
            this.btnLeft.UseVisualStyleBackColor = true;
            // 
            // btnRight
            // 
            this.btnRight.Location = new System.Drawing.Point(1083, 549);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(30, 30);
            this.btnRight.TabIndex = 28;
            this.btnRight.Text = "→";
            this.btnRight.UseVisualStyleBackColor = true;
            // 
            // btnSettings
            //
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnSettings.Location = new System.Drawing.Point(1044, 5);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(100, 23);
            this.btnSettings.TabIndex = 43;
            this.btnSettings.Text = "Scroll Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            //
            // lblMoveStep
            // 
            this.lblMoveStep.AutoSize = true;
            this.lblMoveStep.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblMoveStep.Location = new System.Drawing.Point(754, 555);
            this.lblMoveStep.Name = "lblMoveStep";
            this.lblMoveStep.Size = new System.Drawing.Size(64, 13);
            this.lblMoveStep.TabIndex = 29;
            this.lblMoveStep.Text = "Move Step:";
            // 
            // txtMoveStep
            // 
            this.txtMoveStep.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.txtMoveStep.Location = new System.Drawing.Point(817, 552);
            this.txtMoveStep.Name = "txtMoveStep";
            this.txtMoveStep.Size = new System.Drawing.Size(40, 22);
            this.txtMoveStep.TabIndex = 30;
            this.txtMoveStep.Text = "50";
            // 
            // btnHelp
            // 
            this.btnHelp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHelp.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnHelp.Location = new System.Drawing.Point(1148, 5);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(100, 23);
            this.btnHelp.TabIndex = 22;
            this.btnHelp.Text = "Help";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // chkEnableScroll
            // 
            this.chkEnableScroll.AutoSize = true;
            this.chkEnableScroll.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.chkEnableScroll.Location = new System.Drawing.Point(1240, 536);
            this.chkEnableScroll.Name = "chkEnableScroll";
            this.chkEnableScroll.Size = new System.Drawing.Size(117, 17);
            this.chkEnableScroll.TabIndex = 23;
            this.chkEnableScroll.Text = "Enable Auto Scroll";
            this.chkEnableScroll.UseVisualStyleBackColor = true;
            // 
            // chkAutoStagePreview
            // 
            this.chkAutoStagePreview.AutoSize = true;
            this.chkAutoStagePreview.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkAutoStagePreview.Location = new System.Drawing.Point(612, 360);
            this.chkAutoStagePreview.Name = "chkAutoStagePreview";
            this.chkAutoStagePreview.Size = new System.Drawing.Size(127, 19);
            this.chkAutoStagePreview.TabIndex = 24;
            this.chkAutoStagePreview.Text = "Auto Stage Preview";
            this.chkAutoStagePreview.UseVisualStyleBackColor = true;
            // 
            // lblSelectionSize
            // 
            this.lblSelectionSize.AutoSize = true;
            this.lblSelectionSize.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblSelectionSize.Location = new System.Drawing.Point(751, 537);
            this.lblSelectionSize.Name = "lblSelectionSize";
            this.lblSelectionSize.Size = new System.Drawing.Size(101, 13);
            this.lblSelectionSize.TabIndex = 31;
            this.lblSelectionSize.Text = "Width: 0, Height: 0";
            // 
            // chkLaserPointer
            // 
            this.chkLaserPointer.AutoSize = true;
            this.chkLaserPointer.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkLaserPointer.Location = new System.Drawing.Point(608, 205);
            this.chkLaserPointer.Name = "chkLaserPointer";
            this.chkLaserPointer.Size = new System.Drawing.Size(94, 19);
            this.chkLaserPointer.TabIndex = 32;
            this.chkLaserPointer.Text = "Laser Pointer";
            this.chkLaserPointer.UseVisualStyleBackColor = true;
            // 
            // btnHighlighter
            // 
            this.btnHighlighter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHighlighter.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnHighlighter.Location = new System.Drawing.Point(608, 225);
            this.btnHighlighter.Name = "btnHighlighter";
            this.btnHighlighter.Size = new System.Drawing.Size(142, 35);
            this.btnHighlighter.TabIndex = 33;
            this.btnHighlighter.Text = "Highlighter";
            this.btnHighlighter.UseVisualStyleBackColor = true;
            this.btnHighlighter.Click += new System.EventHandler(this.btnHighlighter_Click);
            // 
            // btnEditContent
            //
            this.btnEditContent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditContent.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnEditContent.Location = new System.Drawing.Point(608, 265);
            this.btnEditContent.Name = "btnEditContent";
            this.btnEditContent.Size = new System.Drawing.Size(142, 40);
            this.btnEditContent.TabIndex = 38;
            this.btnEditContent.Text = "Edit / Crop Preview";
            this.btnEditContent.UseVisualStyleBackColor = true;
            //
            // lblPreviewLabel
            //
            this.lblPreviewLabel.AutoSize = true;
            this.lblPreviewLabel.BackColor = System.Drawing.Color.Black;
            this.lblPreviewLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPreviewLabel.ForeColor = System.Drawing.Color.White;
            this.lblPreviewLabel.Location = new System.Drawing.Point(56, 110);
            this.lblPreviewLabel.Name = "lblPreviewLabel";
            this.lblPreviewLabel.Size = new System.Drawing.Size(52, 15);
            this.lblPreviewLabel.TabIndex = 50;
            this.lblPreviewLabel.Text = "Preview";
            //
            // lblStagedLabel
            //
            this.lblStagedLabel.AutoSize = true;
            this.lblStagedLabel.BackColor = System.Drawing.Color.Black;
            this.lblStagedLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStagedLabel.ForeColor = System.Drawing.Color.White;
            this.lblStagedLabel.Location = new System.Drawing.Point(756, 108);
            this.lblStagedLabel.Name = "lblStagedLabel";
            this.lblStagedLabel.Size = new System.Drawing.Size(46, 15);
            this.lblStagedLabel.TabIndex = 51;
            this.lblStagedLabel.Text = "Staged";
            //
            // btnMessageOkay
            // 
            this.btnMessageOkay.Location = new System.Drawing.Point(1277, 568);
            this.btnMessageOkay.Name = "btnMessageOkay";
            this.btnMessageOkay.Size = new System.Drawing.Size(75, 23);
            this.btnMessageOkay.TabIndex = 21;
            this.btnMessageOkay.Text = "Okay";
            this.btnMessageOkay.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.SystemColors.Control;
            this.pictureBox1.Image = global::DreamsLive_Solutions_PresenterApp1.Properties.Resources.DreamsLiveSolutions_Logo1;
            this.pictureBox1.Location = new System.Drawing.Point(56, 35);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(277, 63);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 34;
            this.pictureBox1.TabStop = false;
            // 
            // picSecondaryPreview
            //
            this.picSecondaryPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picSecondaryPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picSecondaryPreview.Location = new System.Drawing.Point(2, 2);
            this.picSecondaryPreview.Name = "picSecondaryPreview";
            this.picSecondaryPreview.Size = new System.Drawing.Size(596, 421);
            this.picSecondaryPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picSecondaryPreview.TabIndex = 0;
            this.picSecondaryPreview.TabStop = false;
            //
            // picPreview
            // 
            this.picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreview.Location = new System.Drawing.Point(754, 106);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(600, 425);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreview.TabIndex = 4;
            this.picPreview.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(111, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(166, 21);
            this.label1.TabIndex = 35;
            this.label1.Text = "Where Ideas Go Live.";
            // 
            // btnSetDatabaseFolder
            //
            this.btnSetDatabaseFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSetDatabaseFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetDatabaseFolder.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnSetDatabaseFolder.Location = new System.Drawing.Point(250, 544);
            this.btnSetDatabaseFolder.Name = "btnSetDatabaseFolder";
            this.btnSetDatabaseFolder.Size = new System.Drawing.Size(120, 25);
            this.btnSetDatabaseFolder.TabIndex = 36;
            this.btnSetDatabaseFolder.Text = "Set Database Folder";
            this.btnSetDatabaseFolder.UseVisualStyleBackColor = true;
            this.btnSetDatabaseFolder.Click += new System.EventHandler(this.btnSetDatabaseFolder_Click);
            //
            // btnOpenGallery
            //
            this.btnOpenGallery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOpenGallery.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenGallery.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnOpenGallery.Location = new System.Drawing.Point(520, 544);
            this.btnOpenGallery.Name = "btnOpenGallery";
            this.btnOpenGallery.Size = new System.Drawing.Size(120, 25);
            this.btnOpenGallery.TabIndex = 53;
            this.btnOpenGallery.Text = "Open Gallery";
            this.btnOpenGallery.UseVisualStyleBackColor = true;
            this.btnOpenGallery.Click += new System.EventHandler(this.btnOpenGallery_Click);
            //
            // lblDatabaseFolderPath
            //
            this.lblDatabaseFolderPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblDatabaseFolderPath.AutoSize = true;
            this.lblDatabaseFolderPath.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblDatabaseFolderPath.Location = new System.Drawing.Point(376, 549);
            this.lblDatabaseFolderPath.Name = "lblDatabaseFolderPath";
            this.lblDatabaseFolderPath.Size = new System.Drawing.Size(130, 13);
            this.lblDatabaseFolderPath.TabIndex = 37;
            this.lblDatabaseFolderPath.Text = "Database Folder: Not Set";
            //
            // pnlSideBar
            //
            this.pnlSideBar.Controls.Add(this.btnSettings);
            this.pnlSideBar.Controls.Add(this.btnHelp);
            this.pnlSideBar.Controls.Add(this.btnToggleTheme);
            this.pnlSideBar.Controls.Add(this.btnOpenGallery);
            this.pnlSideBar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSideBar.Location = new System.Drawing.Point(0, 32);
            this.pnlSideBar.Name = "pnlSideBar";
            this.pnlSideBar.Size = new System.Drawing.Size(50, 568);
            this.pnlSideBar.TabIndex = 60;
            //
            // pnlTitleBar
            //
            this.pnlTitleBar.Controls.Add(this.lblAppTitle);
            this.pnlTitleBar.Controls.Add(this.btnAppMinimize);
            this.pnlTitleBar.Controls.Add(this.btnAppClose);
            this.pnlTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Location = new System.Drawing.Point(0, 0);
            this.pnlTitleBar.Name = "pnlTitleBar";
            this.pnlTitleBar.Size = new System.Drawing.Size(1359, 32);
            this.pnlTitleBar.TabIndex = 61;
            this.pnlTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlTitleBar_MouseDown);
            //
            // btnAppClose
            //
            this.btnAppClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnAppClose.FlatAppearance.BorderSize = 0;
            this.btnAppClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAppClose.Font = new System.Drawing.Font("Segoe MDL2 Assets", 10F);
            this.btnAppClose.Location = new System.Drawing.Point(1314, 0);
            this.btnAppClose.Name = "btnAppClose";
            this.btnAppClose.Size = new System.Drawing.Size(45, 32);
            this.btnAppClose.TabIndex = 0;
            this.btnAppClose.Text = "";
            this.btnAppClose.UseVisualStyleBackColor = true;
            this.btnAppClose.Click += new System.EventHandler(this.btnAppClose_Click);
            //
            // btnAppMinimize
            //
            this.btnAppMinimize.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnAppMinimize.FlatAppearance.BorderSize = 0;
            this.btnAppMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAppMinimize.Font = new System.Drawing.Font("Segoe MDL2 Assets", 10F);
            this.btnAppMinimize.Location = new System.Drawing.Point(1269, 0);
            this.btnAppMinimize.Name = "btnAppMinimize";
            this.btnAppMinimize.Size = new System.Drawing.Size(45, 32);
            this.btnAppMinimize.TabIndex = 1;
            this.btnAppMinimize.Text = "";
            this.btnAppMinimize.UseVisualStyleBackColor = true;
            this.btnAppMinimize.Click += new System.EventHandler(this.btnAppMinimize_Click);
            //
            // lblAppTitle
            //
            this.lblAppTitle.AutoSize = true;
            this.lblAppTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            this.lblAppTitle.Location = new System.Drawing.Point(10, 8);
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Size = new System.Drawing.Size(200, 15);
            this.lblAppTitle.TabIndex = 2;
            this.lblAppTitle.Text = "Dreams LIVE Solutions Presenter App";
            this.lblAppTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlTitleBar_MouseDown);
            //
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1359, 600);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Controls.Add(this.pnlSideBar);
            this.Controls.Add(this.pnlTitleBar);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
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
            this.Controls.Add(this.btnOpenGallery);
            this.Controls.Add(this.lblDatabaseFolderPath);
            this.Controls.Add(this.btnSnip);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panelSecondaryPreviewBorder.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSecondaryPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
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
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSetDatabaseFolder;
        private System.Windows.Forms.Label lblDatabaseFolderPath;
        private System.Windows.Forms.Panel pnlSideBar;
        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Button btnAppClose;
        private System.Windows.Forms.Button btnAppMinimize;
        private System.Windows.Forms.Label lblAppTitle;
        private System.Windows.Forms.Button btnOpenGallery;
        private System.Windows.Forms.Button btnSnip;
    }
}
