using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NinimumDelivery.Models;
using NinimumDelivery.Services;
using NinimumDelivery.Views;
using System.Collections.ObjectModel;

namespace NinimumDelivery.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly DeliveryApiService api;
    public ObservableCollection<DeliveryJob> Jobs { get; } = new();
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isRefreshing;
    public HistoryViewModel(DeliveryApiService api) => this.api = api;

    [RelayCommand]
    public async Task Load()
    {
        if (IsLoading) return;
        try
        {
            IsLoading = true;
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var response = await api.History(timeout.Token);
            timeout.Token.ThrowIfCancellationRequested();
            if (response?.resultCode == "100" && response.resultData != null)
            {
                Jobs.Clear();
                foreach (var item in response.resultData) Jobs.Add(item);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[History] Load failed: {ex}");
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private Task Open(DeliveryJob job) => Shell.Current.GoToAsync($"{nameof(DeliveryDetailPage)}?jobId={job.jobId}");
}
