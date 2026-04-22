using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace DreamsLive_Solutions_PresenterApp1
{
    public class PowerPointManager
    {
        private PowerPoint.Application _pptApp;
        private PowerPoint.Presentation _presentation;
        private PowerPoint.SlideShowWindow _slideShowWindow;
        private int _pptProcessId = -1;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public event EventHandler<string> NotesChanged;
        public event EventHandler SlideChanged;

        public bool IsRunning => _pptApp != null && _presentation != null;

        public string CurrentNotes { get; private set; }
        public int CurrentSlideIndex { get; private set; }
        public int TotalSlides { get; private set; }

        public void OpenPresentation(string filePath, Screen targetScreen)
        {
            try
            {
                _pptApp = new PowerPoint.Application();

                try
                {
                    uint pid;
                    GetWindowThreadProcessId((IntPtr)_pptApp.HWND, out pid);
                    _pptProcessId = (int)pid;
                }
                catch { }

                // MsoTriState: msoFalse = 0, msoTrue = -1
                _presentation = _pptApp.Presentations.Open(filePath, 0, 0, -1);

                TotalSlides = _presentation.Slides.Count;

                // Configure slide show settings
                PowerPoint.SlideShowSettings settings = _presentation.SlideShowSettings;
                // ppShowTypeSpeaker = 1
                settings.ShowType = PowerPoint.PpSlideShowType.ppShowTypeSpeaker;

                _slideShowWindow = settings.Run();

                // Move slide show window to target screen
                _slideShowWindow.Left = targetScreen.Bounds.Left;
                _slideShowWindow.Top = targetScreen.Bounds.Top;
                _slideShowWindow.Width = targetScreen.Bounds.Width;
                _slideShowWindow.Height = targetScreen.Bounds.Height;

                // Subscribe to events
                _pptApp.SlideShowNextSlide += _pptApp_SlideShowNextSlide;
                _pptApp.SlideShowNextClick += _pptApp_SlideShowNextClick;

                UpdateCurrentStatus();
            }
            catch (Exception ex)
            {
                Cleanup();
                throw new Exception("Failed to open PowerPoint presentation: " + ex.Message);
            }
        }

        private void _pptApp_SlideShowNextClick(PowerPoint.SlideShowWindow Wn, PowerPoint.Effect nEffect)
        {
             UpdateCurrentStatus();
             SlideChanged?.Invoke(this, EventArgs.Empty);
        }

        private void _pptApp_SlideShowNextSlide(PowerPoint.SlideShowWindow Wn)
        {
            UpdateCurrentStatus();
            SlideChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateCurrentStatus()
        {
            if (_slideShowWindow == null) return;

            try
            {
                CurrentSlideIndex = _slideShowWindow.View.Slide.SlideIndex;
                ExtractNotes();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error updating PPT status: " + ex.Message);
            }
        }

        private void ExtractNotes()
        {
            try
            {
                PowerPoint.Slide currentSlide = _slideShowWindow.View.Slide;
                StringBuilder sb = new StringBuilder();

                foreach (PowerPoint.Shape shape in currentSlide.NotesPage.Shapes)
                {
                    // HasTextFrame = -1 (msoTrue)
                    if (shape.HasTextFrame == -1)
                    {
                        // HasText = -1 (msoTrue)
                        if (shape.TextFrame.HasText == -1)
                        {
                            sb.AppendLine(shape.TextFrame.TextRange.Text);
                        }
                    }
                }

                CurrentNotes = sb.ToString().Trim();
                NotesChanged?.Invoke(this, CurrentNotes);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error extracting PPT notes: " + ex.Message);
                CurrentNotes = "";
            }
        }

        public void Next()
        {
            try
            {
                _slideShowWindow?.View.Next();
            }
            catch { }
        }

        public void Previous()
        {
            try
            {
                _slideShowWindow?.View.Previous();
            }
            catch { }
        }

        public void Cleanup()
        {
            try
            {
                if (_pptApp != null)
                {
                    _pptApp.SlideShowNextSlide -= _pptApp_SlideShowNextSlide;
                    _pptApp.SlideShowNextClick -= _pptApp_SlideShowNextClick;
                }

                if (_presentation != null)
                {
                    try { _presentation.Close(); } catch { }
                    Marshal.ReleaseComObject(_presentation);
                    _presentation = null;
                }

                if (_pptApp != null)
                {
                    try { _pptApp.Quit(); } catch { }
                    Marshal.ReleaseComObject(_pptApp);
                    _pptApp = null;
                }

                if (_pptProcessId != -1)
                {
                    try
                    {
                        Process p = Process.GetProcessById(_pptProcessId);
                        if (p != null && !p.HasExited) p.Kill();
                    }
                    catch { }
                    _pptProcessId = -1;
                }
            }
            catch { }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
