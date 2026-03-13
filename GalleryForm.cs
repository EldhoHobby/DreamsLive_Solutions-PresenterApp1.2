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

        public GalleryForm(MainForm mainForm)
        {
            InitializeComponent();
            _mainForm = mainForm;
            this.Opacity = 0.85;
            _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DreamsLivePresenterApp", "gallery_settings.json");
            LoadGallerySettings();
        }

        private void GalleryForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            RefreshSubfolders();
            RestoreWindowSettings();
            RefreshGallery();
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
                    Size = new Size(_settings.ThumbnailSize, _settings.ThumbnailSize + 20),
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
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 8),
                    Height = 20
                };

                thumbContainer.Controls.Add(pb);
                thumbContainer.Controls.Add(lbl);
                flowThumbs.Controls.Add(thumbContainer);

                // Load thumb async
                _ = Task.Run(() => LoadThumbnailAsync(pb, file));
            }
        }

        private async Task LoadThumbnailAsync(PictureBox pb, DatabaseFileInfo file)
        {
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
                if (pb.InvokeRequired)
                {
                    pb.Invoke((Action)(() => pb.Image = thumb));
                }
                else
                {
                    pb.Image = thumb;
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshSubfolders();
            RefreshGallery();
        }

        private void cmbSubfolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshGallery();
        }

        private void trackThumbSize_Scroll(object sender, EventArgs e)
        {
            _settings.ThumbnailSize = trackThumbSize.Value;
            RefreshGallery();
        }

        private void DisposeControlsRecursive(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
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
            SaveGallerySettings();
        }

        private void ApplyTheme()
        {
            bool isDark = _mainForm.IsDarkMode;
            Color backColor = isDark ? Color.FromArgb(45, 45, 48) : SystemColors.Control;
            Color foreColor = isDark ? Color.White : SystemColors.ControlText;

            this.BackColor = backColor;
            this.ForeColor = foreColor;

            pnlTop.BackColor = isDark ? Color.FromArgb(30, 30, 30) : SystemColors.Control;
            pnlTop.ForeColor = foreColor;

            cmbSubfolders.BackColor = isDark ? Color.FromArgb(63, 63, 70) : SystemColors.Window;
            cmbSubfolders.ForeColor = isDark ? Color.White : SystemColors.WindowText;

            btnRefresh.BackColor = isDark ? Color.FromArgb(63, 63, 70) : SystemColors.Control;
            btnRefresh.ForeColor = isDark ? Color.White : SystemColors.ControlText;
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
