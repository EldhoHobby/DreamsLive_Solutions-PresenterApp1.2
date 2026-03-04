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
                    await ProcessRequest(context);
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
                        string statusJson = GetStatusJson();
                        buffer = Encoding.UTF8.GetBytes(statusJson);
                        response.ContentType = "application/json";
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        break;
                    case "/preview/main":
                        await WriteImageResponse(response, _mainForm.GetPreviewImage());
                        break;
                    case "/preview/secondary":
                        await WriteImageResponse(response, _mainForm.GetSecondaryPreviewImage());
                        break;
                    case "/splash.png":
                        await WriteFileResponse(response, "splash.png", "image/png");
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

                using (var ms = new MemoryStream())
                {
                    await request.InputStream.CopyToAsync(ms);
                    byte[] bodyBytes = ms.ToArray();

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
                                fileData = new byte[dataEndIndex - dataStartIndex];
                                Array.Copy(bodyBytes, dataStartIndex, fileData, 0, fileData.Length);
                            }
                        }
                        currentPos = nextBoundaryIndex;
                    }

                    if (fileData == null) throw new Exception("No file data found");

                    string finalFilename = !string.IsNullOrEmpty(customName) ? customName + Path.GetExtension(originalFilename) : originalFilename;
                    string targetDir = "";

                    if (isDatabase && !string.IsNullOrEmpty(_mainForm.DatabaseFolderPath))
                    {
                        targetDir = string.IsNullOrEmpty(targetSubfolder) ? _mainForm.DatabaseFolderPath : Path.Combine(_mainForm.DatabaseFolderPath, targetSubfolder);
                    }

                    if (string.IsNullOrEmpty(targetDir))
                    {
                        string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                        File.WriteAllBytes(tempFilePath, fileData);
                        _mainForm.Invoke((Action)(() => _mainForm.ProcessUploadedFile(tempFilePath, originalFilename)));
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
                        _mainForm.Invoke((Action)(() => _mainForm.ProcessNewImage(destPath)));
                    }

                    response.StatusCode = (int)HttpStatusCode.OK;
                    byte[] buffer = Encoding.UTF8.GetBytes("Upload successful");
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
                if (!string.IsNullOrEmpty(relativePath))
                {
                    _mainForm.OpenMediaFile(relativePath);
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

            string ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".pdf")
            {
                // For PDF, we render the current page
                using (var doc = PdfiumViewer.PdfDocument.Load(path))
                {
                    using (var img = doc.Render(_mainForm.CurrentPageNumber, 300, 300, true))
                    {
                        await WriteImageResponse(response, img);
                    }
                }
            }
            else
            {
                // For standard images, we serve the file
                string contentType = "image/jpeg";
                if (ext == ".png") contentType = "image/png";
                else if (ext == ".gif") contentType = "image/gif";
                else if (ext == ".bmp") contentType = "image/bmp";
                await WriteFileResponseFull(response, path, contentType);
            }
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

            _mainForm.Invoke((Action)(() =>
            {
                mainPreviewAvailable = _mainForm.GetPreviewImage() != null;
                secondaryPreviewAvailable = _mainForm.GetSecondaryPreviewImage() != null;
                pdfCurrentPage = _mainForm.GetCurrentPdfPage();
                pdfTotalPages = _mainForm.GetTotalPdfPages();

                var picPreview = _mainForm.Controls.Find("picPreview", true).FirstOrDefault() as PictureBox;
                if (picPreview != null && picPreview.Height > 0) mainPreviewAspectRatio = (double)picPreview.Width / picPreview.Height;

                var picSecondaryPreview = _mainForm.Controls.Find("picSecondaryPreview", true).FirstOrDefault() as PictureBox;
                if (picSecondaryPreview != null && picSecondaryPreview.Height > 0) secondaryPreviewAspectRatio = (double)picSecondaryPreview.Width / picSecondaryPreview.Height;

                var panelSecondaryPreviewBorder = _mainForm.Controls.Find("panelSecondaryPreviewBorder", true).FirstOrDefault() as Panel;
                if (panelSecondaryPreviewBorder != null) secondaryPreviewBorderColor = ColorTranslator.ToHtml(panelSecondaryPreviewBorder.BackColor);

                var chkLink = _mainForm.Controls.Find("chkLinkLocalPreviewToPresenter", true).FirstOrDefault() as CheckBox;
                if (chkLink != null) autoSend = chkLink.Checked;

                var lblMessage = _mainForm.Controls.Find("lblMessage", true).FirstOrDefault() as Label;
                if (lblMessage != null && lblMessage.Visible)
                {
                    message = lblMessage.Text;
                    if (lblMessage.ForeColor == Color.Red) messageType = "error";
                    else if (lblMessage.ForeColor == Color.Orange) messageType = "warning";
                    else messageType = "info";
                }

                var btnPushToPresenter = _mainForm.Controls.Find("btnPushToPresenter", true).FirstOrDefault() as Button;
                if (btnPushToPresenter != null) { goLiveButtonText = btnPushToPresenter.Text; goLiveButtonEnabled = btnPushToPresenter.Enabled; }

                var btnCloseLivePresenter = _mainForm.Controls.Find("btnCloseLivePresenter", true).FirstOrDefault() as Button;
                if (btnCloseLivePresenter != null) { closeLiveButtonText = btnCloseLivePresenter.Text; closeLiveButtonEnabled = btnCloseLivePresenter.Enabled; }

                var btnClearPresenterDisplay = _mainForm.Controls.Find("btnClearPresenterDisplay", true).FirstOrDefault() as Button;
                if (btnClearPresenterDisplay != null) { blackoutButtonText = btnClearPresenterDisplay.Text; blackoutButtonEnabled = btnClearPresenterDisplay.Enabled; }

                pdfPrevButtonEnabled = _mainForm.IsPdfPrevButtonEnabled;
                pdfNextButtonEnabled = _mainForm.IsPdfNextButtonEnabled;
                currentFilePath = _mainForm.SelectedImagePath;
                currentPage = _mainForm.CurrentPageNumber;
            }));

            var statusObject = new
            {
                mainPreview = mainPreviewAvailable ? $"/preview/main?t={DateTime.UtcNow.Ticks}" : "",
                currentFilePath,
                currentPage,
                secondaryPreview = secondaryPreviewAvailable ? $"/preview/secondary?t={DateTime.UtcNow.Ticks}" : "",
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
                pdfNextButtonEnabled
            };

            return JsonConvert.SerializeObject(statusObject);
        }

        private async Task WriteImageResponse(HttpListenerResponse response, Image image)
        {
            if (image == null)
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            using (var ms = new MemoryStream())
            {
                using (var bmp = new Bitmap(image))
                {
                    bmp.Save(ms, ImageFormat.Png);
                }
                byte[] buffer = ms.ToArray();
                response.ContentType = "image/png";
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
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