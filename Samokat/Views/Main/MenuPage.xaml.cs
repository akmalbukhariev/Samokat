using Samokat.ViewModels;

namespace Samokat.Views.Main;

public partial class MenuPage : BasePage
{
    public MenuPage(MenuPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}