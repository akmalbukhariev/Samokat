using Samokat.ViewModels;

namespace Samokat.Views.LoginRegister;

public partial class RegisterPage : BasePage
{
    public RegisterPage(RegisterPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}