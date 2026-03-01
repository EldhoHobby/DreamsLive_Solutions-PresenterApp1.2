using System.Windows.Forms;
using System.Drawing;

namespace DreamsLive_Solutions_PresenterApp1
{
    partial class CopyableMessageBox
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.PictureBox picIcon;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.picIcon = new System.Windows.Forms.PictureBox();
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).BeginInit();
            this.SuspendLayout();

            // picIcon
            this.picIcon.Location = new System.Drawing.Point(15, 15);
            this.picIcon.Name = "picIcon";
            this.picIcon.Size = new System.Drawing.Size(32, 32);
            this.picIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            this.picIcon.TabStop = false;

            // txtMessage
            this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessage.BackColor = System.Drawing.SystemColors.Control;
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMessage.Location = new System.Drawing.Point(55, 15);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(317, 70);
            this.txtMessage.TabIndex = 1;

            // flpButtons
            this.flpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpButtons.Location = new System.Drawing.Point(12, 95);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Size = new System.Drawing.Size(360, 35);
            this.flpButtons.TabIndex = 0;

            // CopyableMessageBox
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 141);
            this.Controls.Add(this.flpButtons);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.picIcon);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 180);
            this.Name = "CopyableMessageBox";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void SetupButtons(MessageBoxButtons buttons)
        {
            flpButtons.Controls.Clear();

            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    AddButton("OK", DialogResult.OK);
                    break;
                case MessageBoxButtons.OKCancel:
                    AddButton("Cancel", DialogResult.Cancel);
                    AddButton("OK", DialogResult.OK);
                    break;
                case MessageBoxButtons.YesNo:
                    AddButton("No", DialogResult.No);
                    AddButton("Yes", DialogResult.Yes);
                    break;
                case MessageBoxButtons.YesNoCancel:
                    AddButton("Cancel", DialogResult.Cancel);
                    AddButton("No", DialogResult.No);
                    AddButton("Yes", DialogResult.Yes);
                    break;
                case MessageBoxButtons.RetryCancel:
                     AddButton("Cancel", DialogResult.Cancel);
                    AddButton("Retry", DialogResult.Retry);
                    break;
                case MessageBoxButtons.AbortRetryIgnore:
                    AddButton("Ignore", DialogResult.Ignore);
                    AddButton("Retry", DialogResult.Retry);
                    AddButton("Abort", DialogResult.Abort);
                    break;
            }
        }

        private void AddButton(string text, DialogResult result)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.DialogResult = result;
            btn.Size = new System.Drawing.Size(75, 23);
            btn.UseVisualStyleBackColor = true;
            btn.Click += (sender, e) => { this.Close(); };
            flpButtons.Controls.Add(btn);

            if (result == DialogResult.OK || result == DialogResult.Yes || result == DialogResult.Retry)
                this.AcceptButton = btn;
            else if (result == DialogResult.Cancel || result == DialogResult.No)
                this.CancelButton = btn;
        }

        private void SetupIcon(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Information:
                    picIcon.Image = SystemIcons.Information.ToBitmap();
                    break;
                case MessageBoxIcon.Warning:
                    picIcon.Image = SystemIcons.Warning.ToBitmap();
                    break;
                case MessageBoxIcon.Error:
                    picIcon.Image = SystemIcons.Error.ToBitmap();
                    break;
                case MessageBoxIcon.Question:
                    picIcon.Image = SystemIcons.Question.ToBitmap();
                    break;
                default:
                    picIcon.Visible = false;
                    // Adjust message box layout if no icon
                    txtMessage.Location = new Point(15, 15);
                    txtMessage.Size = new Size(this.ClientSize.Width - 30, txtMessage.Size.Height);
                    break;
            }
        }
    }
}
