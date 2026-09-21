using Ninimum.Resources.Languages;
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
    private ICommand forgotPasswordCommand;
    
    [ObservableProperty]
    private ICommand confirmSmsCodeCommand;

    [ObservableProperty]
    private ICommand resendSmsCommand;

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
        ForgotPasswordCommand = new Command(async () => await OnForgotPassword());
        ConfirmSmsCodeCommand = new Command<string>(OnConfirmSmsCode);
        ResendSmsCommand = new Command(async () => await OnResendSms());
    }

    private async Task OnLogin()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                await AlertService.ShowAlertAsync(AppResource.Information, AppResource.EnterPhoneNumber);
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                await AlertService.ShowAlertAsync(AppResource.Information, AppResource.PleaseEnterPassword);
                return;
            }

            var monitor = AppService.Get<ConnectionMonitorService>();
            if (monitor != null && !await monitor.CheckNowAsync())
                await monitor.WaitUntilConnectedAsync();

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
                    await AlertService.ShowAlertAsync(AppResource.Error, AppResource.SMSWasNotSent);
                }
            }
            else
            {
                string message = response.resultCode switch
                {
                    "PASSWORD_IS_NOT_MATCHED" => AppResource.IncorrectPassword,
                    "USER_NOT_EXIST" => AppResource.UserNotFound,
                    _ => response.resultMsg ?? AppResource.AnErrorOccurred
                };

                await AlertService.ShowAlertAsync(AppResource.Error, message);
            }
        }
        catch (Exception ex)
        {
            await AlertService.ShowAlertAsync(AppResource.Error, ex.Message);
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
            await AlertService.ShowAlertAsync(AppResource.Code, AppResource.IncorrectCode);
            return;
        }

        HideSmsPopupAction?.Invoke();

        await appControl.InitLoginPage(response.resultData, PhoneNumber, Password);
    }

    private async Task OnResendSms()
    {
        if (string.IsNullOrWhiteSpace(PhoneNumber))
            return;

        try
        {
            verificationCode = string.Empty;
            IsLoading = true;

            string? code = await appControl.SendVerificationCode(PhoneNumber.Trim());

            if (string.IsNullOrWhiteSpace(code))
            {
                await AlertService.ShowAlertAsync(AppResource.Error, AppResource.SMSWasNotSent);
                return;
            }

            verificationCode = code;
        }
        catch (Exception ex)
        {
            await AlertService.ShowAlertAsync(AppResource.Error, ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnRegister()
    {
        await AppNavigatorService.NavigateTo(nameof(AuthorizationPage));
    }

    private async Task OnForgotPassword()
    {
        if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            await AlertService.ShowAlertAsync(AppResource.InformationAscii, AppResource.EnterYourPhoneNumberFirst);
            return;
        }

        await AppNavigatorService.NavigateTo(
            nameof(ForgotPasswordPage),
            new Dictionary<string, object>
            {
                ["PhoneNumber"] = PhoneNumber.Trim()
            });
    }
}