using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Samokat.Models;
using Samokat.Models.Main;

namespace Samokat.ViewModels;

public partial class DetailProductPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<MainProductCardItem> similarProducts = new();

    [ObservableProperty]
    private ObservableCollection<ProductImageDetailInfo> productImages = new();

    [ObservableProperty]
    private int currentImageIndex;

    [ObservableProperty]
    private string productTitle = "Kabrita 3 GOLD echki sutiga asoslangan kukunli sutli ichimlik, 12+ oy, 800 g";

    [ObservableProperty]
    private string stockText = "Omborda 100 dona mavjud; sotuvda mavjud!";

    [ObservableProperty]
    private string rating = "4.8";

    [ObservableProperty]
    private string reviewText = "301 sharhlar ∙ 100+ buyurtmalar";

    [ObservableProperty]
    private string subscriptionPrice = "486 000 so’m";

    [ObservableProperty]
    private string regularPrice = "545 000 so’m";

    [ObservableProperty]
    private string deliveryLabel = "Yetzaib berish";

    [ObservableProperty]
    private string subscriptionDeliveryText = "bepul ∙ 30 minut";

    [ObservableProperty]
    private string regularDeliveryText = "pullik ∙ 1 kun";

    [ObservableProperty]
    private string description =
        "Kabrita 3 GOLD - bu 12 oydan oshgan bolalar uchun mo'ljallangan echki sutiga asoslangan quruq sutli ichimlik. Mahsulot sigir sutidan butunlay voz kechadi va sigir suti oqsillariga sezgir bolalar uchun alternativa taklif qiladi. Ichimlik faol o'sishni, jismoniy va aqliy rivojlanishni qo'llab-quvvatlash hamda bolaning immunitet tizimini mustahkamlash uchun barcha zarur ozuqa moddalari bilan boyitilgan.";

    [ObservableProperty]
    private int quantity = 1;

    public IRelayCommand BackCommand { get; }

    public DetailProductPageViewModel()
    {
        BackCommand = new RelayCommand(OnBack);

        ProductImages = new ObservableCollection<ProductImageDetailInfo>
        {
            new ProductImageDetailInfo("product_2.png"),
            new ProductImageDetailInfo("product_2.png"),
            new ProductImageDetailInfo("product_2.png"),
            new ProductImageDetailInfo("product_2.png")
        };

        SimilarProducts = new ObservableCollection<MainProductCardItem>
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

    private async void OnBack()
    {
        if (Application.Current?.MainPage == null)
            return;

        await Shell.Current.GoToAsync("..");
    }
}