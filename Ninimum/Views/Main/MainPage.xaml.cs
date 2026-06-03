using System.ComponentModel;
using Ninimum.ViewModels;
using Ninimum.Views.Search;

namespace Ninimum.Views.Main;

public partial class MainPage : BasePage
{
    private readonly MainPageViewModel viewModel;
    private bool _isStickyVisible = false;
    private bool isFirstLoaded = true;

    public MainPage(MainPageViewModel vm)
    {
        InitializeComponent();

        viewModel = vm;
        BindingContext = viewModel;

        Shell.SetTabBarIsVisible(this, true);

        InlineSearchBarView.MenuClicked += LeftMenuClicked;
        InlineSearchBarView.SearchClicked += SearchClicked;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (isFirstLoaded)
        {
            isFirstLoaded = false;
            await viewModel.LoadInitialAsync();
        }
    }

    private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(viewModel.ShowLikedView) && viewModel.ShowLikedView)
        {
            await likeView.DisplayAsAnimation();
            viewModel.ShowLikedView = false;
        }
    }

    private async void LeftMenuClicked()
    {
        await AppNavigatorService.NavigateTo(nameof(MenuPage));
    }

    private async void SearchClicked()
    { 
        await AppNavigatorService.NavigateTo(nameof(SearchPage));
    }

    private void MainCollectionView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        double threshold = 100; // adjust if needed

        bool shouldShow = e.VerticalOffset > threshold;

        if (_isStickyVisible != shouldShow)
        {
            _isStickyVisible = shouldShow;
            StickySearchBarContainer.IsVisible = shouldShow;
        }
    }
}