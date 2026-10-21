using System.Text.Json.Serialization;
namespace NinimumWarehouse.Services;
// M14 Q-14: when the owner requires the scanner, products with a barcode can't be confirmed by typing the code.
public static class WarehouseRules
{
    private sealed class Rules { [JsonPropertyName("scanner_required")] public bool ScannerRequired { get; set; } }
    public static bool ScannerRequired { get; private set; }
    public static bool ManualCodeAllowed => !ScannerRequired;
    public static async Task RefreshAsync(WarehouseApi api)
    {
        // An older backend has no rules endpoint: keep manual entry allowed.
        try { ScannerRequired = (await api.Request<Rules>("rules")).ScannerRequired; } catch (ApiException) { }
    }
}
