using CommunityToolkit.Mvvm.ComponentModel;

namespace Samokat.Models.Main;

public partial class MainProductImageItem: ObservableObject
{
    [ObservableProperty] private  string imageSource = string.Empty;
}