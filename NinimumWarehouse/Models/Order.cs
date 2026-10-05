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
    public string? Note { get; set; }
    public int Quantity { get; set; }
    public List<OrderItem> Items { get; set; } = [];
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
