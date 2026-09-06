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
    private bool _isMediaPickerOpen;

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
    public bool HasSelectedImages => SelectedImages.Count > 0;
    public bool CanAddMorePhotos => SelectedImages.Count < 3;
    public string PhotoLimitText => $"{SelectedImages.Count}/3 ta fotosurat";

    public LeaveCommentPage(UserApiService apiService, AppControl appControl)
    {
        InitializeComponent();
        this.apiService = apiService;
        this.appControl = appControl;
        BindingContext = this;
        SelectedImages.CollectionChanged += (_, _) => UpdatePhotoState();
        InitializePage();
    }

    private void InitializePage()
    {
        _selectedRating = 0;
        UpdateStars(_selectedRating);
        UpdatePhotoState();
    }

    private void UpdatePhotoState()
    {
        OnPropertyChanged(nameof(HasSelectedImages));
        OnPropertyChanged(nameof(CanAddMorePhotos));
        OnPropertyChanged(nameof(PhotoLimitText));
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

    private async void OnTakePhotoTapped(object sender, TappedEventArgs e)
    {
        if (!CanAddMorePhotos || _isMediaPickerOpen)
            return;

        if (!MediaPicker.Default.IsCaptureSupported)
        {
            await DisplayAlert("Ogohlantirish", "Ushbu qurilmada kamera orqali rasm olish mavjud emas.", "OK");
            return;
        }

        try
        {
            _isMediaPickerOpen = true;

            PermissionStatus permission = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (permission != PermissionStatus.Granted)
                permission = await Permissions.RequestAsync<Permissions.Camera>();

            if (permission != PermissionStatus.Granted)
            {
                await DisplayAlert("Kamera ruxsati", "Rasmga olish uchun ilovaga kamera ruxsatini bering.", "OK");
                return;
            }

            // Keep the original EXIF orientation until ApiService.ResizeImage processes it.
            // ResizeImage rotates the actual pixels, resizes to the upload limit and then
            // re-encodes as JPEG, so the server never depends on EXIF for display orientation.
            FileResult? file = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Sharh uchun rasmga olish",
                RotateImage = false,
                PreserveMetaData = true
            });

            if (file != null && CanAddMorePhotos)
                SelectedImages.Add(new ReviewPhotoItem(file));
        }
        catch (PermissionException)
        {
            await DisplayAlert("Kamera ruxsati", "Kamera ruxsati berilmagan.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Xatolik", $"Rasmga olib bo'lmadi: {ex.Message}", "OK");
        }
        finally
        {
            _isMediaPickerOpen = false;
        }
    }

    private async void OnPickPhotoTapped(object sender, TappedEventArgs e)
    {
        if (!CanAddMorePhotos || _isMediaPickerOpen)
            return;

        try
        {
            _isMediaPickerOpen = true;
            int remainingCount = 3 - SelectedImages.Count;

            List<FileResult> files = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
            {
                Title = "Sharh uchun fotosurat tanlang",
                SelectionLimit = remainingCount,
                RotateImage = false,
                PreserveMetaData = true
            });

            foreach (FileResult file in files.Take(remainingCount))
                SelectedImages.Add(new ReviewPhotoItem(file));
        }
        catch (PermissionException)
        {
            await DisplayAlert("Ruxsat", "Fotosuratlarni tanlash uchun galereyaga ruxsat bering.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Xatolik", $"Fotosuratni tanlab bo'lmadi: {ex.Message}", "OK");
        }
        finally
        {
            _isMediaPickerOpen = false;
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
            PageDataRefreshState.MarkDirty(PageDataRefreshState.Main);
            PageDataRefreshState.MarkDirty(PageDataRefreshState.Favorites);

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
