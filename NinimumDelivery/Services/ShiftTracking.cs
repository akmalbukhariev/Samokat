namespace NinimumDelivery.Services;

/// <summary>
/// Courier GPS V1b: background tracking runs while the courier is signed in and "Ishga tayyor" is on.
/// Started from the app (login, app start, the switch); Android keeps it running with a permanent notification.
/// </summary>
public static class ShiftTracking
{
    public static async Task Sync(bool shouldRun)
    {
#if ANDROID
        try
        {
            var context = Android.App.Application.Context;
            var intent = new Android.Content.Intent(context, typeof(NinimumDelivery.Platforms.Android.TrackingService));
            if (!shouldRun) { context.StopService(intent); return; }
            var status = await MainThread.InvokeOnMainThreadAsync(() => Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>());
            if (status != PermissionStatus.Granted)
                status = await MainThread.InvokeOnMainThreadAsync(() => Permissions.RequestAsync<Permissions.LocationWhenInUse>());
            if (status != PermissionStatus.Granted) return;   // nothing recorded; web admin shows the courier without a position
            if (OperatingSystem.IsAndroidVersionAtLeast(26)) context.StartForegroundService(intent);
            else context.StartService(intent);
            AskBatteryExemption(context);
        }
        catch (Exception ex) { Console.WriteLine($"ShiftTracking: {ex.Message}"); }
#else
        await Task.CompletedTask;
#endif
    }

#if ANDROID
    /// <summary>Asks once per app start to let tracking run without battery limits (phones otherwise stop it in the background).</summary>
    static bool askedBattery;
    static void AskBatteryExemption(Android.Content.Context context)
    {
        if (askedBattery || !OperatingSystem.IsAndroidVersionAtLeast(23)) return;
        askedBattery = true;
        try
        {
            var power = (Android.OS.PowerManager?)context.GetSystemService(Android.Content.Context.PowerService);
            if (power == null || power.IsIgnoringBatteryOptimizations(context.PackageName)) return;
            var intent = new Android.Content.Intent(Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations, Android.Net.Uri.Parse("package:" + context.PackageName));
            intent.AddFlags(Android.Content.ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
        catch (Exception ex) { Console.WriteLine($"Battery exemption: {ex.Message}"); }
    }
#endif
}
