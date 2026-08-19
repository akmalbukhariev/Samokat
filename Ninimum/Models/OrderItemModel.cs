using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Ninimum.Models;

public partial class OrderItemModel : ObservableObject
{
    public long OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal TotalPriceValue { get; set; }
    public int ProductCount { get; set; }

    public string DisplayOrderNumber => $"#{OrderId:D6}";

    public string StatusText
    {
        get
        {
            return Status?.ToUpperInvariant() switch
            {
                "PENDING" => "Buyurtma qabul qilindi",
                "CONFIRMED" => "Buyurtma tasdiqlandi",
                "PREPARING" => "Yig'ish jarayonida",
                "ON_THE_WAY" => "Yetkazib berilmoqda",
                "DELIVERED" => "Yetkazib berildi",
                "CANCELLED" => "Buyurtma bekor qilindi",
                _ => ""
            };
        }
    }

    [ObservableProperty] private string status = string.Empty;
    [ObservableProperty] private string orderDate = string.Empty;
    [ObservableProperty] private string totalPrice = string.Empty;
    [ObservableProperty] private bool isExpanded;
    [ObservableProperty] private bool isLoading;

    public ObservableCollection<OrderProductItemModel> Products { get; } = new();

    partial void OnStatusChanged(string value)
    {
        OnPropertyChanged(nameof(StatusText));
    }
}