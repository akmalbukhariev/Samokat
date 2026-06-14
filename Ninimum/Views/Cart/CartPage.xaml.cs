using Microsoft.Maui.Controls.Shapes;
using Ninimum.Models;
using Ninimum.ViewModels;

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