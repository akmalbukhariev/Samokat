using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NinimumWarehouse.Models;
using NinimumWarehouse.Resources.Languages;
using NinimumWarehouse.Services;
namespace NinimumWarehouse.ViewModels;
public sealed class OrderItemViewModel : ObservableObject
{
    private OrderItem item;
    private bool isEditable, showCodeInput;
    private string quantityInput, barcodeInput = "", reason = "";
    public long Id => item.Id;
    public string Name => item.Name;
    public string Image => ProductImageSource.Resolve(item.ImageUrl);
    public string Barcode => string.IsNullOrWhiteSpace(item.Barcode) ? AppResource.NoBarcode : item.Barcode;
    public bool HasBarcode => !string.IsNullOrWhiteSpace(item.Barcode);
    public bool NoBarcode => !HasBarcode;
    public string ProgressText => string.Format(CultureInfo.CurrentCulture,AppResource.ProductProgressFormat,item.Checked,item.Required);
    public string CheckState => IsChecked ? AppResource.Checked : AppResource.ToCheck;
    public bool IsChecked => item.Checked == item.Required;
    public bool IsEditable { get => isEditable; private set => SetProperty(ref isEditable,value); }
    public string QuantityInput { get => quantityInput; set => SetProperty(ref quantityInput,value); }
    public string BarcodeInput { get => barcodeInput; set => SetProperty(ref barcodeInput,value); }
    public string Reason { get => reason; set => SetProperty(ref reason,value); }
    public bool ShowCodeInput { get => showCodeInput; set => SetProperty(ref showCodeInput,value); }
    public IAsyncRelayCommand ScanCommand { get; }
    public IAsyncRelayCommand ConfirmCommand { get; }
    public IRelayCommand ToggleCodeCommand { get; }
    public OrderItemViewModel(OrderItem item,bool editable,Func<OrderItemViewModel,bool,Task> check)
    {
        this.item = item; isEditable = editable;
        quantityInput = (item.Checked > 0 ? item.Checked : item.Required).ToString(CultureInfo.CurrentCulture);
        ScanCommand = new AsyncRelayCommand(() => check(this,true));
        ConfirmCommand = new AsyncRelayCommand(() => check(this,false));
        ToggleCodeCommand = new RelayCommand(() => ShowCodeInput = !ShowCodeInput);
    }
    public void Update(OrderItem next,bool editable,bool checkedThisItem)
    {
        var previousImage = Image;
        item = next; IsEditable = editable;
        // Avoid restarting a pending image download when only preparation progress changed.
        if (Image != previousImage) OnPropertyChanged(nameof(Image));
        if(checkedThisItem) { QuantityInput = next.Checked.ToString(CultureInfo.CurrentCulture); BarcodeInput = ""; ShowCodeInput = false; }
        foreach(var property in new[] { nameof(Name),nameof(Barcode),nameof(HasBarcode),nameof(NoBarcode),nameof(ProgressText),nameof(IsChecked),nameof(CheckState) }) OnPropertyChanged(property);
    }
}
public sealed class OrderDetailViewModel : ViewModelBase
{
    private readonly long id;
    private readonly Func<Task<string?>> scan;
    private readonly Func<Task<bool>> confirmReady;
    private readonly Func<Task<string?>> reportProblem;
    private Order? order;
    private CancellationTokenSource? refreshCancellation;
    private Task? pendingRefresh;
    public ObservableCollection<OrderItemViewModel> Items { get; } = [];
    public string Number => order?.Number ?? "";
    public string Status => order?.Status ?? "WAITING";
    public string StatusText => LocalizedMessages.Status(Status);
    public string? Note => order?.Note;
    public bool HasNote => !string.IsNullOrWhiteSpace(Note);
    public bool HasOrder => order is not null;
    public bool Available => order?.Payment == "PAID" && order.OrderStatus is "CONFIRMED" or "PREPARING" or "READY";
    private bool Owned => order?.Worker == Api.WorkerCode;
    public bool Unavailable => HasOrder && !Available;
    public bool OtherWorker => HasOrder && !Owned && Status is "PICKING" or "BLOCKED";
    public bool CanStart => Available && Status == "WAITING";
    public bool CanCheck => Available && Owned && Status == "PICKING";
    public bool CanResume => Available && Owned && Status == "BLOCKED";
    public bool IsReady => Available && Status == "READY";
    public bool ShowActions => CanStart || CanCheck || CanResume;
    public bool AllChecked => Items.Count > 0 && Items.All(x => x.IsChecked);
    public string ProgressText => string.Format(CultureInfo.CurrentCulture,AppResource.ProgressFormat,order?.Items.Sum(x=>x.Checked) ?? 0,order?.Items.Sum(x=>x.Required) ?? 0);
    public double Progress
    {
        get
        {
            var required = order?.Items.Sum(x => x.Required) ?? 0;
            return required > 0 ? (double)order!.Items.Sum(x => x.Checked) / required : 0;
        }
    }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand StartCommand { get; }
    public IAsyncRelayCommand ReadyCommand { get; }
    public IAsyncRelayCommand ResumeCommand { get; }
    public IAsyncRelayCommand ProblemCommand { get; }
    public OrderDetailViewModel(WarehouseApi api,long id,Func<Task<string?>> scan,Func<Task<bool>> confirmReady,Func<Task<string?>> reportProblem) : base(api)
    {
        this.id=id;this.scan=scan;this.confirmReady=confirmReady;this.reportProblem=reportProblem;
        RefreshCommand = new AsyncRelayCommand(() => RefreshAsync(showErrors: true));
        StartCommand = new AsyncRelayCommand(() => Act("claim"));
        ResumeCommand = new AsyncRelayCommand(() => Act("resume"));
        ReadyCommand = new AsyncRelayCommand(() => RunAsync(async () =>
        {
            if (!CanCheck) throw new ApiException("WAREHOUSE_INVALID_STATE");
            if (!AllChecked) throw new ApiException("WAREHOUSE_INCOMPLETE");
            if (await confirmReady()) Apply(await Api.Action(id,"ready"));
        }));
        ProblemCommand = new AsyncRelayCommand(() => RunAsync(async () => { var reason=await reportProblem(); if (reason is null) return;
            if (string.IsNullOrWhiteSpace(reason)) throw new ApiException("REASON_REQUIRED");
            Apply(await Api.Action(id,"problem",new {reason=reason.Trim()})); }));
    }
    public void StopRefresh() => refreshCancellation?.Cancel();
    public async Task RefreshAsync(bool showErrors = false)
    {
        if (pendingRefresh is { IsCompleted: false }) await pendingRefresh;
        pendingRefresh = LoadAsync(showErrors);
        await pendingRefresh;
    }
    private Task LoadAsync(bool showErrors) => RunAsync(async () =>
    {
        using var cancellation = new CancellationTokenSource();
        refreshCancellation = cancellation;
        try
        {
            var next = await Api.Request<Order>($"orders/{id}", ct: cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            Apply(next);
        }
        finally
        {
            if (ReferenceEquals(refreshCancellation, cancellation)) refreshCancellation = null;
        }
    }, showErrors);
    private Task Act(string action) => RunAsync(async () => Apply(await Api.Action(id,action)));
    private void Apply(Order next,long? checkedItem=null)
    {
        order = next;
        var existing = Items.ToDictionary(x => x.Id);
        var updated = next.Items.Select(x => {
            if(existing.TryGetValue(x.Id,out var row)) { row.Update(x,CanCheck,x.Id==checkedItem); return row; }
            return new OrderItemViewModel(x,CanCheck,CheckAsync);
        }).ToList();
        // Keep existing product views attached while their asynchronous images load.
        // Clearing/re-adding every row disconnects MAUI's native Image handler mid-download.
        var currentIds = updated.Select(row => row.Id).ToHashSet();
        for (var index = Items.Count - 1; index >= 0; index--)
            if (!currentIds.Contains(Items[index].Id)) Items.RemoveAt(index);
        for (var index = 0; index < updated.Count; index++)
        {
            var row = updated[index];
            var existingIndex = Items.IndexOf(row);
            if (existingIndex < 0) Items.Insert(index, row);
            else if (existingIndex != index) Items.Move(existingIndex, index);
        }
        Notify(nameof(Number),nameof(Status),nameof(StatusText),nameof(Note),nameof(HasNote),nameof(HasOrder),nameof(Unavailable),nameof(OtherWorker),nameof(CanStart),nameof(CanCheck),nameof(CanResume),nameof(IsReady),nameof(ShowActions),nameof(AllChecked),nameof(ProgressText),nameof(Progress));
        ReadyCommand.NotifyCanExecuteChanged();
    }
    private Task CheckAsync(OrderItemViewModel item,bool useCamera) => RunAsync(async () => {
        if (!CanCheck || !item.IsEditable) throw new ApiException("WAREHOUSE_INVALID_STATE");
        if(!int.TryParse(item.QuantityInput,out var qty) || qty<=0) throw new ApiException("QUANTITY_INVALID");
        var required = order!.Items.First(x => x.Id == item.Id).Required;
        if (qty > required) throw new ApiException("WAREHOUSE_TOO_MANY");
        string? barcode = item.HasBarcode ? item.BarcodeInput.Trim() : "";
        if(useCamera) { barcode = await scan(); if(barcode is null) return; }
        if(item.HasBarcode && string.IsNullOrWhiteSpace(barcode)) throw new ApiException("BARCODE_REQUIRED");
        if(!item.HasBarcode && string.IsNullOrWhiteSpace(item.Reason)) throw new ApiException("REASON_REQUIRED");
        Apply(await Api.Action(id,"check",new {product_id=item.Id,quantity=qty,barcode,reason=item.Reason.Trim()}),item.Id);
    });
}
