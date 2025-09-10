using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WindowsEventSample;

public class WinEventReceivedArgs : EventArgs
{
    public required uint EventId { get; init; }
    public required HWND Hwnd { get; init; }
    public required OBJECT_IDENTIFIER ObjectId { get; init; }
    public required int ChildId { get; init; }
    public required uint EventThreadId { get; init; }
    public required DateTimeOffset EventTime { get; init; }
}
