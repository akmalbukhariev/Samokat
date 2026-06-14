
using System.ComponentModel;
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

        viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await viewModel.LoadInitialAsync();
    }
    
    private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(viewModel.ShowCartView) && viewModel.ShowCartView)
        {
            await cartView.DisplayAsAnimation();
            viewModel.ShowCartView = false;
        }
    }
}