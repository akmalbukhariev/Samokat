using Microsoft.Maui.Controls.Shapes;
using Models.Requests;
using Ninimum.Models;
using Ninimum.Services;
using Ninimum.ViewModels;
using Ninimum.Views.Payment;
using Utils;

namespace Ninimum.Views.Cart;

public partial class CartPage : BasePage
{
    private readonly CartPageViewModel viewModel;
    private bool _hasLoaded;

    public CartPage(CartPageViewModel vm, AppControl appControl)
    {
        InitializeComponent();
        viewModel = vm;
        this.appControl = appControl;
        BindingContext = vm;

        Shell.SetTabBarIsVisible(this, true);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!await appControl.EnsureAuthenticatedAsync(true))
            return;

        bool needsRefresh = PageDataRefreshState.ConsumeDirty(PageDataRefreshState.Cart);

        if (_hasLoaded && !needsRefresh)
            return;

        _hasLoaded = true;
        await viewModel.LoadCartListAsync();
    }
}