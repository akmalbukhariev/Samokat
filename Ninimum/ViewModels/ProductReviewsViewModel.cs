using System.Collections.ObjectModel;
using System.Windows.Input;
using Api.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models.Requests;
using Models.Responses;
using Ninimum.Models;
using Ninimum.Models.Dto;
using Ninimum.Services;
using Utils;

namespace Ninimum.ViewModels;

[QueryProperty(nameof(ProductId), "productId")]
[QueryProperty(nameof(Title), "title")]
public partial class ProductReviewsViewModel : ObservableObject
{
    private readonly UserApiService apiService;
    private readonly AppControl appControl;

    private int _quantity = 1;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    [ObservableProperty] private int productId;
    [ObservableProperty] private string title;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private bool canWriteReview;
    [ObservableProperty] private bool canEditReview;
    [ObservableProperty] private bool canShowReviewAction;
    [ObservableProperty] private string reviewActionText = "Sharh qoldirish";
    [ObservableProperty] private ReviewDto? existingReview;
    [ObservableProperty] private bool showReviewEligibilityMessage;
    [ObservableProperty] private string reviewEligibilityText = string.Empty;
    [ObservableProperty] private long? eligibleOrderId;

    [ObservableProperty]
    private ICommand backCommand;

    [ObservableProperty]
    private ICommand filterTapCommand;

    [ObservableProperty]
    private ICommand applyFilterCommand;

    [ObservableProperty]
    private ICommand selectSortCommand;

    [ObservableProperty]
    private ICommand increaseCommand;

    [ObservableProperty]
    private ICommand decreaseCommand;

    [ObservableProperty]
    private ICommand previewImageCommand;

    [ObservableProperty]
    private ICommand writeReviewCommand;

    public IAsyncRelayCommand RefreshCommand { get; }

    public event Action<string>? ImagePreviewRequested;

    [ObservableProperty]
    private ObservableCollection<string> buyerPhotos;

    [ObservableProperty]
    private ObservableCollection<ProductReviewItem> reviews;

    [ObservableProperty]
    private ObservableCollection<ProductReviewItem> allReviews;

    [ObservableProperty]
    private bool isSortNewestFirst = true;

    [ObservableProperty]
    private bool isSortRatingHighFirst;

    [ObservableProperty]
    private bool isSortRatingLowFirst;

    [ObservableProperty]
    private bool isPhotoOnly;

