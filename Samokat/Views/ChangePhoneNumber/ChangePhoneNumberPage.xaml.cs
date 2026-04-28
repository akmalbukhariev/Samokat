using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Samokat.Views.ChangePhoneNumber;

public partial class ChangePhoneNumberPage : BasePage, INotifyPropertyChanged
{
    private string _phoneNumber = string.Empty;
    private string _password = string.Empty;
    private bool _isAgreementChecked = true;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public string CurrentPhoneNumber { get; set; } = "+998888888888";

    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            if (_phoneNumber != value)
            {
                _phoneNumber = value;
                OnPropertyChanged();
                UpdateState();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (_password != value)
            {
                _password = value;
                OnPropertyChanged();
                UpdateState();
            }
        }
    }

    public bool IsAgreementChecked
    {
        get => _isAgreementChecked;
        set
        {
            if (_isAgreementChecked != value)
            {
                _isAgreementChecked = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AgreementIcon));
                UpdateState();
            }
        }
    }

    public string AgreementIcon =>
        IsAgreementChecked ? "ic_check.png" : "ic_uncheck.png";

    public bool CanSubmit =>
        !string.IsNullOrWhiteSpace(PhoneNumber) &&
        !string.IsNullOrWhiteSpace(Password) &&
        IsAgreementChecked;

    public Color SubmitButtonColor =>
        CanSubmit ? Color.FromArgb("#FD473C") : Color.FromArgb("#DADADA");

    public ICommand ToggleAgreementCommand { get; }
    public ICommand SubmitCommand { get; }

    public ChangePhoneNumberPage()
    {
        InitializeComponent();

        ToggleAgreementCommand = new Command(() =>
        {
            IsAgreementChecked = !IsAgreementChecked;
        });

        SubmitCommand = new Command(OnSubmit);
        
         BindingContext = this;
    }

    private async void OnSubmit()
    {
        if (!CanSubmit)
        {
            await DisplayAlert("Xatolik", "Iltimos barcha maydonlarni to‘ldiring", "OK");
            return;
        }

        await DisplayAlert("OK", "Telefon raqami o‘zgartirilmoqda", "OK");

        // TODO: API call
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