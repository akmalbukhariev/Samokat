using CommunityToolkit.Mvvm.ComponentModel;

namespace Ninimum.Models.Main;

public partial class AdBannerItem : ObservableObject
{
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string buttonText = "Sotib olish";
    [ObservableProperty] private string image = string.Empty;
}