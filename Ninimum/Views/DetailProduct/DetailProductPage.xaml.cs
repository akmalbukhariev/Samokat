using Microsoft.Maui.Controls.Shapes;
using Ninimum.Models;
using Ninimum.Services;
using Ninimum.ViewModels;

namespace Ninimum.Views.DetailProduct;

public partial class DetailProductPage : BasePage
{
    private readonly DetailProductPageViewModel? viewModel;
    private int _currentPosition = -1;
    private bool _isCarouselUpdating;
    private bool _hasAppeared;

    public DetailProductPage(DetailProductPageViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;

        Loaded += DetailProductPage_Loaded;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_hasAppeared)
        {
            _hasAppeared = true;
            return;
        }

        if (viewModel == null || viewModel.ProductId <= 0)
            return;

        if (!PageDataRefreshState.ConsumeDirty(PageDataRefreshState.DetailProduct(viewModel.ProductId)))
            return;

        await viewModel.LoadInitialAsync();
    }

    private void DetailProductPage_Loaded(object? sender, EventArgs e)
    {
        UpdateFavoriteImage();

        if (viewModel == null || viewModel.ProductImages.Count == 0)
            return;

        var firstItem = viewModel.ProductImages[0];
        _currentPosition = 0;
        ProductCarousel.CurrentItem = firstItem;

        UpdateUi(0);
    }

    private async void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (viewModel == null)
            return;

        if (e.PropertyName == nameof(DetailProductPageViewModel.ProductLiked))
        {
            MainThread.BeginInvokeOnMainThread(UpdateFavoriteImage);
        }

        if (e.PropertyName == nameof(DetailProductPageViewModel.ShowLikedView))
        {
            if (!viewModel.ShowLikedView)
                return;

            await likeView.DisplayAsAnimation();
            viewModel.ShowLikedView = false;
        }

        if (e.PropertyName == nameof(viewModel.ShowCartView) && viewModel.ShowCartView)
        {
            await cartView.DisplayAsAnimation();
            viewModel.ShowCartView = false;
        }
    }

    private async void Like_Tapped(object sender, TappedEventArgs e)
    {
        if (viewModel == null)
            return;

        await ClickGuard.RunAsync((VisualElement)sender, async () =>
        {
            AppVibrationService.Like();

            bool success = await viewModel.ToggleProductLikeAsync();

            if (!success)
                return;

            UpdateFavoriteImage();

            await brdLiked.ScaleTo(1.3, 100, Easing.CubicOut);
            await brdLiked.ScaleTo(1.0, 100, Easing.CubicIn);
        });
    }

    private void ProductCarousel_CurrentItemChanged(object? sender, CurrentItemChangedEventArgs e)
    {
        if (viewModel == null || _isCarouselUpdating)
            return;

        if (e.CurrentItem is not ProductImageDetailInfo currentItem)
            return;

        var index = viewModel.ProductImages.IndexOf(currentItem);
        if (index < 0 || index == _currentPosition)
            return;

        _currentPosition = index;
        UpdateUi(index);
    }

    private async void OnSmallImageTapped(object sender, TappedEventArgs e)
    {
        var tappedImage = sender as Image;

        if (tappedImage?.Source != null)
        {
            await ImagePreview.ShowAsync(tappedImage.Source);
        }
    }

    private void OnThumbnailTapped(object? sender, TappedEventArgs e)
    {
        if (viewModel == null || e.Parameter is not ProductImageDetailInfo tappedItem)
            return;

        var index = viewModel.ProductImages.IndexOf(tappedItem);
        if (index < 0 || index == _currentPosition)
            return;

        _isCarouselUpdating = true;
        ProductCarousel.CurrentItem = tappedItem;
        _currentPosition = index;
        UpdateUi(index);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            _isCarouselUpdating = false;
        });
    }

    private async void FullDescription_Tapped(object sender, TappedEventArgs e)
    {
        await DescriptionPopup.ShowAsync();
    }

    private void UpdateUi(int position)
    {
        if (viewModel == null)
            return;

        viewModel.CurrentImageIndex = position;
        UpdateCustomIndicator(position);
        UpdateThumbnailSelection(position);
    }

    private void UpdateThumbnailSelection(int position)
    {
        if (viewModel?.ProductImages == null)
            return;

        for (int i = 0; i < viewModel.ProductImages.Count; i++)
        {
            viewModel.ProductImages[i].IsSelected = i == position;
        }
    }

    private void UpdateCustomIndicator(int position)
    {
        CustomIndicatorLayout.Children.Clear();

        if (viewModel?.ProductImages == null || viewModel.ProductImages.Count == 0)
            return;

        for (int i = 0; i < viewModel.ProductImages.Count; i++)
        {
            bool isSelected = i == position;

            var container = new Grid
            {
                WidthRequest = 30,
                HeightRequest = 12,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            var indicator = new Border
            {
                StrokeThickness = 0,
                BackgroundColor = isSelected
                    ? Color.FromArgb("#FD473C")
                    : Color.FromArgb("#DADADA"),
                WidthRequest = isSelected ? 30 : 12,
                HeightRequest = 12,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = isSelected
                        ? new CornerRadius(6)
                        : new CornerRadius(999)
                }
            };

            container.Children.Add(indicator);
            CustomIndicatorLayout.Children.Add(container);
        }
    }

    private void UpdateFavoriteImage()
    {
        imLiked.Source = viewModel?.ProductLiked == true
            ? "liked.png"
            : "unliked.png";
    }

    private async void Comment_Tapped(object sender, TappedEventArgs e)
    {
        await ClickGuard.RunAsync((VisualElement)sender, async () =>
        {
            await AnimateElementScaleDown(sender as VisualElement);

            await AppNavigatorService.NavigateTo(
           $"{nameof(ProductReviews)}" +
           $"?productId={viewModel.ProductId}" +
           $"&title={Uri.EscapeDataString(viewModel.ProductTitle)}");
        });
    }

    private async void Cart_Tapped(object sender, TappedEventArgs e)
    {
        await ClickGuard.RunAsync((VisualElement)sender, async () =>
        {
            await AnimateElementScaleDown(sender as VisualElement);

            await viewModel.AddProductToCartAsync();
        });
    }

    private void Minus_Tapped(object sender, TappedEventArgs e)
    {
        viewModel?.DecreaseQuantity();
    }

    private void Plus_Tapped(object sender, TappedEventArgs e)
    {
        viewModel?.IncreaseQuantity();
    }

    private async void Purchase_Clicked(object sender, EventArgs e)
    {
        if (viewModel == null)
            return;

        await ClickGuard.RunAsync((VisualElement)sender, viewModel.PurchaseNowAsync);
    }
}