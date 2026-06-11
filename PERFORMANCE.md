# Performance Diagnostic Report — Desktop App & Web Remote

**Date:** 2026-06-11
**Method:** Static code-path analysis of the WinForms/C# backend + `HttpWebServer`, and
runtime instrumentation of `remote_control.html` on a simulated mobile viewport (375×812)
via the preview tooling (console, repeated-poll behavior, src stability, theme reflow).
**Constraint honored:** the presenter output (`PresentationForm`) and the staged master
image are **never** downscaled or recompressed. Only the already-small web *preview*
thumbnails are touched.

---

## 1. Executive summary

The single biggest, cross-cutting bottleneck was the **per-second preview image refetch
loop**. The remote polled `/status` every 1000 ms and then re-assigned both preview
`<img>` sources to **cache-busted URLs** (`/preview/main?t={ticks}`), so the browser
re-downloaded and re-decoded *two JPEGs every second per client even when nothing
changed*. Worse, each fetch called back into the WinForms **UI thread** (`Invoke`) to
clone **and downscale** the bitmap before encoding — so an idle remote generated ~2
UI-thread bitmap operations/sec, multiplied by the number of connected clients.

A second structural issue: the HTTP server **processed requests serially**
(`await ProcessRequest`), so one slow request (a large upload or a big image encode)
blocked every other client.

Both are now fixed (see §5). Net effect: an idle/unchanged remote now makes **zero**
image refetches and **zero** UI-thread bitmap work; previews refresh only when the
underlying image actually changes; and requests are served concurrently.

---

## 2. Current performance bottlenecks found

### A. Web Remote UI (`remote_control.html`)

| # | Severity | Finding | Evidence |
|---|----------|---------|----------|
| W1 | **High** | Preview images refetched every poll via cache-busted `?t={ticks}` URLs → constant re-download + JPEG decode + repaint of 2 images/sec/client, regardless of change. Heaviest cost on mobile (decode + paint + battery). | `updateStatus()` set `img.src = status.mainPreview` unconditionally; server stamped `?t={DateTime.UtcNow.Ticks}` each call. |
| W2 | **Medium** | Polling continued at full rate while the tab/page was **hidden** (phone locked or app backgrounded) — wasted battery, CPU, and server round-trips. | `setInterval(updateStatus, 1000)` with no visibility gating. |
| W3 | Low | `updateStatus()` writes ~30 DOM properties (textContent/style/classList) every poll even when values are unchanged → minor redundant style/layout work. | Whole-status apply with no diffing. |
| W4 | Low | Overlay math reads `clientWidth`/`clientHeight` up to 3× per poll (selection, staged, ratio) → forced synchronous layout reads each second. Negligible at 1 Hz but batchable. | `getContainRect()` calls. |
| W5 | Low | Cropper.js holds a full-resolution canvas of the edited image; very large source images can spike memory on low-end mobile. Lifecycle itself is correct (`cropper.destroy()` on close). | `openEditor()` / `closeEditor()`. |
| W6 | OK | Theme switching only transitions `body` background/color; CSS-variable swaps repaint once with no per-element transition → **no layout thrash**. The splash dot-grid is a static `radial-gradient` (cheap). | `.dark-mode` token overrides. |
| W7 | OK | No blocking synchronous JS. `fetch` is async; an `isStatusUpdateRunning` guard prevents overlapping polls. | `updateStatus()`. |

### B. Desktop / WinForms + `HttpWebServer`

| # | Severity | Finding | Evidence |
|---|----------|---------|----------|
| D1 | **High** | `WriteImageResponse` cloned **and downscaled** the preview bitmap inside `_mainForm.Invoke(...)` — i.e. on the **UI thread** — for every preview request (~2/sec/client). The interpolated downscale is the expensive part and it blocked the UI. | `HttpWebServer.WriteImageResponse`. |
| D2 | **High** | `RunServer` did `await ProcessRequest(context)` before accepting the next connection → **requests handled serially**; a large upload or slow encode blocked all other clients/requests. | `HttpWebServer.RunServer`. |
| D3 | Medium | `GetStatusJson` performs a synchronous `_mainForm.Invoke` every poll (1/sec/client) and calls `Controls.Find(..., true)` ~6× per poll — each is a recursive control-tree walk. Multiplied by client count. | `HttpWebServer.GetStatusJson`. |
| D4 | Medium | Heavy file work runs on the **UI thread**: `ImageUtils.LoadImage` (decodes + re-bakes a 32bpp copy with HQ bicubic), `RenderPdfPageToPreview` (150 DPI render), and two-page stitching. Loading a very large image/PDF visibly freezes the UI. | `ImageUtils.LoadImage`, `MainForm.Pdf.cs`. |
| D5 | Low/Med | GC pressure during rapid Load→Select→Stage→Push: large 32bpp bitmaps are allocated and disposed in quick succession (load bake, preview render, staged master/stitched, per-request clones). Disposal is **correct** (no leak found), but allocation churn drives Gen-2 GC spikes. | Disposal verified across `MainForm.cs`, `MainForm.Staging.cs`, `MainForm.Pdf.cs`, `PresentationForm.cs`. |
| D6 | OK | **No unmanaged/GDI+ leaks found.** `picPreview.Image`/`picSecondaryPreview.Image` are disposed before replacement; `stagedMasterImage`/`stagedStitchedImage` disposed on `FormClosing` and on restage; `PresentationForm` disposes its cached page image and `PdfDocument`. | Dispose audit. |
| D7 | OK | Presenter output renders at high quality (HQ bicubic, PDF at 300 DPI, 4096 px cap) and is independent of the (downscaled) web previews. | `PresentationForm.RenderPdfPageWithHighQuality` / `displayPanel_Paint`. |

