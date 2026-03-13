using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DreamsLive_Solutions_PresenterApp1
{
    public static class ModernUIHelper
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        public const int WM_NCHITTEST = 0x84;
        public const int HTLEFT = 10;
        public const int HTRIGHT = 11;
        public const int HTTOP = 12;
        public const int HTTOPLEFT = 13;
        public const int HTTOPRIGHT = 14;
        public const int HTBOTTOM = 15;
        public const int HTBOTTOMLEFT = 16;
        public const int HTBOTTOMRIGHT = 17;

        public static void DragForm(IntPtr handle)
        {
            ReleaseCapture();
            SendMessage(handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        public static GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float r = radius;
            path.AddArc(rect.X, rect.Y, r, r, 180, 90);
            path.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
            path.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void ApplyRoundedCorners(Control control, int radius)
        {
            control.Region = new Region(GetRoundedRectanglePath(control.ClientRectangle, radius));
            control.Resize += (s, e) =>
            {
                if (control.ClientRectangle.Width > 0 && control.ClientRectangle.Height > 0)
                {
                    control.Region = new Region(GetRoundedRectanglePath(control.ClientRectangle, radius));
                }
            };
        }

        public static void HandleResize(ref Message m, Form form, int borderSize = 10)
        {
            if (m.Msg == WM_NCHITTEST)
            {
                Point pos = new Point(m.LParam.ToInt32());
                pos = form.PointToClient(pos);

                if (pos.Y < borderSize)
                {
                    if (pos.X < borderSize) m.Result = (IntPtr)HTTOPLEFT;
                    else if (pos.X >= form.ClientSize.Width - borderSize) m.Result = (IntPtr)HTTOPRIGHT;
                    else m.Result = (IntPtr)HTTOP;
                }
                else if (pos.Y >= form.ClientSize.Height - borderSize)
                {
                    if (pos.X < borderSize) m.Result = (IntPtr)HTBOTTOMLEFT;
                    else if (pos.X >= form.ClientSize.Width - borderSize) m.Result = (IntPtr)HTBOTTOMRIGHT;
                    else m.Result = (IntPtr)HTBOTTOM;
                }
                else
                {
                    if (pos.X < borderSize) m.Result = (IntPtr)HTLEFT;
                    else if (pos.X >= form.ClientSize.Width - borderSize) m.Result = (IntPtr)HTRIGHT;
                }
                if (m.Result != IntPtr.Zero) return;
            }
        }

        // MDL2 Assets font icons (common in Windows 10/11)
        public static class Icons
        {
            public const string Settings = "\uE713";
            public const string Help = "\uE897";
            public const string Photo = "\uE91B";
            public const string Play = "\uE768";
            public const string Close = "\uE8BB";
            public const string Minimize = "\uE921";
            public const string Maximize = "\uE922";
            public const string Restore = "\uE923";
            public const string GlobalNavButton = "\uE700";
            public const string Folder = "\uED25";
            public const string DarkTheme = "\uE708";
            public const string LightTheme = "\uE706";
            public const string Gallery = "\uE158";
            public const string Delete = "\uE74D";
            public const string Refresh = "\uE72C";
        }
    }
}
