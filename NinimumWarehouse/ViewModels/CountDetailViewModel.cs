using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NinimumWarehouse.Models;
using NinimumWarehouse.Resources.Languages;
using NinimumWarehouse.Services;
namespace NinimumWarehouse.ViewModels;
public sealed class CountLineViewModel : ObservableObject
{
    private readonly CountLine line;
    private string quantityInput = "", barcodeInput = "", scanned = "";
    private bool showCodeInput;
    public long BatchId => line.BatchId;
    public string Name => line.ProductName;
    public string BatchText => string.Format(CultureInfo.CurrentCulture, CountsViewModel.Text("CountBatchFormat"), string.IsNullOrWhiteSpace(line.BatchNumber) ? "—" : line.BatchNumber, line.ExpiryText);
    public bool HasBarcode => !string.IsNullOrWhiteSpace(line.Barcode);
    public bool NoBarcode => !HasBarcode;
    public string Scanned => scanned;
    public bool IsScanned => scanned.Length > 0;
    public bool NeedsScan => HasBarcode && !IsScanned;
    public string QuantityInput { get => quantityInput; set => SetProperty(ref quantityInput, value); }
    public string BarcodeInput { get => barcodeInput; set => SetProperty(ref barcodeInput, value); }
    public bool ShowCodeInput { get => showCodeInput; set => SetProperty(ref showCodeInput, value); }
    public IAsyncRelayCommand ScanCommand { get; }
    public IAsyncRelayCommand ConfirmCodeCommand { get; }
    public IRelayCommand ToggleCodeCommand { get; }
    public CountLineViewModel(CountLine line, Func<Task<string?>> scan, Func<string, Task> showError)
    {
        this.line = line;
        ScanCommand = new AsyncRelayCommand(async () => { try { Accept(await scan()); } catch (ApiException ex) { await showError(LocalizedMessages.Error(ex.Message)); } });
        ConfirmCodeCommand = new AsyncRelayCommand(async () => { try { Accept(BarcodeInput); } catch (ApiException ex) { await showError(LocalizedMessages.Error(ex.Message)); } });
        ToggleCodeCommand = new RelayCommand(() => ShowCodeInput = !ShowCodeInput);
    }
    // The backend checks the code again; checking here gives the worker instant feedback.
    private void Accept(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return;
        if (code.Trim() != line.Barcode?.Trim()) throw new ApiException("WAREHOUSE_BARCODE_MISMATCH");
        scanned = code.Trim(); BarcodeInput = ""; ShowCodeInput = false;
        foreach (var name in new[] { nameof(Scanned), nameof(IsScanned), nameof(NeedsScan) }) OnPropertyChanged(name);
    }
}
public sealed class CountDetailViewModel : ViewModelBase
{
    private readonly long id;
    private readonly Func<Task<string?>> scan;
    private readonly Func<Task> done;
    private string varianceReason = "", overrideReason = "";
    private bool varianceRequired, loaded;
    public ObservableCollection<CountLineViewModel> Lines { get; } = [];
    public string Title { get; }
    public bool Loaded { get => loaded; private set => SetProperty(ref loaded, value); }
    public bool HasNoBarcodeLines => Lines.Any(l => l.NoBarcode);
    public bool VarianceRequired { get => varianceRequired; private set => SetProperty(ref varianceRequired, value); }
    public string VarianceReason { get => varianceReason; set => SetProperty(ref varianceReason, value); }
    public string OverrideReason { get => overrideReason; set => SetProperty(ref overrideReason, value); }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand SubmitCommand { get; }
    public CountDetailViewModel(WarehouseApi api, long id, string bin, Func<Task<string?>> scan, Func<Task> done) : base(api)
    {
        this.id = id; this.scan = scan; this.done = done;
        Title = $"INV-{id} · {bin}";
        RefreshCommand = new AsyncRelayCommand(() => RefreshAsync(true));
        SubmitCommand = new AsyncRelayCommand(() => RunAsync(SubmitAsync));
    }
    public Task RefreshAsync(bool showErrors = false) => RunAsync(async () => {
        var detail = await Api.Count(id);
        if (detail.Status != "DRAFT") throw new ApiException("WAREHOUSE_INVALID_STATE");
        Lines.Clear();
        foreach (var line in detail.Payload.Lines) Lines.Add(new CountLineViewModel(line, scan, message => ShowErrorAsync?.Invoke(message) ?? Task.CompletedTask));
        Loaded = true; Notify(nameof(HasNoBarcodeLines));
    }, showErrors);
    private async Task SubmitAsync()
    {
        var counts = new List<object>();
        foreach (var line in Lines)
        {
            if (!int.TryParse(line.QuantityInput.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var quantity)) throw new ApiException("QUANTITY_INVALID");
            if (line.NeedsScan) throw new ApiException("BARCODE_REQUIRED");
            counts.Add(new { batch_id = line.BatchId, quantity, scanned_barcode = line.Scanned });
        }
        if (HasNoBarcodeLines && string.IsNullOrWhiteSpace(OverrideReason)) throw new ApiException("REASON_REQUIRED");
        try { await Api.SubmitCount(id, counts, VarianceReason.Trim(), OverrideReason.Trim()); }
        catch (ApiException ex) when (ex.Message == "INVENTORY_VARIANCE_REASON_REQUIRED") { VarianceRequired = true; throw; }
        await done();
    }
}
