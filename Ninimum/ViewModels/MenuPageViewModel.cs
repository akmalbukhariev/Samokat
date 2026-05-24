using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ninimum.Models.Menu;
using System.Collections.ObjectModel;

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
            Title = "Bolajonlar",
            Icon = "ic_baby",
            IsExpanded = false,
            Items = new ObservableCollection<MenuSubItemModel>
            {
                new() { Title = "Suxoy smes" },
                new() { Title = "Kashalar" },
                new() { Title = "Pyurelar va yogurt" },
                new() { Title = "Sut butilkalar" },
                new() { Title = "Soskalar" },
                new() { Title = "Tagliglar" },
                new() { Title = "Salfetka" },
                new() { Title = "Aksesuarlar" },
                new() { Title = "Pechenilar" }
            }
        });

        Categories.Add(new MenuCategoryModel
        {
            Title = "Onalar uchun",
            Icon = "ic_mother",
            IsExpanded = false,
            Items = new ObservableCollection<MenuSubItemModel>
            {
                new() { Title = "Suxoy smes" },
                new() { Title = "Kashalar" },
                new() { Title = "Pyurelar va yogurt" },
                new() { Title = "Sut butilkalar" },
                new() { Title = "Soskalar" },
                new() { Title = "Tagliglar" },
                new() { Title = "Salfetka" },
                new() { Title = "Aksesuarlar" },
                new() { Title = "Pechenilar" }
            }
        });

        Categories.Add(new MenuCategoryModel
        {
            Title = "Shaxsiy gigiena",
            Icon = "ic_hygiene",
            IsExpanded = false,
            Items = new ObservableCollection<MenuSubItemModel>()
        });

        Categories.Add(new MenuCategoryModel
        {
            Title = "Oziqlantiruvchi",
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