using CommunityToolkit.Mvvm.ComponentModel;

namespace Samokat.Models.Tariff;

public partial class TariffPlan : ObservableObject
{
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string price = string.Empty;
    [ObservableProperty] private Color color = Colors.Green;
    [ObservableProperty] private string deliveryText = string.Empty;
    [ObservableProperty] private string partnerIcon = "✓";
    [ObservableProperty] private Color partnerIconColor = Color.FromArgb("#16D14E");
}