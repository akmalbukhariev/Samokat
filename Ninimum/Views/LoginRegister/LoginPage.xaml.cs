using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ninimum.Services;
using Ninimum.ViewModels;
using Ninimum.Views.Main;

namespace Ninimum.Views.LoginRegister;

public partial class LoginPage : BasePage
{
    private readonly LoginPageViewModel viewModel;
    private readonly AppControl appControl;

    public LoginPage(AppControl appControl, LoginPageViewModel viewModel)
    {
        InitializeComponent();

        this.appControl = appControl;

        this.viewModel = viewModel;

        this.viewModel.ShowSmsPopupAction = ShowSmsPopup;
        this.viewModel.HideSmsPopupAction = HideSmsPopup;

        BindingContext = this.viewModel;
    }

    private async void ForgotPassword_Tapped(object sender, TappedEventArgs e)
    {
        await AppNavigatorService.NavigateTo(nameof(ForgotPasswordPage));
    }

    private void ShowSmsPopup()
    {
        popupSms.Show();
    }

    private void HideSmsPopup()
    {
        popupSms.Hide();
        appControl.Login();
    }
}