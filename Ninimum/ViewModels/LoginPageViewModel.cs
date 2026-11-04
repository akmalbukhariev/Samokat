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

            PhoneNumber = appControl.NormalizePhoneNumber(PhoneNumber);

            if (string.IsNullOrWhiteSpace(Password))
            {
                await AlertService.ShowAlertAsync(AppResource.Information, AppResource.PleaseEnterPassword);
                return;
            }

            var monitor = AppService.Get<ConnectionMonitorService>();
            if (monitor != null && !await monitor.CheckNowAsync())
                await monitor.WaitUntilConnectedAsync();

            IsLoading = true;

            response = await apiService.Login(new LoginUserRequest
            {
                phone_number = PhoneNumber,
                password = Password,
                device_token = appControl.DeviceToken
            });

            if (response.resultCode == ApiResult.SUCCESS.GetCodeToString())
            {
                // This phone confirmed a code before: no SMS needed.
                await appControl.InitLoginPage(response.resultData, PhoneNumber, Password);
            }
            else if (response.resultCode == "SMS_CODE_SENT")
            {
                ShowSmsPopupAction?.Invoke();
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
            await AlertService.ShowAlertAsync(AppResource.Error, global::Utils.ServerMessage.Exception(ex));
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async void OnConfirmSmsCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            await AlertService.ShowAlertAsync(AppResource.Code, AppResource.IncorrectCode);
            return;
        }

        try
        {
            IsLoading = true;
            response = await apiService.Login(new LoginUserRequest { phone_number = PhoneNumber, password = Password, sms_code = code.Trim() });
        }
        finally
        {
            IsLoading = false;
        }

        if (response.resultCode != ApiResult.SUCCESS.GetCodeToString() || response.resultData == null)
        {
            await AlertService.ShowAlertAsync(AppResource.Code, response.resultMsg ?? AppResource.IncorrectCode);
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
            IsLoading = true;

            // Signing in again without a code makes the server send a new one (at most one a minute).
            var resend = await apiService.Login(new LoginUserRequest { phone_number = PhoneNumber.Trim(), password = Password });
            if (resend.resultCode != "SMS_CODE_SENT")
                await AlertService.ShowAlertAsync(AppResource.Error, resend.resultMsg ?? AppResource.SMSWasNotSent);
        }
        catch (Exception ex)
        {
            await AlertService.ShowAlertAsync(AppResource.Error, global::Utils.ServerMessage.Exception(ex));
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
                ["PhoneNumber"] = appControl.NormalizePhoneNumber(PhoneNumber)
            });
    }
}