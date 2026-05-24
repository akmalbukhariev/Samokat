using CommunityToolkit.Mvvm.ComponentModel;

namespace Ninimum.Models.Startup;

public partial class PopupItemModel : ObservableObject
{
    [ObservableProperty] private string text = string.Empty;
    [ObservableProperty] private string leftImage = string.Empty;
    [ObservableProperty] private string rightImage = string.Empty;
}