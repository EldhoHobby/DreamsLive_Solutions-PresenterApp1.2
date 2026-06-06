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

    }
}
