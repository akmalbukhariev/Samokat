using Microsoft.Maui.Controls.Shapes;
using Samokat.Models;
using Samokat.ViewModels;

namespace Samokat.Views.BasketProduct;

public partial class BasketProductPage : BasePage
{
    private BasketProductPageViewModel viewModel;
    public BasketProductPage(BasketProductPageViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;

    }
}