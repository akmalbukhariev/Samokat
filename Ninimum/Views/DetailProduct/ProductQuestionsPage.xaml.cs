using Ninimum.Services;
using Ninimum.ViewModels;

namespace Ninimum.Views.DetailProduct;

public partial class ProductQuestionsPage : BasePage
{
    private readonly ProductQuestionsViewModel viewModel;
    private bool hasLoaded;

    public ProductQuestionsPage(ProductQuestionsViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
        Shell.SetTabBarIsVisible(this, false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (viewModel.ProductId <= 0)
            return;

        bool needsRefresh = PageDataRefreshState.ConsumeDirty(
            PageDataRefreshState.ProductQuestions(viewModel.ProductId));

        if (hasLoaded && !needsRefresh)
            return;

        hasLoaded = true;
        await viewModel.LoadAsync();
    }
}
