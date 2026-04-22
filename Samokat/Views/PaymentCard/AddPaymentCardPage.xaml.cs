using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Samokat.Views.PaymentCard;

public partial class AddPaymentCardPage : BasePage, INotifyPropertyChanged
{
    private string _cardNumber = "8600 1204 2881 8500";
    private string _expireDate = string.Empty;
    private string _cvv = string.Empty;
    private bool _rememberCard = true;
    private bool _isUpdatingExpireDate;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public string CardNumber
    {
        get => _cardNumber;
        set
        {
            if (_cardNumber != value)
            {
                _cardNumber = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string Cvv
    {
        get => _cvv;
        set
        {
            if (_cvv != value)
            {
                _cvv = value;
                OnPropertyChanged();
            }
        }
    }

    public bool RememberCard
    {
        get => _rememberCard;
        set
        {
            if (_rememberCard != value)
            {
                _rememberCard = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand ToggleRememberCommand { get; }
    public ICommand SaveCardCommand { get; }

    public AddPaymentCardPage()
    {
        InitializeComponent();

        ToggleRememberCommand = new Command(() =>
        {
            RememberCard = !RememberCard;
        });

        SaveCardCommand = new Command(async () => await OnSaveCard());

        BindingContext = this;
    }

    private string _expireMonth = string.Empty;
    public string ExpireMonth
    {
        get => _expireMonth;
        set
        {
            var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digits.Length > 2)
                digits = digits[..2];

            if (_expireMonth != digits)
            {
                _expireMonth = digits;
                OnPropertyChanged();
            }
        }
    }

    private string _expireYear = string.Empty;
    public string ExpireYear
    {
        get => _expireYear;
        set
        {
            var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digits.Length > 2)
                digits = digits[..2];

            if (_expireYear != digits)
            {
                _expireYear = digits;
                OnPropertyChanged();
            }
        }
    }

    public string ExpireDate => $"{ExpireMonth}/{ExpireYear}";

    private async Task OnSaveCard()
    {
        await DisplayAlert("Info", "Karta saqlandi.", "OK");
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}