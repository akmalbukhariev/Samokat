using Newtonsoft.Json;
namespace NinimumDelivery.Models;
public class ReturnQuantity
{
    public long productId { get; set; }
    public int quantity { get; set; }
}
public class PartialReturnInfo
{
    public string status { get; set; } = "";
    [JsonProperty("refund_amount")] public decimal refundAmount { get; set; }
    public List<PartialReturnItem> items { get; set; } = new();
}
public class PartialReturnItem
{
    [JsonProperty("product_name")] public string productName { get; set; } = "";
    public int quantity { get; set; }
}
