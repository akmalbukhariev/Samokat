using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Api.Services;
using Models.Requests;
using Ninimum.Services;
using Utils;

namespace Ninimum.Views.ChangePhoneNumber;

public partial class ChangePhoneNumberPage : BasePage, INotifyPropertyChanged
{
    private readonly UserApiService apiService;
    private readonly AppControl appControl;
    private readonly AppStoreService storeService;
    private readonly IKeyboardHelper keyboardHelper;

    private string _phoneNumber = string.Empty;
    private bool _isAgreementChecked = true;
    private bool _isLoading;
    private string verificationCode = string.Empty;
    private string pendingPhoneNumber = string.Empty;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public string CurrentPhoneNumber => FormatPhone(appControl.userDto.phone_number);

    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            if (_phoneNumber == value)
                return;

            _phoneNumber = value;
            OnPropertyChanged();
            UpdateState();
        }
    }

    public bool IsAgreementChecked
    {
        get => _isAgreementChecked;
        set
        {
            if (_isAgreementChecked == value)
                return;

            _isAgreementChecked = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AgreementIcon));
            UpdateState();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading == value)
                return;

            _isLoading = value;
            OnPropertyChanged();
            UpdateState();
        }
    }

    public string AgreementIcon => IsAgreementChecked ? "ic_check.png" : "ic_uncheck.png";
    public bool CanSubmit => !IsLoading && !string.IsNullOrWhiteSpace(PhoneNumber) && IsAgreementChecked;
    public Color SubmitButtonColor => CanSubmit ? Color.FromArgb("#7CB518") : Color.FromArgb("#DADADA");

    public ICommand ToggleAgreementCommand { get; }
    public ICommand SubmitCommand { get; }
    public ICommand ConfirmSmsCodeCommand { get; }
    public ICommand ResendSmsCommand { get; }

    public ChangePhoneNumberPage(
        UserApiService apiService,
        AppControl appControl,
        AppStoreService storeService,
        IKeyboardHelper keyboardHelper)
    {
        InitializeComponent();

        this.apiService = apiService;
        this.appControl = appControl;
        this.storeService = storeService;
        this.keyboardHelper = keyboardHelper;

        ToggleAgreementCommand = new Command(() => IsAgreementChecked = !IsAgreementChecked);
        SubmitCommand = new Command(async () => await OnSubmitAsync());
        ConfirmSmsCodeCommand = new Command<string>(OnConfirmSmsCode);
        ResendSmsCommand = new Command(async () => await SendSmsAsync());

        BindingContext = this;
    }

    private async Task OnSubmitAsync()
    {
        if (!CanSubmit)
            return;

        keyboardHelper.HideKeyboard();

        string normalizedPhone = NormalizePhone(PhoneNumber);

        if (!appControl.IsValidUzbekistanPhoneNumber(normalizedPhone))
        {
            await DisplayAlert("Xatolik", "Telefon raqam noto‘g‘ri. Masalan: 901234567", "Yopish");
            return;
        }

        string currentPhone = NormalizePhone(appControl.userDto.phone_number);
        if (string.Equals(normalizedPhone, currentPhone, StringComparison.Ordinal))
        {
            await DisplayAlert("Ma’lumot", "Yangi telefon raqam amaldagi raqamdan farq qilishi kerak.", "Yopish");
            return;
        }

        try
        {
            IsLoading = true;

            var checkResponse = await apiService.CheckPhoneNumber(new CheckPhoneNumberRequest
            {
                phone_number = normalizedPhone
            });

            if (checkResponse.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                await DisplayAlert("Xatolik", checkResponse.resultMsg ?? "Telefon raqamni tekshirib bo‘lmadi.", "Yopish");
                return;
            }

            if (string.Equals(checkResponse.resultData?.existsYn, "Y", StringComparison.OrdinalIgnoreCase))
            {
                await DisplayAlert("Telefon raqam", "Bu telefon raqam allaqachon ro‘yxatdan o‘tgan.", "Yopish");
                return;
            }

            pendingPhoneNumber = normalizedPhone;
            await SendSmsAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SendSmsAsync()
    {
        if (string.IsNullOrWhiteSpace(pendingPhoneNumber))
            pendingPhoneNumber = NormalizePhone(PhoneNumber);

        if (!appControl.IsValidUzbekistanPhoneNumber(pendingPhoneNumber))
            return;

        try
        {
            IsLoading = true;

            string? code = await appControl.SendVerificationCode(pendingPhoneNumber);

            if (string.IsNullOrWhiteSpace(code))
            {
                await DisplayAlert("Xatolik", "SMS kod yuborilmadi. Iltimos, qayta urinib ko‘ring.", "Yopish");
                return;
            }

            verificationCode = code;
            popupSms.Show();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async void OnConfirmSmsCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code) ||
            !string.Equals(code, verificationCode, StringComparison.Ordinal))
        {
            await DisplayAlert("Kod", "SMS kod noto‘g‘ri.", "Yopish");
            return;
        }

        try
        {
            popupSms.Hide();
            IsLoading = true;

            var response = await apiService.ChangePhoneNumber(new ChangePhoneNumberRequest
            {
                userId = appControl.CurrentUserId,
                phoneNumber = pendingPhoneNumber
            });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                await DisplayAlert("Xatolik", response.resultMsg ?? "Telefon raqamni o‘zgartirib bo‘lmadi.", "Yopish");
                return;
            }

            appControl.userDto.phone_number = pendingPhoneNumber;
            storeService.Set(AppKeys.PhoneNumber, pendingPhoneNumber);

            await DisplayAlert(
                "Muvaffaqiyatli",
                "Telefon raqamingiz o‘zgartirildi. Xavfsizlik uchun qayta kirishingiz kerak.",
                "OK");

            await appControl.StartGuestMode(clearSavedLogin: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] ChangePhoneNumber => {ex}");
            await DisplayAlert("Xatolik", "Telefon raqamni o‘zgartirib bo‘lmadi.", "Yopish");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string NormalizePhone(string? value)
    {
        string digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());

        if (digits.Length == 9)
            return "998" + digits;

        if (digits.Length == 12 && digits.StartsWith("998", StringComparison.Ordinal))
            return digits;

        return digits;
    }

    private static string FormatPhone(string? value)
    {
        string phone = NormalizePhone(value);

        if (phone.Length != 12)
            return value ?? string.Empty;

        return $"+{phone[..3]} {phone.Substring(3, 2)} {phone.Substring(5, 3)} {phone.Substring(8, 2)} {phone.Substring(10, 2)}";
    }

    private void UpdateState()
    {
        OnPropertyChanged(nameof(CanSubmit));
        OnPropertyChanged(nameof(SubmitButtonColor));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
