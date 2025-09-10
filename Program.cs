using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WindowsEventSample;

public static class Program
{
    public static async Task Main()
    {
        await using (var listener = new WinEventListener())
        {
            listener.WinEventReceived += (_, e) =>
            {
                if (
                    e.ObjectId != OBJECT_IDENTIFIER.OBJID_WINDOW
                    || e.ChildId != PInvoke.CHILDID_SELF
                )
                {
                    return;
                }

                var eventName = WindowHelper.GetEventName(e.EventId);
                var windowTitle = WindowHelper.GetTitleText(e.Hwnd);
                if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(windowTitle))
                {
                    return;
                }

                PInvoke.GetWindowRect(e.Hwnd, out var rect);
                var w = rect.right - rect.left;
                var h = rect.bottom - rect.top;
                if (w <= 0 || h <= 0)
                {
                    return;
                }

                var pid = WindowHelper.GetProcessId(e.Hwnd);
                var processName = WindowHelper.GetProcessName((int)pid);

                Console.WriteLine(
                    $"{e.EventTime:s} [{eventName, -20}] {pid}:{processName} {w}x{h} {windowTitle}"
                );
            };

            await listener.Ready;

            Console.WriteLine("SetWinEventHook OK, Try Click Other Window...");
            Console.ReadLine();
        }

        Console.WriteLine("Program Exited.");
    }
}
