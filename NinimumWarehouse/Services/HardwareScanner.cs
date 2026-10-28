namespace NinimumWarehouse.Services;
// The PDA's built-in scanner (M5). Android hands over the code as a broadcast or as typed keys + Enter
// (Platforms/Android: ScannerReceiver, MainActivity). The open page listens; the newest listener gets the scan.
public static class HardwareScanner
{
    private static readonly List<Func<string, Task>> listeners = [];
    private static string last = "";
    private static DateTime lastAt;

    public static IDisposable Listen(Func<string, Task> onScan)
    {
        lock (listeners) listeners.Add(onScan);
        return new Subscription(onScan);
    }

    public static void Publish(string? raw)
    {
        var code = (raw ?? "").Trim().Trim('\0', '\r', '\n');
        if (code.Length == 0) return;
        var now = DateTime.UtcNow;
        // Some firmwares send the same scan as a broadcast and as keys.
        if (code == last && (now - lastAt).TotalMilliseconds < 500) return;
        last = code; lastAt = now;
        Func<string, Task>? handler;
        lock (listeners) handler = listeners.LastOrDefault();
        if (handler is null) return;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try { await handler(code); }
            catch (Exception ex) { Console.WriteLine($"[HardwareScanner] {ex}"); }
        });
    }

    private sealed class Subscription(Func<string, Task> handler) : IDisposable
    {
        public void Dispose() { lock (listeners) listeners.Remove(handler); }
    }
}
