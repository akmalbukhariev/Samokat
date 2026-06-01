using Api.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Models.Requests;
using Models.Responses;
using Ninimum.Services;
using Ninimum.Views.Authorization;
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
    private string verificationCode = "";

    private readonly UserApiService apiService;
    private readonly AppControl appControl;
    private LoginUserResponse response;
    public LoginPageViewModel(UserApiService apiService, AppControl appControl)
    {
        this.apiService = apiService;
        this.appControl = appControl;

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
            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                await AlertService.ShowAlertAsync("Info", "Telefon raqamni kiriting");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                await AlertService.ShowAlertAsync("Info", "Parolni kiriting");
                return;
            }
            
            IsLoading = true;

            var request = new LoginUserRequest
            {
                phone_number = PhoneNumber,
                password = Password
            };

            response = await apiService.Login(request);

            if (response.resultCode == ApiResult.SUCCESS.GetCodeToString())
            {
                string? code = await appControl.SendVerificationCode(PhoneNumber);

                if (!string.IsNullOrEmpty(code))
                {
                    verificationCode = code;
                    ShowSmsPopupAction?.Invoke();
                }
                else
                {
                    await AlertService.ShowAlertAsync("Error", "SMS yuborilmadi");
                }
            }
            else
            {
                string message = response.resultCode switch
                {
                    "PASSWORD_IS_NOT_MATCHED" => "Parol noto‘g‘ri",
                    "USER_NOT_EXIST" => "Foydalanuvchi topilmadi",
                    _ => response.resultMsg ?? "Xatolik yuz berdi"
                };

                await AlertService.ShowAlertAsync("Error", message);
            }
        }
        catch (Exception ex)
        {
            await AlertService.ShowAlertAsync("Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async void OnConfirmSmsCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code != verificationCode)
        {
            await AlertService.ShowAlertAsync("Code", "Kod noto‘g‘ri");
            return;
        }

        HideSmsPopupAction?.Invoke();

        await appControl.InitLoginPage(response.resultData, PhoneNumber, Password);
    }

    private async Task OnRegister()
    {
        await AppNavigatorService.NavigateTo(nameof(AuthorizationPage));
    }

    private async Task OnGuestLogin()
    {
         
    }

    private async Task OnForgotPassword()
    {
         await AppNavigatorService.NavigateTo(nameof(ForgotPasswordPage));
    }
}