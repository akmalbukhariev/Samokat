using Microsoft.Maui.Devices.Sensors;
namespace NinimumDelivery.Services;

/// <summary>
/// Courier GPS V1a: where the phone is when the courier presses a button. Never blocks the courier:
/// if there is no location, the reason (PERMISSION_DENIED / GPS_OFF / TIMEOUT) is sent instead, and web admin sees it.
/// </summary>
public static class CourierLocation
{
    public sealed record Stamp(double? latitude, double? longitude, double? accuracy, bool mock, string? locationError);

    public static async Task<Stamp> Current()
    {
        try
        {
            var status = await MainThread.InvokeOnMainThreadAsync(() => Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>());
            if (status != PermissionStatus.Granted)
                status = await MainThread.InvokeOnMainThreadAsync(() => Permissions.RequestAsync<Permissions.LocationWhenInUse>());
            if (status != PermissionStatus.Granted) return new(null, null, null, false, "PERMISSION_DENIED");
            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);
            // A fresh fix is best; a recent last-known one (under 2 minutes) is accepted when the sky is blocked.
            if (location == null)
            {
                var last = await Geolocation.Default.GetLastKnownLocationAsync();
                if (last != null && DateTimeOffset.UtcNow - last.Timestamp < TimeSpan.FromMinutes(2)) location = last;
            }
            if (location == null) return new(null, null, null, false, "TIMEOUT");
            return new(location.Latitude, location.Longitude, location.Accuracy, location.IsFromMockProvider, null);
        }
        catch (FeatureNotEnabledException) { return new(null, null, null, false, "GPS_OFF"); }
        catch (PermissionException) { return new(null, null, null, false, "PERMISSION_DENIED"); }
        catch (Exception) { return new(null, null, null, false, "TIMEOUT"); }
    }
}
