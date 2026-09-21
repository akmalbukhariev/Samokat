using Ninimum.Resources.Languages;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Api.Services;
using Models.Requests;
using Models.Responses;
using Ninimum.Services;
using Ninimum.Views.LoginRegister;
using Utils;

namespace Ninimum.Views.Authorization;

public partial class AuthorizationPage : BasePage, INotifyPropertyChanged
{
    private string verificationCode = "";

    private string _phoneNumber = "";
    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            _phoneNumber = value;
            OnPropertyChanged();
        }
    }
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }
    public ICommand SendCommand { get; }
    public ICommand ResendSmsCommand { get; }
    public ICommand ConfirmSmsCodeCommand { get; }

    private CheckPhoneNumberResponse checkPhoneNumberResponse;

    private readonly UserApiService apiService;
    private readonly AppControl appControl;
    private readonly IKeyboardHelper keyboardHelper;
     
    public AuthorizationPage(
        UserApiService apiService,
        IKeyboardHelper keyboardHelper,
        AppControl appControl)
    {
        InitializeComponent();

        this.apiService = apiService;
        this.appControl = appControl;
        this.keyboardHelper = keyboardHelper;
        
        SendCommand = new Command(OnSendTapped);
        ConfirmSmsCodeCommand = new Command<string>(OnConfirmSmsCode);
        ResendSmsCommand = new Command(OnResendSms);

        BindingContext = this;
    }

    private async void OnSendTapped()
    {
        await ClickGuard.RunAsync(btnSend, async () =>
        {
            keyboardHelper.HideKeyboard();

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                await AlertService.ShowAlertAsync(AppResource.Information, AppResource.EnterPhoneNumber);
                return;
            }

            bool canUsePhoneNumber = await CheckPhoneNumber();

            if (!canUsePhoneNumber)
                return;

            IsLoading = true;
            string? code = await appControl.SendVerificationCode(PhoneNumber);
            IsLoading = false;

            if (!string.IsNullOrEmpty(code))
            {
                verificationCode = code;
                popupSms.Show();
            }
            else
            {
                await AlertService.ShowAlertAsync(AppResource.Error, AppResource.SMSWasNotSent);
            }
        });
    }

    private async void OnResendSms()
    {
        verificationCode = string.Empty;
        IsLoading = true;
        string? code = await appControl.SendVerificationCode(PhoneNumber);
        IsLoading = false;

        if (!string.IsNullOrEmpty(code))
        {
            verificationCode = code;
        }
        else
        {
            await AlertService.ShowAlertAsync(AppResource.Error, AppResource.SMSWasNotSent);
        }
    }

    private async void OnConfirmSmsCode(string code)
    {
        if (!CheckVerificationCode(code))
        {
            await AlertService.ShowAlertAsync(AppResource.Code, AppResource.IncorrectCode);
            return;
        }

        popupSms.Hide();

        await AppNavigatorService.NavigateTo(
            nameof(RegisterPage),
            new Dictionary<string, object>
            {
                ["PhoneNumber"] = PhoneNumber.Trim()
            });
    }
    
    private bool CheckVerificationCode(string code)
    {
        if (string.IsNullOrEmpty(code))
            return false;

        return string.Equals(code, verificationCode);
    }

    private async Task<bool> CheckPhoneNumber()
    {
        try
        {
            CheckPhoneNumberRequest data = new CheckPhoneNumberRequest()
            {
                phone_number = PhoneNumber
            };

            IsLoading = true;

            checkPhoneNumberResponse = await apiService.CheckPhoneNumber(data);

            if (checkPhoneNumberResponse?.resultCode == ApiResult.SUCCESS.GetCodeToString())
            {
                if (checkPhoneNumberResponse.resultData.existsYn == "Y")
                {
                    await AlertService.ShowAlertAsync(AppResource.Information, AppResource.ThisPhoneNumberIsAlreadyRegistered);
                    return false;
                }

                return true;
            }

            await AlertService.ShowAlertAsync(AppResource.Error, checkPhoneNumberResponse?.resultMsg);
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}