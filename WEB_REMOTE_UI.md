# Web Remote UI â€” Design Reference

Living reference for `remote_control.html`, the mobile browser remote served by
`HttpWebServer` on port **21011**. Keep this file in sync when the remote UI changes.

## Goals

- **Mobile-first.** The remote is used primarily from a phone held in one hand while
  presenting. Touch targets are â‰¥46px, the primary actions sit near the top, and
  secondary/advanced controls collapse out of the way.
- **Consistent with the desktop app.** Colors, typography, the splash, and the
  workflow ordering mirror the WinForms app's `LinearTheme` so the two surfaces feel
  like one product.

## How it's served (important)

`HttpWebServer.cs` reads the file from the **build output directory**:

```
AppDomain.CurrentDomain.BaseDirectory + "remote_control.html"   ->  bin\Debug\remote_control.html
```

The **source of truth is the repo-root `remote_control.html`**. The `.csproj` marks it
(and `brand_logo.png`) `CopyToOutputDirectory = PreserveNewest`, so a build refreshes the
served copy. When editing only the HTML without a rebuild, also copy the source over
`bin\Debug\remote_control.html` so a running instance serves the new version.

### Image routes

`HttpWebServer.cs` serves images by **explicit route only** (no generic static handler).
The brand logo is served at **`/brand_logo.png`** (repo-root `brand_logo.png`, a copy of
`Resources\DreamsLiveSolutions_Logo1.png` â€” the same asset the desktop app embeds). The
old `/splash.png` route and the `splash.png` / `Resources\splash_bg.png` assets were
removed. Adding a new web image = add a `case "/x.png"` route + a `<Content>` csproj
entry, then rebuild.

## Structure

The file is one self-contained document: `<style>` + markup + a large inline
`<script>`. The script owns all behavior (status polling, Cropper.js crop editor,
pan/zoom, uploads). **Do not rename element IDs or change the layout the crop editor
depends on** â€” the script is wired to specific IDs and to the preview boxes'
`outline`-based borders. The preview boxes are a **fixed 16:9 size** (CSS
`aspect-ratio`); selection overlays are positioned **letterbox-aware** via
`getContainRect()` / `placeNormalizedOverlay()` against the rendered image rect, so the
JS no longer mutates the box `aspect-ratio`.

### Layout order (top â†’ bottom) â€” mirrors the desktop workflow

1. **Splash** (`#spinner`) â€” auto-hides ~2s after load.
2. **App bar** (`.appbar`, sticky) â€” brand play-tile + "Dreams**Live** Solutions"
   wordmark, and a `#live-pill` that mirrors presenter state (READY / LIVE / BLACKOUT /
   OFFLINE).
3. **Previews** (`.preview-container`) â€” fixed **16:9** boxes (content scales with
   `object-fit: contain`, never resizing per image): **Local Preview** (left, tap to open
   the crop editor, with the staged + active selection overlays) and **Presenter View**
   (right, pinch/zoom) with a dashed-lavender **ratio overlay** (`#presenter-ratio-overlay`)
   framing the active presentation aspect (`secondaryPreviewAspectRatio`). Overlays are
   placed letterbox-aware so they stay accurate inside the fixed box.
4. **Primary actions** (`.actions`) â€” **Stage Preview â†’ Go Live** (lavender CTAs), each
   with its toggle (Auto Preview / Auto Send). Order matches desktop
   *Load â†’ Select â†’ Stage â†’ Push*.
5. **Contextual live controls** â€” Edit/Crop, Blackout, Close Live (shown by polling).
6. **PDF navigation** (`#dashboard-pdf-controls`) â€” shown only for PDFs.
7. **Load** â€” Choose File / Capture.
8. **Gallery** (`#gallery-section`) â€” database browser + subfolder select.
9. **Advanced** (`#scroll-controls-section`, `<details>`) â€” auto-scroll + nudge d-pad,
   collapsed by default.
10. **Footer** â€” Scroll Settings + Toggle Theme.
11. **Status bar** â€” System / Presenter / Connection.
12. **Modals** (upload-choice, settings, editor) + bottom toast. The **editor** footer is
    ordered topâ†’bottom: Present Now / Done / Close â†’ PDF nav â†’ rotate buttons â†’ rotation
    slider â†’ Live Sync / Auto Send â†’ **(pinned bottom)** Auto Scroll + D-pad + W/H. The
    desktop `EditContentForm` footer mirrors this (top band = actions / modifiers / rotate,
    bottom band = auto-scroll + d-pad).

## Design tokens

CSS custom properties mirror `LinearTheme.cs`. Light is the `:root` base; the app boots
into `.dark-mode` by default (the theme script adds it unless the user chose light).

