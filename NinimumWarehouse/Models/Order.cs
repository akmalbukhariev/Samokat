using System.Text.Json.Serialization;
namespace NinimumWarehouse.Models;
public class Worker
{
    [JsonPropertyName("worker_code")] public string Code { get; set; } = "";
    [JsonPropertyName("full_name")] public string Name { get; set; } = "";
    public string Token { get; set; } = "";
}
public class Order
{
    public long Id { get; set; }
    [JsonPropertyName("order_number")] public string Number { get; set; } = "";
    [JsonPropertyName("preparation_status")] public string Status { get; set; } = "WAITING";
    [JsonPropertyName("worker_code")] public string? Worker { get; set; }
    [JsonPropertyName("payment_status")] public string Payment { get; set; } = "";
    [JsonPropertyName("order_status")] public string OrderStatus { get; set; } = "";
    // "TAXI": fast delivery by an outside taxi — pack it first.
    [JsonPropertyName("delivery_method")] public string? DeliveryMethod { get; set; }
    public string TaxiMark => DeliveryMethod == "TAXI" ? "  🚕 TAKSI" : "";
    public string? Note { get; set; }
    [JsonPropertyName("return_photo")] public string? ReturnPhoto { get; set; }
    [JsonPropertyName("return_reason")] public string? ReturnReason { get; set; }
    [JsonPropertyName("partial_return_status")] public string? PartialReturnStatus { get; set; }
    public int Quantity { get; set; }
    public List<OrderItem> Items { get; set; } = [];
    [JsonPropertyName("batch_allocations")] public List<BatchPick> Picks { get; set; } = [];
}
// Where the server reserved this order's goods: the picker takes them from exactly this shelf and batch.
public class BatchPick
{
    [JsonPropertyName("product_id")] public long ProductId { get; set; }
    [JsonPropertyName("location_code")] public string? Location { get; set; }
    [JsonPropertyName("batch_number")] public string? Batch { get; set; }
    [JsonPropertyName("expiry_text")] public string? Expiry { get; set; }
    [JsonPropertyName("held_quantity")] public long Held { get; set; }
}
public class OrderItem
{
    [JsonPropertyName("product_id")] public long Id { get; set; }
    [JsonPropertyName("product_name")] public string Name { get; set; } = "";
    [JsonPropertyName("product_image_url")] public string? ImageUrl { get; set; }
    public string? Barcode { get; set; }
    [JsonPropertyName("required_quantity")] public int Required { get; set; }
    [JsonPropertyName("checked_quantity")] public int Checked { get; set; }
}
public class OrderPage
{
    public List<Order> Items { get; set; } = [];
    public int Total { get; set; }
}
