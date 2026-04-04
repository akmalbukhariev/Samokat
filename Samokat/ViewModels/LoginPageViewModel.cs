using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace Samokat.ViewModels;

public partial class LoginPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand GuestLoginCommand { get; }
    public ICommand ForgotPasswordCommand { get; }
    public ICommand ConfirmSmsCodeCommand { get; }

    public Action? ShowSmsPopupAction { get; set; }
    public Action? HideSmsPopupAction { get; set; }

    public LoginPageViewModel()
    {
        LoginCommand = new Command(async () => await OnLogin());
        RegisterCommand = new Command(async () => await OnRegister());
        GuestLoginCommand = new Command(async () => await OnGuestLogin());
        ForgotPasswordCommand = new Command(async () => await OnForgotPassword());
        ConfirmSmsCodeCommand = new Command<string>(OnConfirmSmsCode);
    }

    private async Task OnLogin()
    {
        ShowSmsPopupAction?.Invoke();
    }

    private void OnConfirmSmsCode(string code)
    {
        HideSmsPopupAction?.Invoke();
    }

    private async Task OnRegister()
    {
        
    }

    private async Task OnGuestLogin()
    {
         
    }

    private async Task OnForgotPassword()
    {
         
    }
}