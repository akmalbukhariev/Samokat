using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Samokat.Views.ChangePassword;

public partial class ChangePasswordPage : BasePage, INotifyPropertyChanged
{
    private string _currentPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public string CurrentPassword
    {
        get => _currentPassword;
        set
        {
            if (_currentPassword != value)
            {
                _currentPassword = value;
                OnPropertyChanged();
                UpdateState();
            }
        }
    }

    public string NewPassword
    {
        get => _newPassword;
        set
        {
            if (_newPassword != value)
            {
                _newPassword = value;
                OnPropertyChanged();
                UpdateState();
            }
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            if (_confirmPassword != value)
            {
                _confirmPassword = value;
                OnPropertyChanged();
                UpdateState();
            }
        }
    }

    public bool CanSubmit =>
        !string.IsNullOrWhiteSpace(CurrentPassword)
        && !string.IsNullOrWhiteSpace(NewPassword)
        && !string.IsNullOrWhiteSpace(ConfirmPassword);

    public Color SubmitButtonColor =>
        CanSubmit ? Color.FromArgb("#7CB518") : Color.FromArgb("#DADADA");

    public ICommand SubmitCommand { get; }

    public ChangePasswordPage()
    {
        InitializeComponent();
        BindingContext = this;

        SubmitCommand = new Command(OnSubmit);
    }

    private async void OnSubmit()
    {
        if (!CanSubmit)
        {
            await DisplayAlert("Xatolik", "Iltimos, barcha parol maydonlarini to’ldiring.", "OK");
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            await DisplayAlert("Xatolik", "Yangi parol va takroriy parol mos emas.", "OK");
            return;
        }

        await DisplayAlert("OK", "Parolni o’zgartirish bosildi.", "OK");

        // TODO: Call API here
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