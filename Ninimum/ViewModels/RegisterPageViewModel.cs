using Ninimum.Resources.Languages;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Api.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using Models.Requests;
using Models.Responses;
using Ninimum.Models;
using Ninimum.Services;
using Ninimum.Views.LoginRegister;
using Utils;
using CommunityToolkit.Mvvm.Messaging;

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
    private string address = string.Empty;
    [ObservableProperty]
    private double locationLatitude;

    [ObservableProperty]
    private double locationLongitude;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private ObservableCollection<ChildInputModel> children = new();

    [ObservableProperty]
    private bool isAgreementChecked = false;
    #endregion

    [ObservableProperty]
    private ICommand addressTapCommand;

    private readonly UserApiService apiService;
    private readonly AppControl appControl;

    public RegisterPageViewModel(UserApiService apiService, AppControl appControl)
    {
        this.apiService = apiService;
        this.appControl = appControl;
  
        WeakReferenceMessenger.Default.Register<object, SelectedAddressModel, string>(
                this,
                "SelectedAddress",
                (recipient, selectedAddress) =>
                {
                    Address = selectedAddress.Address;
                    LocationLatitude = selectedAddress.Latitude;
                    LocationLongitude = selectedAddress.Longitude;
                });

        AddressTapCommand = new Command(AdressTapped);

        /*FirstName = "Akmal";
        LastName = "Karimov";
        PhoneNumber = "998998887766";
        Region = appControl.CurrentRegionName;
        Password = "123";
        ConfirmPassword = "123";
        IsAgreementChecked = true;*/

        /*Children.Add(new ChildInputModel
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
        });*/
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
                AppResource.Warning,
                AppResource.PleaseEnterFirstName);
            return;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            await AlertService.ShowAlertAsync(
                AppResource.Warning,
                AppResource.PleaseEnterLastName);
            return;
        }

        if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            await AlertService.ShowAlertAsync(
                AppResource.Warning,
                AppResource.PhoneNumberNotFoundPleaseRestartRegistration);
            return;
        }

        if (string.IsNullOrWhiteSpace(Address) || Address == AppResource.Address)
        {
            await AlertService.ShowAlertAsync(
                AppResource.Warning,
                AppResource.PleaseSelectYourAddress);
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            await AlertService.ShowAlertAsync(
                AppResource.Warning,
                AppResource.PleaseEnterPassword);
            return;
        }

        if (string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            await AlertService.ShowAlertAsync(
                AppResource.Warning,
                AppResource.PleaseConfirmPassword);
            return;
        }

        if (!IsAgreementChecked)
        {
            await AlertService.ShowAlertAsync(
                AppResource.Warning,
                AppResource.PleaseSelectTheConfirmationCheckbox);
            return;
        }

        if (Password != ConfirmPassword)
        {
            await AlertService.ShowAlertAsync(
                AppResource.Warning,
                AppResource.PasswordsDoNotMatch);
            return;
        }

        var request = new RegisterUserRequest
        {
            region_id = appControl.SelectedRegionId,
            first_name = FirstName.Trim(),
            last_name = LastName.Trim(),
            location_latitude = LocationLatitude,
            location_longitude = LocationLongitude,
            address = Address.Trim(),
            phone_number = PhoneNumber.Trim(),
            password = Password
        };

        try
        {
            IsLoading = true;

            Response response = await apiService.RegisterUser(request);

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                await AlertService.ShowAlertAsync(
                    AppResource.Error,
                    response.resultMsg ?? AppResource.CouldNotRegister);
                return;
            }

            bool loggedIn = await appControl.Login(PhoneNumber.Trim(), Password);

            if (!loggedIn)
            {
                await AlertService.ShowAlertAsync(
                    AppResource.InformationAscii,
                    AppResource.RegistrationWasSuccessfulButAutomaticSignInFailed);
            }
        }
        catch (Exception ex)
        {
            await AlertService.ShowAlertAsync(AppResource.Error, ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleAgreement()
    {
        IsAgreementChecked = !IsAgreementChecked;
    }

    private async void AdressTapped()
    {
        AddressSelectionNavigationStore.Prepare(
            AddressSelectionMode.Registration,
            Address,
            LocationLatitude == 0 ? null : LocationLatitude,
            LocationLongitude == 0 ? null : LocationLongitude);

        await AppNavigatorService.NavigateTo(nameof(AddressPage));
    }
}