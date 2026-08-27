using Api.Services;
using Models.Requests;
using Ninimum.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Utils;

namespace Ninimum.Views.PaymentCard;

public partial class CancelOrderPage : BasePage, INotifyPropertyChanged, IQueryAttributable
{
    private readonly UserApiService apiService;
    private readonly AppControl appControl;

    private long orderId;
    private string productName = string.Empty;
    private string orderNumber = string.Empty;
    private string orderDate = string.Empty;
    private string orderAmount = string.Empty;
    private string cancelReason = string.Empty;
    private bool isCancelling;
    private bool cancellationSucceeded;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public string ProductName
    {
        get => productName;
        private set => SetField(ref productName, value);
    }

    public string OrderNumber
    {
        get => orderNumber;
        private set => SetField(ref orderNumber, value);
    }

    public string OrderDate
    {
        get => orderDate;
        private set => SetField(ref orderDate, value);
    }

    public string OrderAmount
    {
        get => orderAmount;
        private set => SetField(ref orderAmount, value);
    }

    public string CancelReason
    {
        get => cancelReason;
        set => SetField(ref cancelReason, value);
    }

    public bool IsCancelling
    {
        get => isCancelling;
        private set => SetField(ref isCancelling, value);
    }

    public ICommand CancelOrderCommand { get; }
    public ICommand DoNotCancelCommand { get; }

    public CancelOrderPage(UserApiService apiService, AppControl appControl)
    {
        InitializeComponent();

        this.apiService = apiService;
        this.appControl = appControl;

        CancelOrderCommand = new Command(OnCancelOrder);
        DoNotCancelCommand = new Command(OnDoNotCancel);

        CancelOrderPopup.Confirmed += OnCancelConfirmed;
        CancelOrderPopup.Closed += OnPopupClosed;

        BindingContext = this;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("OrderId", out var orderIdValue))
            long.TryParse(orderIdValue?.ToString(), out orderId);

        if (query.TryGetValue("ProductName", out var productNameValue))
            ProductName = productNameValue?.ToString() ?? string.Empty;

        if (query.TryGetValue("OrderNumber", out var orderNumberValue))
            OrderNumber = orderNumberValue?.ToString() ?? string.Empty;

        if (query.TryGetValue("OrderDate", out var orderDateValue))
            OrderDate = orderDateValue?.ToString() ?? string.Empty;

        if (query.TryGetValue("OrderAmount", out var orderAmountValue))
            OrderAmount = orderAmountValue?.ToString() ?? string.Empty;
    }

    private async void OnCancelOrder()
    {
        if (orderId <= 0)
        {
            await DisplayAlert("Xatolik", "Buyurtma ma’lumoti topilmadi.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(CancelReason))
        {
            await DisplayAlert("Xatolik", "Iltimos, bekor qilish sababini yozing.", "OK");
            return;
        }

        CancelOrderPopup.ShowConfirm();
    }

    private async void OnCancelConfirmed(object? sender, EventArgs e)
    {
        if (IsCancelling)
            return;

        try
        {
            IsCancelling = true;

            var response = await apiService.CancelOrder(new CancelOrderRequest
            {
                orderId = orderId,
                userId = (long)appControl.userDto.id,
                reason = CancelReason.Trim()
            });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                CancelOrderPopup.IsVisible = false;
                await DisplayAlert("Xatolik", response.resultMsg ?? "Buyurtmani bekor qilib bo‘lmadi.", "OK");
                return;
            }

            cancellationSucceeded = true;
            CancelOrderPopup.ShowSuccess();
        }
        catch (Exception ex)
        {
            CancelOrderPopup.IsVisible = false;
            await DisplayAlert("Xatolik", $"Buyurtmani bekor qilib bo‘lmadi.\n{ex.Message}", "OK");
        }
        finally
        {
            IsCancelling = false;
        }
    }

    private async void OnDoNotCancel()
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnPopupClosed(object? sender, EventArgs e)
    {
        if (!cancellationSucceeded)
            return;

        cancellationSucceeded = false;
        await Shell.Current.GoToAsync("..");
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
