using Samokat.ViewModels;

namespace Samokat.Views.Main;

public partial class MainPage : BasePage
{
    private readonly MainPageViewModel viewModel;
    private bool _isStickyVisible = false;

    public MainPage()
    {
        InitializeComponent();

        viewModel = new MainPageViewModel();
        BindingContext = viewModel;
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