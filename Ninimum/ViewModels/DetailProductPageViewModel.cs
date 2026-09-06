using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Api.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models.Requests;
using Models.Responses;
using Ninimum.Models;
using Ninimum.Models.Dto;
using Ninimum.Models.Main;
using Ninimum.Services;
using Ninimum.Views.DetailProduct;
using Ninimum.Views.Formalization;
using Ninimum.Views.PaymentCard;
using Utils;

namespace Ninimum.ViewModels;

[QueryProperty(nameof(ProductId), "productId")]
public partial class DetailProductPageViewModel : ObservableObject
{
    #region Properties
    [ObservableProperty] private long productId;
    [ObservableProperty] private bool productLiked;
    private int offset = 0;
    private const int PageSize = 10;
    private bool hasMoreItems = true;
    private bool isRequestRunning = false;
    [ObservableProperty] private double similarProductsHeight;
    [ObservableProperty] private ImageSource? previewImageSource;
    [ObservableProperty] private bool showImagePreview;
    private readonly HashSet<long> loadedProductIds = new();
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private bool showLikedView;
    [ObservableProperty] private bool showCartView;
    [ObservableProperty] private bool isLikedViewLiked;

    [ObservableProperty] private ObservableCollection<MainProductCardItem> similarProducts = new();

    [ObservableProperty] private ObservableCollection<ProductImageDetailInfo> productImages = new();
    [ObservableProperty] private ObservableCollection<string> stars = new();
    [ObservableProperty] private int currentImageIndex;
    [ObservableProperty] private string productTitle = ".......";

    [ObservableProperty] private string stockText = ".......";
    [ObservableProperty] private int ratingStarCount = 0;

    [ObservableProperty] private string rating = ".......";

    [ObservableProperty] private string reviewText = ".......";

    [ObservableProperty] private string subscriptionPrice = ".......";

    [ObservableProperty] private string regularPrice = ".......";

    [ObservableProperty] private string deliveryLabel = "Yetkazib berish";

    [ObservableProperty] private string subscriptionDeliveryText = "bepul ∙ minut";

    [ObservableProperty] private string regularDeliveryText = "pullik ∙ 1 kun";

    [ObservableProperty] private string description = ".......";

    [ObservableProperty] private int quantity = 1;
    [ObservableProperty] private string finalPrice = "0 so’m";
    private double FinalPriceValue = 0.0;
    private bool hasActiveSubscription;
    #endregion

    #region Commands
    [ObservableProperty] private ICommand likeSimilarProductCommand;
    [ObservableProperty] private ICommand clickProductCommand;
    [ObservableProperty] private ICommand clickCartCommand;
    [ObservableProperty] private IAsyncRelayCommand loadMoreCommand;
    [ObservableProperty] private IAsyncRelayCommand refreshCommand;
    #endregion

    private readonly AppControl appControl;
    private readonly UserApiService apiService;
    public DetailProductPageViewModel(AppControl appControl, UserApiService apiService)
    {
        this.appControl = appControl;
        this.apiService = apiService;

        LikeSimilarProductCommand = new Command<MainProductCardItem>(SimilarProductLiked);
        ClickProductCommand = new Command<MainProductCardItem>(ProductClicked);
        ClickCartCommand = new Command<MainProductCardItem>(CartClicked);

        LoadMoreCommand = new AsyncRelayCommand(LoadMoreAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);

        SimilarProducts = new ObservableCollection<MainProductCardItem>();
        ProductImages = new ObservableCollection<ProductImageDetailInfo>();
    }

