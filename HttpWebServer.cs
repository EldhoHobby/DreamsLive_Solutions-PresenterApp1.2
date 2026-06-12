using System;
using System.Collections.Generic; // Added for List
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets; // Added for IP detection
using System.Text;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DreamsLive_Solutions_PresenterApp1
{
    public class HttpWebServer
    {
        private HttpListener _listener;
        private readonly MainForm _mainForm;
        private CancellationTokenSource _cts;

        public string ServerUrl { get; private set; }
        public bool IsRunning => _listener != null && _listener.IsListening;

        private bool _cachedIsLicenseExpired;
        private DateTime _licenseExpiredCheckTime = DateTime.MinValue;

        // Preview change tracking: a version is bumped only when the underlying preview
        // Image reference is replaced, so the remote re-fetches a preview ONLY when it
        // actually changes — not on every 1s status poll.
        private long _mainPreviewVersion = 0;
        private long _secondaryPreviewVersion = 0;
        private Image _lastMainPreviewRef = null;
        private Image _lastSecondaryPreviewRef = null;

        // D-rec-1b: short-lived status cache so rapid / multi-client polls share one build.
        private string _cachedStatusJson = null;
        private DateTime _cachedStatusJsonTime = DateTime.MinValue;
        private readonly object _statusCacheLock = new object();

        // D-rec-1a: ref-keyed encoded preview cache (per endpoint) so concurrent / repeat
        // requests for an unchanged preview reuse a single JPEG encode.
        private sealed class PreviewCache { public Image SourceRef; public byte[] Bytes; public string ContentType; }
        private readonly PreviewCache _mainPreviewCache = new PreviewCache();
        private readonly PreviewCache _secondaryPreviewCache = new PreviewCache();
        private readonly object _previewCacheLock = new object();

        // D-rec-4: cached control references (resolved once; these controls are stable).
        private CheckBox _ctlChkLink;
        private Label _ctlLblMessage;
        private Button _ctlBtnPush, _ctlBtnClose, _ctlBtnBlackout;
        private PictureBox _ctlPicSecondary;
        private Panel _ctlPanelSecondaryBorder;
        private bool _ctlsResolved = false;

        public HttpWebServer(MainForm mainForm)
        {
            _mainForm = mainForm;
        }

        public bool Start()
        {
            Stop();
            _cts = new CancellationTokenSource();

            // This MUST match the wildcard used in the netsh command
            string wildcardPrefix = "http://*:21011/";

            _listener = new HttpListener();
            try
            {
                _listener.Prefixes.Add(wildcardPrefix);
                _listener.Start();

                // For the UI display, use the actual IP so you know what to type in the browser
                ServerUrl = $"http://{GetLocalIPAddress()}:21011/";

                Task.Run(() => RunServer(_listener, _cts.Token));
                return true;
            }
            catch (Exception ex)
            {
                _listener.Close();
                _listener = null;
                CopyableMessageBox.Show(_mainForm, $"Server failed to start: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // FIX: Helper method to find the machine's IP on the local network
        private string GetLocalIPAddress()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint?.Address.ToString();
                }
            }
            catch { return "127.0.0.1"; }
        }

        public void Stop()
        {
            _cts?.Cancel();
            try
            {
                if (_listener != null && _listener.IsListening)
                {
                    _listener.Stop();
                }
                _listener?.Close();
            }
            catch { }
            finally
            {
                _listener = null;
            }
        }

        private async Task RunServer(HttpListener listener, CancellationToken token)
        {
            while (!token.IsCancellationRequested && listener.IsListening)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    // Handle each request concurrently — do NOT await, so a slow request
                    // (large upload or image encode) doesn't block other clients/requests.
                    // ProcessRequest has its own try/catch/finally that always closes the response.
                    _ = ProcessRequest(context);
                }
                catch (HttpListenerException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Web server error: {ex.Message}");
                }
            }
        }

        private async Task ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string path = request.Url.AbsolutePath;

            try
            {
                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");

                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                    await response.OutputStream.WriteAsync(new byte[0], 0, 0);
                    response.OutputStream.Close();
                    return;
                }

                byte[] buffer;
                switch (path)
                {
                    case "/":
                        buffer = Encoding.UTF8.GetBytes(GetHtmlContent());
                        response.ContentType = "text/html";
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        break;
                    case "/status":
                        buffer = Encoding.UTF8.GetBytes(GetCachedStatusJson());
                        response.ContentType = "application/json";
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        break;
                    case "/events":
                        await HandleSseConnection(response);
                        return; // stream stays open until the client disconnects
                    case "/preview/main":
                        await WritePreviewResponse(response, true);
                        break;
                    case "/preview/secondary":
                        await WritePreviewResponse(response, false);
                        break;
                    case "/brand_logo.png":
                        await WriteFileResponse(response, "brand_logo.png", "image/png");
                        break;
                    case "/upload":
                        await HandleFileUpload(request, response);
                        break;
                    case "/database/folders":
                        await HandleGetFolders(response);
                        break;
                    case "/database/gallery":
                        await HandleGetGallery(request, response);
                        break;
                    case "/database/file":
                        await HandleGetDatabaseFile(request, response);
                        break;
                    case "/database/current":
                        await HandleGetCurrentFile(response);
                        break;
                    default:
                        if (path.StartsWith("/action/"))
                        {
                            HandleAction(path, request, response);
                        }
                        else
                        {
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    byte[] buffer = Encoding.UTF8.GetBytes($"Server Error: {ex.Message}");
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                catch { }
            }
            finally
            {
                try { response.OutputStream.Close(); } catch { }
            }
        }

        private async Task HandleFileUpload(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.HttpMethod != "POST" || !request.ContentType.StartsWith("multipart/form-data"))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            try
            {
                string boundary = Regex.Match(request.ContentType, @"boundary=(.+)").Groups[1].Value;
                byte[] boundaryBytes = Encoding.UTF8.GetBytes("--" + boundary);

                long contentLen = request.ContentLength64;
                using (var ms = contentLen > 0 ? new MemoryStream((int)contentLen) : new MemoryStream())
                {
                    await request.InputStream.CopyToAsync(ms);
                    // D-rec-3: avoid a second full-body copy when the buffer is exactly sized.
                    byte[] bodyBytes = (contentLen > 0 && ms.GetBuffer().Length == ms.Length) ? ms.GetBuffer() : ms.ToArray();

                    int currentPos = 0;
                    string targetSubfolder = "";
                    string customName = "";
                    bool isDatabase = false;
                    byte[] fileData = null;
                    string originalFilename = "";

                    while (true)
                    {
                        int boundaryIndex = FindBytes(bodyBytes, currentPos, boundaryBytes);
                        if (boundaryIndex == -1) break;

                        int headersStartIndex = boundaryIndex + boundaryBytes.Length + 2;
                        if (headersStartIndex >= bodyBytes.Length) break;

                        int headersEndIndex = FindBytes(bodyBytes, headersStartIndex, new byte[] { 13, 10, 13, 10 });
                        if (headersEndIndex == -1) break;

                        string headers = Encoding.UTF8.GetString(bodyBytes, headersStartIndex, headersEndIndex - headersStartIndex);
                        int dataStartIndex = headersEndIndex + 4;
                        int nextBoundaryIndex = FindBytes(bodyBytes, dataStartIndex, boundaryBytes);
                        if (nextBoundaryIndex == -1) break;
                        int dataEndIndex = nextBoundaryIndex - 2;

                        Match nameMatch = Regex.Match(headers, @"name=""([^""]+)""");
                        if (nameMatch.Success)
                        {
                            string fieldName = nameMatch.Groups[1].Value;
                            if (fieldName == "subfolder")
                            {
                                targetSubfolder = Encoding.UTF8.GetString(bodyBytes, dataStartIndex, dataEndIndex - dataStartIndex).Trim();
                            }
                            else if (fieldName == "customName")
                            {
                                customName = Encoding.UTF8.GetString(bodyBytes, dataStartIndex, dataEndIndex - dataStartIndex).Trim();
                            }
                            else if (fieldName == "isDatabase")
                            {
                                string val = Encoding.UTF8.GetString(bodyBytes, dataStartIndex, dataEndIndex - dataStartIndex).Trim();
                                bool.TryParse(val, out isDatabase);
                            }
                            else if (fieldName == "file")
                            {
                                Match filenameMatch = Regex.Match(headers, @"filename=""([^""]+)""");
                                if (filenameMatch.Success) originalFilename = filenameMatch.Groups[1].Value;
                                int dataLength = dataEndIndex - dataStartIndex;
                                if (dataLength < 0) break; // malformed multipart: boundary too close
                                fileData = new byte[dataLength];
                                Array.Copy(bodyBytes, dataStartIndex, fileData, 0, fileData.Length);
                            }
                        }
                        currentPos = nextBoundaryIndex;
                    }

                    if (fileData == null) throw new Exception("No file data found");

                    // Sanitize custom filename to prevent path traversal via filename
                    string sanitizedCustomName = !string.IsNullOrEmpty(customName) ? Path.GetFileName(customName) : "";
                    string finalFilename = !string.IsNullOrEmpty(sanitizedCustomName) ? sanitizedCustomName + Path.GetExtension(originalFilename) : Path.GetFileName(originalFilename);
                    string targetDir = "";

                    string dbPath = "";
                    if (isDatabase)
                    {
                        if (string.IsNullOrEmpty(_mainForm.DatabaseFolderPath))
                        {
                            throw new Exception("Database folder not configured on host application.");
                        }

                        dbPath = Path.GetFullPath(_mainForm.DatabaseFolderPath);
                        string combinedPath = Path.GetFullPath(Path.Combine(dbPath, targetSubfolder));

                        // Ensure the target subfolder is within the database root
                        if (combinedPath.StartsWith(dbPath, StringComparison.OrdinalIgnoreCase))
                        {
                            targetDir = combinedPath;
                        }
                        else
                        {
                            throw new Exception("Invalid target subfolder path");
                        }
                    }

                    string savedRelativePath = "";

                    if (string.IsNullOrEmpty(targetDir))
                    {
                        string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                        File.WriteAllBytes(tempFilePath, fileData);
                        _mainForm.Invoke((Action)(() => _mainForm.ProcessUploadedFile(tempFilePath, originalFilename, updateStaging: false)));
                    }
                    else
                    {
                        Directory.CreateDirectory(targetDir);
                        string destPath = Path.Combine(targetDir, finalFilename);
                        if (File.Exists(destPath))
                        {
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(finalFilename);
                            string ext = Path.GetExtension(finalFilename);
                            int counter = 1;
                            while (File.Exists(destPath))
                            {
                                destPath = Path.Combine(targetDir, $"{nameWithoutExt}_{counter}{ext}");
                                counter++;
                            }
                        }
                        File.WriteAllBytes(destPath, fileData);
                        _mainForm.Invoke((Action)(() => _mainForm.ProcessNewImage(destPath, updateStaging: false)));
                        savedRelativePath = destPath.Substring(dbPath.Length).TrimStart(Path.DirectorySeparatorChar);
                    }

                    response.StatusCode = (int)HttpStatusCode.OK;
                    byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { message = "Upload successful", path = savedRelativePath }));
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                byte[] buffer = Encoding.UTF8.GetBytes("Upload failed: " + ex.Message);
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        private int FindBytes(byte[] array, int startIndex, byte[] pattern)
        {
            for (int i = startIndex; i <= array.Length - pattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (array[i + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found) return i;
            }
            return -1;
        }

        private async Task HandleGetFolders(HttpListenerResponse response)
        {
            var folders = _mainForm.GetDatabaseSubfolders();
            byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(folders));
            response.ContentType = "application/json";
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        private async Task HandleGetGallery(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                string subfolder = request.QueryString.Get("subfolder") ?? "";
                var files = _mainForm.GetDatabaseMediaFiles(subfolder);
                byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(files));
                response.ContentType = "application/json";
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { error = ex.Message }));
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        private async Task HandleGetDatabaseFile(HttpListenerRequest request, HttpListenerResponse response)
        {
            string relativePath = request.QueryString.Get("path");
            if (string.IsNullOrEmpty(relativePath) || string.IsNullOrEmpty(_mainForm.DatabaseFolderPath))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            try
            {
                string fullPath = Path.GetFullPath(Path.Combine(_mainForm.DatabaseFolderPath, relativePath));
                string dbPath = Path.GetFullPath(_mainForm.DatabaseFolderPath);
                if (!dbPath.EndsWith(Path.DirectorySeparatorChar.ToString())) dbPath += Path.DirectorySeparatorChar;

                if (File.Exists(fullPath) && fullPath.StartsWith(dbPath, StringComparison.OrdinalIgnoreCase))
                {
                    string ext = Path.GetExtension(fullPath).ToLowerInvariant();
                    string contentType = "application/octet-stream";
                    if (ext == ".jpg" || ext == ".jpeg") contentType = "image/jpeg";
                    else if (ext == ".png") contentType = "image/png";
                    else if (ext == ".gif") contentType = "image/gif";
                    else if (ext == ".bmp") contentType = "image/bmp";
                    else if (ext == ".pdf") contentType = "application/pdf";

                    byte[] buffer = File.ReadAllBytes(fullPath);
                    response.ContentType = contentType;
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            catch
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        private void HandleAction(string path, HttpListenerRequest request, HttpListenerResponse response)
        {
            string action = path.Substring("/action/".Length);

            if (action.StartsWith("open"))
            {
                var query = request.QueryString;
                string relativePath = query.Get("path");
                bool.TryParse(query.Get("edit"), out bool isEdit);
                if (!string.IsNullOrEmpty(relativePath))
                {
                    _mainForm.OpenMediaFile(relativePath, updateStaging: !isEdit);
                }
            }
            else if (action.StartsWith("auto-send"))
            {
                var query = request.QueryString;
                bool.TryParse(query.Get("enable"), out bool isEnabled);
                _mainForm.Invoke((Action)(() =>
                {
                    var chkLink = _mainForm.Controls.Find("chkLinkLocalPreviewToPresenter", true).FirstOrDefault() as CheckBox;
                    if (chkLink != null) chkLink.Checked = isEnabled;
                }));
            }
            else if (action.StartsWith("pdf-goto"))
            {
                var query = request.QueryString;
                if (int.TryParse(query.Get("page"), out int pageNum))
                {
                    _mainForm.Invoke((Action)(() => _mainForm.GoToPdfPage(pageNum)));
                }
            }
            else if (action.StartsWith("remote-crop"))
            {
                var query = request.QueryString;
                float.TryParse(query.Get("x"), out float x);
                float.TryParse(query.Get("y"), out float y);
                float.TryParse(query.Get("w"), out float w);
                float.TryParse(query.Get("h"), out float h);
                _mainForm.RemoteCrop(x, y, w, h);
            }
            else if (action.StartsWith("rotate"))
            {
                var query = request.QueryString;
                int.TryParse(query.Get("dir"), out int dir);
                _mainForm.Invoke((Action)(() => _mainForm.RotateContent(dir)));
            }
            else if (action.StartsWith("move-selection"))
            {
                var query = request.QueryString;
                int.TryParse(query.Get("x"), out int x);
                int.TryParse(query.Get("y"), out int y);
                bool.TryParse(query.Get("isShift"), out bool isShift);
                _mainForm.Invoke((Action)(() => _mainForm.MoveSelection(x, y, isShift)));
            }
            else if (action.StartsWith("update-settings"))
            {
                var query = request.QueryString;
                _mainForm.Invoke((Action)(() =>
                {
                    if (bool.TryParse(query.Get("skipOnePage"), out bool skipVal)) _mainForm.SkipOnePage = skipVal;
                    if (bool.TryParse(query.Get("twoPagePdf"), out bool twoPageVal)) _mainForm.TwoPagePdf = twoPageVal;
                    if (bool.TryParse(query.Get("enableScroll"), out bool scrollVal)) _mainForm.EnableAutoScroll = scrollVal;
                    if (bool.TryParse(query.Get("autoStage"), out bool stageVal)) _mainForm.AutoStagePreview = stageVal;
                    if (query.Get("moveStep") != null) _mainForm.MoveStepText = query.Get("moveStep");
                    _mainForm.SaveSettings();
                }));
            }
            else
            {
                _mainForm.Invoke((Action)(() =>
                {
                    switch (action)
                    {
                        case "stage": _mainForm.btnStageContent_Click(null, EventArgs.Empty); break;
                        case "push": _mainForm.btnPushToPresenter_Click(null, EventArgs.Empty); break;
                        case "blackout": _mainForm.btnClearPresenterDisplay_Click(null, EventArgs.Empty); break;
                        case "close": _mainForm.btnCloseLivePresenter_Click(null, EventArgs.Empty); break;
                        case "pdf-prev": _mainForm.PreviousPage(); break;
                        case "pdf-next": _mainForm.NextPage(); break;
                        case "clear-message": _mainForm.ClearMessage(); break;
                    }
                }));
            }
            response.StatusCode = (int)HttpStatusCode.OK;
        }

        private async Task HandleGetCurrentFile(HttpListenerResponse response)
        {
            string path = _mainForm.SelectedImagePath;
            if (string.IsNullOrEmpty(path))
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            // Serve the SAME image the dashboard preview uses (`picPreview.Image`) for BOTH
            // images and PDFs. The host normalizes selection coordinates against this exact
            // image, so the remote editor and the dashboard share one coordinate basis and
            // crop overlays never shift. Previously PDFs were served a separate 300-DPI render
            // ("full PDF canvas"), whose downscaled aspect did not exactly match the 150-DPI
            // preview the host normalizes against — causing the PDF-only gray/active overlay
            // shift. (The preview is downscaled for transport anyway, so there is no loss of
            // crop precision, and the presenter still renders the region at full resolution.)
            await WriteImageResponse(response, _mainForm.GetPreviewImage());
        }

        private async Task WriteFileResponseFull(HttpListenerResponse response, string filePath, string contentType)
        {
            if (File.Exists(filePath))
            {
                byte[] buffer = File.ReadAllBytes(filePath);
                response.ContentType = contentType;
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }

        private string GetStatusJson()
        {
            bool mainPreviewAvailable = false;
            bool secondaryPreviewAvailable = false;
            int pdfCurrentPage = -1;
            int pdfTotalPages = -1;
            double mainPreviewAspectRatio = 1.0;
            double secondaryPreviewAspectRatio = 1.0;
            string secondaryPreviewBorderColor = "transparent";
            bool autoSend = false;
            string message = null;
            string messageType = "info";
            string goLiveButtonText = "";
            bool goLiveButtonEnabled = false;
            string closeLiveButtonText = "";
            bool closeLiveButtonEnabled = false;
            string blackoutButtonText = "";
            bool blackoutButtonEnabled = false;
            bool pdfPrevButtonEnabled = false;
            bool pdfNextButtonEnabled = false;
            string currentFilePath = "";
            int currentPage = -1;
            RectangleF? currentSelectionNormalized = null;
            RectangleF? stagedSelectionNormalized = null;
            bool skipOnePage = false;
            bool twoPagePdf = false;
            bool enableScroll = false;
            bool autoStage = false;
            string moveStep = "50";
            bool editButtonEnabled = false;
            bool isBlackout = false;
            int selectionWidth = 0;
            int selectionHeight = 0;
            bool isLicenseExpired = false;
            long galleryVersion = 0;

            _mainForm.Invoke((Action)(() =>
            {
                Image mainImg = _mainForm.GetPreviewImage();
                Image secondaryImg = _mainForm.GetSecondaryPreviewImage();

                mainPreviewAvailable = mainImg != null;
                secondaryPreviewAvailable = secondaryImg != null;

                // Bump preview versions only when the Image object is actually replaced.
                if (!object.ReferenceEquals(mainImg, _lastMainPreviewRef)) { _mainPreviewVersion++; _lastMainPreviewRef = mainImg; }
                if (!object.ReferenceEquals(secondaryImg, _lastSecondaryPreviewRef)) { _secondaryPreviewVersion++; _lastSecondaryPreviewRef = secondaryImg; }

                pdfCurrentPage = _mainForm.GetCurrentPdfPage();
                pdfTotalPages = _mainForm.GetTotalPdfPages();

                if (mainImg != null && mainImg.Height > 0)
                    mainPreviewAspectRatio = (double)mainImg.Width / mainImg.Height;

                // D-rec-4: resolve these stable controls once, then reuse the references.
                if (!_ctlsResolved)
                {
                    _ctlPicSecondary = _mainForm.Controls.Find("picSecondaryPreview", true).FirstOrDefault() as PictureBox;
                    _ctlPanelSecondaryBorder = _mainForm.Controls.Find("panelSecondaryPreviewBorder", true).FirstOrDefault() as Panel;
                    _ctlChkLink = _mainForm.Controls.Find("chkLinkLocalPreviewToPresenter", true).FirstOrDefault() as CheckBox;
                    _ctlLblMessage = _mainForm.Controls.Find("lblMessage", true).FirstOrDefault() as Label;
                    _ctlBtnPush = _mainForm.Controls.Find("btnPushToPresenter", true).FirstOrDefault() as Button;
                    _ctlBtnClose = _mainForm.Controls.Find("btnCloseLivePresenter", true).FirstOrDefault() as Button;
                    _ctlBtnBlackout = _mainForm.Controls.Find("btnClearPresenterDisplay", true).FirstOrDefault() as Button;
                    _ctlsResolved = true;
                }

                if (_ctlPicSecondary != null && _ctlPicSecondary.Height > 0)
                    secondaryPreviewAspectRatio = (double)_ctlPicSecondary.Width / _ctlPicSecondary.Height;

                if (_ctlPanelSecondaryBorder != null) secondaryPreviewBorderColor = ColorTranslator.ToHtml(_ctlPanelSecondaryBorder.BackColor);

                if (_ctlChkLink != null) autoSend = _ctlChkLink.Checked;

                if (_ctlLblMessage != null && _ctlLblMessage.Visible)
                {
                    message = _ctlLblMessage.Text;
                    if (_ctlLblMessage.ForeColor == Color.Red) messageType = "error";
                    else if (_ctlLblMessage.ForeColor == Color.Orange) messageType = "warning";
                    else messageType = "info";
                }

                if (_ctlBtnPush != null) { goLiveButtonText = _ctlBtnPush.Text; goLiveButtonEnabled = _ctlBtnPush.Enabled; }
                if (_ctlBtnClose != null) { closeLiveButtonText = _ctlBtnClose.Text; closeLiveButtonEnabled = _ctlBtnClose.Enabled; }
                if (_ctlBtnBlackout != null) { blackoutButtonText = _ctlBtnBlackout.Text; blackoutButtonEnabled = _ctlBtnBlackout.Enabled; }

                pdfPrevButtonEnabled = _mainForm.IsPdfPrevButtonEnabled;
                pdfNextButtonEnabled = _mainForm.IsPdfNextButtonEnabled;
                currentFilePath = _mainForm.SelectedImagePath;
                currentPage = _mainForm.CurrentPageNumber;
                currentSelectionNormalized = _mainForm.GetCurrentSelectionNormalized();
                stagedSelectionNormalized = _mainForm.GetStagedSelectionNormalized();

                skipOnePage = _mainForm.SkipOnePage;
                twoPagePdf = _mainForm.TwoPagePdf;
                enableScroll = _mainForm.EnableAutoScroll;
                autoStage = _mainForm.AutoStagePreview;
                moveStep = _mainForm.MoveStepText;
                editButtonEnabled = !string.IsNullOrEmpty(_mainForm.SelectedImagePath);
                isBlackout = _mainForm.PresenterDisplayIsBlack;
                selectionWidth = _mainForm.SelectionWidth;
                selectionHeight = _mainForm.SelectionHeight;
                if ((DateTime.UtcNow - _licenseExpiredCheckTime).TotalSeconds > 30)
                {
                    _cachedIsLicenseExpired = new UsageManager().IsLicenseExpired();
                    _licenseExpiredCheckTime = DateTime.UtcNow;
                }
                isLicenseExpired = _cachedIsLicenseExpired;
                galleryVersion = _mainForm.GalleryVersion;
            }));

            var statusObject = new
            {
                mainPreview = mainPreviewAvailable ? $"/preview/main?v={_mainPreviewVersion}" : "",
                currentFilePath,
                currentPage,
                currentSelectionNormalized,
                stagedSelectionNormalized,
                secondaryPreview = secondaryPreviewAvailable ? $"/preview/secondary?v={_secondaryPreviewVersion}" : "",
                pdfCurrentPage,
                pdfTotalPages,
                mainPreviewAspectRatio,
                secondaryPreviewAspectRatio,
                secondaryPreviewBorderColor,
                autoSend,
                message,
                messageType,
                goLiveButtonText,
                goLiveButtonEnabled,
                closeLiveButtonText,
                closeLiveButtonEnabled,
                blackoutButtonText,
                blackoutButtonEnabled,
                pdfPrevButtonEnabled,
                pdfNextButtonEnabled,
                skipOnePage,
                twoPagePdf,
                enableScroll,
                autoStage,
                moveStep,
                editButtonEnabled,
                isBlackout,
                selectionWidth,
                selectionHeight,
                isLicenseExpired,
                galleryVersion
            };

            return JsonConvert.SerializeObject(statusObject);
        }

        // D-rec-1b: serve a recently-built status to rapid / concurrent callers without
        // re-marshalling to the UI thread for each one.
        private string GetCachedStatusJson()
        {
            lock (_statusCacheLock)
            {
                if (_cachedStatusJson != null && (DateTime.UtcNow - _cachedStatusJsonTime).TotalMilliseconds < 200)
                    return _cachedStatusJson;
            }
            string json = GetStatusJson();
            lock (_statusCacheLock)
            {
                _cachedStatusJson = json;
                _cachedStatusJsonTime = DateTime.UtcNow;
            }
            return json;
        }

        // W-rec-1: Server-Sent Events. One long-lived connection per client; pushes status
        // only when it actually changes (the JSON string differs). Heartbeats detect dead
        // sockets. The remote falls back to polling if this endpoint is unavailable.
        private async Task HandleSseConnection(HttpListenerResponse response)
        {
            response.ContentType = "text/event-stream";
            try { response.Headers.Add("Cache-Control", "no-cache"); } catch { }
            response.SendChunked = true;

            var token = _cts?.Token ?? CancellationToken.None;
            string last = null;
            int idleTicks = 0;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    string json = GetCachedStatusJson();
                    if (json != last)
                    {
                        last = json;
                        byte[] payload = Encoding.UTF8.GetBytes("data: " + json + "\n\n");
                        await response.OutputStream.WriteAsync(payload, 0, payload.Length, token);
                        await response.OutputStream.FlushAsync(token);
                        idleTicks = 0;
                    }
                    else if (++idleTicks >= 60) // ~15s keepalive comment
                    {
                        byte[] ka = Encoding.UTF8.GetBytes(": keepalive\n\n");
                        await response.OutputStream.WriteAsync(ka, 0, ka.Length, token);
                        await response.OutputStream.FlushAsync(token);
                        idleTicks = 0;
                    }
                    await Task.Delay(250, token);
                }
            }
            catch { /* client disconnected or server stopping — finally closes the stream */ }
        }

        // D-rec-1a: serve previews from a per-endpoint cache keyed by the source Image
        // reference, so concurrent / repeat requests for an unchanged preview reuse one
        // encode. Only a lightweight clone happens on the UI thread; scale + encode is
        // off-thread (see EncodePreview).
        private async Task WritePreviewResponse(HttpListenerResponse response, bool isMain)
        {
            PreviewCache cache = isMain ? _mainPreviewCache : _secondaryPreviewCache;

            Image refNow = null;
            Bitmap snapshot = null;
            bool cacheHit = false;
            _mainForm.Invoke((Action)(() =>
            {
                refNow = isMain ? _mainForm.GetPreviewImage() : _mainForm.GetSecondaryPreviewImage();
                if (refNow == null) return;
                lock (_previewCacheLock)
                {
                    if (object.ReferenceEquals(refNow, cache.SourceRef) && cache.Bytes != null) cacheHit = true;
                }
                if (!cacheHit)
                {
                    try { snapshot = new Bitmap(refNow); } catch { }
                }
            }));

            if (refNow == null) { response.StatusCode = (int)HttpStatusCode.NotFound; return; }

            byte[] bytes;
            string contentType;
            if (cacheHit)
            {
                lock (_previewCacheLock) { bytes = cache.Bytes; contentType = cache.ContentType; }
            }
            else
            {
                if (snapshot == null) { response.StatusCode = (int)HttpStatusCode.InternalServerError; return; }
                bytes = EncodePreview(snapshot, out contentType); // disposes snapshot
                if (bytes == null) { response.StatusCode = (int)HttpStatusCode.InternalServerError; return; }
                lock (_previewCacheLock) { cache.SourceRef = refNow; cache.Bytes = bytes; cache.ContentType = contentType; }
            }

            response.ContentType = contentType;
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        }

        // Off-UI-thread downscale + JPEG encode of a snapshot bitmap (which is disposed here).
        private byte[] EncodePreview(Bitmap snapshot, out string contentType)
        {
            contentType = "image/jpeg";
            const int maxRemoteDim = 1024;
            const long jpegQuality = 50;

            Bitmap bmp;
            try
            {
                if (snapshot.Width > maxRemoteDim || snapshot.Height > maxRemoteDim)
                {
                    float scale = (float)maxRemoteDim / Math.Max(snapshot.Width, snapshot.Height);
                    int newW = (int)(snapshot.Width * scale);
                    int newH = (int)(snapshot.Height * scale);
                    bmp = new Bitmap(newW, newH);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                        g.DrawImage(snapshot, 0, 0, newW, newH);
                    }
                    snapshot.Dispose();
                }
                else
                {
                    bmp = snapshot;
                }
            }
            catch { snapshot.Dispose(); return null; }

            try
            {
                using (var ms = new MemoryStream())
                {
                    ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                    if (jpegEncoder != null)
                    {
                        var ep = new EncoderParameters(1);
                        ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, jpegQuality);
                        bmp.Save(ms, jpegEncoder, ep);
                        contentType = "image/jpeg";
                    }
                    else
                    {
                        bmp.Save(ms, ImageFormat.Png);
                        contentType = "image/png";
                    }
                    return ms.ToArray();
                }
            }
            catch { return null; }
            finally { bmp.Dispose(); }
        }

        private async Task WriteImageResponse(HttpListenerResponse response, Image image)
        {
            if (image == null)
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            // Performance: Limit remote image size and quality
            const int maxRemoteDim = 1024;
            const long jpegQuality = 50;

            // Snapshot the image on the UI thread only (GDI+ images aren't thread-safe and
            // the source PictureBox.Image may be replaced/disposed mid-encode). The expensive
            // downscale + JPEG encode then run OFF the UI thread so they don't block the UI.
            Bitmap snapshot = null;
            _mainForm.Invoke((Action)(() =>
            {
                try { snapshot = new Bitmap(image); } catch { }
            }));

            if (snapshot == null) return;

            Bitmap bmpToProcess;
            try
            {
                if (snapshot.Width > maxRemoteDim || snapshot.Height > maxRemoteDim)
                {
                    float scale = (float)maxRemoteDim / Math.Max(snapshot.Width, snapshot.Height);
                    int newW = (int)(snapshot.Width * scale);
                    int newH = (int)(snapshot.Height * scale);
                    bmpToProcess = new Bitmap(newW, newH);
                    using (Graphics g = Graphics.FromImage(bmpToProcess))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                        g.DrawImage(snapshot, 0, 0, newW, newH);
                    }
                    snapshot.Dispose();
                }
                else
                {
                    bmpToProcess = snapshot;
                }
            }
            catch
            {
                snapshot.Dispose();
                return;
            }

            try
            {
                using (var ms = new MemoryStream())
                {
                    ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                    if (jpegEncoder != null)
                    {
                        System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                        System.Drawing.Imaging.EncoderParameter qualityParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, jpegQuality);
                        encoderParams.Param[0] = qualityParam;
                        bmpToProcess.Save(ms, jpegEncoder, encoderParams);
                        response.ContentType = "image/jpeg";
                    }
                    else
                    {
                        bmpToProcess.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        response.ContentType = "image/png";
                    }

                    byte[] buffer = ms.ToArray();
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing image response: {ex.Message}");
            }
            finally
            {
                bmpToProcess?.Dispose();
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private async Task WriteFileResponse(HttpListenerResponse response, string fileName, string contentType)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (File.Exists(filePath))
            {
                byte[] buffer = File.ReadAllBytes(filePath);
                response.ContentType = contentType;
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }

        private string GetHtmlContent()
        {
            string htmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "remote_control.html");
            return File.Exists(htmlFilePath) ? File.ReadAllText(htmlFilePath) : "<html><body><h2>Error: remote_control.html not found</h2></body></html>";
        }
    }
}