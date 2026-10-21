using NinimumWarehouse.Resources.Languages;
using NinimumWarehouse.Services;
using NinimumWarehouse.ViewModels;
namespace NinimumWarehouse.Views;
public partial class CountDetailPage : ContentPage
{
    private bool loaded;
    private CountDetailViewModel ViewModel => (CountDetailViewModel)BindingContext;
    public CountDetailPage(WarehouseApi api, long id, string bin)
    {
        InitializeComponent();
        BindingContext = new CountDetailViewModel(api, id, bin, ScanAsync, SubmittedAsync);
        ErrorDialogs.Attach(this, ViewModel);
        HintLabel.Text = CountsViewModel.Text("CountHint");
        OverrideLabel.Text = CountsViewModel.Text("CountNoBarcodeReason");
        VarianceLabel.Text = CountsViewModel.Text("CountVarianceReason");
        SubmitButton.Text = CountsViewModel.Text("CountSubmit");
    }
    // Load once: a refresh would clear the quantities the worker has typed.
    protected override async void OnAppearing() { base.OnAppearing(); if (loaded) return; loaded = true; await ViewModel.RefreshAsync(true); }
    private async Task SubmittedAsync()
    {
        await DisplayAlertAsync(CountsViewModel.Text("CountSubmittedTitle"), CountsViewModel.Text("CountSubmitted"), AppResource.Ok);
        await Navigation.PopAsync();
    }
    private async Task<string?> ScanAsync()
    {
        if (await Permissions.RequestAsync<Permissions.Camera>() != PermissionStatus.Granted) throw new ApiException("CAMERA");
        var scan = new ScanPage(); await Navigation.PushModalAsync(scan); return await scan.Result;
    }
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopAsync();
}
