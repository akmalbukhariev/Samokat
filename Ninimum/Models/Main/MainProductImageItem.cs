using CommunityToolkit.Mvvm.ComponentModel;

namespace Ninimum.Models.Main;

public partial class MainProductImageItem: ObservableObject
{
    [ObservableProperty] private  string imageSource = string.Empty;
}