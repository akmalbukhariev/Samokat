using CommunityToolkit.Mvvm.ComponentModel;

namespace Ninimum.Models.Tariff;

public partial class TariffPlan : ObservableObject
{
    [ObservableProperty] private long id;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string price = string.Empty;
    [ObservableProperty] private int durationMonth = 1;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private Color color = Colors.Green;
    [ObservableProperty] private string deliveryText = string.Empty;
    [ObservableProperty] private string partnerIcon = "ic_check_circle.png";
    [ObservableProperty] private Color partnerIconColor = Color.FromArgb("#16D14E");

    // UI state used by TariffsPage.
    // The active tariff cannot be purchased again, while the other tariffs
    // can be selected as a replacement plan.
    [ObservableProperty] private bool isCurrent;
    [ObservableProperty] private bool canPurchase = true;
    [ObservableProperty] private string actionText = "Sotib olish";
}
