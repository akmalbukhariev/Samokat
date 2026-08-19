using Api.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models.Requests;
using Models.Responses;
using Ninimum.Models;
using Ninimum.Services;
using Ninimum.Views.Formalization;
using Ninimum.Views.Payment;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Windows.Input;
using Utils;

namespace Ninimum.ViewModels;

public partial class CartPageViewModel : ObservableObject
{
    private int offset = 0;
    private const int PageSize = 10;
    private bool hasMoreItems = true;
    private bool isRequestRunning = false;
    private readonly HashSet<int> loadedCartIds = new();

    private readonly UserApiService apiService;
    private readonly AppControl appControl;

    public ObservableCollection<CartProductItemModel> CartProducts { get; } = new();

    public string CartCountText => $"Savatchadagi mahsulotlar soni {CartProducts.Count} ta";

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isRefreshing;

    [ObservableProperty] private string selectAllIcon = "ic_uncheck.png";
    [ObservableProperty] private string summaryTopText = "Savatchadagi tanlangan 0 ta mahsulotni";
    [ObservableProperty] private string bottomTotalPrice = "0 so’m";
    [ObservableProperty] private string bottomSelectedCountText = "0 ta mahsulot";
    [ObservableProperty] private string totalRegularPrice = "0";
    [ObservableProperty] private string totalTariffPrice = "0";

    [ObservableProperty] private IAsyncRelayCommand loadMoreCommand;
    [ObservableProperty] private IAsyncRelayCommand refreshCommand;

    public CartPageViewModel(UserApiService apiService, AppControl appControl)
    {
        this.apiService = apiService;
        this.appControl = appControl;

        LoadMoreCommand = new AsyncRelayCommand(LoadMoreAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
    }
    
    public async Task LoadCartListAsync()
    {
        offset = 0;
        hasMoreItems = true;
        loadedCartIds.Clear();
        CartProducts.Clear();

        await LoadCartProductsAsync();
    }

    private async Task LoadCartProductsAsync(bool isRefresh = false)
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
                loadedCartIds.Clear();
                CartProducts.Clear();
            }
            else
            {
                IsLoading = true;
            }

            CartResponse response = await apiService.GetCartList(new CartListRequest
            {
                user_id = (int)appControl.userDto.id,
                pageSize = PageSize,
                offset = offset
            });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
                return;

            var items = response.resultData;

            if (items == null || items.Count == 0)
            {
                hasMoreItems = false;
                return;
            }

            foreach (var product in items)
            {
                if (loadedCartIds.Contains(product.cart_id))
                    continue;

                loadedCartIds.Add(product.cart_id);

                CartProducts.Add(new CartProductItemModel
                {
                    CartId = product.cart_id,
                    ProductId = (int)(product.id ?? 0),
                    ProductImageSource = product.images?.FirstOrDefault()?.image_url ?? "",
                    Title = product.name ?? "",

                    Price = FormatPrice(product.price ?? 0),
                    SubscriptionPrice = FormatPrice(product.subscription_price ?? 0),

                    PriceValue = (int)(product.price ?? 0),
                    SubscriptionPriceValue = (int)(product.subscription_price ?? 0),

                    IsChecked = true,
                    Quantity = product.quantity <= 0 ? 1 : product.quantity,
                    ParentChangedCommand = new RelayCommand(UpdateSummary)
                });
            }

            offset += items.Count;

            if (items.Count < PageSize)
                hasMoreItems = false;

            OnPropertyChanged(nameof(CartCountText));
            UpdateSummary();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] LoadCartProductsAsync: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
            isRequestRunning = false;
        }
    }

    private async Task LoadMoreAsync()
    {
        await LoadCartProductsAsync();
    }

    private async Task RefreshAsync()
    {
        AppVibrationService.Click();

        await LoadCartProductsAsync(isRefresh: true);
    }

    [RelayCommand]
    private void ToggleSelectAll()
    {
        bool shouldSelectAll = CartProducts.Any(x => !x.IsChecked);

        foreach (var item in CartProducts)
            item.IsChecked = shouldSelectAll;

        UpdateSummary();
    }

    [RelayCommand]
    private async Task DeleteSelected()
    {
        var selected = CartProducts.Where(x => x.IsChecked).ToList();

        if (!selected.Any())
            return;

        try
        {
            IsLoading = true;
            foreach (var item in selected)
            {
                Response response = await apiService.DeleteCartProduct(new DeleteCartRequest
                {
                    cart_id = item.CartId,
                    user_id = (int)appControl.userDto.id
                });

                if (response.resultCode == ApiResult.SUCCESS.GetCodeToString())
                {
                    CartProducts.Remove(item);
                    loadedCartIds.Remove(item.CartId);
                    offset = Math.Max(0, offset - 1);
                }
            }
        }
        catch (Exception ex)
        {

        }
        finally
        {
            IsLoading = false;
        }

        OnPropertyChanged(nameof(CartCountText));
        UpdateSummary();
    }

    [RelayCommand]
    private void JoinTariff()
    {
    }

    [RelayCommand]
    private async Task Checkout()
    {
        AppVibrationService.Click();

        var selectedProducts = CartProducts
            .Where(x => x.IsChecked)
            .ToList();

        if (!selectedProducts.Any())
        {
            await Shell.Current.DisplayAlert(
                "Xatolik",
                "Iltimos, kamida bitta mahsulotni tanlang.",
                "OK");

            return;
        }

        FormalizationNavigationStore.Data = new FormalizationData
        {
            UserId = (long)appControl.userDto.id,
            AddressText = appControl.userDto.address,

            Products = selectedProducts
                .Select(x => new FormalizationProductItem
                {
                    ProductId = x.ProductId,
                    Name = x.Title,
                    ImageSource = x.ProductImageSource,
                    Quantity = x.Quantity,
                    Price = x.PriceValue
                })
                .ToList()
        };

        await AppNavigatorService.NavigateTo(nameof(FormalizationPage));
    } 

    private void UpdateSummary()
    {
        int selectedCount = CartProducts.Count(x => x.IsChecked);

        int tariffTotal = CartProducts
            .Where(x => x.IsChecked)
            .Sum(x => x.SubscriptionPriceValue * x.Quantity);

        int regularTotal = CartProducts
            .Where(x => x.IsChecked)
            .Sum(x => x.PriceValue * x.Quantity);

        SummaryTopText = $"Savatchadagi tanlangan {selectedCount} ta mahsulotni";

        TotalTariffPrice = FormatPrice(tariffTotal);
        TotalRegularPrice = FormatPrice(regularTotal);
        BottomTotalPrice = $"{FormatPrice(regularTotal)} so’m";
        BottomSelectedCountText = $"{selectedCount} ta mahsulot";

        SelectAllIcon = CartProducts.Any() && CartProducts.All(x => x.IsChecked)
            ? "ic_check.png"
            : "ic_uncheck.png";

        OnPropertyChanged(nameof(CartCountText));
    }

    private string FormatPrice(double value)
    {
        return string.Format("{0:N0}", value).Replace(",", " ");
    }
}