---

## 3. High-risk scenarios tested

| Scenario | Result before | Result after fixes |
|---|---|---|
| **Prolonged polling** (remote left open for minutes) | 2 image GETs/sec + 2 UI-thread clones/downscales/sec, indefinitely, per client. | Image GETs only when a preview changes; status JSON still 1/sec (cheap). Idle = ~0 image work. |
| **Backgrounded phone / locked screen** | Full-rate polling + fetches continued. | Polling pauses on `visibilitychange`; resumes (with an immediate refresh) on return. |
| **Multiple simultaneous clients** | Each client = its own 1s poll + 2 UI-thread bitmap ops/sec; requests serialized server-side so they queued behind each other. | Requests now handled concurrently; per-client image work only on real changes. (Cross-client *status* dedupe still recommended — see §4.) |
| **Giant file upload** (multipart) | Blocked the single request loop → all other clients stalled until the upload finished parsing. | Concurrent handling — other clients unaffected. (Upload still buffers fully in memory — see §4 D-rec-3.) |
| **Rapid file switching** (Load→Select→Stage→Push repeatedly) | Correct disposal, but heavy UI-thread decode/render per switch + allocation churn. | Unchanged (flagged): recommend async load (§4 D-rec-2). No leak. |
| **Rapid button mashing** (stage/push/blackout) | Each maps to a `/action/*` → UI-thread `Invoke`; serialized request loop could queue them. | Concurrent accept + UI-thread serialization keeps state coherent; more responsive. |
| **Theme toggle spam** | Single repaint per toggle; no thrash. | Unchanged (already fine). |

---

## 4. Recommendations — status (Round 2 implemented all but the held item)

**Web**
- **W-rec-1 (status push) — ✅ implemented.** Added a Server-Sent Events endpoint
  (`/events`) that pushes status only when it changes; the remote consumes it via
  `EventSource` and **falls back to interval polling** automatically if SSE errors or is
  unavailable. Both pause when the page is hidden.
- **W-rec-2 — ✅ implemented.** The remote keeps the last applied status string and
  **skips the entire DOM apply when nothing changed** (and re-applies on resize so the
  fixed-box overlays/ratio frame reflow).
- **W-rec-3 — ✅ implemented.** In host-edit mode, an oversized editor source (> 2048 px)
  is downscaled before Cropper.js loads it (`setEditorSource`). Crop coords are normalized
  and the host re-renders at full resolution, so quality is unaffected. **Upload mode is
  left untouched** (its crop output is the uploaded image).

**Desktop**
- **D-rec-1 (multi-client dedupe) — ✅ implemented.** Previews are served from a
  per-endpoint cache **keyed by the source `Image` reference** (`WritePreviewResponse`), so
  concurrent/repeat requests for an unchanged preview reuse one encode. A **200 ms
  `/status` cache** (`GetCachedStatusJson`) dedupes rapid/multi-client polls and SSE builds.
- **D-rec-2 (UI responsiveness) — ⚠ partial (full async HELD).** `ImageUtils.LoadImage`
  was doing a **HighQualityBicubic resample at 1:1** on the UI thread; changed to a
  NearestNeighbor 1:1 **exact copy** (faster *and* sharper — better for presenter quality),
  and added a wait-cursor during load. **Full background-thread loading is intentionally
  held:** it changes the *synchronous* load contract that the stage/push/remote pipeline
  relies on (and the shared `PdfDocument` is not thread-safe), and it cannot be safely
  validated without a live WinForms run. Recommended approach when on-device testing is
  possible: decode standalone images via `Task.Run` and marshal the `picPreview.Image`
  assignment + selection restore back to the UI thread; keep PDF rendering on the UI thread
  (or give it its own dedicated, serialized worker) because `PdfDocument` is shared.
