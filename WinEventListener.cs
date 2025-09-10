using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WindowsEventSample;

public sealed class WinEventListener : IAsyncDisposable
{
    private readonly TaskCompletionSource _started;
    private readonly TaskCompletionSource _stopped;
    private uint _loopThreadId;

    public event EventHandler<WinEventReceivedArgs>? WinEventReceived;

    public WinEventListener()
    {
        _started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _stopped = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = Task.Run(() =>
        {
            _loopThreadId = PInvoke.GetCurrentThreadId();
            var hook = PInvoke.SetWinEventHook(
                PInvoke.EVENT_MIN,
                PInvoke.EVENT_MAX,
                HMODULE.Null,
                (_, eventId, hwnd, idObject, idChild, idEventThread, dwmsEventTime) =>
                {
                    var now = DateTimeOffset.Now;
                    var tick = Environment.TickCount & int.MaxValue;
                    var eventTime = now.AddMilliseconds(dwmsEventTime - tick);
                    WinEventReceived?.Invoke(
                        this,
                        new WinEventReceivedArgs
                        {
                            EventId = eventId,
                            Hwnd = hwnd,
                            IdObject = idObject,
                            IdChild = idChild,
                            IdEventThread = idEventThread,
                            EventTime = eventTime,
                        }
                    );
                },
                0,
                0,
                PInvoke.WINEVENT_OUTOFCONTEXT
            );

            PInvoke.PeekMessage(out _, HWND.Null, 0, 0, PEEK_MESSAGE_REMOVE_TYPE.PM_NOREMOVE);
            _started.SetResult();
            while (PInvoke.GetMessage(out var msg, HWND.Null, 0, 0))
            {
                PInvoke.DispatchMessage(msg);
            }

            PInvoke.UnhookWinEvent(hook);
            _stopped.SetResult();
        });
    }

    public Task Ready => _started.Task;
    public Task Completion => _stopped.Task;

    public async ValueTask DisposeAsync()
    {
        await Ready;

        if (_loopThreadId != 0U)
        {
            _loopThreadId = 0U;
            PInvoke.PostThreadMessage(_loopThreadId, PInvoke.WM_QUIT, default, default);
        }

        await Completion;
    }
}
