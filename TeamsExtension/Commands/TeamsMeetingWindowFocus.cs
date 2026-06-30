using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TeamsExtension.Commands;

internal static class TeamsMeetingWindowFocus
{
    private const int WindowTextLength = 512;
    private const int ClassNameLength = 256;
    private const int RestoreWindow = 9;

    public static async Task ActivateAsync()
    {
        await Task.Delay(250).ConfigureAwait(false);

        try
        {
            var window = FindTeamsMeetingWindow();
            if (window != IntPtr.Zero)
            {
                ActivateWindow(window);
            }
        }
        catch
        {
            // Best-effort focus only; the Teams API command already toggled the share tray.
        }
    }

    private static IntPtr FindTeamsMeetingWindow()
    {
        var fullMeetingWindow = IntPtr.Zero;
        var meetingWindow = IntPtr.Zero;
        var compactMeetingWindow = IntPtr.Zero;

        EnumWindows((window, _) =>
        {
            if (!IsWindowVisible(window))
            {
                return true;
            }

            var className = GetClassNameText(window);
            if (!string.Equals(className, "TeamsWebView", StringComparison.Ordinal))
            {
                return true;
            }

            var title = GetWindowTitle(window);
            if (title.StartsWith("Microsoft Teams meeting", StringComparison.OrdinalIgnoreCase))
            {
                fullMeetingWindow = window;
                return true;
            }

            if (title.Contains("Microsoft Teams meeting", StringComparison.OrdinalIgnoreCase)
                && !title.Contains("compact view", StringComparison.OrdinalIgnoreCase))
            {
                meetingWindow = window;
                return true;
            }

            if (compactMeetingWindow == IntPtr.Zero
                && title.Contains("Microsoft Teams meeting", StringComparison.OrdinalIgnoreCase))
            {
                compactMeetingWindow = window;
            }

            return true;
        }, IntPtr.Zero);

        if (fullMeetingWindow != IntPtr.Zero)
        {
            return fullMeetingWindow;
        }

        if (meetingWindow != IntPtr.Zero)
        {
            return meetingWindow;
        }

        return compactMeetingWindow;
    }

    private static bool ActivateWindow(IntPtr window)
    {
        var currentThreadId = GetCurrentThreadId();
        var foregroundWindow = GetForegroundWindow();
        var foregroundThreadId = foregroundWindow == IntPtr.Zero
            ? 0
            : GetWindowThreadProcessId(foregroundWindow, out _);
        var targetThreadId = GetWindowThreadProcessId(window, out _);

        var attachedForegroundThread = false;
        var attachedTargetThread = false;

        try
        {
            if (foregroundThreadId != 0 && foregroundThreadId != currentThreadId)
            {
                attachedForegroundThread = AttachThreadInput(currentThreadId, foregroundThreadId, true);
            }

            if (targetThreadId != 0 && targetThreadId != currentThreadId)
            {
                attachedTargetThread = AttachThreadInput(currentThreadId, targetThreadId, true);
            }

            ShowWindow(window, RestoreWindow);
            BringWindowToTop(window);
            SetActiveWindow(window);
            SetFocus(window);

            if (SetForegroundWindow(window))
            {
                return true;
            }

            return false;
        }
        finally
        {
            if (attachedTargetThread)
            {
                AttachThreadInput(currentThreadId, targetThreadId, false);
            }

            if (attachedForegroundThread)
            {
                AttachThreadInput(currentThreadId, foregroundThreadId, false);
            }
        }
    }

    private static string GetWindowTitle(IntPtr window)
    {
        var text = new char[WindowTextLength];
        var length = GetWindowText(window, text, text.Length);
        return length > 0 ? new string(text, 0, length) : string.Empty;
    }

    private static string GetClassNameText(IntPtr window)
    {
        var text = new char[ClassNameLength];
        var length = GetClassName(window, text, text.Length);
        return length > 0 ? new string(text, 0, length) : string.Empty;
    }

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr extraData);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr window);

    [DllImport("user32.dll", EntryPoint = "GetWindowTextW", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr window, [Out] char[] text, int maxCount);

    [DllImport("user32.dll", EntryPoint = "GetClassNameW", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr window, [Out] char[] className, int maxCount);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr window, int commandShow);

    [DllImport("user32.dll")]
    private static extern bool BringWindowToTop(IntPtr window);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr window);

    [DllImport("user32.dll")]
    private static extern IntPtr SetActiveWindow(IntPtr window);

    [DllImport("user32.dll")]
    private static extern IntPtr SetFocus(IntPtr window);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool attach);

    private delegate bool EnumWindowsProc(IntPtr window, IntPtr extraData);
}
