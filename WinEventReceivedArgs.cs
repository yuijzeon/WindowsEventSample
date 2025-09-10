using Windows.Win32.Foundation;

namespace WindowsEventSample;

public class WinEventReceivedArgs : EventArgs
{
    public required uint EventId { get; init; }
    public required HWND Hwnd { get; init; }
    public required int IdObject { get; init; }
    public required int IdChild { get; init; }
    public required uint IdEventThread { get; init; }
    public required DateTimeOffset EventTime { get; init; }
}