    public event Action? OpenFilterRequested;
    public event Action? BackRequested;

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }
    }

    public string BuyerPhotosCountText => $"{BuyerPhotos?.Count ?? 0} ta";

    public bool IsNewestSelected
    {
        get => IsSortNewestFirst;
        set
        {
            if (IsSortNewestFirst != value)
            {
                IsSortNewestFirst = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsRatingHighSelected
    {
        get => IsSortRatingHighFirst;
        set
        {
            if (IsSortRatingHighFirst != value)
            {
                IsSortRatingHighFirst = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsRatingLowSelected
    {
        get => IsSortRatingLowFirst;
        set
        {
            if (IsSortRatingLowFirst != value)
            {
                IsSortRatingLowFirst = value;
                OnPropertyChanged();
            }
        }
    }

    public ProductReviewsViewModel(AppControl appControl, UserApiService apiService)
    {
        this.appControl = appControl;
        this.apiService = apiService;

        BackCommand = new Command(OnBackTapped);
        FilterTapCommand = new Command(OnFilterTapped);
        ApplyFilterCommand = new Command(OnApplyFilterTapped);
        SelectSortCommand = new Command<string>(OnSelectSort);
        PreviewImageCommand = new Command<string>(OnPreviewImageTapped);
        WriteReviewCommand = new Command(async () => await OnWriteReviewTapped());
        RefreshCommand = new AsyncRelayCommand(ManualRefreshAsync);

        IncreaseCommand = new Command(() =>
        {
            Quantity++;
        });

        DecreaseCommand = new Command(() =>
        {
            if (Quantity > 1)
                Quantity--;
        });

        BuyerPhotos = new ObservableCollection<string>();
        Reviews = new ObservableCollection<ProductReviewItem>();
        AllReviews = new ObservableCollection<ProductReviewItem>();
    }

    private void OnPreviewImageTapped(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return;

        ImagePreviewRequested?.Invoke(imageUrl);
    }

    partial void OnBuyerPhotosChanged(ObservableCollection<string> value)
    {
        OnPropertyChanged(nameof(BuyerPhotosCountText));
    }

    partial void OnIsSortNewestFirstChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNewestSelected));
    }

    partial void OnIsSortRatingHighFirstChanged(bool value)
    {
        OnPropertyChanged(nameof(IsRatingHighSelected));
    }

    partial void OnIsSortRatingLowFirstChanged(bool value)
    {
        OnPropertyChanged(nameof(IsRatingLowSelected));
    }

    public async Task RefreshAsync(bool showLoading = true)
    {
        await _refreshLock.WaitAsync();

        try
        {
            if (showLoading)
                IsLoading = true;

            await LoadReviewsAsync();
            await LoadReviewEligibilityAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            if (showLoading)
                IsLoading = false;

            _refreshLock.Release();
        }
    }

    private async Task ManualRefreshAsync()
    {
        AppVibrationService.Click();

        try
        {
            IsRefreshing = true;
            await RefreshAsync(showLoading: false);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task LoadReviewEligibilityAsync()
    {
        CanWriteReview = false;
        CanEditReview = false;
        CanShowReviewAction = false;
        ReviewActionText = "Sharh qoldirish";
        ExistingReview = null;
        EligibleOrderId = null;
        ShowReviewEligibilityMessage = true;

        if (!appControl.IsAuthenticated)
        {
            ReviewEligibilityText = "Sharh qoldirish uchun akkauntingizga kiring.";
            return;
        }

        ReviewEligibilityResponse response = await apiService.GetReviewEligibility(new ReviewEligibilityRequest
        {
            product_id = ProductId
        });

        if (response.resultCode != ApiResult.SUCCESS.GetCodeToString() || response.resultData == null)
        {
            ReviewEligibilityText = "Sharh qoldirish holatini tekshirib bo'lmadi.";
            return;
        }

        ExistingReview = response.resultData.existing_review;
        CanWriteReview = response.resultData.can_review && response.resultData.order_id.HasValue;
        EligibleOrderId = response.resultData.order_id;

        if (CanWriteReview)
        {
            CanShowReviewAction = true;
            ReviewActionText = "Sharh qoldirish";
            ReviewEligibilityText = "Siz bu mahsulotni xarid qilgansiz. Tajribangizni boshqalar bilan ulashing.";
            return;
        }

        CanEditReview = response.resultData.already_reviewed && ExistingReview?.id is > 0 && ExistingReview?.order_id is > 0;
        if (CanEditReview)
        {
            CanShowReviewAction = true;
            ReviewActionText = "Sharhni tahrirlash";
            ReviewEligibilityText = "Siz ushbu mahsulot uchun sharh qoldirgansiz. Xohlasangiz uni tahrirlashingiz mumkin.";
            return;
        }

        if (!response.resultData.has_purchased)
            ReviewEligibilityText = "Sharh faqat ushbu mahsulotni kamida bir marta xarid qilgan foydalanuvchilar uchun mavjud.";
        else
            ReviewEligibilityText = "Hozircha sharh qoldirish mumkin emas.";
    }

    private async Task OnWriteReviewTapped()
    {
        if (CanWriteReview && EligibleOrderId.HasValue)
        {
            await AppNavigatorService.NavigateTo(
                $"{nameof(Ninimum.Views.DetailProduct.LeaveCommentPage)}" +
                $"?productId={ProductId}" +
                $"&orderId={EligibleOrderId.Value}" +
                $"&title={Uri.EscapeDataString(Title ?? string.Empty)}");
            return;
        }

        if (!CanEditReview || ExistingReview == null || !ExistingReview.id.HasValue || !ExistingReview.order_id.HasValue)
            return;

        await AppNavigatorService.NavigateTo(
            $"{nameof(Ninimum.Views.DetailProduct.LeaveCommentPage)}" +
            $"?productId={ProductId}" +
            $"&orderId={ExistingReview.order_id.Value}" +
            $"&reviewId={ExistingReview.id.Value}" +
            $"&title={Uri.EscapeDataString(Title ?? string.Empty)}");
    }

    private async Task LoadReviewsAsync()
    {
        try
        {
            BuyerPhotos.Clear();
            AllReviews.Clear();
            Reviews.Clear();

            ReviewProductResponse response = await apiService.GetProductReviewList(
                    new ReviewListRequest
                    {
                        product_id = ProductId,
                        pageSize = 100,
                        offset = 0
                    });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
                return;

            if (response.resultData == null)
                return;

            foreach (ReviewDto review in response.resultData)
            {
                ProductReviewItem item = new()
                {
                    CustomerName = string.IsNullOrWhiteSpace(review.customer_name) ? "Xaridor" : review.customer_name,
                    ReviewDate = review.created_at?.ToString("dd.MM.yyyy") ?? "",
                    ReviewDateValue = review.created_at ?? DateTime.MinValue,
                    Rating = review.rating ?? 0,
                    ReviewText = review.comment ?? "",
                    ReplyText = "",
                    IsVerifiedPurchase = review.verified_purchase ?? false,
                    Photos = new ObservableCollection<string>()
                };

                if (review.images != null)
                {
                    foreach (var image in review.images)
                    {
                        if (string.IsNullOrWhiteSpace(image.image_url))
                            continue;

                        item.Photos.Add(image.image_url);

                        /*if (!BuyerPhotos.Contains(image.image_url))
                        {
                            BuyerPhotos.Add(image.image_url);
                        }*/
                    }
                }

                AllReviews.Add(item);
            }

            Reviews = new ObservableCollection<ProductReviewItem>(AllReviews.OrderByDescending(x => x.ReviewDateValue));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }

        OnPropertyChanged(nameof(BuyerPhotosCountText));
    }

    private void OnBackTapped()
    {
        BackRequested?.Invoke();
    }

    private void OnFilterTapped()
    {
        OpenFilterRequested?.Invoke();
    }

    private void OnApplyFilterTapped()
    {
        IEnumerable<ProductReviewItem> filtered = AllReviews;

        if (IsPhotoOnly)
            filtered = filtered.Where(x => x.HasPhotos);

        if (IsSortNewestFirst)
            filtered = filtered.OrderByDescending(x => x.ReviewDateValue);
        else if (IsSortRatingHighFirst)
            filtered = filtered.OrderByDescending(x => x.Rating);
        else if (IsSortRatingLowFirst)
            filtered = filtered.OrderBy(x => x.Rating);

        Reviews = new ObservableCollection<ProductReviewItem>(filtered);
    }

    private void OnSelectSort(string sortType)
    {
        IsSortNewestFirst = sortType == "newest";
        IsSortRatingHighFirst = sortType == "high";
        IsSortRatingLowFirst = sortType == "low";
    }
}