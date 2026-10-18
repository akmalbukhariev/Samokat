using CommunityToolkit.Mvvm.ComponentModel;
namespace Ninimum.Models;

public partial class CategoryItem : ObservableObject
{
    public int categoryId { get; set; }
    public int? parentId { get; set; }
    public string categoryName { get; set; } = "";
    public string? categoryNameUz { get; set; }
    public string? categoryNameRu { get; set; }
    public string DisplayName => CategoryDisplayName.Resolve(categoryName, categoryNameUz, categoryNameRu,
        System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
    public string? categoryImageUrl { get; set; }
    public bool HasImage => !string.IsNullOrWhiteSpace(categoryImageUrl);
    public string Image => string.IsNullOrWhiteSpace(categoryImageUrl) ? "no_image.png" : categoryImageUrl;
    [ObservableProperty] private bool isSelected;
}
