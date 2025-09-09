using System.Reflection;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace WindowsEventSample;

public static class WindowHelper
{
    private static readonly Dictionary<uint, string> WinEventNameMapping = new();

    static WindowHelper()
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
        foreach (var f in typeof(PInvoke).GetFields(flags))
        {
            if (!f.Name.StartsWith("EVENT_"))
            {
                continue;
            }

            var val = f.GetRawConstantValue();
            if (val != null)
            {
                WinEventNameMapping[(uint)val] = f.Name;
            }
        }
    }

    public static string? GetEventName(uint id)
    {
        return WinEventNameMapping.GetValueOrDefault(id);
    }

    public static string? GetTitleText(HWND hwnd)
    {
        if (hwnd == HWND.Null || !PInvoke.IsWindow(hwnd))
        {
            return null;
        }

        var len = PInvoke.GetWindowTextLength(hwnd);
        if (len <= 0)
        {
            return null;
        }

        Span<char> buf = stackalloc char[len + 1];
        var written = PInvoke.GetWindowText(hwnd, buf);
        return written > 0 ? new string(buf[..written]) : null;
    }
}
