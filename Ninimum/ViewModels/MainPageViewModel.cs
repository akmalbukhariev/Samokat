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
using Ninimum.Services;
using Ninimum.Views.DetailProduct;
using Ninimum.Views.Formalization;
using Utils;

namespace Ninimum.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    #region Properties
    private int offset = 0;
    private const int PageSize = 10;
    private bool hasMoreItems = true;
    private readonly HashSet<long> loadedProductIds = new();
    private bool isRequestRunning = false;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool showLikedView;
    [ObservableProperty] private bool isLikedViewLiked;
    [ObservableProperty] private bool showCartView;
    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private bool showBanners;
    [ObservableProperty] private ObservableCollection<AdBannerItem> adBanners;
    [ObservableProperty] private ObservableCollection<MainProductCardItem> products;
    #endregion

    #region Properties command
    [ObservableProperty] private ICommand notificationTapCommand;
    [ObservableProperty] private ICommand buyNowCommand;
    [ObservableProperty] private ICommand menuCommand;
    [ObservableProperty] private ICommand purchaseBannerCommand;
    [ObservableProperty] private ICommand clickProductCommand;
    [ObservableProperty] private ICommand clickCartCommand;
    [ObservableProperty] private ICommand likeCommand;
    [ObservableProperty] private IAsyncRelayCommand loadMoreCommand;
    [ObservableProperty] private IAsyncRelayCommand refreshCommand;
    #endregion

    private readonly AppControl appControl;
    private readonly UserApiService apiService;
    public MainPageViewModel(AppControl appControl, UserApiService apiService)
    {
        this.appControl = appControl;
        this.apiService = apiService;

        AdBanners = new ObservableCollection<AdBannerItem>();
        Products = new ObservableCollection<MainProductCardItem>();

        NotificationTapCommand = new Command(OnNotificationTapped);
        PurchaseBannerCommand = new Command<AdBannerItem>(OnPurchaseBanner);
        ClickProductCommand = new Command<MainProductCardItem>(ProductClicked);
        ClickCartCommand = new Command<MainProductCardItem>(CartClicked);

        LikeCommand = new Command<MainProductCardItem>(ProductLiked);
        LoadMoreCommand = new AsyncRelayCommand(LoadMoreAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
    }

    public async Task LoadInitialAsync()
    {
        offset = 0;
        hasMoreItems = true;
        loadedProductIds.Clear();
        Products.Clear();

        await LoadBannersAsync();
        await LoadProductsAsync();
    }    

    private async Task LoadBannersAsync()
    {
        try
        {
            BannerListResponse response = await apiService.GetBannerList();

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
                return;

            var items = response.resultData;

            if (items == null || items.Count == 0)
                return;

            AdBanners.Clear();

            foreach (var item in items.OrderBy(x => x.sort_order ?? 0))
            {
                AdBanners.Add(new AdBannerItem
                {
                    Id = (int)item.id,
                    ProductId = (int)item.product_id,
                    Title = item.short_description ?? "",
                    Image = item.image_url ?? ""
                });
            }

            ShowBanners = AdBanners.Count > 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] LoadBannersAsync: {ex.Message}");
        }
    }
    
    private async Task LoadProductsAsync(bool isRefresh = false)
    {
        if (isRequestRunning || (!hasMoreItems && !isRefresh))
            return;

        try
        {
            isRequestRunning = true;

            if (isRefresh)
            {
                IsRefreshing = true;
                offset = 0;
                hasMoreItems = true;
                loadedProductIds.Clear();
                Products.Clear();
            }
            else if (offset == 0 && Products.Count == 0)
            {
                // Full-screen LoadingView is only for the first/full load.
                // Infinite scrolling must not cover an already loaded page.
                IsLoading = true;
            }

            var request = new ProductListRequest
            {
                user_id = appControl.CurrentUserId,
                category_id = 1,
                pageSize = PageSize,
                offset = offset
            };
 
            ProductResponse response = await apiService.GetProductList(request);

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
                return;

            var items = response.resultData;

            if (items == null || items.Count == 0)
            {
                hasMoreItems = false;
                return;
            }

            foreach (var item in items)
            {
                if (item.id == null)
                    continue;

                if (loadedProductIds.Contains(item.id.Value))
                    continue;

                loadedProductIds.Add(item.id.Value);
                Products.Add(ToMainProductCardItem(item));
            }

            offset += items.Count;

            if (items.Count < PageSize)
                hasMoreItems = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] LoadProductsAsync: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
            isRequestRunning = false;
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
            Liked = item.liked,
            Rating = item.average_rating ?? 0,
            ReviewCount = item.review_count ?? 0,
            ProductId = (int)item.id,
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
        AppVibrationService.Click();

        await LoadBannersAsync();
        await LoadProductsAsync(isRefresh: true);
    }

    private async void ProductLiked(MainProductCardItem product)
    {
        if (product == null)
            return;

        if (!await appControl.EnsureAuthenticatedAsync())
            return;

        bool oldLiked = product.Liked;

        try
        {
            product.Liked = !oldLiked;

            Response response;

            if (!oldLiked)
            {
                response = await apiService.AddFavoriteProduct(
                    new AddFavoriteRequest
                    {
                        user_id = appControl.CurrentUserId,
                        product_id = product.ProductId
                    });
            }
            else
            {
                response = await apiService.DeleteFavoriteProduct(
                    new DeleteFavoriteRequest
                    {
                        user_id = appControl.CurrentUserId,
                        product_id = product.ProductId
                    });
            }

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                product.Liked = oldLiked;
                return;
            }

            IsLikedViewLiked = product.Liked;
            ShowLikedView = true;
            PageDataRefreshState.MarkDirty(PageDataRefreshState.Favorites);
        }
        catch
        {
            product.Liked = oldLiked;
        }
    }

    private async void ProductClicked(MainProductCardItem product)
    {
        await AppNavigatorService.NavigateTo($"{nameof(DetailProductPage)}?productId={product.ProductId}");
    }

    private async void CartClicked(MainProductCardItem product)
    {
        if (product == null || product.IsCartLoading)
            return;

        if (!await appControl.EnsureAuthenticatedAsync())
            return;

        try
        {
            product.IsCartLoading = true;

            Response response = await apiService.AddCartProduct(
                new AddCartRequest
                {
                    user_id = appControl.CurrentUserId,
                    product_id = product.ProductId,
                    quantity = 1
                });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                await AlertService.ShowAlertAsync("Xatolik", response.resultMsg);
                return;
            }

            ShowCartView = true;
            PageDataRefreshState.MarkDirty(PageDataRefreshState.Cart);
        }
        catch
        {
            await AlertService.ShowAlertAsync("Xatolik", "Mahsulotni savatchaga qo’shib bo’lmadi.");
        }
        finally
        {
            product.IsCartLoading = false;
        }
    }

    private async void OnNotificationTapped()
    {
        if (!await appControl.EnsureAuthenticatedAsync())
            return;

        await Application.Current.MainPage.DisplayAlert("Info", "Notification clicked", "OK");
    }
    
    private async void OnPurchaseBanner(AdBannerItem? item)
    {
        if (item == null)
            return;

        await Application.Current!.MainPage!.DisplayAlert("Purchase", item.Title, "OK");
    }
}