using NinimumWarehouse.Resources.Languages;
using NinimumWarehouse.Services;
using NinimumWarehouse.ViewModels;
namespace NinimumWarehouse.Views;
public partial class OrderDetailPage : ContentPage
{
    private OrderDetailViewModel ViewModel => (OrderDetailViewModel)BindingContext;
    public OrderDetailPage(WarehouseApi api,long id)
    {
        InitializeComponent(); BindingContext = new OrderDetailViewModel(api,id,ScanAsync,ConfirmReadyAsync,ReportProblemAsync);
        ErrorDialogs.Attach(this, (OrderDetailViewModel)BindingContext);
    }
    protected override async void OnAppearing() { base.OnAppearing(); await ViewModel.RefreshAsync(); }
    protected override void OnDisappearing()
    {
        ViewModel.StopRefresh();
        base.OnDisappearing();
    }
    public Task RefreshAsync() => ViewModel.RefreshAsync();
    private Task<bool> ConfirmReadyAsync() => ViewModel.IsReturn
        ? DisplayAlertAsync(AppResource.ResourceManager.GetString("ReturnQuestion",AppResource.Culture),AppResource.ResourceManager.GetString("ReturnConfirmation",AppResource.Culture),AppResource.Ok,AppResource.Cancel)
        : DisplayAlertAsync(AppResource.ReadyQuestion,AppResource.ReadyConfirmation,AppResource.YesReady,AppResource.Cancel);
    private Task<string?> ReportProblemAsync() => DisplayPromptAsync(AppResource.Problem,AppResource.ProblemPrompt,AppResource.Ok,AppResource.Cancel,maxLength:AppConstants.MaxReasonLength);
    private async Task<string?> ScanAsync()
    {
        if(await Permissions.RequestAsync<Permissions.Camera>() != PermissionStatus.Granted) throw new ApiException("CAMERA");
        var scan = new ScanPage(); await Navigation.PushModalAsync(scan); return await scan.Result;
    }
    private async void OnBackClicked(object? sender,EventArgs e) => await Navigation.PopAsync();
}
