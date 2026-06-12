# UI test driver for PresenterApp button testing (dot-source this file).
$drvSrc = @'
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
public class UiDrv {
    [DllImport("user32.dll")] static extern bool EnumChildWindows(IntPtr hwnd, EnumProc cb, IntPtr lp);
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc cb, IntPtr lp);
    [DllImport("user32.dll", CharSet=CharSet.Auto)] static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll", CharSet=CharSet.Auto)] static extern int GetClassName(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll")] public static extern bool IsWindowEnabled(IntPtr h);
    [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h, uint m, IntPtr w, IntPtr l);
    [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr h, uint m, IntPtr w, IntPtr l);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    public struct RECT { public int L, T, R, B; }
    delegate bool EnumProc(IntPtr h, IntPtr lp);

    public static IntPtr FindTop(uint pid, string title) {
        IntPtr found = IntPtr.Zero;
        EnumWindows((h, lp) => {
            uint p; GetWindowThreadProcessId(h, out p);
            if (p == pid && IsWindowVisible(h)) {
                var t = new StringBuilder(512); GetWindowText(h, t, 512);
                if (t.ToString().StartsWith(title)) { found = h; return false; }
            }
            return true;
        }, IntPtr.Zero);
        return found;
    }
    public static List<string> TopWindows(uint pid) {
        var r = new List<string>();
        EnumWindows((h, lp) => {
            uint p; GetWindowThreadProcessId(h, out p);
            if (p == pid && IsWindowVisible(h)) {
                var t = new StringBuilder(512); GetWindowText(h, t, 512);
                r.Add(h.ToInt64() + "|" + t);
            }
            return true;
        }, IntPtr.Zero);
        return r;
    }
    public static IntPtr FindChildByText(IntPtr parent, string text) {
        IntPtr found = IntPtr.Zero;
        EnumChildWindows(parent, (h, lp) => {
            var t = new StringBuilder(512); GetWindowText(h, t, 512);
            if (t.ToString() == text) { found = h; return false; }
            return true;
        }, IntPtr.Zero);
        return found;
    }
    public static List<string> Children(IntPtr parent) {
        var r = new List<string>();
        EnumChildWindows(parent, (h, lp) => {
            var t = new StringBuilder(512); GetWindowText(h, t, 512);
            var c = new StringBuilder(256); GetClassName(h, c, 256);
            r.Add(h.ToInt64() + "|" + c + "|" + t + "|vis=" + IsWindowVisible(h) + "|en=" + IsWindowEnabled(h));
            return true;
        }, IntPtr.Zero);
        return r;
    }
}
'@
if (-not ([System.Management.Automation.PSTypeName]'UiDrv').Type) { Add-Type -TypeDefinition $drvSrc }

function Get-App { Get-Process "DreamsLive_Solutions-PresenterApp1.2" -ErrorAction SilentlyContinue }

function Click-Button([string]$text, [IntPtr]$parent = [IntPtr]::Zero) {
    $proc = Get-App
    if ($parent -eq [IntPtr]::Zero) { $parent = $proc.MainWindowHandle }
    $h = [UiDrv]::FindChildByText($parent, $text)
    if ($h -eq [IntPtr]::Zero) { return "NOTFOUND" }
    if (-not [UiDrv]::IsWindowEnabled($h)) { return "DISABLED" }
    [UiDrv]::PostMessage($h, 0x00F5, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null  # BM_CLICK
    return "CLICKED"
}

function Get-AppStatus { Invoke-RestMethod "http://localhost:21011/status" }

function Save-WindowShot([IntPtr]$hwnd, [string]$path) {
    $r = New-Object UiDrv+RECT
    [UiDrv]::GetWindowRect($hwnd, [ref]$r) | Out-Null
    Add-Type -AssemblyName System.Drawing
    $bmp = New-Object System.Drawing.Bitmap(($r.R - $r.L), ($r.B - $r.T))
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($r.L, $r.T, 0, 0, $bmp.Size)
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
}
