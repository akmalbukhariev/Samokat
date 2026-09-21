using Ninimum.Resources.Languages;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Api.Services;
using Models.Requests;
using Ninimum.Services;
using Utils;

namespace Ninimum.Views.LoginRegister;

public partial class ForgotPasswordPage : BasePage, INotifyPropertyChanged, IQueryAttributable
{
    private readonly UserApiService apiService;

    private string _phoneNumber = string.Empty;
    private bool _isLoading;

    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            _phoneNumber = value;
            OnPropertyChanged();
        }
    }

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

    public ForgotPasswordPage(UserApiService apiService)
    {
        InitializeComponent();

        this.apiService = apiService;
        SendCommand = new Command(OnSendTapped);

        BindingContext = this;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("PhoneNumber", out var phoneNumberValue))
            PhoneNumber = phoneNumberValue?.ToString()?.Trim() ?? string.Empty;
    }

    private async void OnSendTapped()
    {
        await ClickGuard.RunAsync(btnSend, async () =>
        {
            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                await AlertService.ShowAlertAsync(
                    AppResource.InformationAscii,
                    AppResource.PhoneNumberNotFoundPleaseReturnToThe);
                return;
            }

            try
            {
                IsLoading = true;

                var response = await apiService.SendTempPassword(new VerifyPhoneNumberRequest
                {
                    phone_number = PhoneNumber.Trim()
                });

                if (response.resultCode == ApiResult.SUCCESS.GetCodeToString())
                {
                    await AlertService.ShowAlertAsync(
                        AppResource.Success,
                        AppResource.ATemporaryPasswordWasSentToYourPhone);

                    await AppNavigatorService.NavigateTo("..");
                    return;
                }

                if (response.resultCode == ApiResult.USER_NOT_EXIST.GetCodeToString())
                {
                    await AlertService.ShowAlertAsync(
                        AppResource.InformationAscii,
                        AppResource.NoUserWasFoundWithThisPhoneNumber);
                    return;
                }

                await AlertService.ShowAlertAsync(
                    AppResource.Error,
                    response.resultMsg ?? AppResource.CouldNotSendTheTemporaryPassword);
            }
            catch (Exception ex)
            {
                await AlertService.ShowAlertAsync(AppResource.Error, ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        });
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
