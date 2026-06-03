
using Ninimum.ViewModels;

namespace Ninimum.Views.Search;

public partial class SearchPage : BasePage
{
    private SearchPageViewModel? viewModel;
    public SearchPage(SearchPageViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;

        Shell.SetTabBarIsVisible(this, false);

        Loaded += SearchPage_Loaded;
    }

    private void SearchPage_Loaded(object? sender, EventArgs e)
    {
        if (BindingContext is SearchPageViewModel vm)
        {
            if (viewModel != null)
            {
                viewModel.OpenFilterRequested -= ViewModel_OpenFilterRequested;
                viewModel.CloseFilterRequested -= ViewModel_CloseFilterRequested;
            }

            viewModel = vm;
            viewModel.OpenFilterRequested += ViewModel_OpenFilterRequested;
            viewModel.CloseFilterRequested += ViewModel_CloseFilterRequested;
        }
    }

    private async void ViewModel_OpenFilterRequested()
    {
        await SearchFilterBottomSheetView.ShowAsync();
    }
    
    private async void ViewModel_CloseFilterRequested()
    {
        await SearchFilterBottomSheetView.HideAsync();
    }
}