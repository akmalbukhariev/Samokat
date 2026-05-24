using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Ninimum.Models.Menu;

public partial class MenuSubItemModel : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private bool isSelected;
}