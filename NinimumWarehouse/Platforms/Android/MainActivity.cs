using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using NinimumWarehouse.Services;

namespace NinimumWarehouse;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private ScannerReceiver? scannerReceiver;
    private readonly System.Text.StringBuilder scanKeys = new();
    private long lastScanKeyAt;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        AndroidX.AppCompat.App.AppCompatDelegate.DefaultNightMode =
            AndroidX.AppCompat.App.AppCompatDelegate.ModeNightNo;
        base.OnCreate(savedInstanceState);
        if (Window is not null)
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(35))
            {
                Window.SetStatusBarColor(Android.Graphics.Color.White);
                Window.SetNavigationBarColor(Android.Graphics.Color.White);
            }
            var controller = AndroidX.Core.View.WindowCompat.GetInsetsController(Window, Window.DecorView);
            if(controller is not null)
            {
                controller.AppearanceLightStatusBars = true;
                controller.AppearanceLightNavigationBars = true;
            }
        }
    }

    // Built-in scanner, broadcast mode: listen while the app is in front.
    protected override void OnResume()
    {
        base.OnResume();
        scannerReceiver ??= new ScannerReceiver();
        var filter = new IntentFilter();
        foreach (var action in ScannerReceiver.Actions) filter.AddAction(action);
        if (OperatingSystem.IsAndroidVersionAtLeast(33)) RegisterReceiver(scannerReceiver, filter, ReceiverFlags.Exported);
        else RegisterReceiver(scannerReceiver, filter);
    }

    protected override void OnPause()
    {
        if (scannerReceiver is not null) { try { UnregisterReceiver(scannerReceiver); } catch (Java.Lang.IllegalArgumentException) { } }
        base.OnPause();
    }

    // Built-in scanner, keyboard mode: fast keys ending with Enter while no text field is focused form one scan.
    // With a text field focused the keys simply type into it, as before.
    public override bool DispatchKeyEvent(KeyEvent? e)
    {
        if (e is not null && CurrentFocus is not Android.Widget.EditText && e.Action == KeyEventActions.Down)
        {
            if (e.KeyCode is Keycode.Enter or Keycode.NumpadEnter)
            {
                var code = scanKeys.ToString(); scanKeys.Clear();
                if (code.Length >= 3) { HardwareScanner.Publish(code); return true; }
            }
            else if (e.UnicodeChar > 31)
            {
                if (e.EventTime - lastScanKeyAt > 300) scanKeys.Clear();
                lastScanKeyAt = e.EventTime;
                scanKeys.Append((char)e.UnicodeChar);
                return true;
            }
        }
        return base.DispatchKeyEvent(e);
    }
}
