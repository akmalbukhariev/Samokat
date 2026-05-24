using CommunityToolkit.Mvvm.ComponentModel;

namespace Ninimum.Models;

public partial class ProductImageDetailInfo : ObservableObject
{
    [ObservableProperty]
    private string image = string.Empty;

    [ObservableProperty]
    private bool isSelected;

    public ProductImageDetailInfo(string image)
    {
        Image = image;
    }
}