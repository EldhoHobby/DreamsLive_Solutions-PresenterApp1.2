import asyncio
from playwright.async_api import async_playwright

async def run():
    async with async_playwright() as p:
        browser = await p.chromium.launch()
        page = await browser.new_page(viewport={'width': 400, 'height': 800})

        # Load the page
        await page.goto('http://localhost:8000/remote_control.html')

        # Take screenshot of splash screen
        await page.screenshot(path='/home/jules/verification/v4_splash.png')

        # Wait for splash to disappear
        await page.wait_for_selector('#spinner', state='hidden')

        # Take a screenshot of the sticky header with new labels
        await page.screenshot(path='/home/jules/verification/v4_header_labels.png')

        # Trigger Upload Options Modal to see the new button
        await page.evaluate("document.getElementById('upload-choice-modal').style.display = 'flex'")
        await page.evaluate("document.getElementById('chkUploadToDatabase').checked = true; toggleDatabaseUpload()")
        await asyncio.sleep(1)
        await page.screenshot(path='/home/jules/verification/v4_upload_modal_new_folder_btn.png')

        # Trigger Editor Modal to check title
        await page.evaluate("openEditor(null, false)")
        await asyncio.sleep(1)
        await page.screenshot(path='/home/jules/verification/v4_editor_title.png')

        await browser.close()

if __name__ == '__main__':
    import os
    import subprocess
    import time

    server = subprocess.Popen(['python3', '-m', 'http.server', '8000'])
    time.sleep(2)
    try:
        asyncio.run(run())
    finally:
        server.terminate()
