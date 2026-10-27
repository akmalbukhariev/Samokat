using Ninimum.Resources.Languages;
using Api.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models.Requests;
using Models.Responses;
using Ninimum.Models;
using Ninimum.Services;
using Ninimum.Views.Formalization;
using Ninimum.Views.MyTariff;
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
    // Products start unchecked; the customer's own choices are kept while the app runs (reloads keep them).
    private static readonly HashSet<int> checkedCartIds = new();
    private long? activeTariffSubscriptionId;

    private readonly UserApiService apiService;
    private readonly AppControl appControl;

    public ObservableCollection<CartProductItemModel> CartProducts { get; } = new();

    public string CartCountText => string.Format(AppResource.CartProductCount, CartProducts.Count);

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private bool hasCartItems;

    [ObservableProperty] private string selectAllIcon = "ic_uncheck.png";
    [ObservableProperty] private string summaryTopText = string.Format(AppResource.SelectedCartProducts, 0);
    [ObservableProperty] private string bottomTotalPrice = AppResource.Text0UZS;
    [ObservableProperty] private string bottomSelectedCountText = AppResource.Text0Products;
    [ObservableProperty] private string totalRegularPrice = "0";
    [ObservableProperty] private string totalTariffPrice = "0";
    [ObservableProperty] private bool hasActiveSubscription;
    [ObservableProperty] private string activeTariffText = string.Empty;
    // Friendly summary box: what is being bought, the price, and the money saved (or that could be saved).
    [ObservableProperty] private string summaryTitle = string.Empty;
    [ObservableProperty] private string summaryPrice = string.Empty;
    [ObservableProperty] private string summarySaving = string.Empty;
    [ObservableProperty] private bool hasSummaryPrice;
    [ObservableProperty] private bool hasSummarySaving;
    // Price without a tariff vs with a tariff, shown when the tariff makes the cart cheaper.
    [ObservableProperty] private bool hasPriceCompare;
    [ObservableProperty] private string compareRegularLabel = string.Empty;
    [ObservableProperty] private string compareRegularPrice = string.Empty;
    [ObservableProperty] private string compareTariffLabel = string.Empty;
    [ObservableProperty] private string compareTariffPrice = string.Empty;
    private string activeTariffName = string.Empty;

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
        UpdateSummary();

        await LoadActiveSubscriptionAsync();
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
                UpdateSummary();
            }
            else if (offset == 0 && CartProducts.Count == 0)
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
                UpdateSummary();
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
                    StockQuantity = product.stock_quantity ?? 0,

                    IsChecked = checkedCartIds.Contains(product.cart_id),
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

        await LoadActiveSubscriptionAsync();
        await LoadCartProductsAsync(isRefresh: true);
        AppService.GetRequired<CartBadgeService>().RequestRefresh();
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
                    PageDataRefreshState.MarkDirty(PageDataRefreshState.Cart);
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
    private async Task JoinTariff()
    {
        AppVibrationService.Click();
        await AppNavigatorService.NavigateTo(nameof(TariffsPage));
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
                AppResource.Error,
                AppResource.PleaseSelectAtLeastOneProduct,
                AppResource.Ok);

            return;
        }

        await LoadActiveSubscriptionAsync();

        FormalizationNavigationStore.Data = new FormalizationData
        {
            UserId = (long)appControl.userDto.id,
            AddressText = appControl.userDto.address,
            AddressLatitude = appControl.userDto.location_latitude,
            AddressLongitude = appControl.userDto.location_longitude,
            TariffSubscriptionId = activeTariffSubscriptionId,

            Products = selectedProducts
                .Select(x => new FormalizationProductItem
                {
                    ProductId = x.ProductId,
                    Name = x.Title,
                    ImageSource = x.ProductImageSource,
                    Quantity = x.Quantity,
                    Price = GetEffectivePrice(x)
                })
                .ToList()
        };

        await AppNavigatorService.NavigateTo(nameof(FormalizationPage));
    } 

    private async Task LoadActiveSubscriptionAsync()
    {
        try
        {
            ActiveSubscriptionResponse response = await apiService.GetActiveSubscription(new ActiveSubscriptionRequest
            {
                userId = appControl.userDto.id ?? 0
            });

            var subscription = response.resultData;
            HasActiveSubscription =
                response.resultCode == ApiResult.SUCCESS.GetCodeToString() &&
                subscription != null &&
                string.Equals(subscription.subscriptionStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase);

            activeTariffSubscriptionId = HasActiveSubscription
                ? subscription!.subscriptionId
                : null;

            ActiveTariffText = HasActiveSubscription
                ? string.Format(AppResource.ActiveTariff, subscription!.tariffName)
                : string.Empty;
            activeTariffName = HasActiveSubscription ? subscription!.tariffName ?? string.Empty : string.Empty;
        }
        catch (Exception ex)
        {
            HasActiveSubscription = false;
            activeTariffSubscriptionId = null;
            ActiveTariffText = string.Empty;
            Debug.WriteLine($"[ERROR] LoadActiveSubscriptionAsync: {ex.Message}");
        }

        UpdateSummary();
    }

    private int GetEffectivePrice(CartProductItemModel product)
    {
        if (HasActiveSubscription && product.SubscriptionPriceValue > 0)
            return product.SubscriptionPriceValue;

        return product.PriceValue;
    }

    private void UpdateSummary()
    {
        HasCartItems = CartProducts.Count > 0;

        foreach (var item in CartProducts)
        {
            if (item.IsChecked) checkedCartIds.Add(item.CartId);
            else checkedCartIds.Remove(item.CartId);
        }

        int selectedCount = CartProducts.Count(x => x.IsChecked);

        int tariffTotal = CartProducts
            .Where(x => x.IsChecked)
            .Sum(x => (x.SubscriptionPriceValue > 0 ? x.SubscriptionPriceValue : x.PriceValue) * x.Quantity);

        int regularTotal = CartProducts
            .Where(x => x.IsChecked)
            .Sum(x => x.PriceValue * x.Quantity);

        SummaryTopText = string.Format(AppResource.SelectedCartProducts, selectedCount);

        TotalTariffPrice = FormatPrice(tariffTotal);
        TotalRegularPrice = FormatPrice(regularTotal);
        int effectiveTotal = CartProducts
            .Where(x => x.IsChecked)
            .Sum(x => GetEffectivePrice(x) * x.Quantity);

        BottomTotalPrice = string.Format(AppResource.UZS_02854f, effectiveTotal);
        UpdateFriendlySummary(selectedCount, effectiveTotal, regularTotal, tariffTotal);
        BottomSelectedCountText = string.Format(AppResource.Products_48d782, selectedCount);

        SelectAllIcon = CartProducts.Any() && CartProducts.All(x => x.IsChecked)
            ? "ic_check.png"
            : "ic_uncheck.png";

        OnPropertyChanged(nameof(CartCountText));
    }

    private static string L(string uz, string ru, string en)
    {
        var lang = (AppResource.Culture ?? System.Globalization.CultureInfo.CurrentUICulture).TwoLetterISOLanguageName;
        return lang == "ru" ? ru : lang == "en" ? en : uz;
    }

    private void UpdateFriendlySummary(int selectedCount, int total, int regularTotal, int tariffTotal)
    {
        int saving = regularTotal - tariffTotal;
        HasSummaryPrice = selectedCount > 0;
        HasPriceCompare = selectedCount > 0 && saving > 0;
        CompareRegularLabel = L("Tarifsiz narx", "Цена без тарифа", "Price without a plan");
        CompareRegularPrice = FormatPrice(regularTotal) + " " + L("so‘m", "сум", "so‘m");
        CompareTariffLabel = HasActiveSubscription
            ? L($"{activeTariffName} tarifi bilan", $"С тарифом {activeTariffName}", $"With your {activeTariffName} plan")
            : L("Tarif bilan narx", "Цена с тарифом", "Price with a plan");
        CompareTariffPrice = FormatPrice(tariffTotal) + " " + L("so‘m", "сум", "so‘m");
        if (selectedCount == 0)
        {
            SummaryTitle = L("Xarid qilish uchun mahsulotlarni belgilang", "Отметьте товары, которые хотите купить", "Tick the products you want to buy");
            SummarySaving = string.Empty;
            HasSummarySaving = false;
            return;
        }

        SummaryTitle = selectedCount == 1
            ? L("Siz 1 ta mahsulot xarid qilyapsiz", "Вы покупаете 1 товар", "You are buying 1 product")
            : L($"Siz {selectedCount} ta mahsulot xarid qilyapsiz", $"Вы покупаете {selectedCount} товара(ов)", $"You are buying {selectedCount} products");
        SummaryPrice = string.Format(AppResource.UZS_02854f, total).Replace(",", " ");

        string money = FormatPrice(saving);
        if (saving <= 0)
        {
            SummarySaving = HasActiveSubscription
                ? L($"{activeTariffName} tarifingiz faol", $"Ваш тариф {activeTariffName} активен", $"Your {activeTariffName} plan is active")
                : string.Empty;
        }
        else if (HasActiveSubscription)
            SummarySaving = L($"🎉 {activeTariffName} tarifi bilan {money} so‘m tejayapsiz", $"🎉 С тарифом {activeTariffName} вы экономите {money} сум", $"🎉 You save {money} so‘m with your {activeTariffName} plan");
        else
            SummarySaving = L($"Tarifga obuna bo‘lsangiz, {money} so‘m tejaysiz", $"С подпиской на тариф вы сэкономите {money} сум", $"Subscribe to a plan and save {money} so‘m");
        HasSummarySaving = !string.IsNullOrEmpty(SummarySaving);
    }

    private string FormatPrice(double value)
    {
        return string.Format("{0:N0}", value).Replace(",", " ");
    }
}