using CommunityToolkit.Mvvm.ComponentModel;
namespace Ninimum.Models;

public partial class CategoryItem : ObservableObject
{
    public int categoryId { get; set; }
    public int? parentId { get; set; }
    public string categoryName { get; set; } = "";
    public string? categoryImageUrl { get; set; }
    public string Image => string.IsNullOrWhiteSpace(categoryImageUrl) ? "no_image.png" : categoryImageUrl;
    [ObservableProperty] private bool isSelected;
}
