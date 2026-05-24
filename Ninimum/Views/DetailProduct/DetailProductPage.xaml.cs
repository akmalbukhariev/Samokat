using Microsoft.Maui.Controls.Shapes;
using Samokat.Models;
using Samokat.ViewModels;

namespace Samokat.Views.DetailProduct;

public partial class DetailProductPage : BasePage
{
    private readonly DetailProductPageViewModel? viewModel;
    private int _currentPosition = -1;
    private bool _isCarouselUpdating;

    public DetailProductPage(DetailProductPageViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;

        Loaded += DetailProductPage_Loaded;
    }

    private void DetailProductPage_Loaded(object? sender, EventArgs e)
    {
        if (viewModel == null || viewModel.ProductImages.Count == 0)
            return;

        var firstItem = viewModel.ProductImages[0];
        _currentPosition = 0;
        ProductCarousel.CurrentItem = firstItem;

        UpdateUi(0);
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

    private async void Comment_Tapped(object sender, TappedEventArgs e)
    {
        await AnimateElementScaleDown(sender as VisualElement);
        
        await AppNavigatorService.NavigateTo(nameof(ProductReviews));
    }
}