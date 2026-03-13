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
        private int _thumbSize = 100;
        private string _currentSubfolder = "";
        private System.Threading.CancellationTokenSource _cts;

        public GalleryForm(MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
            this.Opacity = 0.85; // Set semi-transparency
            LoadGallerySettings();
            RefreshFolders();
            RefreshGallery();

            this.FormClosing += GalleryForm_FormClosing;
        }

        private void RefreshFolders()
        {
            var folders = _mainForm.GetDatabaseSubfolders();
            cmbSubfolders.Items.Clear();
            cmbSubfolders.Items.Add("(Root)");
            foreach (var f in folders) cmbSubfolders.Items.Add(f);

            if (string.IsNullOrEmpty(_currentSubfolder))
                cmbSubfolders.SelectedIndex = 0;
            else
            {
                int idx = cmbSubfolders.FindStringExact(_currentSubfolder);
                cmbSubfolders.SelectedIndex = idx >= 0 ? idx : 0;
            }
        }

        private void RefreshGallery()
        {
            _cts?.Cancel();
            _cts = new System.Threading.CancellationTokenSource();

            // Dispose old controls and their images to prevent memory leaks
            foreach (Control ctrl in flowLayoutPanel1.Controls)
            {
                if (ctrl is Panel panel)
                {
                    foreach (Control child in panel.Controls)
                    {
                        if (child is PictureBox pb && pb.Image != null)
                        {
                            pb.Image.Dispose();
                        }
                    }
                }
                ctrl.Dispose();
            }

            flowLayoutPanel1.Controls.Clear();
            string subfolder = cmbSubfolders.SelectedIndex <= 0 ? "" : cmbSubfolders.SelectedItem.ToString();
            _currentSubfolder = subfolder;

            var files = _mainForm.GetDatabaseMediaFiles(subfolder);
            foreach (var file in files)
            {
                AddGalleryItem(file, _cts.Token);
            }
        }

        private void AddGalleryItem(DatabaseFileInfo file, System.Threading.CancellationToken ct)
        {
            Panel itemPanel = new Panel
            {
                Size = new Size(_thumbSize + 10, _thumbSize + 40),
                Margin = new Padding(5)
            };

            PictureBox pb = new PictureBox
            {
                Size = new Size(_thumbSize, _thumbSize),
                Location = new Point(5, 5),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(40, 40, 40)
            };

            Label lbl = new Label
            {
                Text = file.Name,
                Size = new Size(_thumbSize, 30),
                Location = new Point(5, _thumbSize + 7),
                TextAlign = ContentAlignment.TopCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8f),
                AutoEllipsis = true
            };

            pb.Click += (s, e) => {
                _mainForm.OpenMediaFile(file.RelativePath, updateStaging: true);
            };

            itemPanel.Controls.Add(pb);
            itemPanel.Controls.Add(lbl);
            flowLayoutPanel1.Controls.Add(itemPanel);

            // Set Thumbnail Asynchronously
            Task.Run(() => LoadThumbnailAsync(pb, file.FullPath, file.Extension, ct));
        }

        private async Task LoadThumbnailAsync(PictureBox pb, string fullPath, string ext, System.Threading.CancellationToken ct)
        {
            try
            {
                Image thumb = null;
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
                {
                    using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        using (var img = Image.FromStream(fs))
                        {
                            thumb = new Bitmap(img, new Size(_thumbSize, _thumbSize));
                        }
                    }
                }
                else if (ext == ".pdf")
                {
                    // Render first page of PDF
                    using (var doc = PdfiumViewer.PdfDocument.Load(fullPath))
                    {
                        if (doc.PageCount > 0)
                        {
                            thumb = doc.Render(0, _thumbSize, _thumbSize, true);
                        }
                    }
                }
                else
                {
                    // Use system icon for other files
                    using (Icon icon = Icon.ExtractAssociatedIcon(fullPath))
                    {
                        if (icon != null)
                        {
                            thumb = icon.ToBitmap();
                        }
                    }
                }

                if (thumb != null && !ct.IsCancellationRequested)
                {
                    pb.Invoke((Action)(() => {
                        if (!ct.IsCancellationRequested)
                            pb.Image = thumb;
                        else
                            thumb.Dispose();
                    }));
                }
                else
                {
                    thumb?.Dispose();
                }
            }
            catch
            {
                pb.Invoke((Action)(() => {
                    if (!ct.IsCancellationRequested)
                        pb.BackColor = Color.DarkRed;
                }));
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshFolders();
            RefreshGallery();
        }

        private void cmbSubfolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshGallery();
        }

        private Timer _sliderDebounceTimer;

        private void trackBarThumbSize_Scroll(object sender, EventArgs e)
        {
            _thumbSize = trackBarThumbSize.Value;

            if (_sliderDebounceTimer == null)
            {
                _sliderDebounceTimer = new Timer();
                _sliderDebounceTimer.Interval = 250;
                _sliderDebounceTimer.Tick += (s, ev) => {
                    _sliderDebounceTimer.Stop();
                    RefreshGallery();
                };
            }
            _sliderDebounceTimer.Stop();
            _sliderDebounceTimer.Start();
        }

        private void btnAddFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select File to Add to Database";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string sourcePath = ofd.FileName;

                    // Ask for folder
                    using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                    {
                        fbd.Description = "Select target subfolder in Database";
                        fbd.SelectedPath = _mainForm.DatabaseFolderPath;

                        if (fbd.ShowDialog() == DialogResult.OK)
                        {
                            string targetDir = fbd.SelectedPath;
                            // Ensure it's within database folder
                            if (!targetDir.ToLower().StartsWith(_mainForm.DatabaseFolderPath.ToLower()))
                            {
                                MessageBox.Show("Please select a folder inside the Database path.");
                                return;
                            }

                            string fileName = Path.GetFileName(sourcePath);
                            string destPath = Path.Combine(targetDir, fileName);

                            if (File.Exists(destPath))
                            {
                                if (MessageBox.Show("File already exists. Overwrite?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                                    return;
                            }

                            try
                            {
                                File.Copy(sourcePath, destPath, true);
                                RefreshGallery();

                                // Automatically load into preview
                                string relativePath = destPath.Substring(_mainForm.DatabaseFolderPath.Length).TrimStart(Path.DirectorySeparatorChar);
                                _mainForm.OpenMediaFile(relativePath, updateStaging: true);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error copying file: " + ex.Message);
                            }
                        }
                    }
                }
            }
        }


        private void GalleryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts?.Cancel();
            _sliderDebounceTimer?.Stop();
            SaveGallerySettings();
        }

        private void LoadGallerySettings()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string settingsPath = Path.Combine(appDataPath, "DreamsLivePresenterApp", "gallery_settings.json");

            if (File.Exists(settingsPath))
            {
                try
                {
                    string json = File.ReadAllText(settingsPath);
                    var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                    if (settings.ContainsKey("X") && settings.ContainsKey("Y"))
                    {
                        this.StartPosition = FormStartPosition.Manual;
                        this.Location = new Point(int.Parse(settings["X"]), int.Parse(settings["Y"]));
                    }
                    if (settings.ContainsKey("Width") && settings.ContainsKey("Height"))
                    {
                        this.Size = new Size(int.Parse(settings["Width"]), int.Parse(settings["Height"]));
                    }
                    if (settings.ContainsKey("ThumbSize"))
                    {
                        _thumbSize = int.Parse(settings["ThumbSize"]);
                        trackBarThumbSize.Value = _thumbSize;
                    }
                    if (settings.ContainsKey("LastSubfolder"))
                    {
                        _currentSubfolder = settings["LastSubfolder"];
                    }
                }
                catch { }
            }
        }

        private void SaveGallerySettings()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folderPath = Path.Combine(appDataPath, "DreamsLivePresenterApp");
            Directory.CreateDirectory(folderPath);
            string settingsPath = Path.Combine(folderPath, "gallery_settings.json");

            var settings = new Dictionary<string, string>
            {
                { "X", this.Location.X.ToString() },
                { "Y", this.Location.Y.ToString() },
                { "Width", this.Size.Width.ToString() },
                { "Height", this.Size.Height.ToString() },
                { "ThumbSize", _thumbSize.ToString() },
                { "LastSubfolder", _currentSubfolder }
            };

            try
            {
                File.WriteAllText(settingsPath, JsonConvert.SerializeObject(settings));
            }
            catch { }
        }
    }
}
