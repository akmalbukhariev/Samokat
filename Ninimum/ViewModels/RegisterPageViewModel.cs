using System.Collections.ObjectModel;
using Api.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using Models.Requests;
using Models.Responses;
using Utils;

namespace Ninimum.ViewModels;

public partial class RegisterPageViewModel : ObservableObject
{
    #region Properties
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
    private bool isLoading = false;

    [ObservableProperty]
    private ObservableCollection<ChildInputModel> children = new();

    [ObservableProperty]
    private bool isAgreementChecked = false;
    #endregion

    private UserApiService apiService;
    public RegisterPageViewModel(UserApiService apiService)
    {
        this.apiService = apiService;
        //Children.Add(new ChildInputModel());

        FirstName = "Akmal";
        LastName = "Karimov";
        PhoneNumber = "998998887766";
        Region = "Toshkent";
        Password = "123";
        ConfirmPassword = "123";
        IsAgreementChecked = true;

        Children.Add(new ChildInputModel
        {
            FirstName = "Ali",
            LastName = "Karimov",
            BirthDate = "2020-01-15",
            IsBoySelected = true,
            IsGirlSelected = false
        });

        Children.Add(new ChildInputModel
        {
            FirstName = "Laylo",
            LastName = "Karimova",
            BirthDate = "2022-05-10",
            IsBoySelected = false,
            IsGirlSelected = true
        });
    }
    
    [RelayCommand]
    private void AddChild()
    {
         Children.Add(new ChildInputModel());
    }
 
    [RelayCommand]
    private async Task Register()
    {
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            await AlertService.ShowAlertAsync(
                "Ogohlantirish",
                "Ismni kiriting.");
            return;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            await AlertService.ShowAlertAsync(
                "Ogohlantirish",
                "Familiyani kiriting.");
            return;
        }

        if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            await AlertService.ShowAlertAsync(
                "Ogohlantirish",
                "Telefon raqamni kiriting.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            await AlertService.ShowAlertAsync(
                "Ogohlantirish",
                "Parolni kiriting.");
            return;
        }

        if (string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            await AlertService.ShowAlertAsync(
                "Ogohlantirish",
                "Parolni tasdiqlang.");
            return;
        }

        if (!IsAgreementChecked)
        {
            await AlertService.ShowAlertAsync(
                "Ogohlantirish",
                "Iltimos, tasdiqlash belgisini tanlang.");
            return;
        }

        if (Password != ConfirmPassword)
        {
            await AlertService.ShowAlertAsync(
                "Ogohlantirish",
                "Parol bir xil emas.");
            return;
        }

        var request = new RegisterUserRequest
        {
            region_id = 1,
            first_name = FirstName.Trim(),
            last_name = LastName.Trim(),
            phone_number = PhoneNumber.Trim(),
            password = Password
        };

        IsLoading = true;
        Response response =
            await apiService.RegisterUser(request);
        IsLoading = false;

        if (response.resultCode == ApiResult.SUCCESS.GetCodeToString())
        {
            await AlertService.ShowAlertAsync("Success", "Ro’yxatdan o’tish muvaffaqiyatli.");
        }
    }

    [RelayCommand]
    private void ToggleAgreement()
    {
        IsAgreementChecked = !IsAgreementChecked;
    }
}