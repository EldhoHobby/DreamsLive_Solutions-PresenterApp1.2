# Dreams LIVE Solutions Presenter App

A robust .NET-based presentation tool that allows users to load, crop, and present images and PDFs on a secondary display, with full remote control capabilities via a mobile-friendly web interface.

## Repository Workflow Diagram

```mermaid
flowchart TD
    subgraph Host_PC [Host Application - Windows Forms]
        MF[MainForm - Central Logic]
        WS[HttpWebServer - Internal API]
        EF[EditContentForm - Precise Cropping]
        PF[PresentationForm - Secondary Display Output]
    end

    subgraph Remote_Device [Web Remote Control]
        RC[remote_control.html - Mobile Browser UI]
    end

    subgraph Data_Storage [Local Storage]
        DB[Database Folder - Media Repository]
        JS[selections.json / settings.json]
    end

    %% Workflow Connections
    RC -- "1. Load / Upload" --> WS
    WS -- "2. Process File" --> MF
    DB -- "3. Read Media" --> MF
    MF -- "4. Save State" --> JS

    MF -- "5. Interactive Selection" --> EF
    EF -- "6. Return Coordinates" --> MF

    RC -- "7. Remote Crop / Rotate" --> WS
    WS -- "8. Update UI Thread" --> MF

    MF -- "9. Stage Content" --> MF
    MF -- "10. Render to Secondary Display" --> PF

    WS -- "11. Live status (SSE push / polling fallback)" --> RC
    MF -- "12. Serve versioned previews" --> WS
```

## Workflow Description

### 1. Media Acquisition
- **Host Loading:** Users can browse for images/PDFs directly on the PC or drag and drop them onto the main preview.
- **Remote Upload:** Users can capture photos with a mobile camera or upload files via the web remote. All uploads are funneled through the `HttpWebServer` and optionally stored in the configured `Database Folder`.

### 2. Region Selection and Editing
- **Interactive Crop:** On the host PC, users can drag and resize a red selection rectangle over the source media.
- **Remote Editor:** The web remote features a specialized `Cropper.js` interface for intuitive touch-based selection.
- **Edit Modal:** A dedicated `EditContentForm` on the host allows for precise, aspect-ratio-locked cropping while maintaining cumulative manual rotations.

### 3. Staging and Pre-Visualization
- **Staging Preview:** Before going live, content is "staged" to a secondary preview area (gray dotted box on the source). This renders exactly what *would* appear on the projector if pushed.
- **High-Resolution Rendering:** For PDFs, the staging logic performs high-quality (300+ DPI) renders, ensuring the final output is crisp.

### 4. Presentation
- **Go Live:** Pushing content sends the staged region to the `PresentationForm`.
- **Live Sync:** If "Auto Send" is enabled, any movement or adjustment on the host or remote is instantly reflected on the secondary display.
- **Blackout:** A dedicated feature to immediately clear the secondary display without losing the current staging state.

### 5. Synchronization & State
- **Live Updates:** The remote stays in sync via **Server-Sent Events** (`/events`) — the host pushes status only when it changes — with a 1-second `/status` polling **fallback** if SSE is unavailable. Both pause while the remote's browser tab is hidden.
- **Efficient Previews:** Preview image URLs are **versioned** (`/preview/main?v={n}`), so the remote re-downloads a preview only when it actually changes; the server caches encoded previews and the status JSON to absorb multiple clients.
- **Thread Safety:** The `HttpWebServer` handles requests concurrently and uses `Invoke/BeginInvoke` to safely communicate with the Windows Forms UI thread.
- **Persistence:** User settings and image-specific selection regions are persisted in JSON files within the user's AppData directory.

> The web remote UI itself (mobile-first layout, fixed-size Local/Presenter preview windows, the presenter aspect-ratio overlay, theming, and asset/route details) is documented in **`WEB_REMOTE_UI.md`**; performance work is in **`PERFORMANCE.md`**.
