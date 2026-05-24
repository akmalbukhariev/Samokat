using CommunityToolkit.Mvvm.ComponentModel;

namespace Samokat.Models.Main;

public partial class AdBannerItem : ObservableObject
{
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string buttonText = "Sotib olish";
    [ObservableProperty] private string image = string.Empty;
}