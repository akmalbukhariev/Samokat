using NinimumDelivery.ViewModels;
namespace NinimumDelivery.Views;
public partial class DeliveriesPage : ContentPage
{
    private readonly DeliveriesViewModel vm;
    public DeliveriesPage(DeliveriesViewModel vm) { InitializeComponent(); BindingContext = this.vm = vm; }
    protected override async void OnAppearing() { base.OnAppearing(); await vm.LoadCommand.ExecuteAsync(null); }
}
