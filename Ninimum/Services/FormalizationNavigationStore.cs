namespace Ninimum.Views.Formalization;

public static class FormalizationNavigationStore
{
    public static FormalizationData? Data { get; set; }

    public static void Clear()
    {
        Data = null;
    }
}

public class FormalizationData
{
    public long UserId { get; set; }
    public long? AddressId { get; set; }

    public List<FormalizationProductItem> Products { get; set; } = new();

    public string AddressText { get; set; } = string.Empty;
}

public class FormalizationProductItem
{
    public long ProductId { get; set; }
    public string ImageSource { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double Price { get; set; }

    public double TotalPrice => Price * Quantity;

    public string QuantityText => $"{Quantity} dona";
    public string PriceText => $"{TotalPrice:N0} so’m".Replace(",", " ");
}