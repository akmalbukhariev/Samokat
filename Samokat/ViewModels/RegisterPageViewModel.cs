using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Samokat.ViewModels;

public partial class RegisterPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private string region = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    private string childFirstName = string.Empty;

    [ObservableProperty]
    private string childLastName = string.Empty;

    [ObservableProperty]
    private string childBirthDate = string.Empty;

    [ObservableProperty]
    private bool isBoySelected = true;

    [ObservableProperty]
    private bool isGirlSelected = false;

    [ObservableProperty]
    private bool isAgreementChecked = false;

    [RelayCommand]
    private async Task Back()
    {
        if (Application.Current?.MainPage is not null)
            await Application.Current.MainPage.Navigation.PopAsync();
    }

    [RelayCommand]
    private void SelectGender(string gender)
    {
        if (gender == "boy")
        {
            IsBoySelected = true;
            IsGirlSelected = false;
        }
        else if (gender == "girl")
        {
            IsBoySelected = false;
            IsGirlSelected = true;
        }
    }

    [RelayCommand]
    private async Task AddChild()
    {
        if (Application.Current?.MainPage is not null)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Info",
                "Add child logic goes here.",
                "OK");
        }
    }

    [RelayCommand]
    private async Task Register()
    {
        if (!IsAgreementChecked)
        {
            if (Application.Current?.MainPage is not null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ogohlantirish",
                    "Iltimos, tasdiqlash belgisini tanlang.",
                    "OK");
            }
            return;
        }

        if (Application.Current?.MainPage is not null)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Info",
                "Register logic goes here.",
                "OK");
        }
    }

    [RelayCommand]
    private void ToggleAgreement()
    {
        IsAgreementChecked = !IsAgreementChecked;
    }
}