using Samokat.ViewModels.LoginRegister;

namespace Samokat.Views.LoginRegister;

public partial class RegisterPage : BasePage
{
    public RegisterPage()
    {
        InitializeComponent();
        BindingContext = new RegisterPageViewModel();
    }
}