using CommunityToolkit.Mvvm.ComponentModel;

namespace Ninimum.Models.Main;

public partial class AdBannerItem : ObservableObject
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    [ObservableProperty] private string productName = string.Empty;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string image = string.Empty;
    [ObservableProperty] private double price;
    [ObservableProperty] private double subscriptionPrice;
}
