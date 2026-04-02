using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Samokat.Views.LoginRegister;

public partial class ForgotPasswordPage : BasePage, INotifyPropertyChanged
{
    private string _phoneNumber;

    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            _phoneNumber = value;
            OnPropertyChanged();
        }
    }

    public ICommand BackCommand { get; }
    public ICommand SendCommand { get; }

    public ForgotPasswordPage()
    {
        InitializeComponent();

        BackCommand = new Command(OnBackTapped);
        SendCommand = new Command(OnSendTapped);
        
        BindingContext = this;
    }

    private async void OnBackTapped()
    {
        //await Navigation.PopAsync();
    }

    private async void OnSendTapped()
    {
        await AnimateElementScaleDown(btnSend);

        if (string.IsNullOrWhiteSpace(PhoneNumber))
            return;

        // your API call or popup logic here
        await DisplayAlert("Info", "Temporary password sending logic goes here.", "OK");
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}