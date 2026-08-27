
using System.ComponentModel;
using Ninimum.ViewModels;
using Ninimum.Services;

namespace Ninimum.Views.FavoriteProduct;

public partial class FavoritePage : BasePage
{
    private readonly FavoritePageViewModel viewModel;

    public FavoritePage(FavoritePageViewModel vm, AppControl appControl)
    {
        InitializeComponent();
        viewModel = vm;
        this.appControl = appControl;
        BindingContext = vm;

        Shell.SetTabBarIsVisible(this, true);

        viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!await appControl.EnsureAuthenticatedAsync(true))
            return;

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