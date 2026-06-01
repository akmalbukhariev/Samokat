using Api.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models.Requests;
using Models.Responses;
using Ninimum.Models;
using Ninimum.Services;
using Utils;

namespace Ninimum.Views.LoginRegister;

public partial class ChangePasswordPage : BasePage
{
    private readonly UserApiService apiService;
    private readonly AppControl appControl;

    public ChangePasswordPage(UserApiService apiService, AppControl appControl)
    {
        InitializeComponent();
        BindingContext = this;

        this.apiService = apiService;
        this.appControl = appControl;
    }

    private bool isLoading;
    public bool IsLoading
    {
        get => isLoading;
        set
        {
            isLoading = value;
            OnPropertyChanged();
        }
    }

    public string PhoneNumber { get; set; }

    [RelayCommand]
    private async Task Save()
    {
        string password = txtPassword.Text?.Trim();
        string confirmPassword = txtConfirmPassword.Text?.Trim();

        if (string.IsNullOrWhiteSpace(password))
        {
            await AlertService.ShowAlertAsync("Xato", "Yangi parolni kiriting.");
            return;
        }

        if (password.Length < 6)
        {
            await AlertService.ShowAlertAsync("Xato", "Parol kamida 6 ta belgidan iborat bo‘lishi kerak.");
            return;
        }

        if (password != confirmPassword)
        {
            await AlertService.ShowAlertAsync("Xato", "Parollar mos emas.");
            return;
        }

        ForgotPasswordParam request = new ForgotPasswordParam
        {
            phoneNumber = PhoneNumber,
            tempPassword = password
        };

        IsLoading = true;
        Response response = await apiService.UpdateUserPassword(request);
        IsLoading = false;

        if (response.resultCode == ApiResult.SUCCESS.GetCodeToString())
        {
            await AlertService.ShowAlertAsync("Success", "Parol muvaffaqiyatli yangilandi.");
            await appControl.Login(PhoneNumber, password);
        }
        else
        {
            await AlertService.ShowAlertAsync("Xato", response.resultMsg);
        }
    }
}