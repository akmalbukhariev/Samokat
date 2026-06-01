using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Api.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models.Requests;
using Models.Responses;
using Ninimum.Models.Dto;
using Ninimum.Models.Main;
using Ninimum.Views.DetailProduct;
using Ninimum.Views.Formalization;
using Utils;

namespace Ninimum.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty] private ICommand notificationTapCommand;
    [ObservableProperty] private ICommand buyNowCommand;
    [ObservableProperty] private ICommand menuCommand;
    [ObservableProperty] private ObservableCollection<AdBannerItem> adBanners;
    [ObservableProperty] private ICommand purchaseBannerCommand;
    [ObservableProperty] private ICommand clickProductCommand;
    [ObservableProperty] private ICommand clickTomorrowCommand;
    [ObservableProperty] private ObservableCollection<MainProductCardItem> products;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isRefreshing;

    private readonly UserApiService apiService;

    private int offset = 0;
    private const int PageSize = 10;
    private bool hasMoreItems = true;

    public IAsyncRelayCommand LoadMoreCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }

    public MainPageViewModel(UserApiService apiService)
    {
        this.apiService = apiService;

        Products = new ObservableCollection<MainProductCardItem>();

        NotificationTapCommand = new Command(OnNotificationTapped);
        PurchaseBannerCommand = new Command<AdBannerItem>(OnPurchaseBanner);
        ClickProductCommand = new Command<MainProductCardItem>(ProductClicked);
        ClickTomorrowCommand = new Command<MainProductCardItem>(TomorrowClicked);

        LoadMoreCommand = new AsyncRelayCommand(LoadMoreAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);

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
    }

    public async Task LoadInitialAsync()
    {
        offset = 0;
        hasMoreItems = true;
        Products.Clear();

        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync(bool isRefresh = false)
    {
        if (IsLoading || (!hasMoreItems && !isRefresh))
            return;

        try
        {
            if (isRefresh)
            {
                IsRefreshing = true;
                offset = 0;
                hasMoreItems = true;
                Products.Clear();
            }
            else
            {
                IsLoading = true;
            }

            var request = new ProductListRequest
            {
                categoryId = 1,
                pageSize = PageSize,
                offset = offset
            };

            ProductListResponse response = await apiService.GetProductList(request);

            if (response.resultCode == ApiResult.SUCCESS.GetCodeToString())
            {
                List<ProductDto> items = response.resultData;

                if (items == null || items.Count == 0)
                {
                    hasMoreItems = false;
                    return;
                }

                foreach (var item in items)
                {
                    Products.Add(ToMainProductCardItem(item));
                }

                offset += items.Count;

                if (items.Count < PageSize)
                    hasMoreItems = false;
            }
            else
            {
                hasMoreItems = false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] LoadProductsAsync: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
        }
    }

    private MainProductCardItem ToMainProductCardItem(ProductDto item)
    {
        var images = new ObservableCollection<MainProductImageItem>();

        if (item.images != null && item.images.Count > 0)
        {
            foreach (var img in item.images.OrderBy(x => x.sort_order ?? 0))
            {
                images.Add(new MainProductImageItem
                {
                    ImageSource = img.image_url
                });
            }
        }
        else
        {
            images.Add(new MainProductImageItem
            {
                ImageSource = "product_1.png"
            });
        }

        return new MainProductCardItem
        {
            Price = item.price?.ToString("N0").Replace(",", " ") ?? "0",
            Subscription_price = item.subscription_price?.ToString("N0").Replace(",", " ") ?? "0",
            Title = item.name ?? "",
            Rating = 4.8,
            ReviewCount = 301,
            ActionText = "+ Ertaga",
            Images = images
        };
    }

    private async Task LoadMoreAsync()
    {
        await LoadProductsAsync();
    }

    private async Task RefreshAsync()
    {
        await LoadProductsAsync(isRefresh: true);
    }

    private async void ProductClicked(MainProductCardItem product)
    {
        await AppNavigatorService.NavigateTo(nameof(DetailProductPage));
    }

    private async void TomorrowClicked(MainProductCardItem product)
    {
        await AppNavigatorService.NavigateTo(nameof(FormalizationPage));
    }

    private async void OnNotificationTapped()
    {
        await Application.Current.MainPage.DisplayAlert("Info", "Notification clicked", "OK");
    }
    
    private async void OnPurchaseBanner(AdBannerItem? item)
    {
        if (item == null)
            return;

        await Application.Current!.MainPage!.DisplayAlert("Purchase", item.Title, "OK");
    }
}