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
        // Theme switching logic — delegates to the Linear-inspired theme engine.
        private void ApplyTheme()
        {
            LinearTheme.SetMode(isDarkMode);
            LinearTheme.Apply(this);

            // Web-server URL label flags an offline server in red.
            if (this.lblWebServerUrl != null)
            {
                this.lblWebServerUrl.ForeColor = (_httpWebServer != null && !_httpWebServer.IsRunning)
                    ? LinearTheme.Current.Danger
                    : LinearTheme.Current.InkSubtle;
            }

            // Re-apply state-driven styling for the blackout/highlighter buttons.
            UpdateButtonAppearanceAndState();

            // Keep an open gallery window in sync with the active theme.
            if (_galleryForm != null && !_galleryForm.IsDisposed)
            {
                LinearTheme.Apply(_galleryForm);
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
