using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Samokat.Views.LoginRegister;

public partial class LoginPage : BasePage, INotifyPropertyChanged
{
    private string _phoneNumber = string.Empty;
    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            if (_phoneNumber != value)
            {
                _phoneNumber = value;
                OnPropertyChanged();
            }
        }
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set
        {
            if (_password != value)
            {
                _password = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand GuestLoginCommand { get; }
    public ICommand ForgotPasswordCommand { get; }
    public ICommand ConfirmSmsCodeCommand { get; }

    public LoginPage()
    {
        InitializeComponent();

        LoginCommand = new Command(OnLogin);
        RegisterCommand = new Command(OnRegister);
        GuestLoginCommand = new Command(OnGuestLogin);
        ForgotPasswordCommand = new Command(OnForgotPassword);
        ConfirmSmsCodeCommand = new Command<string>(OnConfirmSmsCode);
        popupSms.ConfirmCommand = ConfirmSmsCodeCommand;
        
        BindingContext = this;
    }

    private async void OnLogin()
    {
        await AnimateElementScaleDown(btnLogin);

        popupSms.Show();

        /*if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            await DisplayAlert("Xatolik", "Telefon raqamni kiriting", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            await DisplayAlert("Xatolik", "Parolni kiriting", "OK");
            return;
        }
 
        await DisplayAlert("Login", $"Phone: {PhoneNumber}\nPassword: {Password}", "OK");*/
    }

    private void OnConfirmSmsCode(string code)
    {
        popupSms.Hide();
    }

    private async void OnRegister()
    {
        await AnimateElementScaleDown(btnRegister);
        await DisplayAlert("Register", "Ro’yxatdan o’tish bosildi", "OK");
    }

    private async void OnGuestLogin()
    {
        await AnimateElementScaleDown(btnGeust);
        await DisplayAlert("Guest", "Mehmon sifatida kirish bosildi", "OK");
    }

    private async void OnForgotPassword()
    {
        // TODO: forgot password page logic here
        await DisplayAlert("Forgot Password", "Parolni tiklash bosildi", "OK");
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}