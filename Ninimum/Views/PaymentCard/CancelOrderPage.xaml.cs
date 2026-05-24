using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Ninimum.Views.PaymentCard;

public partial class CancelOrderPage : BasePage, INotifyPropertyChanged
{
    private string _cancelReason = string.Empty;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public string ProductName { get; set; } =
        "Kabrita 3 GOLD echki sutiga asoslangan kukunli sutli ichimlik, 12+ oy, 800 g";

    public string OrderNumber { get; set; } = "123456";

    public string OrderDate { get; set; } = "15.02.2026";

    public string OrderAmount { get; set; } = "545 000 so’m";

    public string CancelReason
    {
        get => _cancelReason;
        set
        {
            if (_cancelReason != value)
            {
                _cancelReason = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand CancelOrderCommand { get; }
    public ICommand DoNotCancelCommand { get; }

    public CancelOrderPage()
    {
        InitializeComponent();

        CancelOrderCommand = new Command(OnCancelOrder);
        DoNotCancelCommand = new Command(OnDoNotCancel);

        CancelOrderPopup.Confirmed += OnCancelConfirmed;
        BindingContext = this;
    }

    private async void OnCancelOrder()
    {
        if (string.IsNullOrWhiteSpace(CancelReason))
        {
            await DisplayAlert("Xatolik", "Iltimos, bekor qilish sababini yozing.", "OK");
            return;
        }

        CancelOrderPopup.ShowConfirm();
    }
 
    private async void OnCancelConfirmed(object? sender, EventArgs e)
    {
        await Task.Delay(300);
        CancelOrderPopup.ShowSuccess();
    }

    private async void OnDoNotCancel()
    {
        await Navigation.PopAsync();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}