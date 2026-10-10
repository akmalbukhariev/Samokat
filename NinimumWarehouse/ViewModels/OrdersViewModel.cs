using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using NinimumWarehouse.Models;
using NinimumWarehouse.Resources.Languages;
using NinimumWarehouse.Services;
namespace NinimumWarehouse.ViewModels;
public sealed class OrderCardViewModel(Order order, IAsyncRelayCommand<OrderCardViewModel> openCommand)
{
    public IAsyncRelayCommand<OrderCardViewModel> OpenCommand => openCommand;
    public long Id => order.Id;
    public string Number => $"#{order.Id:D6}";
    public string Status => order.Status;
    public string StatusText => LocalizedMessages.Status(Status);
    public string QuantityText => string.Format(CultureInfo.CurrentCulture,AppResource.QuantityFormat,order.Quantity);
    public string? Note => order.Note;
    public bool HasNote => !string.IsNullOrWhiteSpace(Note);
}
public sealed class OrdersViewModel : ViewModelBase
{
    private readonly string view;
    private int page = 1, total;
    private bool isRefreshing;
    private CancellationTokenSource? loadCancellation;
    private Task? pendingLoad;
    public ObservableCollection<OrderCardViewModel> Orders { get; } = [];
    private static string Text(string key) => AppResource.ResourceManager.GetString(key,AppResource.Culture) ?? key;
    public string Title => view switch { "returns" => Text("Returns"), AppConstants.MineView => AppResource.MyOrders, AppConstants.HistoryView => AppResource.History, _ => AppResource.Queue };
    public string Subtitle => view switch { "returns" => Text("ReturnHint"), AppConstants.MineView => AppResource.MyOrdersSubtitle, AppConstants.HistoryView => AppResource.HistorySubtitle, _ => AppResource.QueueSubtitle };
    public string EmptyTitle => view switch { "returns" => Text("ReturnEmpty"), AppConstants.MineView => AppResource.EmptyMineTitle, AppConstants.HistoryView => AppResource.EmptyHistoryTitle, _ => AppResource.EmptyQueueTitle };
    public string EmptyMessage => view switch { "returns" => "", AppConstants.MineView => AppResource.EmptyMineMessage, AppConstants.HistoryView => AppResource.EmptyHistoryMessage, _ => AppResource.EmptyQueueMessage };
    public string TotalText => string.Format(CultureInfo.CurrentCulture,AppResource.TotalFormat,total);
    public string PageText => string.Format(CultureInfo.CurrentCulture,AppResource.PageFormat,page,Math.Max(1,(int)Math.Ceiling((double)total/AppConstants.PageSize)));
    public bool ShowPagination => total > AppConstants.PageSize;
    public bool CanPrevious => page > 1;
    public bool CanNext => page*AppConstants.PageSize < total;
    public bool IsRefreshing { get => isRefreshing; set => SetProperty(ref isRefreshing,value); }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand PreviousCommand { get; }
    public IAsyncRelayCommand NextCommand { get; }
    public IAsyncRelayCommand<OrderCardViewModel> OpenCommand { get; }
    public OrdersViewModel(WarehouseApi api,string view,Func<long,Task> open) : base(api)
    {
        this.view = view;
        RefreshCommand = new AsyncRelayCommand(() => RefreshAsync(showErrors: true));
        PreviousCommand = new AsyncRelayCommand(async () => { if(!IsBusy && CanPrevious) { page--; await RefreshAsync(showErrors: true); } });
        NextCommand = new AsyncRelayCommand(async () => { if(!IsBusy && CanNext) { page++; await RefreshAsync(showErrors: true); } });
        OpenCommand = new AsyncRelayCommand<OrderCardViewModel>(row => RunAsync(async () => { if(row is not null) await open(row.Id); }));
    }
    public void StopLoading() => loadCancellation?.Cancel();
    public async Task RefreshAsync(bool reload = false, bool showErrors = false)
    {
        if(pendingLoad is {IsCompleted:false}) { await pendingLoad; if(!reload) return; }
        pendingLoad = LoadAsync(showErrors); await pendingLoad;
    }
    private async Task LoadAsync(bool showErrors)
    {
        await RunAsync(async () => {
            using var cts = new CancellationTokenSource(); loadCancellation = cts;
            try {
                var result = await Api.Request<OrderPage>($"orders?view={view}&page={page}",ct:cts.Token);
                cts.Token.ThrowIfCancellationRequested();
                // A shorter queue can remove the final page while workers are preparing orders.
                if(result.Items.Count == 0 && page > 1) {
                    page = Math.Max(1,(int)Math.Ceiling((double)result.Total/AppConstants.PageSize));
                    result = await Api.Request<OrderPage>($"orders?view={view}&page={page}",ct:cts.Token);
                    cts.Token.ThrowIfCancellationRequested();
                }
                total = result.Total; Orders.Clear();
                foreach(var order in result.Items) Orders.Add(new OrderCardViewModel(order, OpenCommand));
                Notify(nameof(TotalText),nameof(PageText),nameof(ShowPagination),nameof(CanPrevious),nameof(CanNext));
            }
            finally { if(ReferenceEquals(loadCancellation,cts)) loadCancellation = null; }
        }, showErrors);
        IsRefreshing = false;
    }
}
