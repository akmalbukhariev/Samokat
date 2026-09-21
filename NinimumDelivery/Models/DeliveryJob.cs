using CommunityToolkit.Mvvm.ComponentModel;
using NinimumDelivery.Resources.Languages;
using System.Collections.ObjectModel;

namespace NinimumDelivery.Models;

public partial class DeliveryJob : ObservableObject
{
    public long jobId { get; set; }
    public long orderId { get; set; }
    public string orderNumber { get; set; } = string.Empty;
    public string customerName { get; set; } = string.Empty;
    public string? customerPhone { get; set; }
    public string deliveryAddress { get; set; } = string.Empty;
    public double? locationLatitude { get; set; }
    public double? locationLongitude { get; set; }
    public string jobStatus { get; set; } = string.Empty;
    public string orderStatus { get; set; } = string.Empty;
    public string paymentStatus { get; set; } = string.Empty;
    public decimal totalPrice { get; set; }
    public int productCount { get; set; }
    public string? createdAt { get; set; }
    public string? acceptedAt { get; set; }
    public string? onTheWayAt { get; set; }
    public string? deliveredAt { get; set; }
    public string? note { get; set; }
    public ObservableCollection<DeliveryProduct> products { get; set; } = new();

    public string DisplayOrderNumber => string.IsNullOrWhiteSpace(orderNumber) ? $"#{orderId:D6}" : $"#{orderId:D6}";
    public string PriceText => $"{totalPrice:N0}".Replace(",", " ") + " so'm";
    public string DisplayPhone => string.IsNullOrWhiteSpace(customerPhone) ? AppResource.NotAssignedPhone : customerPhone;
    public string ProductCountText => string.Format(AppResource.ProductsCount, productCount);
    public string StatusText => jobStatus switch
    {
        "WAITING_ASSIGNMENT" => AppResource.Waiting,
        "ACCEPTED" => AppResource.Accepted,
        "ON_THE_WAY" => AppResource.OnTheWay,
        "DELIVERED" => AppResource.Delivered,
        "FAILED" => AppResource.Failed,
        "CANCELLED" => AppResource.Cancelled,
        _ => jobStatus
    };
    public bool IsWaiting => jobStatus == "WAITING_ASSIGNMENT";
    public bool IsAccepted => jobStatus == "ACCEPTED";
    public bool IsOnTheWay => jobStatus == "ON_THE_WAY";
    public bool IsActive => IsAccepted || IsOnTheWay;
}
