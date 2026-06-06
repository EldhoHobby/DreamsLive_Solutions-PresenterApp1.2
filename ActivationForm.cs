using System;
using System.Drawing;
using System.Windows.Forms;

namespace DreamsLive_Solutions_PresenterApp1
{
    public partial class ActivationForm : Form
    {
        public ActivationForm()
        {
            InitializeComponent();
            LinearTheme.Apply(this);
            this.TopMost = true;

            // Handle focus explicitly when the form is fully displayed
            this.Shown += (s, e) => closeButton.Focus();

            Form mainForm = null;
            foreach (Form openForm in Application.OpenForms)
            {
                if (openForm is MainForm)
                {
                    mainForm = openForm;
                    break;
                }
            }

            if (mainForm != null)
            {
                this.StartPosition = FormStartPosition.CenterParent;
                this.Owner = mainForm;
            }
            else
            {
                this.StartPosition = FormStartPosition.CenterScreen;
            }

            try
            {
                machineIdTextBox.Text = MachineIdentifier.GetMachineId();
                UpdateStatus();
            }
            catch { /* Handle potential ID retrieval errors */ }
        }

        private void UpdateStatus()
        {
            statusLabel.Text = "Status: " + ActivationStatusHelper.GetActivationStatusString();
        }

        private void activateButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(licenseKeyTextBox.Text))
            {
                CopyableMessageBox.Show(this, "Please enter a license key.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SecureLicenseManager licenseManager = new SecureLicenseManager();
            LicenseStatus status = licenseManager.ActivateLicense(licenseKeyTextBox.Text);

            switch (status)
            {
                case LicenseStatus.Valid:
                    CopyableMessageBox.Show(this, "Software activated successfully! The application will now restart.", "Activation Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Restart();
                    break;
                case LicenseStatus.Expired:
                    CopyableMessageBox.Show(this, "The license key has expired.", "Activation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case LicenseStatus.WrongMachineId:
                    CopyableMessageBox.Show(this, "The license key is for a different machine.", "Activation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case LicenseStatus.InvalidKey:
                    CopyableMessageBox.Show(this, "The license key is invalid.", "Activation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #region Windows Form Designer generated code

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private Label instructionLabel;
        private Label emailLabel;
        private Label supportLabel;
        private TextBox machineIdTextBox;
        private Button activateButton;
        private Button closeButton;
        private Label machineIdLabel;
        private Label statusLabel;
        private TextBox licenseKeyTextBox;
        private Label taglineLabel;

        private void InitializeComponent()
        {
            this.instructionLabel = new System.Windows.Forms.Label();
            this.emailLabel = new System.Windows.Forms.Label();
            this.supportLabel = new System.Windows.Forms.Label();
            this.machineIdTextBox = new System.Windows.Forms.TextBox();
            this.activateButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.machineIdLabel = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.licenseKeyTextBox = new System.Windows.Forms.TextBox();
            this.taglineLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // instructionLabel
            this.instructionLabel.AutoSize = true;
            this.instructionLabel.Location = new System.Drawing.Point(12, 15);
            this.instructionLabel.Name = "instructionLabel";
            this.instructionLabel.Size = new System.Drawing.Size(205, 13);
            this.instructionLabel.TabIndex = 9;
            this.instructionLabel.Text = "Free Activation: Email your Machine ID to:";

            // emailLabel
            this.emailLabel.AutoSize = true;
            this.emailLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.emailLabel.Location = new System.Drawing.Point(12, 33);
            this.emailLabel.Name = "emailLabel";
            this.emailLabel.Size = new System.Drawing.Size(199, 15);
            this.emailLabel.TabIndex = 8;
            this.emailLabel.Text = "dreamslivekottappady@gmail.com";

            // supportLabel
            this.supportLabel.AutoSize = true;
            this.supportLabel.Location = new System.Drawing.Point(12, 53);
            this.supportLabel.Name = "supportLabel";
            this.supportLabel.Size = new System.Drawing.Size(267, 13);
            this.supportLabel.TabIndex = 7;
            this.supportLabel.Text = "Contact us at the same address for support or requests.";

            // machineIdTextBox
            this.machineIdTextBox.Location = new System.Drawing.Point(83, 80);
            this.machineIdTextBox.Name = "machineIdTextBox";
            this.machineIdTextBox.ReadOnly = true;
            this.machineIdTextBox.Size = new System.Drawing.Size(189, 20);
            this.machineIdTextBox.TabIndex = 6;

            // activateButton
            this.activateButton.Location = new System.Drawing.Point(116, 210);
            this.activateButton.Name = "activateButton";
            this.activateButton.Size = new System.Drawing.Size(75, 23);
            this.activateButton.TabIndex = 1; // Changed to 1
            this.activateButton.Text = "Activate";
            this.activateButton.UseVisualStyleBackColor = true;
            this.activateButton.Click += new System.EventHandler(this.activateButton_Click);

            // closeButton
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(197, 210);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 0; // SET TO 0 FOR INITIAL FOCUS
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);

            // machineIdLabel
            this.machineIdLabel.AutoSize = true;
            this.machineIdLabel.Location = new System.Drawing.Point(12, 83);
            this.machineIdLabel.Name = "machineIdLabel";
            this.machineIdLabel.Size = new System.Drawing.Size(65, 13);
            this.machineIdLabel.TabIndex = 3;
            this.machineIdLabel.Text = "Machine ID:";

            // statusLabel
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(12, 108);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(52, 13);
            this.statusLabel.TabIndex = 2;
            this.statusLabel.Text = "Status: ...";

            // licenseKeyTextBox
            this.licenseKeyTextBox.Location = new System.Drawing.Point(15, 128);
            this.licenseKeyTextBox.Multiline = true;
            this.licenseKeyTextBox.Name = "licenseKeyTextBox";
            this.licenseKeyTextBox.Size = new System.Drawing.Size(257, 45);
            this.licenseKeyTextBox.TabIndex = 2; // Changed to 2

            // taglineLabel
            this.taglineLabel.AutoSize = true;
            this.taglineLabel.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Italic);
            this.taglineLabel.ForeColor = System.Drawing.Color.Gray;
            this.taglineLabel.Location = new System.Drawing.Point(12, 185);
            this.taglineLabel.Name = "taglineLabel";
            this.taglineLabel.Size = new System.Drawing.Size(204, 12);
            this.taglineLabel.TabIndex = 11;

            // ActivationForm
            this.AcceptButton = this.activateButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(284, 245);
            this.Controls.Add(this.taglineLabel);
            this.Controls.Add(this.licenseKeyTextBox);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.machineIdLabel);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.activateButton);
            this.Controls.Add(this.machineIdTextBox);
            this.Controls.Add(this.supportLabel);
            this.Controls.Add(this.emailLabel);
            this.Controls.Add(this.instructionLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ActivationForm";
            this.Text = "Software Activation";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }
}