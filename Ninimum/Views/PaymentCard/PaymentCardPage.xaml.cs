using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Ninimum.Views.PaymentCard;

public partial class PaymentCardPage : BasePage
{
    public ObservableCollection<PaymentCardModel> Cards { get; set; }

    public ICommand TogglePrimaryCommand { get; }
    public ICommand DeleteCardCommand { get; }
    public ICommand AddCardCommand { get; }

    public PaymentCardPage()
    {
        InitializeComponent();

        Cards = new ObservableCollection<PaymentCardModel>
        {
            new PaymentCardModel
            {
                CardNumber = "8600 **** **** 8500",
                ExpireDate = "01/28",
                IsPrimary = true
            },
            new PaymentCardModel
            {
                CardNumber = "5660 **** **** 1474",
                ExpireDate = "11/27",
                IsPrimary = false
            }
        };

        TogglePrimaryCommand = new Command<PaymentCardModel>(OnTogglePrimary);
        DeleteCardCommand = new Command<PaymentCardModel>(async card => await OnDeleteCard(card));
        AddCardCommand = new Command(async () => await OnAddCard());

        BindingContext = this;
    }

    private void OnTogglePrimary(PaymentCardModel selectedCard)
    {
        if (selectedCard == null)
            return;

        foreach (var card in Cards)
        {
            card.IsPrimary = card == selectedCard;
        }
    }

    private async Task OnDeleteCard(PaymentCardModel card)
    {
        if (card == null)
            return;

        bool confirm = await DisplayAlert(
            "Kartani o‘chirish",
            $"{card.CardNumber} kartasini o‘chirmoqchimisiz?",
            "Ha",
            "Yo‘q");

        if (!confirm)
            return;

        Cards.Remove(card);

        if (Cards.Count > 0 && !Cards.Any(x => x.IsPrimary))
        {
            Cards[0].IsPrimary = true;
        }
    }

    private async Task OnAddCard()
    {
        // TODO: replace with your real navigation or bottom sheet
        await DisplayAlert("Info", "Yangi karta qo‘shish bosildi.", "OK");

        // Example:
        // await Navigation.PushAsync(new AddPaymentCardPage());
    }
}

public class PaymentCardModel : INotifyPropertyChanged
{
    private string _cardNumber;
    private string _expireDate;
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