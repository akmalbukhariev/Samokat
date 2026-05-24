using Microsoft.Maui.Controls.Shapes;
using Ninimum.Models;
using Ninimum.ViewModels;

namespace Ninimum.Views.BasketProduct;

public partial class BasketProductPage : BasePage
{
    private BasketProductPageViewModel viewModel;
    public BasketProductPage(BasketProductPageViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;

        Shell.SetTabBarIsVisible(this, true);
    }
}