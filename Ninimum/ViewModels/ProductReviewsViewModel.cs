using System.Collections.ObjectModel;
using System.Windows.Input;
using Api.Services;
using CommunityToolkit.Mvvm.ComponentModel;
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

    [ObservableProperty] private int productId;
    [ObservableProperty] private string title;
    [ObservableProperty] private bool isLoading;

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

    partial void OnProductIdChanged(int value)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await LoadReviewsAsync();
        });
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

    private async Task LoadReviewsAsync()
    {
        try
        {
            BuyerPhotos.Clear();
            AllReviews.Clear();
            Reviews.Clear();

            IsLoading = true;
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
                    CustomerName = "Xaridor",
                    ReviewDate = review.created_at?.ToString("dd.MM.yyyy") ?? "",
                    ReviewDateValue = review.created_at ?? DateTime.MinValue,
                    Rating = review.rating ?? 0,
                    ReviewText = review.comment ?? "",
                    ReplyText = "",
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
        finally
        { 
            IsLoading = false;
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