namespace NinimumDelivery.Services;

// The notification switch looks like the warehouse app's: red (#FF443B) when on, grey when off.
// Uses the switch's own colour settings, so the app's other colours stay as they are.
public static class RedSwitch
{
    private static readonly Color Red = Color.FromArgb("#FF443B");
    private static readonly Color RedTrack = Color.FromArgb("#80FF443B");
    private static readonly Color OffThumb = Color.FromArgb("#ECECEC");

    public static void Apply(Switch view)
    {
        view.OnColor = RedTrack;
        Paint(view);
        view.Toggled += (_, _) => Paint(view);
    }

    private static void Paint(Switch view) => view.ThumbColor = view.IsToggled ? Red : OffThumb;
}
