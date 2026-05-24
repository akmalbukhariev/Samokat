
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Ninimum.Models.Main;

namespace Ninimum.ViewModels;

public partial class FavoritePageViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<MainProductCardItem> products;

    public FavoritePageViewModel()
    { 
        Products = new ObservableCollection<MainProductCardItem>
        {
            new MainProductCardItem
            {
                OldPrice = "545 000",
                NewPrice = "486 000",
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
                OldPrice = "545 000",
                NewPrice = "486 000",
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
                OldPrice = "545 000",
                NewPrice = "486 000",
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
                OldPrice = "545 000",
                NewPrice = "486 000",
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
}