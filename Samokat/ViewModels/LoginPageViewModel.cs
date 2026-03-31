using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Samokat.ViewModels;

public partial class LoginPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [RelayCommand]
    private void Login()
    {
    }

    [RelayCommand]
    private void Register()
    {
    }

    [RelayCommand]
    private void GuestLogin()
    {
    }

    [RelayCommand]
    private void ForgotPassword()
    {
    }
}