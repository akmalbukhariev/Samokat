using Api.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Models.Requests;
using Models.Responses;
using Ninimum.Views.LoginRegister;
using System.Windows.Input;
using Utils;

namespace Ninimum.ViewModels;

public partial class LoginPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private ICommand loginCommand;

    [ObservableProperty]
    private ICommand registerCommand;

    [ObservableProperty]
    private ICommand guestLoginCommand;

    [ObservableProperty]
    private ICommand forgotPasswordCommand;
    
    [ObservableProperty]
    private ICommand confirmSmsCodeCommand;

    public Action? ShowSmsPopupAction { get; set; }
    public Action? HideSmsPopupAction { get; set; }
    private readonly UserApiService apiService;

    public LoginPageViewModel(UserApiService apiService)
    {
        this.apiService = apiService;

        LoginCommand = new Command(async () => await OnLogin());
        RegisterCommand = new Command(async () => await OnRegister());
        GuestLoginCommand = new Command(async () => await OnGuestLogin());
        ForgotPasswordCommand = new Command(async () => await OnForgotPassword());
        ConfirmSmsCodeCommand = new Command<string>(OnConfirmSmsCode);
    }

    private async Task OnLogin()
    {
        try
        {
            IsLoading = true;

            var request = new LoginUserRequest
            {
                phone_number = PhoneNumber,
                password = Password
            };

            LoginUserResponse response = await apiService.Login(request);
            if (response.resultCode == ApiResult.SUCCESS.GetCodeToString())
            {
                
            }
            else
            {

             }

            ShowSmsPopupAction?.Invoke();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnConfirmSmsCode(string code)
    {
        HideSmsPopupAction?.Invoke();
    }

    private async Task OnRegister()
    {
        await AppNavigatorService.NavigateTo(nameof(RegisterPage));
    }

    private async Task OnGuestLogin()
    {
         
    }

    private async Task OnForgotPassword()
    {
         await AppNavigatorService.NavigateTo(nameof(ForgotPasswordPage));
    }
}