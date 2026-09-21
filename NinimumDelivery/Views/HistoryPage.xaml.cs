using NinimumDelivery.ViewModels;
namespace NinimumDelivery.Views;
public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel vm;
    public HistoryPage(HistoryViewModel vm) { InitializeComponent(); BindingContext = this.vm = vm; }
    protected override async void OnAppearing() { base.OnAppearing(); await vm.LoadCommand.ExecuteAsync(null); }
}
