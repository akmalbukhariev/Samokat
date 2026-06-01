using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ninimum.Services;
using Ninimum.ViewModels;
using Ninimum.Views.Main;
using Utils;

namespace Ninimum.Views.LoginRegister;

public partial class LoginPage : BasePage
{
    private readonly LoginPageViewModel viewModel;
    private readonly AppControl appControl;
    private readonly AppStoreService storeService;

    public LoginPage(AppControl appControl, LoginPageViewModel viewModel, AppStoreService storeService)
    {
        InitializeComponent();

        this.appControl = appControl;
        this.viewModel = viewModel;
        this.storeService = storeService;
  
        this.viewModel.ShowSmsPopupAction = ShowSmsPopup;
        this.viewModel.HideSmsPopupAction = HideSmsPopup;

        BindingContext = this.viewModel;
    }

    protected override async void OnAppearing()
    {
        appControl.IsLoggedIn = storeService.Get(AppKeys.IsLoggedIn, false);
        string phoneNumber = storeService.Get(AppKeys.PhoneNumber, "");
        string password = storeService.Get(AppKeys.Password, "");

        if (appControl.IsBlocked)
        {
            appControl.IsBlocked = false;
            //Show block page
        }
        else
        {
            if (appControl.IsLoggedIn)
            {
                viewModel.IsLoading = true;
                await appControl.Login(phoneNumber, password);
                viewModel.IsLoading = false;
            }
        }
    }

    private async void ForgotPassword_Tapped(object sender, TappedEventArgs e)
    {
        await AnimateElementScaleDown(sender as Label);
        await AppNavigatorService.NavigateTo(nameof(ForgotPasswordPage));
    }

    private void ShowSmsPopup()
    {
        popupSms.Show();
    }

    private void HideSmsPopup()
    {
        popupSms.Hide();
    }
}