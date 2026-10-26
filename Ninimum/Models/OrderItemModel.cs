using Ninimum.Resources.Languages;
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
    [ObservableProperty] private string? partialReturnStatus;
    partial void OnPartialReturnStatusChanged(string? value) => OnPropertyChanged(nameof(StatusText));

    public string DisplayOrderNumber => $"#{OrderId:D6}";

    public string StatusText
    {
        get
        {
            if(PartialReturnStatus!=null) return AppResource.ResourceManager.GetString(PartialReturnStatus=="RECEIVED"?"PartialReturnReceived":"PartialReturnPending",AppResource.Culture) ?? "Delivered with partial return";
            return Status?.ToUpperInvariant() switch
            {
                "PENDING" => AppResource.OrderReceived,
                "CONFIRMED" => AppResource.OrderConfirmed,
                "PREPARING" => AppResource.BeingPrepared,
                "READY" => AppResource.ResourceManager.GetString("OrderReady", AppResource.Culture) ?? "Ready",
                "ON_THE_WAY" => AppResource.OutForDelivery,
                "RETURNED" => AppResource.ResourceManager.GetString("OrderReturned", AppResource.Culture) ?? "Received by warehouse",
                "RETURNING" => AppResource.ResourceManager.GetString("OrderReturning", AppResource.Culture) ?? "Returning",
                "DELIVERED" => AppResource.Delivered,
                "CANCELLED" => AppResource.OrderCancelled,
                _ => ""
            };
        }
    }

    public bool CanCancel =>
        string.Equals(Status, "PENDING", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Status, "CONFIRMED", StringComparison.OrdinalIgnoreCase);

    [ObservableProperty] private string status = string.Empty;
    [ObservableProperty] private string orderDate = string.Empty;
    [ObservableProperty] private string totalPrice = string.Empty;
    [ObservableProperty] private bool isExpanded;
    [ObservableProperty] private bool isLoading;

    public ObservableCollection<OrderProductItemModel> Products { get; } = new();

    // Taxi delivery: the customer pays the driver; the details appear once the operator books the taxi.
    [ObservableProperty] private string? deliveryMethod;
    [ObservableProperty] private string? taxiCompany;
    [ObservableProperty] private string? taxiDriverName;
    [ObservableProperty] private string? taxiDriverPhone;
    [ObservableProperty] private string? taxiCarNumber;
    [ObservableProperty] private decimal? taxiFare;
    public bool IsTaxi => string.Equals(DeliveryMethod, "TAXI", StringComparison.OrdinalIgnoreCase);
    public bool HasTaxiPhone => IsTaxi && !string.IsNullOrWhiteSpace(TaxiDriverPhone);
    // Buttons "Mahsulotni oldim" / "Muammo bor" while the booked taxi is on the way.
    public bool CanConfirmTaxi => IsTaxi && string.Equals(Status, "ON_THE_WAY", StringComparison.OrdinalIgnoreCase);
    public string TaxiText
    {
        get
        {
            if (!IsTaxi) return string.Empty;
            var lang = (AppResource.Culture ?? System.Globalization.CultureInfo.CurrentUICulture).TwoLetterISOLanguageName;
            string T(string uz, string ru, string en) => lang == "ru" ? ru : lang == "en" ? en : uz;
            if (string.IsNullOrWhiteSpace(TaxiCarNumber))
                return T("🚕 Taksi orqali tezkor yetkazish. Taksi haqini haydovchiga o‘zingiz to‘laysiz.", "🚕 Срочная доставка на такси. Стоимость такси вы платите водителю.", "🚕 Fast delivery by taxi. You pay the taxi fare to the driver.");
            var lines = new List<string> { $"🚕 {TaxiCompany} · {TaxiCarNumber}" };
            lines.Add(T("Haydovchi", "Водитель", "Driver") + $": {(string.IsNullOrWhiteSpace(TaxiDriverName) ? "" : TaxiDriverName + ", ")}{TaxiDriverPhone}");
            if (TaxiFare.HasValue) lines.Add(T($"Taksi haqi: ~{TaxiFare.Value:N0} so‘m (haydovchiga to‘lanadi)", $"Такси: ~{TaxiFare.Value:N0} сум (оплата водителю)", $"Taxi fare: ~{TaxiFare.Value:N0} so‘m (pay the driver)").Replace(",", " "));
            return string.Join("\n", lines);
        }
    }
    partial void OnDeliveryMethodChanged(string? value) => NotifyTaxi();
    partial void OnTaxiCompanyChanged(string? value) => NotifyTaxi();
    partial void OnTaxiDriverNameChanged(string? value) => NotifyTaxi();
    partial void OnTaxiDriverPhoneChanged(string? value) => NotifyTaxi();
    partial void OnTaxiCarNumberChanged(string? value) => NotifyTaxi();
    partial void OnTaxiFareChanged(decimal? value) => NotifyTaxi();
    private void NotifyTaxi() { OnPropertyChanged(nameof(IsTaxi)); OnPropertyChanged(nameof(TaxiText)); OnPropertyChanged(nameof(HasTaxiPhone)); OnPropertyChanged(nameof(CanConfirmTaxi)); }

    partial void OnStatusChanged(string value)
    {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(CanCancel));
        OnPropertyChanged(nameof(CanConfirmTaxi));
    }
}