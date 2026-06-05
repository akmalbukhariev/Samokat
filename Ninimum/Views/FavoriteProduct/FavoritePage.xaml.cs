
using Ninimum.ViewModels;

namespace Ninimum.Views.FavoriteProduct;

public partial class FavoritePage : BasePage
{
    private FavoritePageViewModel viewModel;
    public FavoritePage(FavoritePageViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;

        Shell.SetTabBarIsVisible(this, true);
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await viewModel.LoadInitialAsync();
    }
}