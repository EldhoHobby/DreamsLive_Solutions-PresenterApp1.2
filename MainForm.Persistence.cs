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
        private string GetAppDataFolderPath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appSpecificFolder = Path.Combine(appDataPath, "DreamsLivePresenterApp");
            Directory.CreateDirectory(appSpecificFolder);
            return appSpecificFolder;
        }

        private string GetSelectionsFilePath()
        {
            return Path.Combine(GetAppDataFolderPath(), "selections.json");
        }

        private string GetSettingsFilePath()
        {
            return Path.Combine(GetAppDataFolderPath(), "settings.json");
        }

        private void LoadSettings()
        {
            string filePath = GetSettingsFilePath();
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (settings != null)
                    {
                        if (settings.ContainsKey("DatabaseFolderPath"))
                        {
                            DatabaseFolderPath = settings["DatabaseFolderPath"];
                            if (!string.IsNullOrEmpty(DatabaseFolderPath))
                            {
                                lblDatabaseFolderPath.Text = "Database Folder: " + DatabaseFolderPath;
                                SetupDatabaseWatcher();
                            }
                            else
                            {
                                lblDatabaseFolderPath.Text = "Database Folder: Not Selected";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading settings: {ex.Message}");
                }
            }
            else
            {
                lblDatabaseFolderPath.Text = "Database Folder: Not Selected";
            }
        }

        private void SetupDatabaseWatcher()
        {
            if (_dbWatcher != null)
            {
                _dbWatcher.EnableRaisingEvents = false;
                _dbWatcher.Dispose();
                _dbWatcher = null;
            }

            if (!string.IsNullOrEmpty(DatabaseFolderPath) && Directory.Exists(DatabaseFolderPath))
            {
                _dbWatcher = new FileSystemWatcher(DatabaseFolderPath);
                _dbWatcher.IncludeSubdirectories = true;
                _dbWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

                FileSystemEventHandler handler = (s, e) => {
                    Interlocked.Increment(ref _galleryVersion);
                };

                _dbWatcher.Created += handler;
                _dbWatcher.Deleted += handler;
                _dbWatcher.Renamed += (s, e) => Interlocked.Increment(ref _galleryVersion);

                _dbWatcher.EnableRaisingEvents = true;
                _galleryVersion++; // Trigger initial load
            }
        }

        public void SaveSettings()
        {
            string filePath = GetSettingsFilePath();
            try
            {
                var settings = new Dictionary<string, string>
                {
                    { "DatabaseFolderPath", DatabaseFolderPath },
                    { "SkipOnePage", skipOnePage.ToString() },
                    { "TwoPagePdf", twoPagePdf.ToString() }
                };
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private void btnSetDatabaseFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select the Database Folder for Media Files";
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    DatabaseFolderPath = fbd.SelectedPath;
                    lblDatabaseFolderPath.Text = "Database Folder: " + DatabaseFolderPath;
                    SetupDatabaseWatcher();
                    SaveSettings();
                }
            }
        }

        public List<string> GetDatabaseSubfolders()
        {
            List<string> subfolders = new List<string>();
            if (string.IsNullOrEmpty(DatabaseFolderPath) || !Directory.Exists(DatabaseFolderPath))
                return subfolders;

            try
            {
                subfolders.AddRange(Directory.GetDirectories(DatabaseFolderPath, "*", SearchOption.AllDirectories)
                    .Select(d => d.Substring(DatabaseFolderPath.Length).TrimStart(Path.DirectorySeparatorChar)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting subfolders: {ex.Message}");
            }
            return subfolders;
        }

        public List<DatabaseFileInfo> GetDatabaseMediaFiles(string subfolder = "")
        {
            List<DatabaseFileInfo> files = new List<DatabaseFileInfo>();
            if (string.IsNullOrEmpty(DatabaseFolderPath) || !Directory.Exists(DatabaseFolderPath))
                return files;

            try
            {
                // Security: an empty subfolder means the root; anything else must resolve to a
                // path inside the root, so /database/gallery?subfolder=..\..\ can't enumerate
                // arbitrary directories and leak their absolute paths.
                string targetPath = string.IsNullOrEmpty(subfolder)
                    ? Path.GetFullPath(DatabaseFolderPath)
                    : ResolveWithinDatabase(subfolder);
                if (string.IsNullOrEmpty(targetPath) || !Directory.Exists(targetPath)) return files;

                // Requirement: all files in folder without extension filtering, support non-media with icons
                var allFiles = Directory.EnumerateFiles(targetPath, "*.*", SearchOption.TopDirectoryOnly);

                foreach (var file in allFiles)
                {
                    files.Add(new DatabaseFileInfo(file, DatabaseFolderPath));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting media files: {ex.Message}");
            }
            return files;
        }

        public void OpenMediaFile(string relativePath, bool updateStaging = true)
        {
            // Security: resolve against the database root and reject any path that escapes it
            // (`..`, absolute paths, sibling-prefix tricks). Without this, the unauthenticated
            // /action/open endpoint could load any image/PDF on disk into the presenter.
            string fullPath = ResolveWithinDatabase(relativePath);
            if (fullPath != null && File.Exists(fullPath))
            {
                this.Invoke((Action)(() => ProcessNewImage(fullPath, updateStaging)));
            }
        }

        /// <summary>
        /// Resolves a caller-supplied relative path against the configured database root and
        /// returns the absolute path ONLY if it stays inside that root. Returns null for an
        /// unset root, an empty path, or any traversal attempt. This is the single containment
        /// check shared by every remote endpoint that accepts a path or subfolder.
        /// </summary>
        public string ResolveWithinDatabase(string relativePath)
        {
            if (string.IsNullOrEmpty(DatabaseFolderPath) || string.IsNullOrEmpty(relativePath))
                return null;

            string root = Path.GetFullPath(DatabaseFolderPath);
            string rootWithSep = root.EndsWith(Path.DirectorySeparatorChar.ToString())
                ? root : root + Path.DirectorySeparatorChar;

            string full;
            try { full = Path.GetFullPath(Path.Combine(root, relativePath)); }
            catch { return null; } // malformed path (illegal chars, too long, etc.)

            // The trailing separator is essential: a bare StartsWith(root) would also accept a
            // sibling directory whose name merely begins with the root's name.
            if (full.Equals(root, StringComparison.OrdinalIgnoreCase) ||
                full.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase))
                return full;

            return null;
        }

        private List<ImageSelectionData> LoadSelections()
        {
            string filePath = GetSelectionsFilePath();
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    if (string.IsNullOrWhiteSpace(json)) return new List<ImageSelectionData>();
                    return JsonConvert.DeserializeObject<List<ImageSelectionData>>(json) ?? new List<ImageSelectionData>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading selections: {ex.Message}");
                    // Optionally, handle corrupted file (e.g., backup and create new)
                }
            }
            return new List<ImageSelectionData>();
        }

        private void SaveSelections(List<ImageSelectionData> selections)
        {
            string filePath = GetSelectionsFilePath();
            try
            {
                string json = JsonConvert.SerializeObject(selections, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving selections: {ex.Message}");
                // Optionally, inform the user
            }
        }

        private void SaveCurrentSelection()
        {
            if (!this.selectionRectangle.IsEmpty && this.selectedImagePath != null)
            {
                RectangleF? imageCoordsSelection = GetSelectedRegionInImageCoordinates();
                if (imageCoordsSelection.HasValue && imageCoordsSelection.Value.Width > 0 && imageCoordsSelection.Value.Height > 0)
                {
                    List<ImageSelectionData> selections = LoadSelections();
                    selections.RemoveAll(s => s.ImagePath.Equals(this.selectedImagePath, StringComparison.OrdinalIgnoreCase));
                    selections.Add(new ImageSelectionData(this.selectedImagePath, imageCoordsSelection.Value));
                    SaveSelections(selections);
                }
            }
            else if (this.selectionRectangle.IsEmpty && this.selectedImagePath != null)
            {
                List<ImageSelectionData> selections = LoadSelections();
                int removedCount = selections.RemoveAll(s => s.ImagePath.Equals(this.selectedImagePath, StringComparison.OrdinalIgnoreCase));
                if (removedCount > 0)
                {
                    SaveSelections(selections);
                }
            }
        }

    }
}
