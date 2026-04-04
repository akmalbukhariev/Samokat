using Samokat.ViewModels;

namespace Samokat.Views.Main;

public partial class MainPage : BasePage
{
    private readonly MainPageViewModel viewModel;

    public MainPage()
    {
        InitializeComponent();

        viewModel = new MainPageViewModel();
        BindingContext = viewModel;
    }
}