| Token | Light | Dark |
|---|---|---|
| `--canvas` | `#fbfbfc` | `#08080a` |
| `--surface-1` | `#ffffff` | `#141518` |
| `--hairline` | `#e3e4e8` | `#23252a` |
| `--ink` | `#1c1d21` | `#f7f8f8` |
| `--ink-subtle` | `#6e727c` | `#8a8f98` |
| `--primary` (lavender) | `#5e6ad2` | `#5e6ad2` |
| `--primary-hover` | `#4c57c0` | `#828fff` |
| `--success` / `--danger` / `--warning` | `#1f9d3a` / `#c03a3a` / `#c77f0a` | `#27a644` / `#d25e5e` / `#e0a200` |

Legacy aliases used by the script and inline styles (`--bg-color`, `--text-color`,
`--status-ready/busy/error`, `--border-color`, `--overlay-staged`, â€¦) are defined as
references to the base tokens, so overriding only the base tokens in `.dark-mode` is
enough. **Lavender is scarce** â€” reserved for the Stage / Go Live CTAs.

## Splash & brand logo

The splash recreates the desktop `SplashForm`: `#08080a` canvas, a full-screen faint
dotted grid (`radial-gradient`, 26px), a soft lavender radial glow, the centered brand
logo (`/brand_logo.png` = `DreamsLiveSolutions_Logo1`), the **"Where Ideas Go Live."**
tagline drawn as live text (`.splash__tag`), and an **indeterminate lavender progress
bar** (`#5e6ad2 â†’ #828fff`) sweeping along the bottom edge.

The same logo image is used in the sticky **app bar** (`.brand__logo`) in place of the
former CSS play-tile + text wordmark, matching the desktop header. Note: like the
desktop app bar, the logo's white "Solutions" goes low-contrast on the near-white
**light** theme bar â€” the app defaults to dark, where it reads cleanly. (If light-theme
header legibility ever matters, put `.brand__logo` on a small dark plate.)

## Previewing locally (no C# host)

A static server is enough to check layout/splash/theme (the `/status`, `/database/*`
calls will 404 â†’ the UI shows *Offline*, which is expected):

```powershell
# from repo root
python -m http.server 8123
# then open http://localhost:8123/remote_control.html
```

`.claude/launch.json` defines a `remote-preview` server (python http.server :8123) for
the Claude Code preview tooling. The preview opens `index.html`, so to preview the
remote, temporarily copy `remote_control.html` â†’ `index.html` (and delete it after).

## Change log
- **2026-06-12** — Dashboard d-pad disabled-state fix: the Auto-scroll & nudge arrows now
  gray out (`button-disabled`, pointer-events off) whenever `status.enableScroll` is false,
  matching the host-side gate in `MoveSelection` that silently ignored the taps. The editor
  d-pad (local crop nudge) is intentionally not gated. Desktop match: `MainForm` now
  enables/disables its ↑←↓→ buttons from `chkEnableScroll.CheckedChanged`.

- **2026-06-11** â€” Desktop Edit/Crop â€” **wheel-zoom viewfinder bug fixed.** The wheel handler's
  header comment promised a fixed viewfinder (box stays put, document scales underneath), but its
  body called `UpdateCropRectFromNormalized()` â€” recomputing the box's screen rect from the
  document-relative selection â€” so the crop box scaled/moved *with* the document on every wheel
  tick (a leftover from the refactor below; the pan path and Zoom-to-Fit were already correct).
  Now the wheel handler calls `UpdateNormalizedFromCropRect()` instead: the box keeps its
  absolute screen position/size and only the **selection it frames** is re-derived. The crop box
  is now fully independent of the document's zoom level (still freely movable/resizable).
- **2026-06-11** â€” Desktop Edit/Crop â€” **wheel zoom now matches the web cropper exactly**
  (fixed-viewfinder model). The canvas (`picEdit`) draws the image itself with a `_viewZoom`
  + `_viewPan`. The **selection box stays a fixed rectangle on screen**; the wheel
  zooms/pans the **page underneath** it (cursor-anchored; zoom-out may go below fit, with
  margins), so the box never resizes when you scroll and **what gets selected changes** as
  you frame it (the selection is re-derived from what the box covers â€” `UpdateNormalizedFromCropRect`).
  Dragging anywhere except a handle **pans the page** (like Cropper's `dragMode: 'move'`);
  resize the box with its handles. **[Zoom to Fit]** resets the page to fit, leaving the box
  in place.
- **2026-06-11** â€” Desktop Edit/Crop window â€” full layout-parity refactor.
  Rebuilt `EditContentForm`'s footer with **nested layout panels** (TableLayoutPanel /
  FlowLayoutPanel) instead of absolute X/Y, so rows stay centered + anchored and the canvas
  expands fluidly on resize (`picEdit` Dock=Fill; `panelFooter` Dock=Bottom):
  1. **Centered primary actions row** directly under the canvas â€” **[Present Now]**
     (lavender / white), **[Done]** (green / white), **[Close]** (white / dark text+border),
     re-asserted after theming via `ApplyActionButtonStyles()`.
  2. **Full-width pagination row** â€” wide **[â€¹ Prev]** | centered **"x / y"** indicator |
     wide **[Next â€º]** â€” *new*; wired to the host (`PreviousPage`/`NextPage`/
     `GetCurrentPdfPage`/`GetTotalPdfPages`), shown only for PDFs and kept in sync by the
     editor's timer.
  3. **Advanced controls pinned at the very bottom** â€” rotate (â†º â†»), **Zoom to Fit**,
     Live Sync / Auto Send / Auto Scroll, and a 3Ã—3 **D-pad** â€” centered as one cluster.
  4. **Resizing/anchoring** â€” centered rows use `Anchor.None` in full-width
     `TableLayoutPanel` rows; the pager stretches (`Dock=Fill`); a `MinimumSize` prevents
     clipping. No overlap on resize.
