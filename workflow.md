# Presenter App — Workflow Guide

## Operator Workflow (Normal Use)

### 1. Launch & Setup

1. Start the app (`DreamsLive_Solutions-PresenterApp1.2.exe`).
2. The splash screen appears, then `MainForm` loads and runs `LoadSettings()` to restore the database folder path and PDF options.
3. The embedded web server starts automatically on **port 21011**. The URL (e.g. `http://192.168.1.x:21011`) is shown in the status strip at the bottom.
4. Select the **target display** from the monitor dropdown (`cmbDisplays`). This determines the aspect ratio used for constrained region selection.

---

### 2. Load Content

| Method | How |
|---|---|
| **File browse** | Click the Browse button → select an image (PNG, JPG, BMP) or PDF |
| **Drag & drop** | Drag a file onto the main preview (`picPreview`) |
| **Gallery** | Open Gallery → double-click a file from the database folder |
| **Remote upload** | From a phone/tablet on the same network, open `http://<ip>:21011` and upload a file |
| **Snip** | Click Snip to capture a screen region directly into the preview |

For PDFs, page navigation controls (Prev / Next / page number box) appear automatically. Use them to navigate to the correct page.

---

### 3. Select a Region

- Click and drag on the **main preview** (`picPreview`) to draw a selection rectangle.
- The selection is automatically **constrained to the aspect ratio** of the chosen monitor, so the content will fill the screen without distortion.
- To move an existing selection, click inside it and drag.
- To select the entire image/page, clear the selection or use "Stage Content" with no selection drawn.
- Selections are **persisted per file path** in `%AppData%\DreamsLivePresenterApp\selections.json` and restored when the same file is re-opened.

---

### 4. Stage Content

Click **"Stage Content"** (`btnStageContent`).

- The selected region is rendered into the **Staging Preview** (`picSecondaryPreview`).
- The staging border turns **green** — the content is on-deck but not yet live.
- Interact with the staging preview:
  - **Pan**: click and drag inside it.
  - **Zoom**: mouse wheel.
  - **Laser pointer**: move mouse while laser mode is active.
  - **Highlighter**: draw over the staging preview in highlight color.

**Auto-stage:** Enable the "Auto Stage" checkbox to have staging update automatically whenever the selection changes (with a 250 ms debounce).

---

### 5. Push to Presenter (Go Live)

Click **"Push to Presenter"** (`btnPushToPresenter`).

- `PresentationForm` opens on the selected monitor (borderless, full-screen).
- The staging border turns **red** — the staged content is now live.
- `btnPushToPresenter` becomes disabled (content is already live; no need to push again).
- A pulsing **LIVE** indicator (`liveIndicator`) appears on the main form.

**Auto-link:** Enable "Link to Presenter" (`chkLinkLocalPreviewToPresenter`) to bypass the manual Push step — any new staging action immediately updates the live presenter.

---

### 6. During Presentation

| Action | Control |
|---|---|
| **Blackout / restore** | "Blackout" button (`btnClearPresenterDisplay`) — toggles between black screen and restoring last staged content |
| **Close presenter** | `btnCloseLivePresenter` — closes `PresentationForm`; staging border resets to gray |
| **Nudge content** | Up / Down / Left / Right arrow buttons shift the selection rectangle by the step value in `txtMoveStep` |
| **Rotate** | Rotation buttons apply 90° increments to the loaded content |
| **Two-page PDF** | Enable in Settings to stitch two consecutive PDF pages side-by-side |

---

### 7. Remote Control (Phone / Tablet)

1. On any device on the same Wi-Fi network, open a browser and go to the URL shown in the status strip (e.g. `http://192.168.1.10:21011`).
2. The remote page receives live state over **Server-Sent Events** (`/events`, pushed on change) with a 1-second `/status` polling fallback, and shows live thumbnails of both previews (re-fetched only when their version changes), current page info, and border color.
3. Buttons on the remote map to these server endpoints:

| Remote button | Endpoint | Effect |
|---|---|---|
| Stage | `/action/stage` | Same as clicking "Stage Content" |
| Push | `/action/push` | Same as clicking "Push to Presenter" |
| Next Page | `/action/pdf-next` | Next PDF page |
| Prev Page | `/action/pdf-prev` | Previous PDF page |
| Blackout | `/action/blackout` | Toggle blackout |
| Upload | `/upload` (POST) | Upload a file; it is opened as if loaded locally |

---

### 8. Gallery / Database

1. Click **Set Database Folder** to point the app at a folder of media files.
2. The folder is monitored by `FileSystemWatcher`; a version counter (`_galleryVersion`) increments on any file change so the remote and gallery stay in sync.
3. Open **Gallery** to browse files by subfolder. Double-clicking a file opens it in the main preview.
4. Use **Add to Database** to copy files into the database folder from other locations.

---

## Licensing Workflow

1. On first launch (or after the usage limit is reached), the app prompts for activation.
2. The user opens the **Activation** form and enters a license key.
3. `SecureLicenseManager` validates the key against the embedded RSA public key and the machine's unique ID (`MachineIdentifier`).
4. On success, the key is saved to `%LocalAppData%\DreamsLive_Solutions_PresenterApp1\license.key`.
5. **Generating a key** (admin/vendor only): use the Python scripts in `KeyGeneratorTool\` with the private key (not stored in the repo). See `KeyGeneratorTool\README.md`.

---

## Developer Build Workflow

```powershell
# 1. Restore NuGet packages
nuget restore DreamsLive_Solutions-PresenterApp1.sln

# 2. Build
msbuild DreamsLive_Solutions-PresenterApp1.sln /p:Configuration=Debug

# 3. (First time only) Reserve port 21011 for the web server — run as Administrator
netsh http add urlacl url=http://*:21011/ user=Everyone

# 4. Run
.\bin\Debug\DreamsLive_Solutions-PresenterApp1.2.exe
```

Settings and selections persist in `%AppData%\DreamsLivePresenterApp\` between runs.
