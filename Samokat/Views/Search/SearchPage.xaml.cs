
using Samokat.ViewModels;

namespace Samokat.Views.Search;

public partial class SearchPage : BasePage
{
    private SearchPageViewModel? viewModel;
    public SearchPage(SearchPageViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;

        Loaded += SearchPage_Loaded;
    }

    private void SearchPage_Loaded(object? sender, EventArgs e)
    {
        if (BindingContext is SearchPageViewModel vm)
        {
            if (viewModel != null)
                viewModel.OpenFilterRequested -= ViewModel_OpenFilterRequested;

            viewModel = vm;
            viewModel.OpenFilterRequested += ViewModel_OpenFilterRequested;
        }
    }

    private async void ViewModel_OpenFilterRequested()
    {
        await SearchFilterBottomSheetView.ShowAsync();
    }
}