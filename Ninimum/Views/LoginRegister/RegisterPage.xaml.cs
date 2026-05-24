using Ninimum.ViewModels;

namespace Ninimum.Views.LoginRegister;

public partial class RegisterPage : BasePage
{
    public RegisterPage(RegisterPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}