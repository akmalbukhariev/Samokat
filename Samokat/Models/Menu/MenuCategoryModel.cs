using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Samokat.Models.Menu;
  
public partial class MenuCategoryModel : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string icon = string.Empty;

    [ObservableProperty]
    private bool isExpanded;

    public ObservableCollection<MenuSubItemModel> Items { get; set; } = new();
    
    public string ArrowIcon => IsExpanded ? "ic_arrow_up.png" : "ic_arrow_down.png";

    partial void OnIsExpandedChanged(bool value)
    {
        OnPropertyChanged(nameof(ArrowIcon));
    }
}