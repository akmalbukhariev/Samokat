using Microsoft.Maui.Controls.Shapes;
using Models.Requests;
using Ninimum.Models;
using Ninimum.ViewModels;
using Ninimum.Views.Payment;
using Utils;

namespace Ninimum.Views.Cart;

public partial class CartPage : BasePage
{
    private readonly CartPageViewModel viewModel;

    public CartPage(CartPageViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;

        Shell.SetTabBarIsVisible(this, true);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadCartListAsync();
    }
}