- **2026-06-11** â€” PDF editor gray-box re-fix + desktop "Zoom to Fit":
  1. **Gray dotted reference box drift (PDF, edit view) â€” re-fixed (the real cause).**
     `updateEditorReferenceOverlay()` positioned the box from `cropper.getCanvasData()` â€”
     offsets relative to the cropper *container* â€” but the box is `position:absolute` inside
     `.modal-content`, where Cropper **centers** its container. That centering offset
     (vertical for portrait PDF pages) was never added, so the box drifted **down**. Now the
     box is anchored to the **actual rendered canvas via `getBoundingClientRect()`** and
     expressed in the overlay's own `offsetParent` space, so no container-centering or header
     spacing can leak into the X/Y. Verified with a live Cropper (portrait image, canvas
     centered at an offset): full-image **and** sub-region references overlay the canvas with
     **zero drift** (errX/Y/W/H = 0).
  2. **Desktop "Zoom to Fit" (Full Zoom Out port).** Added a **Zoom to Fit** button to the
     WinForms `EditContentForm` (`ZoomToFit()`) that resets the crop selection to the full,
     uncropped image/page â€” mirroring the web remote's full-zoom-out / snap-to-fit so the
     operator can view the complete page at a glance.
- **2026-06-11** â€” Three fixes (desktop parity + two bugs):
  1. **Desktop Edit/Crop parity** â€” `EditContentForm`'s footer was restructured into a
     topâ†’bottom vertical hierarchy that matches the web editor: **Present Now / Done / Close
     â†’ Rotate â†’ Live Sync / Auto Send â†’ (pinned bottom) Enable Auto Scroll + D-pad**.
  2. **PDF crop-overlay shift (PDF-only) â€” fixed.** Root cause: the remote editor was served a
     *separate* 300-DPI PDF render (`/database/current` â†’ `RenderCurrentPdfPage(300)`) and
     normalized the crop against it, while the dashboard preview **and the host's
     normalization** use the 150-DPI `picPreview.Image`. The two independent renders had
     non-identical rounded aspect ratios, so the editor's normalized crop mapped to a slightly
     different region on the host â†’ the gray/active selection overlays shifted. Images were
     unaffected because their edit and dashboard views already share one bitmap. **Fix:**
     `/database/current` now serves `GetPreviewImage()` (the same `picPreview.Image`) for PDFs
     too, so the edit view and the dashboard share **one coordinate basis** â€” exact mapping,
     no shift. Bonus: drops a redundant 300-DPI render per editor open; presenter output is
     unaffected (it still renders the region at full resolution).
  3. **`CopyableMessageBox` invisible button text â€” fixed.** `SetupButtons()` ran *after*
     `LinearTheme.Apply()`, so the dynamically-added buttons kept default colors (a light
     inherited `ForeColor` over a light visual-style button = unreadable in dark mode). **Fix:**
     apply the theme *after* the buttons exist so they get readable ink-on-surface text in
     both light and dark modes.
