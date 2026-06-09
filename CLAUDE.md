# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

This is a **Windows Forms (.NET Framework 4.8)** application. Use Visual Studio or MSBuild.

```powershell
# Build (Debug)
msbuild DreamsLive_Solutions-PresenterApp1.sln /p:Configuration=Debug

# Build (Release)
msbuild DreamsLive_Solutions-PresenterApp1.sln /p:Configuration=Release

# Restore NuGet packages first if needed
nuget restore DreamsLive_Solutions-PresenterApp1.sln

# Run the built executable
.\bin\Debug\DreamsLive_Solutions-PresenterApp1.2.exe
```

There are no automated tests in this project. Verification is done by running the app manually.

## Web Server Setup (required for remote control)

The app hosts an HTTP server on port 21011. On first run, the port may need to be reserved:

```powershell
# Run once as Administrator
netsh http add urlacl url=http://*:21011/ user=Everyone
```

## Architecture

`MainForm` is the central hub — it is split across multiple partial class files to keep concerns separated:

| File | Responsibility |
|---|---|
| `MainForm.cs` | Fields, constructor, lifecycle, `DisplayItem` inner class |
| `MainForm.Designer.cs` | Auto-generated WinForms layout |
| `MainForm.Preview.cs` | Main preview: mouse selection, drag-and-drop, aspect-ratio-constrained region drawing |
| `MainForm.Staging.cs` | Staging preview: pan/zoom on `picSecondaryPreview`, staging pipeline |
| `MainForm.Pdf.cs` | PDF loading, page navigation, PdfiumViewer integration |
| `MainForm.Presenter.cs` | Push-to-presenter, blackout, live state tracking, `PresentationForm` lifecycle |
| `MainForm.Annotations.cs` | Laser pointer and highlighter overlays on the staging preview |
| `MainForm.Remote.cs` | Delegates to `HttpWebServer`; connection info display |
| `MainForm.Persistence.cs` | `LoadSettings` / `SaveSettings` (JSON via Newtonsoft.Json) |
| `MainForm.Theme.cs` | `ApplyTheme()` — dark/light mode, Linear design palette |
| `MainForm.Dialogs.cs` | Opens modal forms: Settings, Gallery, EditContent, Snip |

### Key data flow

1. **Load** — user opens an image or PDF → `selectedImagePath` + optional `currentPdfDocument` set → `picPreview` displays it.
2. **Select** — mouse drag on `picPreview` draws `selectionRectangle` (constrained to the target monitor's aspect ratio).
3. **Stage** — `btnStageContent_Click` renders the selected region into a high-res `stagedMasterImage` (or `stagedStitchedImage` for two-page mode) → displayed in `picSecondaryPreview`. Staged state is stored in `stagedContentPath`, `stagedContentRegion`, etc.
4. **Push** — `btnPushToPresenter_Click` sends `stagedMasterImage` to `PresentationForm` (a borderless full-screen form on the chosen monitor). `isPresenterShowingLiveContent` tracks whether the presenter matches staged content; the staging border turns red when live.
5. **Remote** — `HttpWebServer` (port 21011) serves `remote_control.html`, a `/status` JSON endpoint (images, page info, border color, auto-send state), and `/action/*` endpoints that call back into `MainForm` methods. File uploads arrive via `/upload`.

### Supporting components

- **`PresentationForm`** — borderless window placed on the secondary display; receives a `Bitmap` and renders it.
- **`HttpWebServer`** — self-contained `HttpListener` running on a background `Task`; calls `MainForm` via `Invoke` for all UI updates.
- **`GalleryForm`** — image/document database browser; changes tracked by `FileSystemWatcher` and a version counter (`_galleryVersion`).
- **`ImageUtils`** — static helpers for rendering, cropping, and stitching bitmaps.
- **`SecureLicenseManager`** — RSA signature validation (BouncyCastle) against an embedded public key; license file stored in `%LocalAppData%\DreamsLive_Solutions_PresenterApp1\license.key`.
- **`UsageManager`** — tracks launch count; enforces a usage limit with a 5-minute grace period before forcing exit.
- **`MachineIdentifier`** — generates a stable machine ID used to bind licenses.
- **`LinearTheme`** / `Constants` — Linear.app-inspired color palette used by `ApplyTheme()`.

### License key generation

The `KeyGeneratorTool\` folder contains standalone Python scripts for generating license keys (requires the private key counterpart to the embedded public key). See `KeyGeneratorTool\README.md`.

## NuGet packages

| Package | Use |
|---|---|
| `PdfiumViewer 2.13` + native DLLs | PDF rendering |
| `Newtonsoft.Json 13.0.3` | Settings serialization, HTTP response bodies |
| `BouncyCastle.Cryptography 2.4.0` | RSA license signature verification |
