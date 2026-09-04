using System.Collections.ObjectModel;
using Api.Services;
using Models.Requests;
using Models.Responses;
using Ninimum.Models.Dto;
using Ninimum.Services;
using Utils;

namespace Ninimum.Views.DetailProduct;

[QueryProperty(nameof(ProductId), "productId")]
[QueryProperty(nameof(OrderId), "orderId")]
[QueryProperty(nameof(ReviewId), "reviewId")]
[QueryProperty(nameof(ProductTitle), "title")]
public partial class LeaveCommentPage : BasePage
{
    private readonly UserApiService apiService;
    private readonly AppControl appControl;
    private int _selectedRating;
    private bool _isSubmitting;
    private bool _editLoaded;
    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading == value)
                return;

            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public long ProductId { get; set; }
    public long OrderId { get; set; }
    public long ReviewId { get; set; }
    public bool IsEditMode => ReviewId > 0;

    public string PageTitle => IsEditMode ? "Sharhni tahrirlash" : "Sharh qoldirish";
    public string DefaultSubmitButtonText => IsEditMode ? "Saqlash" : "Yuborish";

    private string productTitle = string.Empty;
    public string ProductTitle
    {
        get => productTitle;
        set
        {
            productTitle = Uri.UnescapeDataString(value ?? string.Empty);
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ReviewPhotoItem> SelectedImages { get; } = new();

    public LeaveCommentPage(UserApiService apiService, AppControl appControl)
    {
        InitializeComponent();
        this.apiService = apiService;
        this.appControl = appControl;
        BindingContext = this;
        InitializePage();
    }

    private void InitializePage()
    {
        _selectedRating = 0;
        UpdateStars(_selectedRating);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(DefaultSubmitButtonText));

        if (SubmitButton != null && !_isSubmitting)
            SubmitButton.Text = DefaultSubmitButtonText;

        if (IsEditMode && !_editLoaded)
            await LoadExistingReviewAsync();
    }

    private async Task LoadExistingReviewAsync()
    {
        if (!appControl.IsAuthenticated || ProductId <= 0 || ReviewId <= 0)
            return;

        try
        {
            IsLoading = true;
            SubmitButton.IsEnabled = false;

            var response = await apiService.GetReviewEligibility(new ReviewEligibilityRequest
            {
                product_id = ProductId
            });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString() || response.resultData?.existing_review == null)
            {
                await DisplayAlert("Xatolik", "Tahrirlash uchun sharh topilmadi.", "OK");
                return;
            }

            ReviewDto review = response.resultData.existing_review;
            if (review.id != ReviewId)
            {
                await DisplayAlert("Xatolik", "Tahrirlash uchun sharh topilmadi.", "OK");
                return;
            }

            OrderId = review.order_id ?? OrderId;
            _selectedRating = review.rating ?? 0;
            UpdateStars(_selectedRating);
            CommentEditor.Text = review.comment ?? string.Empty;

            SelectedImages.Clear();
            if (review.images != null)
            {
                foreach (ReviewImageDto image in review.images.Take(3))
                {
                    if (image.id.HasValue && !string.IsNullOrWhiteSpace(image.image_url))
                        SelectedImages.Add(new ReviewPhotoItem(image));
                }
            }

            _editLoaded = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Xatolik", $"Sharhni yuklab bo'lmadi: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
            SubmitButton.IsEnabled = true;
            SubmitButton.Text = DefaultSubmitButtonText;
        }
    }

    private async void OnBackTapped(object sender, TappedEventArgs e)
    {
        if (Navigation?.NavigationStack?.Count > 1)
            await Navigation.PopAsync();
        else
            await Shell.Current.GoToAsync("..");
    }

    private void OnStar1Tapped(object sender, TappedEventArgs e) => SetRating(1);
    private void OnStar2Tapped(object sender, TappedEventArgs e) => SetRating(2);
    private void OnStar3Tapped(object sender, TappedEventArgs e) => SetRating(3);
    private void OnStar4Tapped(object sender, TappedEventArgs e) => SetRating(4);
    private void OnStar5Tapped(object sender, TappedEventArgs e) => SetRating(5);

    private void SetRating(int rating)
    {
        _selectedRating = rating;
        UpdateStars(rating);
    }

    private void UpdateStars(int rating)
    {
        if (Star1 != null) Star1.Source = rating >= 1 ? "star.png" : "star_gray.png";
        if (Star2 != null) Star2.Source = rating >= 2 ? "star.png" : "star_gray.png";
        if (Star3 != null) Star3.Source = rating >= 3 ? "star.png" : "star_gray.png";
        if (Star4 != null) Star4.Source = rating >= 4 ? "star.png" : "star_gray.png";
        if (Star5 != null) Star5.Source = rating >= 5 ? "star.png" : "star_gray.png";
    }

    private async void OnAddPhotoTapped(object sender, TappedEventArgs e)
    {
        if (SelectedImages.Count >= 3)
        {
            await DisplayAlert("Ogohlantirish", "Ko'pi bilan 3 ta fotosurat joylash mumkin.", "OK");
            return;
        }

        try
        {
            FileResult? file = await MediaPicker.Default.PickPhotoAsync();
            if (file == null)
                return;

            SelectedImages.Add(new ReviewPhotoItem(file));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Xatolik", $"Fotosuratni tanlab bo'lmadi: {ex.Message}", "OK");
        }
    }

    private void OnRemovePhotoTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is ReviewPhotoItem item)
            SelectedImages.Remove(item);
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        if (_isSubmitting)
            return;

        try
        {
            if (!appControl.IsAuthenticated)
            {
                await DisplayAlert("Ogohlantirish", "Sharh qoldirish uchun akkauntingizga kiring.", "OK");
                return;
            }

            string comment = CommentEditor?.Text?.Trim() ?? string.Empty;

            if (_selectedRating <= 0)
            {
                await DisplayAlert("Ogohlantirish", "Iltimos, yulduzcha orqali baho bering.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                await DisplayAlert("Ogohlantirish", "Iltimos, sharh yozing.", "OK");
                return;
            }

            if (comment.Length > 2000)
            {
                await DisplayAlert("Ogohlantirish", "Sharh 2000 ta belgidan oshmasligi kerak.", "OK");
                return;
            }

            if (ProductId <= 0 || OrderId <= 0)
            {
                await DisplayAlert("Xatolik", "Xarid ma'lumotlari topilmadi.", "OK");
                return;
            }

            _isSubmitting = true;
            IsLoading = true;
            SubmitButton.IsEnabled = false;

            var newFiles = SelectedImages
                .Where(x => x.File != null)
                .Select(x => x.File!)
                .ToList();

            Response response;
            if (IsEditMode)
            {
                var request = new UpdateReviewRequest
                {
                    id = ReviewId,
                    rating = _selectedRating,
                    comment = comment,
                    keep_image_ids = SelectedImages
                        .Where(x => x.ExistingImageId.HasValue)
                        .Select(x => x.ExistingImageId!.Value)
                        .ToList()
                };

                response = await apiService.UpdateProductReview(request, newFiles);
            }
            else
            {
                var request = new AddReviewRequest
                {
                    product_id = ProductId,
                    order_id = OrderId,
                    rating = _selectedRating,
                    comment = comment
                };

                response = await apiService.AddProductReview(request, newFiles);
            }

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                await DisplayAlert("Xatolik", response.resultMsg ?? "Sharhni saqlab bo'lmadi.", "OK");
                return;
            }

            PageDataRefreshState.MarkDirty(PageDataRefreshState.ProductReviews(ProductId));
            PageDataRefreshState.MarkDirty(PageDataRefreshState.DetailProduct(ProductId));

            await DisplayAlert(
                "Muvaffaqiyatli",
                IsEditMode ? "Sharhingiz yangilandi." : "Sharhingiz yuborildi.",
                "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Xatolik", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
            _isSubmitting = false;
            if (SubmitButton != null)
            {
                SubmitButton.IsEnabled = true;
                SubmitButton.Text = DefaultSubmitButtonText;
            }
        }
    }
}

public class ReviewPhotoItem
{
    public long? ExistingImageId { get; }
    public FileResult? File { get; }
    public ImageSource PreviewSource { get; }

    public ReviewPhotoItem(FileResult file)
    {
        File = file;
        PreviewSource = ImageSource.FromStream(() => file.OpenReadAsync().GetAwaiter().GetResult());
    }

    public ReviewPhotoItem(ReviewImageDto image)
    {
        ExistingImageId = image.id;
        PreviewSource = ImageSource.FromUri(new Uri(image.image_url!));
    }
}
