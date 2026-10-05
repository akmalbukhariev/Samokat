using NinimumWarehouse.Services;
using NinimumWarehouse.ViewModels;
namespace NinimumWarehouse.Views;
public partial class OrdersPage : ContentPage
{
    private readonly IDispatcherTimer timer;
    private OrdersViewModel ViewModel => (OrdersViewModel)BindingContext;
    public OrdersPage(WarehouseApi api,string view)
    {
        InitializeComponent();
        BindingContext = new OrdersViewModel(api,view,id => Navigation.PushAsync(new OrderDetailPage(api,id)));
        ErrorDialogs.Attach(this, ViewModel);
        timer = Dispatcher.CreateTimer(); timer.Interval = AppConstants.RefreshInterval;
        timer.Tick += OnTimerTick;
    }
    private async void OnTimerTick(object? sender,EventArgs e) => await ViewModel.RefreshAsync();
    protected override async void OnAppearing() { base.OnAppearing(); timer.Start(); await ViewModel.RefreshAsync(reload:true); }
    protected override void OnDisappearing() { timer.Stop(); ViewModel.StopLoading(); base.OnDisappearing(); }
    public Task RefreshAsync() => ViewModel.RefreshAsync();
}
