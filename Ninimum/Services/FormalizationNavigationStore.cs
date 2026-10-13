using Ninimum.Resources.Languages;
using Ninimum.Models;
using Ninimum.Services;

namespace Ninimum.Views.Formalization;

public static class FormalizationNavigationStore
{
    private static FormalizationData? data;
    public static FormalizationData? Data
    {
        get => data;
        set
        {
            if (value != null && value.UserId > 0)
            {
                var saved = new AppStoreService().Get<SelectedAddressModel>(AddressKey(value.UserId));
                if (saved != null)
                {
                    value.AddressId = null;
                    value.AddressText = saved.Address;
                    value.AddressLatitude = saved.Latitude;
                    value.AddressLongitude = saved.Longitude;
                }
            }
            data = value;
        }
    }

    private static string AddressKey(long userId) => $"checkout_address_{userId}";

    public static void SaveAddress(long userId, SelectedAddressModel address)
    {
        if (userId > 0)
            new AppStoreService().Set(AddressKey(userId), address);
    }

    public static void Clear()
    {
        Data = null;
    }
}

public class FormalizationData
{
    public long UserId { get; set; }
    public long? AddressId { get; set; }
    public long? TariffSubscriptionId { get; set; }

    public List<FormalizationProductItem> Products { get; set; } = new();

    public string AddressText { get; set; } = string.Empty;
    public double? AddressLatitude { get; set; }
    public double? AddressLongitude { get; set; }
}

public class FormalizationProductItem
{
    public long ProductId { get; set; }
    public string ImageSource { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double Price { get; set; }

    public double TotalPrice => Price * Quantity;

    public string QuantityText => string.Format(AppResource.PiecesCount, Quantity);
    public string PriceText => string.Format(AppResource.UZS_02854f, TotalPrice).Replace(",", " ");
}
