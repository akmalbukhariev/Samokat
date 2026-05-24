using Samokat.ViewModels;

namespace Samokat.Views.DetailProduct;

public partial class ProductReviews : BasePage
{
    ProductReviewsViewModel viewModel;

    public ProductReviews(ProductReviewsViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = viewModel;

        Loaded += ProductReviews_Loaded;

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

    private async void ViewModel_OpenFilterRequested()
    {
        await ProductReviewsFilterBottomSheetView.ShowAsync();
    }

    private async void ViewModel_BackRequested()
    {
        await Navigation.PopAsync();
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