namespace Ninimum.Services;

public enum AddressSelectionMode
{
    Registration,
    Checkout
}

public sealed class AddressSelectionNavigationData
{
    public AddressSelectionMode Mode { get; set; } = AddressSelectionMode.Registration;
    public string AddressText { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public static class AddressSelectionNavigationStore
{
    public static AddressSelectionNavigationData? Data { get; private set; }

    public static void Prepare(
        AddressSelectionMode mode,
        string? addressText,
        double? latitude,
        double? longitude)
    {
        Data = new AddressSelectionNavigationData
        {
            Mode = mode,
            AddressText = addressText ?? string.Empty,
            Latitude = latitude,
            Longitude = longitude
        };
    }

    public static void UpdateSelection(string addressText, double latitude, double longitude)
    {
        Data ??= new AddressSelectionNavigationData();
        Data.AddressText = addressText;
        Data.Latitude = latitude;
        Data.Longitude = longitude;
    }
}
