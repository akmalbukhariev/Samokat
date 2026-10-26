using Ninimum.ViewModels;
using Ninimum.Services;
using Ninimum.Models;

namespace Ninimum.Views.Orders;

public partial class OrdersPage : BasePage
{
    private readonly OrdersPageViewModel viewModel;
    private readonly Api.Services.UserApiService recurringApi;
    private IDispatcherTimer? statusRefreshTimer;
    private bool isRefreshingStatuses;

    public OrdersPage(OrdersPageViewModel vm, AppControl appControl, Api.Services.UserApiService recurringApi)
    {
        InitializeComponent();

        viewModel = vm;
        this.recurringApi=recurringApi;
        RecurringButton.Text=System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch {"ru"=>"Повторные заказы","uz"=>"Takroriy buyurtmalar",_=>"Recurring orders"};
        this.appControl = appControl;
        BindingContext = vm;
    }

    private async void RecurringClicked(object sender,EventArgs e)
    {
        await Navigation.PushAsync(new RecurringOrdersPage(recurringApi));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!await appControl.EnsureAuthenticatedAsync(true))
            return;

        await viewModel.LoadOrdersAsync();
        StartStatusRefresh();
    }

    protected override void OnDisappearing()
    {
        statusRefreshTimer?.Stop();
        base.OnDisappearing();
    }

    private void StartStatusRefresh()
    {
        if (statusRefreshTimer == null)
        {
            statusRefreshTimer = Dispatcher.CreateTimer();
            // Each tick asks for every active order, so not too often.
            statusRefreshTimer.Interval = TimeSpan.FromSeconds(15);
            statusRefreshTimer.Tick += OnStatusRefreshTick;
        }

        if (!statusRefreshTimer.IsRunning)
            statusRefreshTimer.Start();
    }

    private async void OnStatusRefreshTick(object? sender, EventArgs e)
    {
        if (isRefreshingStatuses)
            return;

        try
        {
            isRefreshingStatuses = true;
            await viewModel.RefreshActiveOrderStatusesAsync();
        }
        finally
        {
            isRefreshingStatuses = false;
        }
    }

    private static string L(string uz, string ru, string en)
    {
        var lang = (Ninimum.Resources.Languages.AppResource.Culture ?? System.Globalization.CultureInfo.CurrentUICulture).TwoLetterISOLanguageName;
        return lang == "ru" ? ru : lang == "en" ? en : uz;
    }

    // "Mahsulotni oldim": the customer confirms the taxi delivered the order.
    private async void TaxiReceived_Clicked(object sender, EventArgs e)
    {
        if (sender is not VisualElement { BindingContext: OrderItemModel order }) return;
        if (!await DisplayAlert(L("Mahsulotni oldingizmi?", "Вы получили заказ?", "Did you receive the order?"), L("Buyurtma «Yetkazildi» deb belgilanadi.", "Заказ будет отмечен как «Доставлен».", "The order will be marked as delivered."), L("Ha, oldim", "Да, получил", "Yes, received"), L("Yo‘q", "Нет", "No"))) return;
        var response = await recurringApi.TaxiReceived(order.OrderId);
        if (response.resultCode != "100") { await DisplayAlert(Ninimum.Resources.Languages.AppResource.Error, response.resultMsg ?? Ninimum.Resources.Languages.AppResource.AnErrorOccurred, Ninimum.Resources.Languages.AppResource.Ok); return; }
        AppVibrationService.Success();
        await viewModel.LoadOrdersAsync();
    }

    // "Muammo bor": the customer writes what went wrong; the operator gets an alert and calls back.
    private async void TaxiProblem_Clicked(object sender, EventArgs e)
    {
        if (sender is not VisualElement { BindingContext: OrderItemModel order }) return;
        var reason = await DisplayPromptAsync(L("Muammo bor", "Есть проблема", "There is a problem"), L("Nima bo‘ldi? (masalan: taksi kelmadi)", "Что случилось? (например: такси не приехало)", "What happened? (e.g. the taxi didn’t come)"), L("Yuborish", "Отправить", "Send"), L("Bekor qilish", "Отмена", "Cancel"), maxLength: 500);
        if (string.IsNullOrWhiteSpace(reason)) return;
        var response = await recurringApi.TaxiProblem(order.OrderId, reason.Trim());
        if (response.resultCode != "100") { await DisplayAlert(Ninimum.Resources.Languages.AppResource.Error, response.resultMsg ?? Ninimum.Resources.Languages.AppResource.AnErrorOccurred, Ninimum.Resources.Languages.AppResource.Ok); return; }
        await DisplayAlert(L("Xabar yuborildi", "Сообщение отправлено", "Message sent"), L("Operator tez orada siz bilan bog‘lanadi.", "Оператор скоро свяжется с вами.", "An operator will contact you soon."), Ninimum.Resources.Languages.AppResource.Ok);
    }

    // Tap the taxi box to call the driver once the taxi is booked.
    private void TaxiInfo_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is not VisualElement { BindingContext: OrderItemModel order } || !order.HasTaxiPhone) return;
        try { PhoneDialer.Default.Open(order.TaxiDriverPhone!); } catch { /* no dialer on this device */ }
    }

    private async void Product_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is not VisualElement element ||
            element.BindingContext is not OrderProductItemModel product)
        return;

        await ClickGuard.RunAsync(element, async () =>
        {
            await element.ScaleTo(0.95, 100, Easing.CubicOut);
            await element.ScaleTo(1.0, 100, Easing.CubicIn);

            AppVibrationService.Like();
            
            if (viewModel.ProductClickedCommand.CanExecute(product))
                viewModel.ProductClickedCommand.Execute(product);
        });
    }
}
