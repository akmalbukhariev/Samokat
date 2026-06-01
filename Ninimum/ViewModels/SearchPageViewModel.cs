using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Ninimum.Models.Main;

namespace Ninimum.ViewModels;

public partial class SearchPageViewModel : ObservableObject
{
    [ObservableProperty] private ICommand filterTapCommand;
    [ObservableProperty] private ICommand applyFilterCommand;
    [ObservableProperty] private ICommand selectSortCommand;
    [ObservableProperty] private ObservableCollection<MainProductCardItem> products;

    [ObservableProperty] private string minPrice = "64 000";
    [ObservableProperty] private string maxPrice = "71 000";

    [ObservableProperty] private string searchText = "";

    [ObservableProperty] private bool isSortCheapFirst = true;
    [ObservableProperty] private bool isSortExpensiveFirst;
    [ObservableProperty] private bool isSortNewestFirst;
    [ObservableProperty] private bool isSortOldestFirst;

    public event Action? OpenFilterRequested;

    public SearchPageViewModel()
    {
        filterTapCommand = new Command(OnFilterTapped);
        applyFilterCommand = new Command(OnApplyFilterTapped);
        selectSortCommand = new Command<string>(OnSelectSort);

        Products = new ObservableCollection<MainProductCardItem>
        {
            new MainProductCardItem
            {
                Price = "545 000",
                Subscription_price = "486 000",
                Title = "Kabrita 3 GOLD echki sutiga asoslangan kukunli sutli ichimlik",
                Rating = 4.8,
                ReviewCount = 301,
                ActionText = "+ Ertaga",
                Images = new ObservableCollection<MainProductImageItem>
                {
                    new MainProductImageItem { ImageSource = "product_1.png" },
                    new MainProductImageItem { ImageSource = "product_1.png" },
                    new MainProductImageItem { ImageSource = "product_1.png" },
                    new MainProductImageItem { ImageSource = "product_1.png" }
                }
            },
            new MainProductCardItem
            {
                Price = "545 000",
                Subscription_price = "486 000",
                Title = "Kabrita 3 GOLD echki sutiga asoslangan kukunli sutli ichimlik",
                Rating = 4.8,
                ReviewCount = 301,
                ActionText = "+ Ertaga",
                Images = new ObservableCollection<MainProductImageItem>
                {
                    new MainProductImageItem { ImageSource = "product_2.png" },
                    new MainProductImageItem { ImageSource = "product_2.png" },
                    new MainProductImageItem { ImageSource = "product_2.png" },
                    new MainProductImageItem { ImageSource = "product_2.png" }
                }
            },
            new MainProductCardItem
            {
                Price = "545 000",
                Subscription_price = "486 000",
                Title = "Kabrita 3 GOLD echki sutiga asoslangan kukunli sutli ichimlik",
                Rating = 4.8,
                ReviewCount = 301,
                ActionText = "+ Ertaga",
                Images = new ObservableCollection<MainProductImageItem>
                {
                    new MainProductImageItem { ImageSource = "product_2.png" },
                    new MainProductImageItem { ImageSource = "product_2.png" },
                    new MainProductImageItem { ImageSource = "product_2.png" },
                    new MainProductImageItem { ImageSource = "product_2.png" }
                }
            },
            new MainProductCardItem
            {
                Price = "545 000",
                Subscription_price = "486 000",
                Title = "Kabrita 3 GOLD echki sutiga asoslangan kukunli sutli ichimlik",
                Rating = 4.8,
                ReviewCount = 301,
                ActionText = "+ Ertaga",
                Images = new ObservableCollection<MainProductImageItem>
                {
                    new MainProductImageItem { ImageSource = "product_1.png" },
                    new MainProductImageItem { ImageSource = "product_1.png" },
                    new MainProductImageItem { ImageSource = "product_1.png" },
                    new MainProductImageItem { ImageSource = "product_1.png" }
                }
            }
        };
    }

    private void OnFilterTapped()
    {
        OpenFilterRequested?.Invoke();
    }

    private void OnApplyFilterTapped()
    {
        // later real filtering logic
    }

    private void OnSelectSort(string sortType)
    {
        IsSortCheapFirst = sortType == "cheap";
        IsSortExpensiveFirst = sortType == "expensive";
        IsSortNewestFirst = sortType == "newest";
        IsSortOldestFirst = sortType == "oldest";
    }
}