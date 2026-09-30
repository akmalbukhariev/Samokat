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
}

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (viewModel.ProductId <= 0)
        {
            viewModel.IsLoading = false;
            return;
        }

        bool needsRefresh = PageDataRefreshState.ConsumeDirty(
            PageDataRefreshState.ProductQuestions(viewModel.ProductId));

        if (hasLoaded && !needsRefresh)
            return;

        hasLoaded = true;
        await viewModel.LoadAsync();
    }
}
