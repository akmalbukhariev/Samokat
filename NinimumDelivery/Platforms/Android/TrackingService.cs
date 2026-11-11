using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using System.Text;
using System.Text.Json;

namespace NinimumDelivery.Platforms.Android;

/// <summary>
/// Courier GPS V1b: records where the courier phone is during the shift ("Ishga tayyor" on), even with the screen off.
/// A point is kept when the courier moved 15 m or 60 s have passed; points wait in a file and are sent every 30 s,
/// so a weak signal leaves no gaps. A permanent notification shows that tracking is on.
/// </summary>
[Service(Exported = false, ForegroundServiceType = ForegroundService.TypeLocation)]
public class TrackingService : Service, ILocationListener
{
    public const string ChannelId = "ninimum_tracking";
    const int NotificationId = 4410, MaxQueue = 5000;
    static readonly object gate = new();
    LocationManager? manager;
    Timer? uploader;
    global::Android.Locations.Location? lastKept;
    DateTime lastKeptAt = DateTime.MinValue;
    static readonly HttpClient http = new() { Timeout = TimeSpan.FromSeconds(20) };
    int uploading;

    public override IBinder? OnBind(Intent? intent) => null;

    public static bool Running { get; private set; }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        CreateChannel();
        var open = PendingIntent.GetActivity(this, 0, PackageManager?.GetLaunchIntentForPackage(PackageName ?? "") ?? new Intent(),
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
        var notification = new Notification.Builder(this, ChannelId)
            .SetContentTitle("Ninimum Go")
            .SetContentText("Ish vaqti: joylashuv yozilmoqda")
            .SetSmallIcon(Resource.Drawable.ic_push)
            .SetOngoing(true)
            .SetContentIntent(open)
            .Build();
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Q) StartForeground(NotificationId, notification, ForegroundService.TypeLocation);
        else StartForeground(NotificationId, notification);
        if (!Running) StartUpdates();
        Running = true;
        return StartCommandResult.Sticky;
    }

    void StartUpdates()
    {
        manager = (LocationManager?)GetSystemService(LocationService);
        try
        {
            if (manager?.IsProviderEnabled(LocationManager.GpsProvider) == true)
                manager.RequestLocationUpdates(LocationManager.GpsProvider, 15000, 0, this, Looper.MainLooper);
            if (manager?.IsProviderEnabled(LocationManager.NetworkProvider) == true)
                manager.RequestLocationUpdates(LocationManager.NetworkProvider, 30000, 0, this, Looper.MainLooper);
        }
        catch (Java.Lang.SecurityException) { /* permission missing: nothing recorded; web admin shows the gap */ }
        uploader = new Timer(_ => _ = Upload(), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30));
    }

    public void OnLocationChanged(global::Android.Locations.Location location)
    {
        // A network fix is only used when GPS has been silent for a minute.
        bool gps = location.Provider == LocationManager.GpsProvider;
        if (!gps && lastKept?.Provider == LocationManager.GpsProvider && DateTime.UtcNow - lastKeptAt < TimeSpan.FromSeconds(60)) return;
        if (lastKept != null && lastKept.DistanceTo(location) < 15 && DateTime.UtcNow - lastKeptAt < TimeSpan.FromSeconds(60)) return;
        lastKept = location; lastKeptAt = DateTime.UtcNow;
        bool mock = Build.VERSION.SdkInt >= BuildVersionCodes.S ? location.Mock :
#pragma warning disable CA1422
            location.IsFromMockProvider;
#pragma warning restore CA1422
        var point = new Point(location.Time > 0 ? location.Time : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            location.Latitude, location.Longitude, location.HasAccuracy ? location.Accuracy : null, location.HasSpeed ? location.Speed : null, Battery(), mock);
        lock (gate)
        {
            var queue = Load();
            queue.Add(point);
            if (queue.Count > MaxQueue) queue.RemoveRange(0, queue.Count - MaxQueue);
            Save(queue);
        }
    }

    async Task Upload()
    {
        if (Interlocked.Exchange(ref uploading, 1) == 1) return;
        try
        {
            List<Point> batch;
            lock (gate) batch = Load().Take(300).ToList();
            if (batch.Count == 0) return;
            var token = Microsoft.Maui.Storage.Preferences.Get(Services.AppConstants.TokenKey, string.Empty);
            if (string.IsNullOrWhiteSpace(token)) return;
            var body = JsonSerializer.Serialize(new { points = batch.Select(p => new { recorded_at = p.t, latitude = p.lat, longitude = p.lon, accuracy = p.acc, speed = p.speed, battery = p.battery, mock = p.mock }) });
            using var request = new HttpRequestMessage(HttpMethod.Post, Services.AppConstants.BaseUrl + "delivery-app/locations") { Content = new StringContent(body, Encoding.UTF8, "application/json") };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            using var response = await http.SendAsync(request);
            var text = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode || !text.Contains("\"resultCode\":\"100\"")) return;   // kept for the next try
            lock (gate)
            {
                var queue = Load();
                queue.RemoveAll(p => batch.Any(b => b.t == p.t));
                Save(queue);
            }
        }
        catch { /* offline: the points stay in the file */ }
        finally { Interlocked.Exchange(ref uploading, 0); }
    }

    int? Battery()
    {
        try { var bm = (BatteryManager?)GetSystemService(BatteryService); return bm?.GetIntProperty((int)BatteryProperty.Capacity); }
        catch { return null; }
    }

    static string QueuePath => Path.Combine(global::Android.App.Application.Context.FilesDir!.AbsolutePath, "courier_points.json");
    static List<Point> Load()
    {
        try { return File.Exists(QueuePath) ? JsonSerializer.Deserialize<List<Point>>(File.ReadAllText(QueuePath)) ?? new() : new(); }
        catch { return new(); }
    }
    static void Save(List<Point> queue) { try { File.WriteAllText(QueuePath, JsonSerializer.Serialize(queue)); } catch { } }

    void CreateChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;
        var channel = new NotificationChannel(ChannelId, "Ish vaqti (joylashuv)", NotificationImportance.Low) { Description = "Ish vaqtida joylashuv yozilayotganini ko‘rsatadi" };
        ((NotificationManager?)GetSystemService(NotificationService))?.CreateNotificationChannel(channel);
    }

    public override void OnDestroy()
    {
        Running = false;
        try { manager?.RemoveUpdates(this); } catch { }
        uploader?.Dispose();
        _ = Upload();
        base.OnDestroy();
    }

    public void OnProviderDisabled(string provider) { }
    public void OnProviderEnabled(string provider) { }
    public void OnStatusChanged(string? provider, Availability status, Bundle? extras) { }

    public sealed record Point(long t, double lat, double lon, float? acc, float? speed, int? battery, bool mock);
}
