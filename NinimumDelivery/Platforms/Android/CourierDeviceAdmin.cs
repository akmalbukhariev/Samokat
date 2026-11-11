using Android.App;
using Android.App.Admin;
using Android.Content;

namespace NinimumDelivery.Platforms.Android;

/// <summary>
/// Courier GPS V1c: the receiver Android needs to make this app the "device owner" of a company courier phone
/// (set once with adb, see tools/courier-phone-setup.md). Then <see cref="DeviceLock"/> keeps the app installed and GPS on.
/// </summary>
[BroadcastReceiver(Name = "com.ninimum.delivery.CourierDeviceAdmin", Permission = "android.permission.BIND_DEVICE_ADMIN", Exported = true)]
[MetaData("android.app.device_admin", Resource = "@xml/device_admin")]
[IntentFilter(new[] { "android.app.action.DEVICE_ADMIN_ENABLED", "android.app.action.PROFILE_PROVISIONING_COMPLETE" })]
public class CourierDeviceAdmin : DeviceAdminReceiver
{
    public override void OnEnabled(Context context, Intent intent) => DeviceLock.Apply(context);
}

/// <summary>Rules applied on a company phone where this app is the device owner. Does nothing on other phones.</summary>
public static class DeviceLock
{
    public static bool IsOwner(Context context)
    {
        var dpm = (DevicePolicyManager?)context.GetSystemService(Context.DevicePolicyService);
        return dpm?.IsDeviceOwnerApp(context.PackageName) == true;
    }

    public static void Apply(Context context)
    {
        try
        {
            var dpm = (DevicePolicyManager?)context.GetSystemService(Context.DevicePolicyService);
            if (dpm == null || !dpm.IsDeviceOwnerApp(context.PackageName)) return;
            var admin = new ComponentName(context, Java.Lang.Class.FromType(typeof(CourierDeviceAdmin)));
            var package = context.PackageName!;
            // The courier can't delete or stop the app, and can't switch location off.
            dpm.SetUninstallBlocked(admin, package, true);
            dpm.AddUserRestriction(admin, global::Android.OS.UserManager.DisallowConfigLocation);
            dpm.AddUserRestriction(admin, global::Android.OS.UserManager.DisallowAppsControl);
            dpm.AddUserRestriction(admin, global::Android.OS.UserManager.DisallowSafeBoot);
            if (OperatingSystem.IsAndroidVersionAtLeast(30)) dpm.SetLocationEnabled(admin, true);
            // Permissions are granted once and can't be turned off by the courier.
            foreach (var permission in new[] { global::Android.Manifest.Permission.AccessFineLocation, global::Android.Manifest.Permission.AccessCoarseLocation,
                         global::Android.Manifest.Permission.AccessBackgroundLocation, global::Android.Manifest.Permission.Camera, "android.permission.POST_NOTIFICATIONS" })
                dpm.SetPermissionGrantState(admin, package, permission, PermissionGrantState.Granted);
        }
        catch (Exception ex) { Console.WriteLine($"DeviceLock: {ex.Message}"); }
    }
}

/// <summary>Courier GPS V1c: after the phone restarts, tracking comes back if the courier was on shift.</summary>
[BroadcastReceiver(Name = "com.ninimum.delivery.CourierBootReceiver", Exported = true)]
[IntentFilter(new[] { Intent.ActionBootCompleted, "android.intent.action.MY_PACKAGE_REPLACED" })]
public class CourierBootReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null) return;
        try
        {
            DeviceLock.Apply(context);
            var token = Microsoft.Maui.Storage.Preferences.Get(Services.AppConstants.TokenKey, string.Empty);
            var worker = Microsoft.Maui.Storage.Preferences.Get(Services.AppConstants.WorkerKey, string.Empty);
            if (string.IsNullOrWhiteSpace(token) || !worker.Contains("\"online\":true")) return;
            var service = new Intent(context, typeof(TrackingService));
            if (OperatingSystem.IsAndroidVersionAtLeast(26)) context.StartForegroundService(service); else context.StartService(service);
        }
        catch (Exception ex) { Console.WriteLine($"CourierBootReceiver: {ex.Message}"); }
    }
}
