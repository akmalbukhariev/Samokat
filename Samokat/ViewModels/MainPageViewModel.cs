using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Samokat.Models.Main;

namespace Samokat.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty] private ICommand notificationTapCommand;
    [ObservableProperty] private ICommand buyNowCommand;
    [ObservableProperty] private ICommand menuCommand;
    [ObservableProperty] private ObservableCollection<AdBannerItem> adBanners;
    [ObservableProperty] private ICommand purchaseBannerCommand;
    [ObservableProperty] private ObservableCollection<MainProductCardItem> products;

    public MainPageViewModel()
    {
        NotificationTapCommand = new Command(OnNotificationTapped);

        AdBanners = new ObservableCollection<AdBannerItem>
        {
            new AdBannerItem
            {
                Title = "Huggies Elite Soft 1\ntagliklari uchun\nmaxsus chegirma",
                ButtonText = "Sotib olish",
                Image = "huggiest.png"
            },
            new AdBannerItem
            {
                Title = "Bolalar uchun\nyangi mahsulotlar",
                ButtonText = "Sotib olish",
                Image = "huggiest.png"
            },
            new AdBannerItem
            {
                Title = "Maxsus takliflar\nfaqat ilovada",
                ButtonText = "Sotib olish",
                Image = "huggiest.png"
            },
            new AdBannerItem
            {
                Title = "Kerakli mahsulotlarni\nqulay narxda toping",
                ButtonText = "Sotib olish",
                Image = "huggiest.png"
            }
        };
        PurchaseBannerCommand = new Command<AdBannerItem>(OnPurchaseBanner);

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

    private async void OnNotificationTapped()
    {
        await Application.Current.MainPage.DisplayAlert("Info", "Notification clicked", "OK");
    }
    
    private async void OnPurchaseBanner(AdBannerItem? item)
    {
        if (item == null)
            return;

        await Application.Current!.MainPage!.DisplayAlert(
            "Purchase",
            item.Title,
            "OK");
    }
}