using NinimumWarehouse.Services;
namespace NinimumWarehouse.Components;
/// <summary>
/// Invisible text field that holds the focus on scan screens. PDA scanners in keyboard mode often type
/// only into a focused field, with or without Enter at the end; whatever lands here becomes one scan.
/// A text field the worker tapped keeps the focus, and no on-screen keyboard opens.
/// </summary>
public sealed class ScanCatcher : Entry
{
    private readonly IDispatcherTimer finish, keepFocus;
    public ScanCatcher()
    {
        Opacity = 0; WidthRequest = 1; HeightRequest = 1;
        HorizontalOptions = LayoutOptions.Start; VerticalOptions = LayoutOptions.Start;
        IsSpellCheckEnabled = false; IsTextPredictionEnabled = false;
        finish = Dispatcher.CreateTimer(); finish.Interval = TimeSpan.FromMilliseconds(200); finish.IsRepeating = false;
        finish.Tick += (_, _) => Flush();
        keepFocus = Dispatcher.CreateTimer(); keepFocus.Interval = TimeSpan.FromSeconds(1);
        keepFocus.Tick += (_, _) => Grab();
        TextChanged += (_, e) => { finish.Stop(); if (!string.IsNullOrEmpty(e.NewTextValue)) finish.Start(); };
        Completed += (_, _) => Flush();
    }
    public void Start() { Grab(); keepFocus.Start(); }
    public void Stop() { keepFocus.Stop(); finish.Stop(); }
    private void Flush()
    {
        finish.Stop();
        var code = (Text ?? "").Trim();
        Text = "";
        if (code.Length >= 3) HardwareScanner.Publish(code);
        Grab();
    }
    private void Grab()
    {
#if ANDROID
        if (Handler?.PlatformView is not Android.Widget.EditText field) return;
        field.ShowSoftInputOnFocus = false;
        // Native request: MAUI's Focus() would open the on-screen keyboard.
        if (Platform.CurrentActivity?.CurrentFocus is not Android.Widget.EditText) field.RequestFocus();
#endif
    }
}
