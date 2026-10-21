using NinimumWarehouse.Services;
using NinimumWarehouse.ViewModels;
namespace NinimumWarehouse.Views;
public partial class CountsPage : ContentPage
{
    private CountsViewModel ViewModel => (CountsViewModel)BindingContext;
    public CountsPage(WarehouseApi api)
    {
        InitializeComponent();
        BindingContext = new CountsViewModel(api, ScanAsync, (id, bin) => Navigation.PushAsync(new CountDetailPage(api, id, bin)));
        ErrorDialogs.Attach(this, ViewModel);
        TitleLabel.Text = CountsViewModel.Text("Counts");
        SubtitleLabel.Text = CountsViewModel.Text("CountsSubtitle");
        DisabledLabel.Text = CountsViewModel.Text("InventoryDisabled");
        StartTitleLabel.Text = CountsViewModel.Text("CountStart");
        StartHintLabel.Text = CountsViewModel.Text("CountStartHint");
        ScanBinButton.Text = CountsViewModel.Text("CountScanBin");
        BinEntry.Placeholder = CountsViewModel.Text("CountBinPlaceholder");
        OpenTitleLabel.Text = CountsViewModel.Text("CountsOpen");
        EmptyLabel.Text = CountsViewModel.Text("CountsEmpty");
    }
    protected override async void OnAppearing() { base.OnAppearing(); await ViewModel.RefreshAsync(); }
    protected override void OnDisappearing() { ViewModel.StopLoading(); base.OnDisappearing(); }
    private async Task<string?> ScanAsync()
    {
        if (await Permissions.RequestAsync<Permissions.Camera>() != PermissionStatus.Granted) throw new ApiException("CAMERA");
        var scan = new ScanPage(); await Navigation.PushModalAsync(scan); return await scan.Result;
    }
}
