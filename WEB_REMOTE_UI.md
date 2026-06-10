# Web Remote UI — Design Reference

Living reference for `remote_control.html`, the mobile browser remote served by
`HttpWebServer` on port **21011**. Keep this file in sync when the remote UI changes.

## Goals

- **Mobile-first.** The remote is used primarily from a phone held in one hand while
  presenting. Touch targets are ≥46px, the primary actions sit near the top, and
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
`Resources\DreamsLiveSolutions_Logo1.png` — the same asset the desktop app embeds). The
old `/splash.png` route and the `splash.png` / `Resources\splash_bg.png` assets were
removed. Adding a new web image = add a `case "/x.png"` route + a `<Content>` csproj
entry, then rebuild.

## Structure

The file is one self-contained document: `<style>` + markup + a large inline
`<script>`. The script owns all behavior (status polling, Cropper.js crop editor,
pan/zoom, uploads). **Do not rename element IDs or change the layout the crop editor
depends on** — the script is wired to specific IDs and to the preview boxes'
`outline`-based borders and JS-set `aspect-ratio`.

### Layout order (top → bottom) — mirrors the desktop workflow

1. **Splash** (`#spinner`) — auto-hides ~2s after load.
2. **App bar** (`.appbar`, sticky) — brand play-tile + "Dreams**Live** Solutions"
   wordmark, and a `#live-pill` that mirrors presenter state (READY / LIVE / BLACKOUT /
   OFFLINE).
3. **Previews** (`.preview-container`) — `Presenter` output (pinch/zoom) and `Source`
   (tap to open the crop editor) with the staged + active selection overlays.
4. **Primary actions** (`.actions`) — **Stage Preview → Go Live** (lavender CTAs), each
   with its toggle (Auto Preview / Auto Send). Order matches desktop
   *Load → Select → Stage → Push*.
5. **Contextual live controls** — Edit/Crop, Blackout, Close Live (shown by polling).
6. **PDF navigation** (`#dashboard-pdf-controls`) — shown only for PDFs.
7. **Load** — Choose File / Capture.
8. **Gallery** (`#gallery-section`) — database browser + subfolder select.
9. **Advanced** (`#scroll-controls-section`, `<details>`) — auto-scroll + nudge d-pad,
   collapsed by default.
10. **Footer** — Scroll Settings + Toggle Theme.
11. **Status bar** — System / Presenter / Connection.
12. **Modals** (upload-choice, settings, editor) + bottom toast.

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
`--status-ready/busy/error`, `--border-color`, `--overlay-staged`, …) are defined as
references to the base tokens, so overriding only the base tokens in `.dark-mode` is
enough. **Lavender is scarce** — reserved for the Stage / Go Live CTAs.

## Splash & brand logo

The splash recreates the desktop `SplashForm`: `#08080a` canvas, a full-screen faint
dotted grid (`radial-gradient`, 26px), a soft lavender radial glow, the centered brand
logo (`/brand_logo.png` = `DreamsLiveSolutions_Logo1`), the **"Where Ideas Go Live."**
tagline drawn as live text (`.splash__tag`), and an **indeterminate lavender progress
bar** (`#5e6ad2 → #828fff`) sweeping along the bottom edge.

The same logo image is used in the sticky **app bar** (`.brand__logo`) in place of the
former CSS play-tile + text wordmark, matching the desktop header. Note: like the
desktop app bar, the logo's white "Solutions" goes low-contrast on the near-white
**light** theme bar — the app defaults to dark, where it reads cleanly. (If light-theme
header legibility ever matters, put `.brand__logo` on a small dark plate.)

## Previewing locally (no C# host)

A static server is enough to check layout/splash/theme (the `/status`, `/database/*`
calls will 404 → the UI shows *Offline*, which is expected):

```powershell
# from repo root
python -m http.server 8123
# then open http://localhost:8123/remote_control.html
```

`.claude/launch.json` defines a `remote-preview` server (python http.server :8123) for
the Claude Code preview tooling. The preview opens `index.html`, so to preview the
remote, temporarily copy `remote_control.html` → `index.html` (and delete it after).

## Change log

- **2026-06-09** — Live/preview polish (web + desktop parity):
  - Live pill now "breathes" smoothly (`@keyframes liveBreathe`, ease-in-out) to match
    the desktop `LiveIndicatorControl` sine pulse, and reads **"Presenter Live"**.
    Desktop: `LinearTheme.PaintLiveIndicator` text → "Presenter Live"; `liveIndicator`
    control widened (94→160px) to fit.
  - Blackout button turns **bright green** (`#22c55e`) while blacked out (text
    "Restore Presenter"), via `.btn-restore` toggled on `status.isBlackout`. Desktop
    match: `Constants` blackout-active colors changed from red to the same green.
  - Fixed the staged/active selection overlays drifting to the wrong location:
    `.preview-container` now uses `align-items: flex-start` so each preview box keeps
    its own `aspect-ratio` instead of being stretched to equal height (which
    letterboxed the image under the `%`-positioned overlays).
  - Swapped the previews: **Local Preview** (tap-to-crop, selection overlays) is now on
    the left, **Presenter View** on the right.
- **2026-06-09** — Brand logo integration. Replaced the lavender play-tile composite
  splash with `DreamsLiveSolutions_Logo1` (served at `/brand_logo.png` via a new server
  route) on the splash and in the app bar; tagline now live text. Removed `splash.png`
  and the orphan `Resources\splash_bg.png`; gallery PDF thumbnail now uses an inline SVG
  document icon. Required C# (`HttpWebServer.cs` route) + `.csproj` changes and a rebuild.
- **2026-06-09** — Full mobile-first redesign. Single clean stylesheet (removed the
  duplicated/conflicting "modern UI 2024" layer), Linear design tokens, branded sticky
  app bar with live pill, **Stage → Go Live** reorder to match desktop, collapsible
  advanced controls, dark-by-default, and a splash that mirrors the desktop `SplashForm`.
  All element IDs and the inline script were preserved unchanged.