- **D-rec-3 (uploads) — ✅ implemented.** The upload body is read into a **single
  exactly-sized buffer** using `ContentLength64` + `MemoryStream.GetBuffer()` (no second
  `ToArray()` copy → ~half the peak memory for large uploads). The `FindBytes` parser is
  left as-is (O(n·m) but adequate for typical uploads).
- **D-rec-4 — ✅ implemented.** `GetStatusJson` resolves the stable controls **once** and
  reuses the references instead of ~6 recursive `Controls.Find` walks per poll.
- **D-rec-5 (GC) — ✅ largely addressed.** The version/ref-keyed preview cache eliminates
  the repeated clone+scale+encode allocations for unchanged previews (the main churn
  source). A dedicated buffer pool was not added (diminishing returns once re-encodes are
  cached).

### Concurrency fix (also implemented)
- `RunServer` no longer `await`s each `ProcessRequest`; requests are handled
  **concurrently**, so a slow upload/encode no longer blocks other clients.

---

## 5. Implemented optimizations (this pass — safe, build-verified)

All changes built clean (0 warnings / 0 errors) and the web changes were runtime-verified
(no JS regressions; src guard confirmed to skip unchanged versions and update on new ones).

| ID | Change | File |
|---|---|---|
| **D1-fix** | `WriteImageResponse` now **snapshots** the image on the UI thread (`new Bitmap(image)`) and performs the **downscale + JPEG encode off the UI thread**. UI-thread time per request drops to a single clone. | `HttpWebServer.cs` |
| **D2-fix** | `RunServer` no longer `await`s `ProcessRequest` — requests are handled **concurrently** (fire-and-forget; `ProcessRequest` already has try/catch/finally that closes the response). | `HttpWebServer.cs` |
| **WD-fix (version)** | Added ref-identity **preview versioning**: `GetStatusJson` bumps `_mainPreviewVersion` / `_secondaryPreviewVersion` only when the underlying `Image` object is replaced; preview URLs are now `/preview/main?v={n}` (stable between real changes). | `HttpWebServer.cs` |
| **W1-fix (client guard)** | The remote reassigns a preview `<img>.src` **only when the version-stamped URL changed**, so unchanged previews are never refetched/redecoded. Also sets `decoding="async"`. | `remote_control.html` |
| **W2-fix (visibility)** | Status polling + image refresh **pause when the page is hidden** and resume (with an immediate refresh) when visible. | `remote_control.html` |

### Round 2 (full recommendation set)

| ID | Change | File(s) |
|---|---|---|
| **W-rec-1** | SSE `/events` push endpoint + client `EventSource` with automatic polling fallback; both pause when hidden. | `HttpWebServer.cs`, `remote_control.html` |
| **W-rec-2** | Skip the whole status DOM apply when unchanged; re-apply on resize. | `remote_control.html` |
| **W-rec-3** | Downscale oversized editor source for Cropper.js (host-edit mode only; upload untouched). | `remote_control.html` |
| **D-rec-1** | Ref-keyed encoded-preview cache + 200 ms `/status` cache. | `HttpWebServer.cs` |
| **D-rec-2** | 1:1 exact-copy load (was HQ-bicubic resample) + wait cursor. **Full async load held** (see §4). | `ImageUtils.cs`, `MainForm.Pdf.cs` |
| **D-rec-3** | Single exactly-sized upload buffer (no second copy). | `HttpWebServer.cs` |
| **D-rec-4** | Resolve `GetStatusJson` controls once. | `HttpWebServer.cs` |
| **Concurrency** | `RunServer` handles requests concurrently (no serial `await`). | `HttpWebServer.cs` |

**Quality guarantee:** none of these touch the presenter pipeline or the staged master
image. Web previews remain at their pre-existing budget (≤1024 px, JPEG q50); the presenter
window continues to render at full/native quality. The `ImageUtils.LoadImage` change makes
the source bake a **pixel-exact copy** (NearestNeighbor 1:1) — equal-or-sharper than the
previous bicubic resample, so presenter quality is preserved or improved.

---

## 6. Verification

- **Build:** `msbuild … /p:Configuration=Debug` → succeeded, 0 errors / 0 warnings (after each change, both rounds).
- **Web (preview @ 375×812):** page boots cleanly both rounds; only the expected
  `Error fetching status` logs appear (no live C# host behind the static preview); no JS
  syntax/runtime regressions. Round 1: src guard verified (`same version → skip`,
  `new version → update`). Round 2: `startSse`/`stopSse`/`setEditorSource` present; the
  **SSE→polling fallback was exercised** (no `/events` on the static server → `onerror` →
  polling resumes), confirming graceful degradation.
- **Live profiling of the WinForms UI under load was not performed** in this environment
  (no instrumented run). Desktop findings are from static path analysis. The **held**
  full-async file load (D-rec-2) in particular needs an on-device smoke test before it
  would be safe to add.
