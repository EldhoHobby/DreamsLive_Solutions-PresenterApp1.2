namespace DreamsLive_Solutions_PresenterApp1
{
    partial class SnipForm
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
            this.SuspendLayout();
            //
            // SnipForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "SnipForm";
            this.Text = "SnipForm";
            this.Load += new System.EventHandler(this.SnipForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SnipForm_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SnipForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SnipForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SnipForm_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
