namespace NinimumDelivery.Models;
public class DeliveryProduct
{
    public long productId { get; set; }
    public string productName { get; set; } = string.Empty;
    public string? productImageUrl { get; set; }
    public decimal unitPrice { get; set; }
    public int quantity { get; set; }
    public decimal totalPrice { get; set; }
}
