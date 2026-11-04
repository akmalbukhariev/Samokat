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

            PhoneNumber = appControl.NormalizePhoneNumber(PhoneNumber);
            if (!appControl.IsUzbekPhoneNumber(PhoneNumber))
            {
                await AlertService.ShowAlertAsync(AppResource.Error, AppResource.InvalidPhoneNumberExample901234567);
                return;
            }

            bool canUsePhoneNumber = await CheckPhoneNumber();

            if (!canUsePhoneNumber)
                return;

            IsLoading = true;
            string? problem = await appControl.SendVerificationCode(PhoneNumber);
            IsLoading = false;

            if (problem is null)
                popupSms.Show();
            else
                await AlertService.ShowAlertAsync(AppResource.Error, problem);
        });
    }

    private async void OnResendSms()
    {
        IsLoading = true;
        string? problem = await appControl.SendVerificationCode(PhoneNumber);
        IsLoading = false;

        if (problem is not null)
            await AlertService.ShowAlertAsync(AppResource.Error, problem);
    }

    private async void OnConfirmSmsCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            await AlertService.ShowAlertAsync(AppResource.Code, AppResource.IncorrectCode);
            return;
        }

        IsLoading = true;
        var check = await apiService.CheckSmsCode(PhoneNumber.Trim(), code.Trim());
        IsLoading = false;
        if (check.resultCode != ApiResult.SUCCESS.GetCodeToString())
        {
            await AlertService.ShowAlertAsync(AppResource.Code, check.resultMsg ?? AppResource.IncorrectCode);
            return;
        }

        popupSms.Hide();

        await AppNavigatorService.NavigateTo(
            nameof(RegisterPage),
            new Dictionary<string, object>
            {
                ["PhoneNumber"] = PhoneNumber.Trim(),
                ["SmsCode"] = code.Trim()
            });
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