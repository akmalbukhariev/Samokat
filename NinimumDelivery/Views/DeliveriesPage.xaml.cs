using NinimumDelivery.ViewModels;

namespace NinimumDelivery.Views;

public partial class DeliveriesPage : ContentPage
{
    private readonly DeliveriesViewModel vm;
    private bool lifecycleSubscribed;
    private Microsoft.Maui.Dispatching.IDispatcherTimer? offersTimer;
    private bool pollingOffers;

    public DeliveriesPage(DeliveriesViewModel vm)
    {
        InitializeComponent();
        BindingContext = this.vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        SubscribeLifecycle();
        await vm.ReloadAsync();
        StartOfferPolling();
    }

    protected override void OnDisappearing()
    {
        offersTimer?.Stop();
        vm.CancelLoading();
        UnsubscribeLifecycle();

        base.OnDisappearing();
    }

    private void StartOfferPolling()
    {
        if (offersTimer == null)
        {
            offersTimer = Dispatcher.CreateTimer();
            offersTimer.Interval = TimeSpan.FromSeconds(15);
            offersTimer.Tick += async (_, _) =>
            {
                if (pollingOffers) return;
                pollingOffers = true;
                try { await vm.RefreshOffersAsync(); }
                finally { pollingOffers = false; }
            };
        }
        offersTimer.Start();
    }

    private void SubscribeLifecycle()
    {
        if (lifecycleSubscribed)
            return;

        App.AppStopped += OnAppStopped;
        App.AppResumed += OnAppResumed;
        lifecycleSubscribed = true;
    }

    private void UnsubscribeLifecycle()
    {
        if (!lifecycleSubscribed)
            return;

        App.AppStopped -= OnAppStopped;
        App.AppResumed -= OnAppResumed;
        lifecycleSubscribed = false;
    }

    private void OnAppStopped(object? sender, EventArgs e)
    {
        offersTimer?.Stop();
        vm.CancelLoading();
    }

    private async void OnAppResumed(object? sender, EventArgs e)
    {
        await vm.ReloadAsync();
        StartOfferPolling();
    }
}
