using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NinimumDelivery.Models;
using NinimumDelivery.Resources.Languages;
using NinimumDelivery.Services;

namespace NinimumDelivery.ViewModels;

public partial class DeliveryDetailViewModel : ObservableObject
{
    private readonly DeliveryApiService api;
    private long jobId;
    [ObservableProperty] private DeliveryJob job = new();
    [ObservableProperty] private bool isLoading;

    public DeliveryDetailViewModel(DeliveryApiService api) => this.api = api;

    public bool ShowClaim => Job.IsWaiting;
    public bool ShowStart => Job.IsAccepted;
    public bool ShowComplete => Job.IsOnTheWay;
    public bool ShowContact => Job.IsActive && !string.IsNullOrWhiteSpace(Job.customerPhone);
    public bool ShowFailure => Job.IsActive;

    public async Task LoadAsync(long id)
    {
        jobId = id;
        try
        {
            IsLoading = true;
            var response = await api.Detail(id);
            if (response?.resultCode == "100" && response.resultData != null)
            {
                Job = response.resultData;
                NotifyState();
            }
            else await AlertService.Show(AppResource.Error, response?.resultMsg ?? AppResource.ActionFailed);
        }
        catch { await AlertService.Show(AppResource.Error, AppResource.ConnectionError); }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task Claim()
    {
        if (!await AlertService.Confirm(AppResource.ClaimConfirmTitle, AppResource.ClaimConfirmMessage, AppResource.Accept, AppResource.Close)) return;
        await RunAction(() => api.Claim(jobId), reload: true);
    }

    [RelayCommand]
    private Task StartDelivery() => RunAction(() => api.UpdateStatus(jobId, "ON_THE_WAY"), reload: true);

    [RelayCommand]
    private async Task Complete()
    {
        if (!await AlertService.Confirm(AppResource.DeliveryCompletedConfirmTitle, AppResource.DeliveryCompletedConfirmMessage, AppResource.Confirm, AppResource.Close)) return;
        await RunAction(() => api.UpdateStatus(jobId, "DELIVERED"), reload: true);
    }

    [RelayCommand]
    private async Task Fail()
    {
        if (!await AlertService.Confirm(AppResource.DeliveryFailedTitle, AppResource.DeliveryFailedMessage, AppResource.Yes, AppResource.No)) return;
        await RunAction(() => api.UpdateStatus(jobId, "FAILED", "Courier could not complete delivery"), reload: true);
    }

    [RelayCommand]
    private async Task Call()
    {
        if (!string.IsNullOrWhiteSpace(Job.customerPhone))
            await Launcher.Default.OpenAsync(new Uri($"tel:{Job.customerPhone}"));
    }

    [RelayCommand]
    private async Task Map()
    {
        string destination;

        if (Job.locationLatitude.HasValue && Job.locationLongitude.HasValue &&
            Math.Abs(Job.locationLatitude.Value) > 0.000001 && Math.Abs(Job.locationLongitude.Value) > 0.000001)
        {
            destination = $"{Job.locationLatitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)},{Job.locationLongitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        }
        else if (!string.IsNullOrWhiteSpace(Job.deliveryAddress))
        {
            destination = Uri.EscapeDataString(Job.deliveryAddress);
        }
        else
        {
            return;
        }

        // Google Maps uses the courier's current location as the origin when origin is omitted.
        var url = $"https://www.google.com/maps/dir/?api=1&destination={destination}&travelmode=driving";
        await Launcher.Default.OpenAsync(new Uri(url));
    }

    [RelayCommand]
    private Task Back() => Shell.Current.GoToAsync("..");

    private async Task RunAction(Func<Task<ApiResponse?>> action, bool reload)
    {
        if (IsLoading) return;
        try
        {
            IsLoading = true;
            var response = await action();
            if (response?.resultCode != "100")
            {
                await AlertService.Show(AppResource.Error, response?.resultMsg ?? AppResource.ActionFailed);
                return;
            }
        }
        catch { await AlertService.Show(AppResource.Error, AppResource.ConnectionError); }
        finally { IsLoading = false; }
        if (reload) await LoadAsync(jobId);
    }

    private void NotifyState()
    {
        OnPropertyChanged(nameof(ShowClaim));
        OnPropertyChanged(nameof(ShowStart));
        OnPropertyChanged(nameof(ShowComplete));
        OnPropertyChanged(nameof(ShowContact));
        OnPropertyChanged(nameof(ShowFailure));
    }
}
