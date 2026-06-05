using CommunityToolkit.Mvvm.ComponentModel;

namespace Ninimum.Models.Main;

public partial class AdBannerItem : ObservableObject
{
    public int Id = 0;
    public int ProductId = 0;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string image = string.Empty;
}