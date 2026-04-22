# App Details for AI Development Agent

This document provides a comprehensive technical and functional overview of the **Dreams LIVE Solutions Presenter App**, intended for an AI agent tasked with improving the software or adding new features.

---

## 1. Overview and Purpose
The Presenter App is a robust .NET-based tool designed for professional presentations. It allows a user (operator) to load images and PDFs on a host PC, select specific regions of interest, and "push" those regions to a secondary display (projector/monitor). A key feature is the mobile-friendly web remote, which allows wireless control and file uploads from a smartphone or tablet.

## 2. Technology Stack
- **Host Application:** C# / .NET Framework 4.8 (Windows Forms).
- **PDF Engine:** `PdfiumViewer` (wrapper for Google's PDFium).
- **Web Server:** `HttpListener` (embedded inside the app).
- **Remote Interface:** HTML5, CSS3, Vanilla JavaScript.
- **Image Editing (Remote):** `Cropper.js` for touch-friendly cropping.
- **Communication:** JSON over HTTP (Polling-based status updates).
- **Security/Licensing:** BouncyCastle (RSA signatures), Machine Identifier (WMI/Registry).
- **Serialization:** `Newtonsoft.Json`.

## 3. High-Level Architecture
The application follows a **Host-Remote** architecture where the Windows application is the central hub and source of truth.

- **Host (MainForm):** Manages the UI, PDF rendering, file system watching, and the secondary presentation window.
- **Web Server (HttpWebServer):** Serves the remote control HTML and provides a REST-like API for status and actions.
- **Remote (remote_control.html):** A mobile-optimized dashboard that polls the host for status every second and sends commands back.
- **Presentation Window (PresentationForm):** A borderless, top-most form that occupies the entire secondary screen.

## 4. Key Components and File Descriptions

### Core Logic
- `MainForm.cs`: The central controller. Handles selection logic, coordinate mapping, PDF navigation, and coordinates between the web server and the presentation window.
- `HttpWebServer.cs`: Implements the embedded server. Contains logic for serving preview images, handling file uploads, and routing API actions.
- `Program.cs`: Handles startup, licensing checks, and the "Grace Period" logic.

### UI & Presentation
- `PresentationForm.cs`: The "Projector" view. Renders high-resolution content (especially for PDFs) based on instructions from the MainForm. Supports real-time annotations (Highlighter/Laser).
- `EditContentForm.cs`: A precise desktop-based cropping tool with aspect ratio locking.
- `GalleryForm.cs`: A non-modal file browser for the "Database" folder with thumbnail caching and subfolder support.
- `SnipForm.cs`: A regional screen capture tool that immediately loads captures into the presenter.

### Utility & Data
- `ImageUtils.cs`: Handles EXIF rotation correction, high-quality scaling, and "baking" rotations into bitmaps.
- `UsageManager.cs`: Tracks application usage and trial limits using encrypted local storage. Manages a 5-minute grace period after expiration.
- `SecureLicenseManager.cs`: Validates RSA-signed license keys against the unique Machine ID.
- `DatabaseFileInfo.cs`: Metadata wrapper for files in the repository.

## 5. Core Workflows

### The "Stage & Push" Pipeline
1. **Load:** File is loaded (Image or PDF).
2. **Select:** User draws a red rectangle on the `picPreview` (Host) or uses the `Cropper` (Remote).
3. **Normalize:** Coordinates are converted to a normalized `RectangleF` (0.0 to 1.0) relative to the source document.
4. **Stage:** Content is rendered into the `picSecondaryPreview`. This is the "on-deck" area.
5. **Go Live:** Staged content is sent to `PresentationForm`. The border turns red to indicate "Live".
6. **Live Sync / Auto Send:** If enabled, any change to the selection (panning, zooming, page turning) is immediately updated on the projector.

### PDF Stitching (Advanced Navigation)
When navigating between PDF pages (or columns in 2-page mode), the app can "stitch" the bottom of one page and the top of the next into a single continuous bitmap. This allows for seamless vertical scrolling transitions that aren't possible with standard PDF viewers.

### Remote File Upload
The remote can upload files directly to a specific "Database" subfolder. The host detects this via `FileSystemWatcher` or direct API callback and immediately makes the file available for presentation.

## 6. API Reference (Internal Web Server)

| Endpoint | Method | Description |
| :--- | :--- | :--- |
| `/status` | GET | Returns a massive JSON object with the current state (paths, selection coords, PDF page, etc.). |
| `/preview/main` | GET | Returns a JPEG of the current document page (with selection rectangle). |
| `/preview/secondary` | GET | Returns a JPEG of what is currently staged. |
| `/action/stage` | GET | Triggers the staging of the current selection. |
| `/action/push` | GET | Pushes staged content to the live presenter. |
| `/action/remote-crop`| GET | Sets the selection coordinates (x, y, w, h) from the remote. |
| `/upload` | POST | Multipart form upload for images/PDFs. Supports target subfolders and custom filenames. |

## 7. Key Nuances for Developers

- **Coordinate Mapping:** The app heavily uses normalized coordinates (`RectangleF`) to ensure that selections remain valid even if the preview window is resized or if the document is rendered at different DPIs.
- **Thread Safety:** All web server requests that modify the UI must be wrapped in `_mainForm.Invoke` to avoid cross-thread exceptions in WinForms.
- **Image Disposal:** To avoid memory leaks and GDI+ "Object in use" errors, the app uses a strict disposal pattern for bitmaps. `MainForm` and `PresentationForm` carefully manage bitmap lifetimes.
- **Gallery Refresh:** The gallery uses a `galleryVersion` counter incremented by a `FileSystemWatcher`. The remote polls this version and only refreshes its DOM when the version changes, preventing flickering.
- **Licensing:** The `MachineID` is a composite of hardware identifiers. Licenses are generated using a private key (Python tool) and verified using a hardcoded public key.

## 8. Ideas for Future Improvements
- **Real-time Sync:** Replace 1-second polling with WebSockets (SignalR) for zero-latency updates.
- **Remote Annotations:** Support for drawing/highlighting on the remote touch screen and having it appear live on the projector.
- **Multi-Monitor:** Support for different content on multiple projectors simultaneously.
- **Transitions:** Fade/Slide transitions between staged content and live content.
- **Cloud Gallery:** Integration with Google Drive or Dropbox for media assets.
- **Remote Preview Quality:** Adjustable preview quality in settings to handle poor Wi-Fi conditions.

---
**Note to Agent:** Always refer to `MainForm.cs` for the definitive state of the presentation logic, as it acts as the primary controller for all other modules.
