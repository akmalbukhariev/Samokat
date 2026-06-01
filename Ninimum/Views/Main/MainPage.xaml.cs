using Ninimum.ViewModels;
using Ninimum.Views.Search;

namespace Ninimum.Views.Main;

public partial class MainPage : BasePage
{
    private readonly MainPageViewModel viewModel;
    private bool _isStickyVisible = false;

    public MainPage(MainPageViewModel vm)
    {
        InitializeComponent();

        viewModel = vm;
        BindingContext = viewModel;

        Shell.SetTabBarIsVisible(this, true);

        InlineSearchBarView.MenuClicked += LeftMenuClicked;
        InlineSearchBarView.SearchClicked += SearchClicked;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await viewModel.LoadInitialAsync();
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