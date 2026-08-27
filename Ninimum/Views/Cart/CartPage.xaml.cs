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

        await viewModel.LoadCartListAsync();
    }
}