- **2026-06-11** â€” PDF page-switch freeze fix (multi-page navigate â†’ stage):
  - **Root cause:** switching to a page rendered it once for the preview
    (`RenderPdfPageToPreview`, 150 DPI), then staging a crop **re-rendered the same page**
    (`RenderContentToPictureBox`, `MainForm.Staging.cs`) â€” two synchronous Pdfium renders on
    the WinForms UI thread back-to-back (two-page mode rendered two pages at 600 DPI). No page
    cache existed, so revisiting a page re-rendered it every time. The PDF was *not* re-read
    from disk (`currentPdfDocument` is cached) â€” the freeze was the redundant render.
  - **Fix:** a UI-thread raw-page **LRU cache** (`GetRenderedPdfPage` in `MainForm.Pdf.cs`)
    shared by the preview and staging renders, so the stage right after a switch is a **cache
    hit** (no second render). **Adjacent pages are pre-rendered during idle**
    (`PrefetchPdfPage` via `BeginInvoke`) so the *next* navigation is instant. Cache is
    invalidated on PDF load/switch/close. Background (off-UI-thread) rendering was
    deliberately avoided â€” `PdfDocument` is not thread-safe and can't be validated without a
    live run (same hold as D-rec-2).
  - No web changes were needed: the fixed-box + letterbox-aware overlays already position
    against the box (not the image's load state), so there is no "compute coordinates before
    the bitmap finished" race on the dashboard; the editor gates crop restore on Cropper's
    `ready` event.
- **2026-06-11** â€” Performance pass 2 (full recommendation set; see `PERFORMANCE.md`):
  - **SSE push** â€” added `/events`; the remote uses `EventSource` and falls back to interval
    polling automatically if SSE is unavailable. Both pause when the page is hidden.
  - **Skip redundant DOM applies** â€” the remote no longer re-applies status when nothing
    changed (re-applies on resize so fixed-box overlays reflow).
  - **Cropper source downscale** â€” oversized editor images (> 2048 px) are downscaled for
    Cropper.js in host-edit mode only (upload output untouched; host re-renders full-res).
  - Server: ref-keyed encoded-preview cache + 200 ms status cache; resolve status controls
    once; single-buffer upload read; concurrent request handling; 1:1 exact-copy image load.
- **2026-06-11** â€” Performance pass (see `PERFORMANCE.md` for the full report):
  - **Version-based previews** â€” preview URLs are now `/preview/main?v={n}` (bumped only
    when the underlying `Image` is replaced); the remote reassigns `<img>.src` only when the
    version changes, so unchanged previews are no longer refetched/redecoded every poll.
  - **Polling pauses when the page is hidden** (Page Visibility API) and resumes on return.
  - `<img decoding="async">` on previews.
  - Server: `WriteImageResponse` downscale + encode moved **off the UI thread** (only a
    clone snapshot stays on it); `RunServer` handles requests **concurrently** (no longer
    awaits each one serially). Presenter output / staged master quality unchanged.
- **2026-06-10** â€” Fixed-size preview windows + editor layout:
  - Local Preview and Presenter View are now strict **fixed 16:9** boxes; the JS no longer
    mutates their `aspect-ratio`. Content scales with `object-fit: contain`.
  - Selection overlays moved to **letterbox-aware** positioning (`getContainRect` /
    `placeNormalizedOverlay`) so they stay aligned with the contained image rect.
  - Added the **presenter ratio overlay** (`#presenter-ratio-overlay`): a dashed-lavender
    frame reflecting the active presentation aspect within the fixed box.
  - Reordered the Edit/Crop footer â€” actions/PDF/rotate/rotation/sync on top, Auto Scroll
    + D-pad + W/H pinned at the bottom. Mirrored on the desktop `EditContentForm`
    (`panelFooter` split into top/bottom bands).
- **2026-06-09** â€” Live/preview polish (web + desktop parity):
  - Live pill now "breathes" smoothly (`@keyframes liveBreathe`, ease-in-out) to match
    the desktop `LiveIndicatorControl` sine pulse, and reads **"Presenter Live"**.
    Desktop: `LinearTheme.PaintLiveIndicator` text â†’ "Presenter Live"; `liveIndicator`
    control widened (94â†’160px) to fit.
  - Blackout button turns **bright green** (`#22c55e`) while blacked out (text
    "Restore Presenter"), via `.btn-restore` toggled on `status.isBlackout`. Desktop
    match: `Constants` blackout-active colors changed from red to the same green.
  - Fixed the staged/active selection overlays drifting to the wrong location:
    `.preview-container` now uses `align-items: flex-start` so each preview box keeps
    its own `aspect-ratio` instead of being stretched to equal height (which
    letterboxed the image under the `%`-positioned overlays).
  - Swapped the previews: **Local Preview** (tap-to-crop, selection overlays) is now on
    the left, **Presenter View** on the right.
- **2026-06-09** â€” Brand logo integration. Replaced the lavender play-tile composite
  splash with `DreamsLiveSolutions_Logo1` (served at `/brand_logo.png` via a new server
  route) on the splash and in the app bar; tagline now live text. Removed `splash.png`
  and the orphan `Resources\splash_bg.png`; gallery PDF thumbnail now uses an inline SVG
  document icon. Required C# (`HttpWebServer.cs` route) + `.csproj` changes and a rebuild.
- **2026-06-09** â€” Full mobile-first redesign. Single clean stylesheet (removed the
  duplicated/conflicting "modern UI 2024" layer), Linear design tokens, branded sticky
  app bar with live pill, **Stage â†’ Go Live** reorder to match desktop, collapsible
  advanced controls, dark-by-default, and a splash that mirrors the desktop `SplashForm`.
  All element IDs and the inline script were preserved unchanged.
