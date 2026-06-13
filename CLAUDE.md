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

`msbuild` is often not on `PATH`; resolve it from the VS install, e.g.
`& (vswhere -latest -find 'MSBuild\**\Bin\MSBuild.exe')`.

There are no automated tests — verify by building and running the app. The web remote's
**layout/JS** can be sanity-checked by serving the repo root with a static server
(`python -m http.server`) and opening `remote_control.html`, but live previews/status/SSE
require the running WinForms host (the page degrades to an offline state without it).

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
| `MainForm.Persistence.cs` | `LoadSettings` / `SaveSettings` and per-file selection regions — JSON in `%AppData%\DreamsLivePresenterApp\` (settings.json, selections.json) |
| `MainForm.Theme.cs` | `ApplyTheme()` — dark/light mode, Linear design palette |
| `MainForm.Dialogs.cs` | Opens modal forms: Settings, Gallery, EditContent, Snip |

### Key data flow

1. **Load** — user opens an image or PDF → `selectedImagePath` + optional `currentPdfDocument` set → `picPreview` displays it.
2. **Select** — mouse drag on `picPreview` draws `selectionRectangle` (constrained to the target monitor's aspect ratio).
3. **Stage** — `btnStageContent_Click` renders the selected region into a high-res `stagedMasterImage` (or `stagedStitchedImage` for two-page mode) → displayed in `picSecondaryPreview`. Staged state is stored in `stagedContentPath`, `stagedContentRegion`, etc.
4. **Push** — `btnPushToPresenter_Click` sends `stagedMasterImage` to `PresentationForm` (a borderless full-screen form on the chosen monitor). `isPresenterShowingLiveContent` tracks whether the presenter matches staged content; the staging border turns red when live.
5. **Remote** — `HttpWebServer` (port 21011) serves `remote_control.html` and the brand logo (`/brand_logo.png`). State reaches the remote via **Server-Sent Events** (`/events`, push-on-change) with a `/status` JSON polling fallback (images, page info, border color, auto-send state). Preview images are served from `/preview/main` and `/preview/secondary` with **versioned URLs** (`?v={n}`) so the remote only re-fetches a preview when it actually changes. `/action/*` endpoints call back into `MainForm`; file uploads arrive via `/upload`; the database browser uses `/database/*`.

### Threading & rendering invariants

These cut across many files; violating them causes UI freezes, crashes, or quality regressions:

- **All `MainForm`/UI access from the server goes through `_mainForm.Invoke(...)`.** `HttpWebServer`
  handles requests **concurrently** on background tasks (`RunServer` does not await each one).
- **GDI+ `Image`s are not thread-safe.** When serving previews, snapshot (clone) on the UI thread,
  then scale + JPEG-encode **off** the UI thread (`WritePreviewResponse` / `EncodePreview`). Encoded
  previews are cached keyed by the source `Image` reference; the `/status` JSON is cached ~200 ms.
- **PdfiumViewer `PdfDocument` is not thread-safe.** `currentPdfDocument` rendering is serialized on
  the UI thread. The shared page-render cache (`GetRenderedPdfPage` in `MainForm.Pdf.cs`, used by both
  the preview and staging renders, with idle adjacent prefetch via `BeginInvoke`) is **UI-thread
  only** — do not move PDF rendering to a background thread without serializing every render site
  behind one lock.
- **Image-quality invariant:** the presenter output (`PresentationForm`) and the staged master image
  must stay full/native quality. Only the **web preview thumbnails** are downscaled (≤1024 px, JPEG
  q50) — never the presenter pipeline.
- **Preview freshness:** the remote re-fetches a preview only when its **version** changes
  (`/preview/main?v={n}`); the version bumps when the underlying `picPreview.Image` /
  `picSecondaryPreview.Image` reference is replaced (computed in `GetStatusJson`).

### Security invariants

The remote server is **unauthenticated and LAN-open by design** (`http://*:21011/`); the local
network is the trust boundary. Keep these when touching the server or remote:

- **Every path/subfolder from a request must go through `MainForm.ResolveWithinDatabase`** before
  any file read/write. It returns the absolute path only if it stays inside the database root
  (rejects `..`, absolute paths, and sibling-prefix escapes). Never `Path.Combine(root, userInput)`
  and use it directly. Used by `/action/open`, `/database/file`, `/database/gallery`, `/upload`,
  and host-side *Add to Database*.
- **Uploads and served files are restricted to the media allowlist** (`AllowedMediaExtensions`:
  jpg/jpeg/png/gif/bmp/pdf); uploads are capped at `MaxUploadBytes` (100 MB) before buffering.
- **Don't add permissive CORS or echo exception text to clients.** The page is same-origin; error
  responses stay generic (log details locally). All responses carry `X-Content-Type-Options: nosniff`.
- **Don't put absolute host paths in `/status`** or other client-visible JSON — `currentFilePath`
  is the file name only.
- **Web UI: data from the host (filenames, folder names, messages) goes into the DOM via
  `textContent`, or `escapeHtml(...)` if it must build an `innerHTML` string.** Never interpolate
  host data straight into `innerHTML`.

### Web remote (`remote_control.html`)

One self-contained file (markup + CSS + a large inline `<script>` that owns all behavior: SSE/poll,
Cropper.js editor, pan/zoom, uploads). **Do not rename element IDs** — the inline script binds to
them. It is **served from the build output** (`bin\Debug\remote_control.html`, i.e.
`AppDomain.BaseDirectory`); the repo-root copy is the source and the build copies it there
(`PreserveNewest`). When iterating on the HTML without a full rebuild, also copy it into `bin\Debug\`.
See `WEB_REMOTE_UI.md` for layout order, design tokens, image routes, and the change log.

### Supporting components

- **`PresentationForm`** — borderless window placed on the secondary display; receives a `Bitmap` and renders it at full quality.
- **`HttpWebServer`** — self-contained `HttpListener` running on a background `Task`; processes requests concurrently and calls `MainForm` via `Invoke` for UI-thread work. Caches the status JSON (~200 ms) and encoded previews (keyed by the source `Image` reference); pushes status over SSE (`/events`).
- **`SplashForm`** — code-painted startup splash (dark dotted canvas, brand logo, "Where Ideas Go Live." tagline, animated lavender bar). The web splash mirrors it.
- **`LiveIndicatorControl`** — the "Presenter Live" status chip on the Program preview; steady gray when idle, smooth breathing red when live (paints via `LinearTheme.PaintLiveIndicator`).
- **`EditContentForm`** — host-side precise crop dialog (footer: actions/modifiers on top, auto-scroll + d-pad at the bottom — mirrors the web editor).
- **`GalleryForm`** — image/document database browser; changes tracked by `FileSystemWatcher` and a version counter (`_galleryVersion`).
- **`ImageUtils`** — static helpers for rendering, cropping, and stitching bitmaps.
- **`SecureLicenseManager`** — RSA signature validation (BouncyCastle) against an embedded public key; license file stored in `%LocalAppData%\DreamsLive_Solutions_PresenterApp1\license.key`.
- **`UsageManager`** — tracks launch count; enforces a usage limit with a 5-minute grace period before forcing exit.
- **`MachineIdentifier`** — generates a stable machine ID used to bind licenses.
- **`LinearTheme`** / `Constants` — Linear.app-inspired color palette used by `ApplyTheme()`.
  Also exposes window icons: `LinearTheme.BrandIcon` (the app's `DreamsLive-Logo.ico`, via the
  EXE's associated icon) and `LinearTheme.FormIcon("<purpose>")`, which builds a title-bar `Icon`
  from an embedded `Resources/icons/<purpose>.png` glyph (cached). Each pop-up sets `this.Icon`
  to its purpose glyph (settings/gallery/edit/adddb/help); the main window uses `BrandIcon`.

### Forms / layout conventions

- **Dialogs are resizable + adaptive.** All normal forms use `FormBorderStyle.Sizable` with a
  `MinimumSize` floor; controls use `Anchor`/`Dock`/`TableLayoutPanel`/`FlowLayoutPanel` so they
  reflow from the minimum up to full screen (inputs anchor `Top|Left|Right` to stretch, action
  buttons anchor `Bottom|Right`). When adding a control to a dialog, give it an anchor — don't
  leave it pinned top-left. **Exceptions (intentionally fixed/borderless):** `SplashForm`
  (startup splash), `PresentationForm` (borderless presenter output sized to the target monitor),
  and `SnipForm` (fullscreen capture overlay).

### License key generation

The `KeyGeneratorTool\` folder contains standalone Python scripts for generating license keys (requires the private key counterpart to the embedded public key). See `KeyGeneratorTool\README.md`.

## Documentation

- **`WEB_REMOTE_UI.md`** — design reference for the web remote (`remote_control.html`):
  layout order, Linear design tokens, splash/brand assets, image routes, and a change log.
  Update it when the remote UI changes.
- **`PERFORMANCE.md`** — performance diagnostic report and the optimizations applied
  (status/preview caching, SSE, concurrent requests, the PDF page-render cache, etc.).

## NuGet packages

| Package | Use |
|---|---|
| `PdfiumViewer 2.13` + native DLLs | PDF rendering |
| `Newtonsoft.Json 13.0.3` | Settings serialization, HTTP response bodies |
| `BouncyCastle.Cryptography 2.4.0` | RSA license signature verification |
