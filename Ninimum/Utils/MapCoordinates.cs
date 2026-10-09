namespace Ninimum.Utils;

public static class MapCoordinates
{
    public static string ForGeocoder(double latitude, double longitude) =>
        FormattableString.Invariant($"{longitude},{latitude}");
}
