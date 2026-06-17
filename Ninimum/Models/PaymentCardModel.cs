using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Ninimum.Models;

public class PaymentCardModel : INotifyPropertyChanged
{
    public long Id { get; set; }

    private string _cardNumber = string.Empty;
    private string _expireDate = string.Empty;
    private bool _isPrimary;

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

    public string ExpireDate
    {
        get => _expireDate;
        set
        {
            if (_expireDate != value)
            {
                _expireDate = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsPrimary
    {
        get => _isPrimary;
        set
        {
            if (_isPrimary != value)
            {
                _isPrimary = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}