using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DreamsLive_Solutions_PresenterApp1
{
    public partial class GalleryForm : Form
    {
        private readonly MainForm _mainForm;
        private string _settingsPath;
        private GallerySettings _settings;
        private FileSystemWatcher _watcher;

        public GalleryForm(MainForm mainForm)
        {
            InitializeComponent();
            _mainForm = mainForm;
            this.Opacity = 0.85;

            // Enable double buffering for FlowLayoutPanel using reflection to reduce flickering
            var property = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            property?.SetValue(flowThumbs, true, null);

            _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DreamsLivePresenterApp", "gallery_settings.json");
            LoadGallerySettings();

            this.timerDebounce = new Timer(this.components);
            this.timerDebounce.Interval = 500;
            this.timerDebounce.Tick += (s, e) => {
                this.timerDebounce.Stop();
                RefreshGallery();
            };
        }

        private void GalleryForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            RefreshSubfolders();
            RestoreWindowSettings();
            SetupWatcher();
            RefreshGallery();
        }

        private void SetupWatcher()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }

            if (string.IsNullOrEmpty(_mainForm.DatabaseFolderPath) || !Directory.Exists(_mainForm.DatabaseFolderPath))
                return;

            string subfolder = cmbSubfolders.SelectedItem?.ToString();
            if (subfolder == "(Root)") subfolder = "";
            string targetPath = string.IsNullOrEmpty(subfolder) ? _mainForm.DatabaseFolderPath : Path.Combine(_mainForm.DatabaseFolderPath, subfolder);

            if (!Directory.Exists(targetPath)) return;

            _watcher = new FileSystemWatcher(targetPath);
            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            _watcher.Created += (s, e) => this.Invoke((Action)(() => RefreshGallery()));
            _watcher.Deleted += (s, e) => this.Invoke((Action)(() => RefreshGallery()));
            _watcher.Renamed += (s, e) => this.Invoke((Action)(() => RefreshGallery()));
            _watcher.EnableRaisingEvents = true;
        }

        private void LoadGallerySettings()
        {
            if (File.Exists(_settingsPath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsPath);
                    _settings = JsonConvert.DeserializeObject<GallerySettings>(json) ?? new GallerySettings();
                }
                catch { _settings = new GallerySettings(); }
            }
            else
            {
                _settings = new GallerySettings();
            }
            trackThumbSize.Value = _settings.ThumbnailSize;
        }

        private void SaveGallerySettings()
        {
            string dir = Path.GetDirectoryName(_settingsPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            _settings.ThumbnailSize = trackThumbSize.Value;
            _settings.X = this.Location.X;
            _settings.Y = this.Location.Y;
            _settings.Width = this.Width;
            _settings.Height = this.Height;
            _settings.LastSubfolder = cmbSubfolders.SelectedItem?.ToString() ?? "";

            try
            {
                string json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(_settingsPath, json);
            }
            catch { }
        }

        private void RestoreWindowSettings()
        {
            if (_settings.Width > 100 && _settings.Height > 100)
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(_settings.X, _settings.Y);
                this.Size = new Size(_settings.Width, _settings.Height);
            }

            if (!string.IsNullOrEmpty(_settings.LastSubfolder))
            {
                int index = cmbSubfolders.FindStringExact(_settings.LastSubfolder);
                if (index != -1) cmbSubfolders.SelectedIndex = index;
            }
        }

        private void RefreshSubfolders()
        {
            var folders = _mainForm.GetDatabaseSubfolders();
            cmbSubfolders.Items.Clear();
            cmbSubfolders.Items.Add("(Root)");
            foreach (var f in folders) cmbSubfolders.Items.Add(f);
            cmbSubfolders.SelectedIndex = 0;
        }

        private void RefreshGallery()
        {
            flowThumbs.SuspendLayout();
            // Dispose old controls and images
            DisposeControlsRecursive(flowThumbs);
            flowThumbs.Controls.Clear();

            string subfolder = cmbSubfolders.SelectedItem?.ToString();
            if (subfolder == "(Root)") subfolder = "";

            var files = _mainForm.GetDatabaseMediaFiles(subfolder);

            foreach (var file in files)
            {
                var thumbContainer = new Panel
                {
                    Size = new Size(_settings.ThumbnailSize, _settings.ThumbnailSize + 40),
                    Margin = new Padding(5)
                };

                var pb = new PictureBox
                {
                    Size = new Size(_settings.ThumbnailSize, _settings.ThumbnailSize),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BorderStyle = BorderStyle.FixedSingle,
                    Cursor = Cursors.Hand,
                    Dock = DockStyle.Top
                };
                pb.Click += (s, e) => _mainForm.OpenMediaFile(file.RelativePath);

                var lbl = new Label
                {
                    Text = file.Name,
                    Dock = DockStyle.Bottom,
                    TextAlign = ContentAlignment.TopCenter,
                    Font = new Font("Segoe UI", 8),
                    Height = 35,
                    AutoEllipsis = true
                };

                thumbContainer.Controls.Add(pb);
                thumbContainer.Controls.Add(lbl);
                flowThumbs.Controls.Add(thumbContainer);

                // Load thumb on a background thread
                _ = Task.Run(() => LoadThumbnail(pb, file));
            }
            flowThumbs.ResumeLayout();
        }

        private void LoadThumbnail(PictureBox pb, DatabaseFileInfo file)
        {
            if (pb.IsDisposed) return;
            Image thumb = null;
            try
            {
                if (file.Extension == ".pdf")
                {
                    // Render first page of PDF
                    using (var doc = PdfiumViewer.PdfDocument.Load(file.FullPath))
                    {
                        if (doc.PageCount > 0)
                        {
                            thumb = doc.Render(0, 96, 96, false);
                        }
                    }
                }
                else if (new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" }.Contains(file.Extension))
                {
                    // Standard image
                    using (var img = ImageUtils.LoadImage(file.FullPath))
                    {
                        thumb = new Bitmap(img, new Size(_settings.ThumbnailSize, _settings.ThumbnailSize));
                    }
                }
                else
                {
                    // Generic file icon
                    Icon icon = Icon.ExtractAssociatedIcon(file.FullPath);
                    if (icon != null)
                    {
                        thumb = icon.ToBitmap();
                    }
                }
            }
            catch { }

            if (thumb != null)
            {
                if (pb.IsDisposed)
                {
                    thumb.Dispose();
                    return;
                }

                if (pb.InvokeRequired)
                {
                    pb.Invoke((Action)(() => {
                        if (!pb.IsDisposed) pb.Image = thumb;
                        else thumb.Dispose();
                    }));
                }
                else
                {
                    pb.Image = thumb;
                }
            }
        }

        private void cmbSubfolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetupWatcher();
            RefreshGallery();
        }

        private void trackThumbSize_Scroll(object sender, EventArgs e)
        {
            _settings.ThumbnailSize = trackThumbSize.Value;
            this.timerDebounce.Stop();
            this.timerDebounce.Start();
        }

        private void DisposeControlsRecursive(Control parent)
        {
            while (parent.Controls.Count > 0)
            {
                Control ctrl = parent.Controls[0];
                DisposeControlsRecursive(ctrl);
                if (ctrl is PictureBox pb)
                {
                    pb.Image?.Dispose();
                    pb.Image = null;
                }
                ctrl.Dispose();
            }
        }

        private void GalleryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
            }
            SaveGallerySettings();
        }

        private void ApplyTheme()
        {
            LinearTheme.SetMode(_mainForm.IsDarkMode);
            LinearTheme.Apply(this);

            // Lift the top/bottom chrome bars one step above the canvas.
            pnlTop.BackColor = LinearTheme.Current.Surface1;
            pnlBottom.BackColor = LinearTheme.Current.Surface1;

        }
    }

    public class GallerySettings
    {
        public int ThumbnailSize { get; set; } = 150;
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; } = 800;
        public int Height { get; set; } = 500;
        public string LastSubfolder { get; set; } = "";
    }
}
