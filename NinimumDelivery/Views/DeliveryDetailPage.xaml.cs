using NinimumDelivery.ViewModels;
namespace NinimumDelivery.Views;
[QueryProperty(nameof(JobId), "jobId")]
public partial class DeliveryDetailPage : ContentPage
{
    private readonly DeliveryDetailViewModel vm;
    public DeliveryDetailPage(DeliveryDetailViewModel vm) { InitializeComponent(); BindingContext = this.vm = vm; }
    public string JobId { set { if (long.TryParse(value, out var id)) MainThread.BeginInvokeOnMainThread(async () => await vm.LoadAsync(id)); } }
}
