using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using NinimumWarehouse.Models;
using NinimumWarehouse.Resources.Languages;
using NinimumWarehouse.Services;
namespace NinimumWarehouse.ViewModels;
public sealed class CountCardViewModel(CountDocument document, IAsyncRelayCommand<CountCardViewModel> openCommand)
{
    public IAsyncRelayCommand<CountCardViewModel> OpenCommand => openCommand;
    public long Id => document.Id;
    public string Number => $"INV-{document.Id}";
    public string OpenText => CountsViewModel.Text("CountOpen");
    public string Bin => document.LocationCode ?? "—";
    public string Details => string.Join(" · ", new[] { document.CreatedBy, Time(document.CreatedAt) }.Where(v => !string.IsNullOrWhiteSpace(v)));
    private static string? Time(string? iso) => DateTimeOffset.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var t)
        ? t.ToLocalTime().ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture) : null;
}
public sealed class CountsViewModel : ViewModelBase
{
    private readonly Func<Task<string?>> scan;
    private readonly Func<long, string, Task> open;
    private bool enabled = true, isRefreshing, showCodeInput;
    private string binInput = "";
    private CancellationTokenSource? loadCancellation;
    public ObservableCollection<CountCardViewModel> Counts { get; } = [];
    public static string Text(string key) => AppResource.ResourceManager.GetString(key, AppResource.Culture) ?? key;
    public bool Enabled { get => enabled; private set { if (SetProperty(ref enabled, value)) OnPropertyChanged(nameof(Disabled)); } }
    public bool Disabled => !Enabled;
    public bool IsRefreshing { get => isRefreshing; set => SetProperty(ref isRefreshing, value); }
    public bool ShowCodeInput { get => showCodeInput; set => SetProperty(ref showCodeInput, value); }
    public string BinInput { get => binInput; set => SetProperty(ref binInput, value); }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand ScanBinCommand { get; }
    public IAsyncRelayCommand StartTypedCommand { get; }
    public IRelayCommand ToggleCodeCommand { get; }
    public IAsyncRelayCommand<CountCardViewModel> OpenCommand { get; }
    public CountsViewModel(WarehouseApi api, Func<Task<string?>> scan, Func<long, string, Task> open) : base(api)
    {
        this.scan = scan; this.open = open;
        RefreshCommand = new AsyncRelayCommand(() => RefreshAsync(showErrors: true));
        ScanBinCommand = new AsyncRelayCommand(() => RunAsync(async () => { var code = await this.scan(); if (!string.IsNullOrWhiteSpace(code)) await StartAsync(code); }));
        StartTypedCommand = new AsyncRelayCommand(() => RunAsync(async () => {
            if (string.IsNullOrWhiteSpace(BinInput)) throw new ApiException("BARCODE_REQUIRED");
            await StartAsync(BinInput);
        }));
        ToggleCodeCommand = new RelayCommand(() => ShowCodeInput = !ShowCodeInput);
        OpenCommand = new AsyncRelayCommand<CountCardViewModel>(row => RunAsync(async () => { if (row is not null) await this.open(row.Id, row.Bin); }));
    }
    // A new count locks the bin until the owner approves it in web admin.
    private async Task StartAsync(string code)
    {
        code = code.Trim();
        var id = await Api.StartCount(code, Text("CountDefaultReason"));
        BinInput = ""; ShowCodeInput = false;
        await open(id, code.ToUpperInvariant());
    }
    public void StopLoading() => loadCancellation?.Cancel();
    public async Task RefreshAsync(bool showErrors = false)
    {
        await RunAsync(async () => {
            using var cts = new CancellationTokenSource(); loadCancellation = cts;
            try
            {
                Enabled = (await Api.InventoryStatus(cts.Token)).Enabled;
                Counts.Clear();
                if (!Enabled) return;
                var page = await Api.OpenCounts(cts.Token);
                cts.Token.ThrowIfCancellationRequested();
                foreach (var document in page.Items) Counts.Add(new CountCardViewModel(document, OpenCommand));
            }
            finally { if (ReferenceEquals(loadCancellation, cts)) loadCancellation = null; }
        }, showErrors);
        IsRefreshing = false;
    }
}
