using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace NinimumWarehouse.Models;
public class InventoryStatus
{
    public bool Enabled { get; set; }
}
public class CountDocument
{
    public long Id { get; set; }
    public string Status { get; set; } = "";
    [JsonPropertyName("location_code")] public string? LocationCode { get; set; }
    public string? Reason { get; set; }
    [JsonPropertyName("created_by")] public string? CreatedBy { get; set; }
    [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
}
public class CountDocumentPage
{
    public List<CountDocument> Items { get; set; } = [];
    public int Total { get; set; }
}
public class CountDetail
{
    public long Id { get; set; }
    public string Status { get; set; } = "";
    public CountPayload Payload { get; set; } = new();
}
public class CountPayload
{
    public List<CountLine> Lines { get; set; } = [];
}
// Expected quantities are removed by the backend while counting (blind count).
public class CountLine
{
    [JsonPropertyName("batch_id")] public long BatchId { get; set; }
    [JsonPropertyName("product_name")] public string ProductName { get; set; } = "";
    [JsonPropertyName("batch_number")] public string? BatchNumber { get; set; }
    // Stored as epoch milliseconds by the backend document serializer; older rows may hold a date string.
    [JsonPropertyName("expiry_date")] public JsonElement Expiry { get; set; }
    public string? Barcode { get; set; }
    public string ExpiryText => Expiry.ValueKind switch {
        JsonValueKind.Number when Expiry.TryGetInt64(out var ms) => DateTimeOffset.FromUnixTimeMilliseconds(ms).ToLocalTime().ToString("dd.MM.yyyy",CultureInfo.InvariantCulture),
        JsonValueKind.String when DateTime.TryParse(Expiry.GetString(),CultureInfo.InvariantCulture,DateTimeStyles.None,out var d) => d.ToString("dd.MM.yyyy",CultureInfo.InvariantCulture),
        _ => "—" };
}
public class BinInfo
{
    public BinLocation Location { get; set; } = new();
    public List<BatchRow> Batches { get; set; } = [];
}
public class BinLocation
{
    public long Id { get; set; }
    public string Code { get; set; } = "";
    public string? Name { get; set; }
}
public class BatchRow
{
    public long Id { get; set; }
    [JsonPropertyName("product_name")] public string ProductName { get; set; } = "";
    public string? Barcode { get; set; }
    [JsonPropertyName("batch_number")] public string? BatchNumber { get; set; }
    [JsonPropertyName("expiry_date")] public JsonElement Expiry { get; set; }
    [JsonPropertyName("available_quantity")] public long Available { get; set; }
    [JsonPropertyName("physical_quantity")] public long Physical { get; set; }
    [JsonPropertyName("count_document")] public long? CountDocument { get; set; }
    public string ExpiryText => new CountLine { Expiry = Expiry }.ExpiryText;
}
