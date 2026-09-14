using System.Runtime.CompilerServices;

public static class ClickGuard
{
    // A per-view lightweight lock; auto-collected when view is GC'd.
    private static readonly ConditionalWeakTable<VisualElement, SemaphoreSlim> Locks = new();

    public static async Task RunAsync(VisualElement view, Func<Task> action, int cooldownMs = 200, bool setInputTransparent = true)
    {
        var gate = Locks.GetValue(view, _ => new SemaphoreSlim(1, 1));

        // If already running, ignore this tap/click.
        if (!await gate.WaitAsync(0)) return;

        try
        {
            // Most controls can safely be made input-transparent while their action is
            // running. For an entire page (for example MyProfilePage), changing the
            // page's native input state during a Shell transition can cause a brief
            // dark/black frame on some Android devices. The semaphore itself is enough
            // to guard duplicate taps, so callers can disable this visual/input change.
            if (setInputTransparent)
                view.InputTransparent = true;

            await action();
        }
        finally
        {
            await Task.Delay(cooldownMs);   // let OS tap queue drain

            if (setInputTransparent)
                view.InputTransparent = false;

            gate.Release();
        }
    }
}