    public async Task<bool> ToggleProductLikeAsync()
    {
        if (!await appControl.EnsureAuthenticatedAsync())
            return false;

        bool oldLiked = ProductLiked;

        try
        {
            ProductLiked = !oldLiked;

            Response response;

            if (!oldLiked)
            {
                response = await apiService.AddFavoriteProduct(
                    new AddFavoriteRequest
                    {
                        user_id = appControl.CurrentUserId,
                        product_id = (int)ProductId
                    });
            }
            else
            {
                response = await apiService.DeleteFavoriteProduct(
                    new DeleteFavoriteRequest
                    {
                        user_id = appControl.CurrentUserId,
                        product_id = (int)ProductId
                    });
            }

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                ProductLiked = oldLiked;
                return false;
            }

            IsLikedViewLiked = ProductLiked;
            ShowLikedView = true;
            PageDataRefreshState.MarkDirty(PageDataRefreshState.Favorites);
            PageDataRefreshState.MarkDirty(PageDataRefreshState.Main);

            return true;
        }
        catch
        {
            ProductLiked = oldLiked;
            return false;
        }
    }

    private async void ProductClicked(MainProductCardItem product)
    {
        await AppNavigatorService.NavigateTo($"{nameof(DetailProductPage)}?productId={product.ProductId}");
    }

    private async void CartClicked(MainProductCardItem product)
    {
        //await AppNavigatorService.NavigateTo(nameof(FormalizationPage));
    }

    partial void OnProductIdChanged(long value)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await LoadInitialAsync();
        });
    }

    private async void SimilarProductLiked(MainProductCardItem product)
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
        }
        catch (Exception ex)
        {
            product.Liked = oldLiked;
        }
    }

    public async Task LoadInitialAsync()
    {
        offset = 0;
        hasMoreItems = true;

        loadedProductIds.Clear();

        SimilarProducts.Clear();
        ProductImages.Clear();

        await LoadActiveSubscriptionAsync();
        await LoadProductDetailAsync();
        await LoadSimilarProductsAsync();
    }

    private async Task LoadProductDetailAsync()
    {
        try
        {
            IsLoading = true;

            DetailProductResponse response = await apiService.GetProductDetail(
                    new ProductDetailRequest
                    {
                        user_id = appControl.CurrentUserId,
                        product_id = (int)ProductId
                    });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
                return;

            ProductDto? product = response.resultData;

            if (product == null)
                return;

            ProductTitle = product.name ?? "";
            Description = product.description ?? "";
            ProductLiked = product.liked;
            StockText = $"Omborda {product.stock_quantity ?? 0} dona mavjud";
            Rating = (product.average_rating ?? 0).ToString("0.0");
            RatingStarCount = (int)Math.Round(product.average_rating ?? 0);
            Stars.Clear();

            int filled = (int)Math.Round((double)product.average_rating);

            for (int i = 0; i < 5; i++)
            {
                Stars.Add(i < filled
                    ? "star.png"
                    : "star_gray.png");
            }
            ReviewText = $"{product.review_count} sharhlar";
            SubscriptionPrice = $"{product.subscription_price?.ToString("N0").Replace(",", " ") ?? "0"} so’m";
            RegularPrice = $"{product.price?.ToString("N0").Replace(",", " ") ?? "0"} so’m";

            double regularPriceValue = product.price ?? 0.0;
            double subscriptionPriceValue = product.subscription_price ?? 0.0;

            FinalPriceValue = hasActiveSubscription && subscriptionPriceValue > 0
                ? subscriptionPriceValue
                : regularPriceValue;

            FinalPrice = $"{FinalPriceValue.ToString("N0").Replace(",", " ")} so’m";
            ProductImages.Clear();

            if (product.images != null && product.images.Count > 0)
            {
                foreach (var image in product.images.OrderBy(x => x.sort_order ?? 0))
                {
                    ProductImages.Add(new ProductImageDetailInfo(image.image_url ?? "no_image.png"));
                }
            }
            else
            {
                ProductImages.Add(new ProductImageDetailInfo("no_image.png"));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] LoadProductDetailAsync: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadActiveSubscriptionAsync()
    {
        if (!appControl.IsAuthenticated)
        {
            hasActiveSubscription = false;
            return;
        }

        try
        {
            ActiveSubscriptionResponse response = await apiService.GetActiveSubscription(new ActiveSubscriptionRequest
            {
                userId = appControl.CurrentUserId
            });

            var subscription = response.resultData;

            hasActiveSubscription =
                response.resultCode == ApiResult.SUCCESS.GetCodeToString() &&
                subscription != null &&
                string.Equals(subscription.subscriptionStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            hasActiveSubscription = false;
            Debug.WriteLine($"[ERROR] LoadActiveSubscriptionAsync: {ex.Message}");
        }
    }

    private async Task LoadSimilarProductsAsync(bool isRefresh = false)
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
                SimilarProducts.Clear();
            }

            ProductResponse response = await apiService.GetSimilarProduct(
                    new SimilarProductListRequest
                    {
                        user_id = appControl.CurrentUserId,
                        product_id = (int)ProductId,
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

            foreach (var item in items)
            {
                if (item.id == null)
                    continue;

                if (loadedProductIds.Contains(item.id.Value))
                    continue;

                loadedProductIds.Add(item.id.Value);

                SimilarProducts.Add(ToMainProductCardItem(item));
            }

            SimilarProductsHeight = Math.Ceiling(SimilarProducts.Count / 2.0) * 320;

            offset += items.Count;

            if (items.Count < PageSize)
                hasMoreItems = false;
        }
        finally
        {
            IsRefreshing = false;
            isRequestRunning = false;
        }
    }

    private MainProductCardItem ToMainProductCardItem(ProductDto item)
    {
        var images =
            new ObservableCollection<MainProductImageItem>();

        if (item.images != null &&
            item.images.Count > 0)
        {
            foreach (var img in item.images.OrderBy(x => x.sort_order ?? 0))
            {
                images.Add(new MainProductImageItem
                {
                    ImageSource = img.image_url
                });
            }
        }

        return new MainProductCardItem
        {
            Price = item.price?.ToString("N0").Replace(",", " ") ?? "0",

            Subscription_price = item.subscription_price?.ToString("N0").Replace(",", " ") ?? "0",

            ProductId = (int)item.id,

            Title = item.name ?? "",
            Liked = item.liked,

            Rating = item.average_rating ?? 0,
            ReviewCount = item.review_count ?? 0,

            ActionText = "+ Ertaga",

            Images = images
        };
    }

    private async Task LoadMoreAsync()
    {
        await LoadSimilarProductsAsync();
    }

    private async Task RefreshAsync()
    {
        await LoadActiveSubscriptionAsync();
        await LoadProductDetailAsync();
        await LoadSimilarProductsAsync(true);
    }

    public async Task<bool> AddProductToCartAsync()
    {
        if (!await appControl.EnsureAuthenticatedAsync())
            return false;

        try
        {
            IsLoading = true;

            Response response = await apiService.AddCartProduct(
                new AddCartRequest
                {
                    user_id = appControl.CurrentUserId,
                    product_id = (int)ProductId,
                    quantity = Quantity
                });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                await AlertService.ShowAlertAsync("Xatolik", response.resultMsg);
                return false;
            }

            ShowCartView = true;
            PageDataRefreshState.MarkDirty(PageDataRefreshState.Cart);
            return true;
        }
        catch
        {
            await AlertService.ShowAlertAsync("Xatolik", "Mahsulotni savatchaga qo’shib bo’lmadi.");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }
    public void IncreaseQuantity()
    {
        Quantity++;
    }

    public void DecreaseQuantity()
    {
        if (Quantity <= 1)
            return;

        Quantity--;
    }

    public async Task PurchaseNowAsync()
    {
        if (!await appControl.EnsureAuthenticatedAsync())
            return;

        FormalizationNavigationStore.Data = new FormalizationData
        {
            UserId = appControl.CurrentUserId,
            AddressText = appControl.userDto.address,

            Products = new List<FormalizationProductItem>
            {
                new FormalizationProductItem
                {
                    ProductId = ProductId,
                    Name = ProductTitle,
                    ImageSource = ProductImages.FirstOrDefault()?.Image ?? string.Empty,
                    Quantity = Quantity,
                    Price = FinalPriceValue
                }
            }
        };

        await AppNavigatorService.NavigateTo(nameof(FormalizationPage));
    }
}