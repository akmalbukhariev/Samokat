using Ninimum.ViewModels;

namespace Ninimum.Views.DetailProduct;

public partial class ProductReviews : BasePage
{
    ProductReviewsViewModel viewModel;

    public ProductReviews(ProductReviewsViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = viewModel;

        Loaded += ProductReviews_Loaded;
        viewModel.ImagePreviewRequested += ViewModel_ImagePreviewRequested;

        Shell.SetTabBarIsVisible(this, false);
    }

    private void ProductReviews_Loaded(object? sender, EventArgs e)
    {
        if (BindingContext is ProductReviewsViewModel vm)
        {
            if (viewModel != null)
            {
                viewModel.OpenFilterRequested -= ViewModel_OpenFilterRequested;
                viewModel.BackRequested -= ViewModel_BackRequested;
            }

            viewModel = vm;
            viewModel.OpenFilterRequested += ViewModel_OpenFilterRequested;
            viewModel.BackRequested += ViewModel_BackRequested;
        }
    }

    private async void ViewModel_ImagePreviewRequested(string imageUrl)
    {
        await ImagePreview.ShowAsync(imageUrl);
    }

    private async void ViewModel_OpenFilterRequested()
    {
        await ProductReviewsFilterBottomSheetView.ShowAsync();
    }

    private async void ViewModel_BackRequested()
    {
        await Navigation.PopAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        viewModel.OpenFilterRequested -= ViewModel_OpenFilterRequested;
        viewModel.BackRequested -= ViewModel_BackRequested;
        viewModel.OpenFilterRequested += ViewModel_OpenFilterRequested;
        viewModel.BackRequested += ViewModel_BackRequested;

        if (viewModel.ProductId > 0)
            await viewModel.RefreshAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (viewModel != null)
        {
            viewModel.OpenFilterRequested -= ViewModel_OpenFilterRequested;
            viewModel.BackRequested -= ViewModel_BackRequested;
        }
    }
}