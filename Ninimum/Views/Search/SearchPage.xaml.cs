
using System.ComponentModel;
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
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
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

    private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(viewModel.ShowLikedView) && viewModel.ShowLikedView)
        {
            await likeView.DisplayAsAnimation();
            viewModel.ShowLikedView = false;
        }

        if (e.PropertyName == nameof(viewModel.ShowCartView) && viewModel.ShowCartView)
        {
            await cartView.DisplayAsAnimation();
            viewModel.ShowCartView = false;
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