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

    }
}
