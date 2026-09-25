using NinimumDelivery.ViewModels;

namespace NinimumDelivery.Views;

public partial class DeliveriesPage : ContentPage
{
    private readonly DeliveriesViewModel vm;
    private bool lifecycleSubscribed;

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
    }

    protected override void OnDisappearing()
    {
        vm.CancelLoading();
        UnsubscribeLifecycle();

        base.OnDisappearing();
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
        vm.CancelLoading();
    }

    private async void OnAppResumed(object? sender, EventArgs e)
    {
        await vm.ReloadAsync();
    }
}
