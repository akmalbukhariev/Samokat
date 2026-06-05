
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
using Ninimum.Views.Formalization;
using Utils;

namespace Ninimum.ViewModels;

public partial class FavoritePageViewModel : ObservableObject
{
    #region Properties
    private int offset = 0;
    private const int PageSize = 10;
    private bool hasMoreItems = true;
    private readonly HashSet<long> loadedProductIds = new();
    private bool isRequestRunning = false;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private ObservableCollection<MainProductCardItem> products;
    #endregion

    #region Properties command
    [ObservableProperty] private ICommand likeCommand;
    [ObservableProperty] private ICommand clickProductCommand;
    [ObservableProperty] private ICommand clickTomorrowCommand;
    [ObservableProperty] private IAsyncRelayCommand loadMoreCommand;
    [ObservableProperty] private IAsyncRelayCommand refreshCommand;
    #endregion

    private readonly AppControl appControl;
    private readonly UserApiService apiService;
    public FavoritePageViewModel(AppControl appControl, UserApiService apiService)
    {
        this.appControl = appControl;
        this.apiService = apiService;

        Products = new ObservableCollection<MainProductCardItem>();

        clickProductCommand = new Command<MainProductCardItem>(ProductClicked);
        ClickTomorrowCommand = new Command<MainProductCardItem>(TomorrowClicked);

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

        await LoadProductsAsync();
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
            else
            {
                IsLoading = true;
            }

            var request = new FavoriteListRequest
            {
                user_id = (int)appControl.userDto.id,
                pageSize = PageSize,
                offset = offset
            };
 
            ProductListResponse response = await apiService.GetFavoriteProductList(request);

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
            Rating = 4.8,
            ReviewCount = 301,
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
        await LoadProductsAsync(isRefresh: true);
    }

    private async void ProductLiked(MainProductCardItem product)
    {
        if (product == null)
            return;

        try
        {
            Response response = await apiService.DeleteFavoriteProduct(
                new DeleteFavoriteRequest
                {
                    user_id = (int)appControl.userDto.id,
                    product_id = product.ProductId
                });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
                return;

            Products.Remove(product);
            loadedProductIds.Remove(product.ProductId);

            if (offset > 0)
                offset--;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] ProductLiked: {ex.Message}");
        }
    }

    private async void ProductClicked(MainProductCardItem product)
    {
        //await AppNavigatorService.NavigateTo(nameof(DetailProductPage));
    }

    private async void TomorrowClicked(MainProductCardItem product)
    {
        //await AppNavigatorService.NavigateTo(nameof(FormalizationPage));
    }

}