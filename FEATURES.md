# Application Features

This document provides a high-level overview of the application's features, intended for internal development purposes.

## Core Functionality: Presentation and Display

The core of the application is a sophisticated presentation pipeline designed to give the user precise control over what is shown on a secondary display.

1.  **Content Loading & Main Preview:** The user begins by loading an image or a PDF file into the main preview area. This can be done via a file browser or by dragging and dropping a file. If the content is a PDF, navigation controls (next page, previous page, and a textbox for direct page entry) become visible, allowing the user to select the correct page.

2.  **Region Selection:** Once a page or image is loaded, the user can click and drag with the mouse on the main preview to select a specific rectangular region. A key feature here is that this selection rectangle is automatically constrained to the aspect ratio of the target display monitor selected in the dropdown menu. This ensures that the selected content will perfectly fit the presentation screen without distortion.

3.  **Staging Preview:** After selecting a region (or leaving the whole image/page selected), the user clicks the "Stage Content" button. This action renders the selected content into the "Staging Preview" area. This preview acts as a final "on-deck" area. Here, the user gets an exact preview of what the audience will see.

4.  **Interactive Pan & Zoom:** The staged content is not static. The user can interact with the Staging Preview by using the mouse to pan (click and drag) and the mouse wheel to zoom. This allows for fine-grained adjustments to the final composition *after* the initial selection, ensuring the most important part of the content is perfectly framed.

5.  **Going Live:** When the user is satisfied with the staged content, they click "Push to Presenter". This sends the content to the `PresentationForm`, a separate, borderless window that is displayed on the chosen secondary monitor. At this point, the border of the Staging Preview turns from green (staged) to red (live), indicating that what is staged is exactly what is being shown. The user can also "blackout" the live display or close it entirely.

## Licensing and Activation

*   **Usage Tracking:** The application tracks the number of times it has been used and prompts for activation when a limit is reached.
*   **Machine ID:** A unique machine identifier is used to tie a license to a specific computer.
*   **License Generation:** A separate tool allows for the generation of permanent, time-limited, or usage-limited license keys.
*   **Activation:** The application has an activation form where users can enter their license key.

## Remote Control

The remote control functionality turns a second device (like a phone or tablet) into a wireless controller for the presentation.

1.  **Embedded Web Server:** The application runs a lightweight web server in the background on port `21011`. This server listens for requests on the local network.

2.  **Web-Based Interface:** The server hosts a file called `remote_control.html`. When a user navigates to the computer's IP address and port (e.g., `http://192.168.1.10:21011`) from another device on the same network, this HTML file is loaded, presenting a remote-control interface in their browser.

3.  **Real-Time Status Updates:** The remote interface continuously polls a `/status` API endpoint on the server. This endpoint returns a JSON object containing the real-time state of the application, including:
    *   Dynamically generated URLs to the main and staging preview images, allowing the remote to see what the operator sees.
    *   The current and total page numbers for loaded PDFs.
    *   The color of the staging preview's border, so the remote user knows if the content is live, staged, or empty.
    *   The status of the "auto-send" (link) feature.

4.  **Action Endpoints:** The remote interface has buttons that correspond to the main application's controls. Pressing a button on the remote sends a request to an `/action/...` endpoint. For example:
    *   `/action/stage` triggers the "Stage Content" action.
    *   `/action/push` triggers the "Push to Presenter" action.
    *   `/action/pdf-next` navigates to the next page of a PDF.

5.  **File Upload:** The remote interface includes a file upload feature. A user can select a file from their remote device, and it will be uploaded to the main application via a `/upload` endpoint. The main application then processes this file as if it had been opened locally, making it immediately available for presentation.

## Other Features

*   **Theme Switching:** The application supports both a light and a dark theme.
*   **Always on Top:** The main window can be set to always be on top of other windows.
*   **Error Handling:** The application includes error handling and displays messages to the user via a custom message box.
