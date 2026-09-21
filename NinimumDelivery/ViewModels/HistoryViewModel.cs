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
    public HistoryViewModel(DeliveryApiService api) => this.api = api;

    [RelayCommand]
    public async Task Load()
    {
        if (IsLoading) return;
        try
        {
            IsLoading = true;
            var response = await api.History();
            Jobs.Clear();
            if (response?.resultCode == "100" && response.resultData != null)
                foreach (var item in response.resultData) Jobs.Add(item);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private Task Open(DeliveryJob job) => Shell.Current.GoToAsync($"{nameof(DeliveryDetailPage)}?jobId={job.jobId}");
}
