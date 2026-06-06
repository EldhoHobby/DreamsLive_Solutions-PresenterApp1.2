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
        private void NotifyPresenterOfLaserPoint()
        {
            if (this.activePresentationForm != null && !this.activePresentationForm.IsDisposed)
            {
                float docRadius = 5f;
                RectangleF contentRect = GetSecondaryPreviewContentRect();
                Image baseImage = (Image)this.stagedStitchedImage ?? this.stagedMasterImage;

                if (contentRect.Width > 0 && baseImage != null)
                {
                    float previewToDocRatio = baseImage.Width / contentRect.Width;
                    docRadius = 5f * previewToDocRatio;
                }
                this.activePresentationForm.UpdateLaserPointer(this.laserPointNormalized, docRadius);
            }
        }

        private void NotifyPresenterOfHighlights()
        {
            if (this.activePresentationForm != null && !this.activePresentationForm.IsDisposed)
            {
                float docWidth = 20f;
                RectangleF contentRect = GetSecondaryPreviewContentRect();
                Image baseImage = (Image)this.stagedStitchedImage ?? this.stagedMasterImage;

                if (contentRect.Width > 0 && baseImage != null)
                {
                    float previewToDocRatio = baseImage.Width / contentRect.Width;
                    docWidth = 20f * previewToDocRatio;
                }
                this.activePresentationForm.UpdateHighlights(this.highlightsNormalized, this.highlighterColor, docWidth);
            }
        }

        private void ClearHighlights()
        {
            this.highlightsNormalized.Clear();
            this.picSecondaryPreview.Invalidate();
            NotifyPresenterOfHighlights();
        }

        private void btnHighlighter_Click(object sender, EventArgs e)
        {
            this.highlighterActive = !this.highlighterActive;

            if (this.highlighterActive)
            {
                this.btnHighlighter.BackColor = Color.Yellow;
                this.btnHighlighter.ForeColor = Color.Black;
            }
            else
            {
                // Clear highlights when disabling
                ClearHighlights();
                this.btnHighlighter.BackColor = LinearTheme.Current.Surface1;
                this.btnHighlighter.ForeColor = LinearTheme.Current.Ink;
            }
        }

        public static void DrawHighlightStroke(Graphics g, PointF[] points, float width, Color? strokeColor = null)
        {
            if (points == null || points.Length == 0) return;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            Color color = strokeColor ?? Color.Yellow;

            // Multiple layers for "glow" effect
            int layers = 3;
            for (int i = layers; i >= 0; i--)
            {
                float currentWidth = width + i * (width * 0.25f);
                int alpha = (i == 0) ? 120 : (int)(50 / (i + 0.5f));
                using (Pen highlightPen = new Pen(Color.FromArgb(alpha, color), currentWidth))
                {
                    highlightPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    highlightPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    highlightPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                    if (points.Length == 1)
                    {
                        using (Brush b = new SolidBrush(Color.FromArgb(alpha, color)))
                        {
                            g.FillEllipse(b, points[0].X - currentWidth / 2f, points[0].Y - currentWidth / 2f, currentWidth, currentWidth);
                        }
                    }
                    else
                    {
                        g.DrawLines(highlightPen, points);
                    }
                }
            }
        }

        public static void DrawLaserPointer(Graphics g, PointF center, float radius)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            // Outer glow
            for (int i = 8; i > 0; i--)
            {
                float r = radius + i * 1.5f;
                int alpha = (int)(60 / (i * 0.8f + 1));
                using (Brush b = new SolidBrush(Color.FromArgb(alpha, Color.Red)))
                {
                    g.FillEllipse(b, center.X - r, center.Y - r, r * 2, r * 2);
                }
            }

            // Inner core glow
            using (Brush b = new SolidBrush(Color.FromArgb(150, Color.White)))
            {
                g.FillEllipse(b, center.X - radius * 0.6f, center.Y - radius * 0.6f, radius * 1.2f, radius * 1.2f);
            }

            // Core
            using (Brush b = new SolidBrush(Color.Red))
            {
                g.FillEllipse(b, center.X - radius * 0.4f, center.Y - radius * 0.4f, radius * 0.8f, radius * 0.8f);
            }
        }

    }
}
