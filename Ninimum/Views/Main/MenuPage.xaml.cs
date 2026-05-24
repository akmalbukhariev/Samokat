using Ninimum.ViewModels;

namespace Ninimum.Views.Main;

public partial class MenuPage : BasePage
{
    public MenuPage(MenuPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}