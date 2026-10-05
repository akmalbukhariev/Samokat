using NinimumWarehouse.Resources.Languages;
using ZXing.Net.Maui;
namespace NinimumWarehouse.Views;
public partial class ScanPage : ContentPage
{
    private readonly TaskCompletionSource<string?> result = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int completed;
    public Task<string?> Result => result.Task;
    public ScanPage()
    {
        InitializeComponent(); Reader.Options = new BarcodeReaderOptions { Formats=BarcodeFormats.All,AutoRotate=true,Multiple=false };
    }
    private void OnBarcodesDetected(object? sender,BarcodeDetectionEventArgs e)
    {
        var value = e.Results.FirstOrDefault()?.Value;
        if(!string.IsNullOrWhiteSpace(value)) MainThread.BeginInvokeOnMainThread(() => _ = FinishAsync(value));
    }
    private async Task FinishAsync(string? value)
    {
        if(Interlocked.CompareExchange(ref completed,1,0)!=0) return;
        Reader.IsDetecting=false; await Navigation.PopModalAsync(); result.TrySetResult(value);
    }
    private void OnTorchClicked(object? sender,EventArgs e) { Reader.IsTorchOn=!Reader.IsTorchOn; TorchButton.Text=Reader.IsTorchOn?AppResource.TorchOff:AppResource.Torch; }
    private async void OnCancelClicked(object? sender,EventArgs e) => await FinishAsync(null);
    protected override void OnDisappearing()
    {
        Reader.IsDetecting=false; Reader.IsTorchOn=false; Reader.Handler?.DisconnectHandler();
        // Native Android back also closes this modal and must complete the pending scan.
        if(Interlocked.CompareExchange(ref completed,1,0)==0) result.TrySetResult(null);
        base.OnDisappearing();
    }
}
