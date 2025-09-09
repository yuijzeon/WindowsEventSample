using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;

namespace WindowsEventSample;

public static class Program
{
    public static async Task Main()
    {
        var onWinEvent = new WINEVENTPROC(
            (_, eventId, hwnd, _, _, idEventThread, eventTime) =>
            {
                var eventName = WindowHelper.GetEventName(eventId);
                var windowTitle = WindowHelper.GetTitleText(hwnd);
                if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(windowTitle))
                {
                    return;
                }

                var tick = Environment.TickCount & int.MaxValue;
                var timeText = $"{DateTime.Now.AddMilliseconds(eventTime - tick):s}";
                Console.WriteLine(
                    $"{timeText} [{eventName, -27}] hwnd={hwnd, -9:X} tid={idEventThread, -6} {windowTitle}"
                );
            }
        );

        var loopThreadId = 0U;
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var stopped = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = Task.Run(() =>
        {
            loopThreadId = PInvoke.GetCurrentThreadId();
            var hook = PInvoke.SetWinEventHook(
                PInvoke.EVENT_MIN,
                PInvoke.EVENT_MAX,
                HMODULE.Null,
                onWinEvent,
                0,
                0,
                PInvoke.WINEVENT_OUTOFCONTEXT
            );

            started.SetResult();
            while (PInvoke.GetMessage(out var msg, HWND.Null, 0, 0))
            {
                PInvoke.DispatchMessage(msg);
            }

            PInvoke.UnhookWinEvent(hook);
            stopped.SetResult();
        });

        await started.Task;

        Console.WriteLine("SetWinEventHook OK, Try Click Other Window...");
        Console.ReadLine();

        PInvoke.PostThreadMessage(loopThreadId, PInvoke.WM_QUIT, default, default);
        await stopped.Task;

        Console.WriteLine("Program Exited.");
    }
}
