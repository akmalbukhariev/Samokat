using System.Runtime.CompilerServices;

public static class ClickGuard
{
    // A per-view lightweight lock; auto-collected when view is GC'd.
    private static readonly ConditionalWeakTable<VisualElement, SemaphoreSlim> Locks = new();

    public static async Task RunAsync(VisualElement view, Func<Task> action, int cooldownMs = 200)
    {
        var gate = Locks.GetValue(view, _ => new SemaphoreSlim(1, 1));

        // If already running, ignore this tap/click
        if (!await gate.WaitAsync(0)) return;

        try
        {
            view.InputTransparent = true;   // ignore further touches on this view
            await action();
        }
        finally
        {
            await Task.Delay(cooldownMs);   // let OS tap queue drain
            view.InputTransparent = false;
            gate.Release();
        }
    }
}