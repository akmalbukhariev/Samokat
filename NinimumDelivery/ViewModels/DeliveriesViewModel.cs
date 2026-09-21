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
    public ObservableCollection<DeliveryJob> AvailableJobs { get; } = new();
    public ObservableCollection<DeliveryJob> ActiveJobs { get; } = new();

    [ObservableProperty] private DeliveryDashboard dashboard = new();
    [ObservableProperty] private bool isLoading;
    public string WorkerName => state.Worker?.fullName ?? string.Empty;
    public string WorkerGreeting => string.Format(NinimumDelivery.Resources.Languages.AppResource.Hello, WorkerName);

    public DeliveriesViewModel(DeliveryApiService api, AppState state)
    {
        this.api = api; this.state = state;
    }

    [RelayCommand]
    public async Task Load()
    {
        if (IsLoading) return;
        try
        {
            IsLoading = true;
            await state.RefreshWorkerAsync();
            OnPropertyChanged(nameof(WorkerName));
            OnPropertyChanged(nameof(WorkerGreeting));
            var dashboardResponse = await api.Dashboard();
            var availableResponse = await api.Available();
            var activeResponse = await api.Active();
            if (dashboardResponse?.resultCode == "100" && dashboardResponse.resultData != null) Dashboard = dashboardResponse.resultData;
            AvailableJobs.Clear();
            if (availableResponse?.resultCode == "100" && availableResponse.resultData != null)
                foreach (var item in availableResponse.resultData) AvailableJobs.Add(item);
            ActiveJobs.Clear();
            if (activeResponse?.resultCode == "100" && activeResponse.resultData != null)
                foreach (var item in activeResponse.resultData) ActiveJobs.Add(item);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private Task Open(DeliveryJob job) => Shell.Current.GoToAsync($"{nameof(DeliveryDetailPage)}?jobId={job.jobId}");
}
