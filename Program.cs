namespace WindowsEventSample;

public static class Program
{
    public static async Task Main()
    {
        await using (var listener = new WinEventListener())
        {
            listener.WinEventReceived += (_, e) =>
            {
                var eventName = WindowHelper.GetEventName(e.EventId);
                var windowTitle = WindowHelper.GetTitleText(e.Hwnd);
                if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(windowTitle))
                {
                    return;
                }

                Console.WriteLine(
                    $"{e.EventTime:s} [{eventName, -27}] hwnd={e.Hwnd, -9:X} tid={e.IdEventThread, -6} {windowTitle}"
                );
            };

            await listener.Ready;

            Console.WriteLine("SetWinEventHook OK, Try Click Other Window...");
            Console.ReadLine();
        }

        Console.WriteLine("Program Exited.");
    }
}
