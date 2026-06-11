# User Manual

Welcome to the presentation app! This guide will walk you through everything you need to know to get up and running.

## Getting Started

### Installation

1. Copy the application folder (containing `DreamsLive_Solutions-PresenterApp1.2.exe`) to your PC.
2. Make sure the **.NET Framework 4.8** runtime is installed (it's included with current versions of Windows).
3. Run `DreamsLive_Solutions-PresenterApp1.2.exe`.
4. **First run only** — to let phones/tablets connect to the remote control, open
   Command Prompt **as Administrator** once and run:
   ```
   netsh http add urlacl url=http://*:21011/ user=Everyone
   ```
   If a device still can't connect, allow TCP port **21011** through Windows Firewall.

### The Main Window

The main window is your control center. Here's a quick look at what everything does:

*   **Main Preview:** This is where you'll see your images or PDF pages.
*   **Staging Preview:** This shows you exactly what will be sent to the presenter screen.
*   **Stage Content Button:** Click this to move your selected area to the Staging Preview.
*   **Push to Presenter Button:** This sends the content from the Staging Preview to the live screen.

## Core Functionality: The Presentation Workflow

### 1. Load Your Content

You can load an image or a PDF file in two ways:

*   Click the "Open File" button and choose a file from your computer.
*   Drag and drop a file directly onto the main preview area.

If you open a PDF, you'll see controls to navigate through the pages.

### 2. Select a Presentation Area

Once your image or PDF page is loaded, click and drag your mouse over the main preview to select the area you want to present. The selection box will automatically keep the same shape as your presentation screen, so you don't have to worry about a distorted image.

### 3. Stage Your Content

Click the "Stage Content" button. You'll see the area you selected appear in the Staging Preview. Now you can make fine-tuned adjustments:

*   **Pan:** Click and drag inside the Staging Preview to move the image around.
*   **Zoom:** Use your mouse wheel to zoom in and out.

The border of the Staging Preview will be green, which means it's ready to go.

### 4. Go Live!

When you're happy with how everything looks in the Staging Preview, click the "Push to Presenter" button. The content will now be showing on your secondary display. The border of the Staging Preview will turn red to let you know that it's live.

You can also use the "Blackout" button to temporarily show a black screen, or the "Close" button to stop presenting.

## Remote Control

You can control the presentation from your phone or tablet! Here's how:

1.  Make sure your computer and your remote device are connected to the same Wi-Fi network.
2.  Open a web browser on your remote device.
3.  Type in your computer's IP address, followed by `:21011`. For example: `http://192.168.1.10:21011`
4.  You'll see a mobile-friendly remote control interface in your browser. It shows two live preview windows — **Local Preview** (your source, tap it to crop) and **Presenter View** (what's on the presenter screen) — and lets you stage content, go live, navigate PDF pages, blackout/restore, and upload or capture files. A breathing **"Presenter Live"** badge appears at the top while you're live.

## Other Features

### Light and Dark Themes

You can switch between a light and a dark theme to match your preference using the theme toggle button. The web remote also has its own theme toggle and opens in dark mode by default.

### Always on Top

If you want the main window to always stay on top of other windows, you can enable the "Always on Top" feature in the settings.

## Licensing and Activation

The first few times you use the application, it will be in a trial mode. After that, you'll be prompted to enter a license key. You can enter your key in the activation form.

## Troubleshooting & FAQ

**Q: Why can't I connect to the remote control?**

**A:** Make sure your computer and remote device are on the same Wi-Fi network. Also, check that you have entered the correct IP address and port number.

**Q: Why is the image on the presenter screen cut off?**

**A:** The application automatically matches the aspect ratio of your selected display. If the image is still not fitting correctly, try re-selecting the region in the main preview.
