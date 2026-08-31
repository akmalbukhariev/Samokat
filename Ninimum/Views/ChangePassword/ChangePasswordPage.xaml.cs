using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Api.Services;
using Models.Requests;
using Ninimum.Services;
using Utils;

namespace Ninimum.Views.ChangePassword;

public partial class ChangePasswordPage : BasePage, INotifyPropertyChanged
{
    private readonly UserApiService apiService;
    private readonly AppControl appControl;
    private readonly AppStoreService storeService;
    private readonly IKeyboardHelper keyboardHelper;

    private string _currentPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;
    private bool _isLoading;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public string CurrentPassword
    {
        get => _currentPassword;
        set
        {
            if (_currentPassword == value)
                return;

            _currentPassword = value;
            OnPropertyChanged();
            UpdateState();
        }
    }

    public string NewPassword
    {
        get => _newPassword;
        set
        {
            if (_newPassword == value)
                return;

            _newPassword = value;
            OnPropertyChanged();
            UpdateState();
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            if (_confirmPassword == value)
                return;

            _confirmPassword = value;
            OnPropertyChanged();
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

    public bool CanSubmit =>
        !IsLoading &&
        !string.IsNullOrWhiteSpace(CurrentPassword) &&
        !string.IsNullOrWhiteSpace(NewPassword) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword);

    public Color SubmitButtonColor =>
        CanSubmit ? Color.FromArgb("#7CB518") : Color.FromArgb("#DADADA");

    public ICommand SubmitCommand { get; }

    public ChangePasswordPage(
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

        SubmitCommand = new Command(async () => await OnSubmitAsync());
        BindingContext = this;
    }

    private async Task OnSubmitAsync()
    {
        if (!CanSubmit)
            return;

        keyboardHelper.HideKeyboard();

        if (NewPassword.Length < 6)
        {
            await DisplayAlert("Xatolik", "Yangi parol kamida 6 ta belgidan iborat bo‘lishi kerak.", "Yopish");
            return;
        }

        if (!string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal))
        {
            await DisplayAlert("Xatolik", "Yangi parol va takroriy parol mos emas.", "Yopish");
            return;
        }

        if (string.Equals(CurrentPassword, NewPassword, StringComparison.Ordinal))
        {
            await DisplayAlert("Xatolik", "Yangi parol amaldagi paroldan farq qilishi kerak.", "Yopish");
            return;
        }

        try
        {
            IsLoading = true;

            var response = await apiService.ChangePassword(new ChangePasswordRequest
            {
                userId = appControl.CurrentUserId,
                currentPassword = CurrentPassword,
                newPassword = NewPassword
            });

            if (response.resultCode == ApiResult.PASSWORD_IS_NOT_MATCHED.GetCodeToString())
            {
                await DisplayAlert("Xatolik", "Amaldagi parol noto‘g‘ri.", "Yopish");
                return;
            }

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                await DisplayAlert("Xatolik", response.resultMsg ?? "Parolni o‘zgartirib bo‘lmadi.", "Yopish");
                return;
            }

            storeService.Set(AppKeys.Password, NewPassword);

            await DisplayAlert("Muvaffaqiyatli", "Parolingiz o‘zgartirildi.", "OK");
            await AppNavigatorService.NavigateTo("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] ChangePassword => {ex}");
            await DisplayAlert("Xatolik", "Parolni o‘zgartirib bo‘lmadi.", "Yopish");
        }
        finally
        {
            IsLoading = false;
        }
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
