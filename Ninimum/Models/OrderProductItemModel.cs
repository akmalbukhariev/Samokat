using Ninimum.Resources.Languages;
namespace Ninimum.Models;

public class OrderProductItemModel
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    public string PriceText => $"{UnitPrice:N0}".Replace(",", " ") + AppResource.UZS_b3b773;
}