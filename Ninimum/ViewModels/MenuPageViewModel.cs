using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ninimum.Models.Menu;
using System.Collections.ObjectModel;
using Ninimum.Resources.Languages;

namespace Ninimum.ViewModels;

public partial class MenuPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<MenuCategoryModel> categories = new();

    public MenuPageViewModel()
    {
        LoadMenu();
    }

    private void LoadMenu()
    {
        Categories.Clear();

        Categories.Add(new MenuCategoryModel
        {
            Title = AppResource.Children,
            Icon = "ic_baby",
            IsExpanded = false,
            Items = new ObservableCollection<MenuSubItemModel>
            {
                new() { Title = AppResource.DryFormula },
                new() { Title = AppResource.Porridges },
                new() { Title = AppResource.PureesAndYogurt },
                new() { Title = AppResource.BabyBottles },
                new() { Title = AppResource.Pacifiers },
                new() { Title = AppResource.Diapers },
                new() { Title = AppResource.Wipes },
                new() { Title = AppResource.Accessories },
                new() { Title = AppResource.Cookies }
            }
        });

        Categories.Add(new MenuCategoryModel
        {
            Title = AppResource.ForMothers,
            Icon = "ic_mother",
            IsExpanded = false,
            Items = new ObservableCollection<MenuSubItemModel>
            {
                new() { Title = AppResource.DryFormula },
                new() { Title = AppResource.Porridges },
                new() { Title = AppResource.PureesAndYogurt },
                new() { Title = AppResource.BabyBottles },
                new() { Title = AppResource.Pacifiers },
                new() { Title = AppResource.Diapers },
                new() { Title = AppResource.Wipes },
                new() { Title = AppResource.Accessories },
                new() { Title = AppResource.Cookies }
            }
        });

        Categories.Add(new MenuCategoryModel
        {
            Title = AppResource.PersonalHygiene,
            Icon = "ic_hygiene",
            IsExpanded = false,
            Items = new ObservableCollection<MenuSubItemModel>()
        });

        Categories.Add(new MenuCategoryModel
        {
            Title = AppResource.Nutrition,
            Icon = "ic_nutrition",
            IsExpanded = false,
            Items = new ObservableCollection<MenuSubItemModel>()
        });
    }

    [RelayCommand]
    private void ToggleCategory(MenuCategoryModel? category)
    {
        if (category == null)
            return;

        category.IsExpanded = !category.IsExpanded;
    }

    [RelayCommand]
    private void SelectSubItem(MenuSubItemModel? item)
    {
        if (item == null)
            return;

        foreach (var category in Categories)
        {
            foreach (var subItem in category.Items)
            {
                subItem.IsSelected = false;
            }
        }

        item.IsSelected = true;

        // TODO:
        // navigation or action here
        // Example:
        // await Shell.Current.GoToAsync(...);
    }
}