using NinimumWarehouse.Resources.Languages;
using NinimumWarehouse.Services;
using NinimumWarehouse.ViewModels;
namespace NinimumWarehouse.Views;
public partial class StockMovePage : ContentPage
{
    private StockMoveViewModel ViewModel => (StockMoveViewModel)BindingContext;
    public StockMovePage(WarehouseApi api, bool transfer)
    {
        InitializeComponent();
        BindingContext = new StockMoveViewModel(api, transfer, ScanAsync, TakePhotoAsync, DoneAsync);
        ErrorDialogs.Attach(this, ViewModel);
        string T(string key) => CountsViewModel.Text(key);
        SourceTitle.Text = T("StockSourceBin"); ScanSourceButton.Text = T("CountScanBin"); SourceEntry.Placeholder = T("CountBinPlaceholder");
        QuantityLabel.Text = T("StockQuantity"); DestinationLabel.Text = T("StockDestinationBin"); ScanDestinationButton.Text = T("StockScanDestination");
        DestinationEntry.Placeholder = T("CountBinPlaceholder"); ReasonLabel.Text = T("StockReasonType"); ReasonPicker.Title = T("StockReasonType");
        PhotoButton.Text = T("StockTakePhoto"); PhotoAddedLabel.Text = T("StockPhotoAdded"); CommentLabel.Text = T("StockComment");
        SubmitButton.Text = T(transfer ? "StockTransferSubmit" : "StockWriteOffSubmit");
    }
    private async Task DoneAsync()
    {
        string T(string key) => CountsViewModel.Text(key);
        await DisplayAlertAsync(T("CountSubmittedTitle"), T(ViewModel.IsTransfer ? "StockTransferDone" : "StockWriteOffDone"), AppResource.Ok);
        await Navigation.PopAsync();
    }
    private async Task<string?> ScanAsync()
    {
        if (await Permissions.RequestAsync<Permissions.Camera>() != PermissionStatus.Granted) throw new ApiException("CAMERA");
        var scan = new ScanPage(); await Navigation.PushModalAsync(scan); return await scan.Result;
    }
    // Same size limits as the courier app's return photos; the backend accepts up to 2 MB.
    private async Task<string?> TakePhotoAsync()
    {
        if (await Permissions.RequestAsync<Permissions.Camera>() != PermissionStatus.Granted) throw new ApiException("CAMERA");
        var options = new MediaPickerOptions { MaximumWidth = 1024, MaximumHeight = 1024, CompressionQuality = 80, RotateImage = true, PreserveMetaData = false };
        var photo = MediaPicker.Default.IsCaptureSupported ? await MediaPicker.Default.CapturePhotoAsync(options) : await MediaPicker.Default.PickPhotoAsync(options);
        if (photo is null) return null;
        await using var input = await photo.OpenReadAsync();
        using var bytes = new MemoryStream();
        await input.CopyToAsync(bytes);
        if (bytes.Length == 0 || bytes.Length > 2 * 1024 * 1024) throw new ApiException("PHOTO_INVALID");
        return Convert.ToBase64String(bytes.ToArray());
    }
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopAsync();
}
