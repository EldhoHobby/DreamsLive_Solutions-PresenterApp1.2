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

                    int boundaryIndex = FindBytes(bodyBytes, 0, boundaryBytes);
                    if (boundaryIndex == -1) throw new Exception("Boundary not found");

                    int headersStartIndex = boundaryIndex + boundaryBytes.Length + 2;
                    int headersEndIndex = FindBytes(bodyBytes, headersStartIndex, new byte[] { 13, 10, 13, 10 });
                    if (headersEndIndex == -1) throw new Exception("Header terminator not found");

                    string headers = Encoding.UTF8.GetString(bodyBytes, headersStartIndex, headersEndIndex - headersStartIndex);
                    Match filenameMatch = Regex.Match(headers, @"filename=""(.+)""");
                    if (!filenameMatch.Success) throw new Exception("Filename not found in headers");
                    string filename = filenameMatch.Groups[1].Value;

                    int fileStartIndex = headersEndIndex + 4;
                    int fileEndIndex = FindBytes(bodyBytes, fileStartIndex, boundaryBytes);
                    if (fileEndIndex == -1) throw new Exception("End boundary not found");
                    fileEndIndex -= 2;

                    string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    using (var fs = new FileStream(tempFilePath, FileMode.Create))
                    {
                        fs.Write(bodyBytes, fileStartIndex, fileEndIndex - fileStartIndex);
                    }

                    _mainForm.Invoke((Action)(() => _mainForm.ProcessUploadedFile(tempFilePath, filename)));

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

        private void HandleAction(string path, HttpListenerRequest request, HttpListenerResponse response)
        {
            string action = path.Substring("/action/".Length);

            if (action.StartsWith("auto-send"))
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
            }));

            var statusObject = new
            {
                mainPreview = mainPreviewAvailable ? $"/preview/main?t={DateTime.UtcNow.Ticks}" : "",
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