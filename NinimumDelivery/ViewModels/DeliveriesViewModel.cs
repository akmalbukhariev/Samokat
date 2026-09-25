using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NinimumDelivery.Models;
using NinimumDelivery.Services;
using NinimumDelivery.Views;
using System.Collections.ObjectModel;

namespace NinimumDelivery.ViewModels;

public partial class DeliveriesViewModel : ObservableObject
{
    private readonly DeliveryApiService api;
    private readonly AppState state;
    private CancellationTokenSource? loadCancellation;
    private int loadVersion;

    public ObservableCollection<DeliveryJob> AvailableJobs { get; } = new();
    public ObservableCollection<DeliveryJob> ActiveJobs { get; } = new();

    [ObservableProperty] private DeliveryDashboard dashboard = new();
    [ObservableProperty] private bool isLoading;

    public string WorkerName => state.Worker?.fullName ?? string.Empty;
    public string WorkerGreeting => string.Format(NinimumDelivery.Resources.Languages.AppResource.Hello, WorkerName);

    public DeliveriesViewModel(DeliveryApiService api, AppState state)
    {
        this.api = api;
        this.state = state;
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    public Task Load() => ReloadAsync();

    public async Task ReloadAsync()
    {
        var version = Interlocked.Increment(ref loadVersion);

        var currentCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var previousCancellation = Interlocked.Exchange(ref loadCancellation, currentCancellation);

        if (previousCancellation != null)
        {
            try { previousCancellation.Cancel(); }
            catch (ObjectDisposedException) { }
            previousCancellation.Dispose();
        }

        IsLoading = true;

        try
        {
            var cancellationToken = currentCancellation.Token;

            await state.RefreshWorkerAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            OnPropertyChanged(nameof(WorkerName));
            OnPropertyChanged(nameof(WorkerGreeting));

            var dashboardTask = api.Dashboard(cancellationToken);
            var availableTask = api.Available(cancellationToken);
            var activeTask = api.Active(cancellationToken);

            await Task.WhenAll(dashboardTask, availableTask, activeTask);
            cancellationToken.ThrowIfCancellationRequested();

            if (version != Volatile.Read(ref loadVersion))
                return;

            var dashboardResponse = await dashboardTask;
            var availableResponse = await availableTask;
            var activeResponse = await activeTask;

            if (dashboardResponse?.resultCode == "100" && dashboardResponse.resultData != null)
                Dashboard = dashboardResponse.resultData;

            AvailableJobs.Clear();
            if (availableResponse?.resultCode == "100" && availableResponse.resultData != null)
            {
                foreach (var item in availableResponse.resultData)
                    AvailableJobs.Add(item);
            }

            ActiveJobs.Clear();
            if (activeResponse?.resultCode == "100" && activeResponse.resultData != null)
            {
                foreach (var item in activeResponse.resultData)
                    ActiveJobs.Add(item);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal when the app goes to the background or a newer refresh replaces this one.
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Deliveries] Load failed: {ex}");
        }
        finally
        {
            if (version == Volatile.Read(ref loadVersion))
            {
                Interlocked.CompareExchange(ref loadCancellation, null, currentCancellation);
                currentCancellation.Dispose();
                IsLoading = false;
            }
        }
    }

    public void CancelLoading()
    {
        Interlocked.Increment(ref loadVersion);

        var cancellation = Interlocked.Exchange(ref loadCancellation, null);
        if (cancellation != null)
        {
            try { cancellation.Cancel(); }
            catch (ObjectDisposedException) { }
            cancellation.Dispose();
        }

        IsLoading = false;
    }

    [RelayCommand]
    private Task Open(DeliveryJob job) => Shell.Current.GoToAsync($"{nameof(DeliveryDetailPage)}?jobId={job.jobId}");
}
