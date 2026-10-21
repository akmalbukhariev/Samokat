using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NinimumWarehouse.Models;
using NinimumWarehouse.Services;
namespace NinimumWarehouse.ViewModels;
public sealed class BatchOptionViewModel(BatchRow batch, IRelayCommand<BatchOptionViewModel> select) : ObservableObject
{
    private bool selected;
    public BatchRow Batch => batch;
    public IRelayCommand<BatchOptionViewModel> SelectCommand => select;
    public string Name => batch.ProductName;
    public string Details => string.Format(CultureInfo.CurrentCulture, CountsViewModel.Text("StockBatchFormat"),
        string.IsNullOrWhiteSpace(batch.BatchNumber) ? "—" : batch.BatchNumber, batch.ExpiryText, batch.Available);
    public bool Selected { get => selected; set => SetProperty(ref selected, value); }
}
/// <summary>Warehouse app transfer (moves at once) or write-off (the owner approves it in web admin).</summary>
public sealed class StockMoveViewModel : ViewModelBase
{
    public static readonly string[] ReasonCodes = ["EXPIRED", "WAREHOUSE_DAMAGE", "DELIVERY_DAMAGE", "LOST", "SAMPLE", "INTERNAL", "OTHER"];
    private readonly Func<Task<string?>> scan;
    private readonly Func<Task<string?>> takePhoto;
    private readonly Func<Task> done;
    private BinInfo? source;
    private BatchOptionViewModel? selected;
    private string sourceInput = "", quantityInput = "", barcodeInput = "", scannedBarcode = "", overrideReason = "", destinationInput = "", comment = "";
    private int reasonIndex = -1;
    private string? photo;
    public bool IsTransfer { get; }
    public bool IsWriteOff => !IsTransfer;
    public string Title => CountsViewModel.Text(IsTransfer ? "StockTransfer" : "StockWriteOff");
    public ObservableCollection<BatchOptionViewModel> Batches { get; } = [];
    public bool HasSource => source is not null;
    public string SourceText => source is null ? "" : string.Format(CultureInfo.CurrentCulture, CountsViewModel.Text("StockBinFormat"), source.Location.Code, source.Batches.Count);
    public bool HasBatch => selected is not null;
    public bool NeedsProductScan => IsTransfer && selected?.Batch.Barcode is { Length: > 0 } && scannedBarcode.Length == 0;
    public bool ProductScanned => scannedBarcode.Length > 0;
    public bool NoBarcode => IsTransfer && selected is not null && string.IsNullOrWhiteSpace(selected.Batch.Barcode);
    public bool ManualCodeAllowed => WarehouseRules.ManualCodeAllowed;
    public bool HasPhoto => photo is not null;
    public string SourceInput { get => sourceInput; set => SetProperty(ref sourceInput, value); }
    public string QuantityInput { get => quantityInput; set => SetProperty(ref quantityInput, value); }
    public string BarcodeInput { get => barcodeInput; set => SetProperty(ref barcodeInput, value); }
    public string OverrideReason { get => overrideReason; set => SetProperty(ref overrideReason, value); }
    public string DestinationInput { get => destinationInput; set => SetProperty(ref destinationInput, value); }
    public string Comment { get => comment; set => SetProperty(ref comment, value); }
    public int ReasonIndex { get => reasonIndex; set => SetProperty(ref reasonIndex, value); }
    public List<string> ReasonLabels { get; } = ReasonCodes.Select(c => CountsViewModel.Text("WriteOff_" + c)).ToList();
    public IAsyncRelayCommand ScanSourceCommand { get; }
    public IAsyncRelayCommand LoadSourceCommand { get; }
    public IRelayCommand<BatchOptionViewModel> SelectCommand { get; }
    public IAsyncRelayCommand ScanProductCommand { get; }
    public IRelayCommand ConfirmBarcodeCommand { get; }
    public IAsyncRelayCommand ScanDestinationCommand { get; }
    public IAsyncRelayCommand PhotoCommand { get; }
    public IAsyncRelayCommand SubmitCommand { get; }
    public StockMoveViewModel(WarehouseApi api, bool transfer, Func<Task<string?>> scan, Func<Task<string?>> takePhoto, Func<Task> done) : base(api)
    {
        IsTransfer = transfer; this.scan = scan; this.takePhoto = takePhoto; this.done = done;
        ScanSourceCommand = new AsyncRelayCommand(() => RunAsync(async () => { var code = await this.scan(); if (!string.IsNullOrWhiteSpace(code)) await LoadAsync(code); }));
        LoadSourceCommand = new AsyncRelayCommand(() => RunAsync(async () => { if (string.IsNullOrWhiteSpace(SourceInput)) throw new ApiException("BARCODE_REQUIRED"); await LoadAsync(SourceInput); }));
        SelectCommand = new RelayCommand<BatchOptionViewModel>(row => {
            foreach (var b in Batches) b.Selected = ReferenceEquals(b, row);
            selected = row; scannedBarcode = ""; BarcodeInput = "";
            Notify(nameof(HasBatch), nameof(NeedsProductScan), nameof(ProductScanned), nameof(NoBarcode));
        });
        ScanProductCommand = new AsyncRelayCommand(() => RunAsync(async () => AcceptBarcode(await this.scan())));
        ConfirmBarcodeCommand = new RelayCommand(() => _ = RunAsync(() => { AcceptBarcode(BarcodeInput); return Task.CompletedTask; }));
        ScanDestinationCommand = new AsyncRelayCommand(() => RunAsync(async () => { var code = await this.scan(); if (!string.IsNullOrWhiteSpace(code)) DestinationInput = code.Trim().ToUpperInvariant(); }));
        PhotoCommand = new AsyncRelayCommand(() => RunAsync(async () => { var p = await this.takePhoto(); if (p is not null) { photo = p; Notify(nameof(HasPhoto)); } }));
        SubmitCommand = new AsyncRelayCommand(() => RunAsync(SubmitAsync));
    }
    private async Task LoadAsync(string code)
    {
        var bin = await Api.Bin(code);
        source = bin; selected = null; scannedBarcode = ""; Batches.Clear();
        foreach (var b in bin.Batches.Where(b => b.Available > 0 && b.CountDocument is null)) Batches.Add(new BatchOptionViewModel(b, SelectCommand));
        if (Batches.Count == 0) throw new ApiException("INVENTORY_BIN_EMPTY");
        SourceInput = "";
        Notify(nameof(HasSource), nameof(SourceText), nameof(HasBatch), nameof(NeedsProductScan), nameof(ProductScanned), nameof(NoBarcode));
    }
    // The backend checks the code again; this gives instant feedback.
    private void AcceptBarcode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code) || selected is null) return;
        if (code.Trim() != selected.Batch.Barcode?.Trim()) throw new ApiException("WAREHOUSE_BARCODE_MISMATCH");
        scannedBarcode = code.Trim(); BarcodeInput = "";
        Notify(nameof(NeedsProductScan), nameof(ProductScanned));
    }
    private async Task SubmitAsync()
    {
        if (source is null || selected is null) throw new ApiException("WAREHOUSE_INVALID_INPUT");
        if (!int.TryParse(QuantityInput.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var quantity) || quantity < 1 || quantity > selected.Batch.Available)
            throw new ApiException("QUANTITY_INVALID");
        if (IsTransfer)
        {
            if (NeedsProductScan) throw new ApiException("BARCODE_REQUIRED");
            if (NoBarcode && string.IsNullOrWhiteSpace(OverrideReason)) throw new ApiException("REASON_REQUIRED");
            if (string.IsNullOrWhiteSpace(DestinationInput)) throw new ApiException("BARCODE_REQUIRED");
            var destination = await Api.Bin(DestinationInput);
            var proof = new Dictionary<string, object?> {
                ["source_code"] = source.Location.Code, ["scanned_barcode"] = scannedBarcode,
                ["scan_override_reason"] = OverrideReason.Trim(), ["destination_code"] = destination.Location.Code };
            var body = new Dictionary<string, object?>(proof) {
                ["kind"] = "TRANSFER", ["reason"] = string.IsNullOrWhiteSpace(Comment) ? CountsViewModel.Text("StockTransferReason") : Comment.Trim(),
                ["batch_id"] = selected.Batch.Id, ["quantity"] = quantity, ["destination_id"] = destination.Location.Id };
            var id = await Api.CreateDocument(body);
            await Api.ApproveDocument(id, proof);
        }
        else
        {
            if (ReasonIndex < 0) throw new ApiException("REASON_REQUIRED");
            string code = ReasonCodes[ReasonIndex];
            string text = string.IsNullOrWhiteSpace(Comment) ? ReasonLabels[ReasonIndex] : $"{ReasonLabels[ReasonIndex]}: {Comment.Trim()}";
            await Api.CreateDocument(new Dictionary<string, object?> {
                ["kind"] = "WRITEOFF", ["reason"] = text, ["reason_code"] = code, ["batch_id"] = selected.Batch.Id,
                ["quantity"] = quantity, ["photo_base64"] = photo });
        }
        await done();
    }
}
