using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Samokat.Views.LoginRegister;

public partial class LoginPage : BasePage
{
    private readonly LoginPageViewModel viewModel;

    public LoginPage()
    {
        InitializeComponent();

        viewModel = new LoginPageViewModel();
        BindingContext = viewModel;

        viewModel.ShowSmsPopupAction = () => popupSms.Show();
        viewModel.HideSmsPopupAction = () => popupSms.Hide();
        
        BindingContext = viewModel;
    }
}