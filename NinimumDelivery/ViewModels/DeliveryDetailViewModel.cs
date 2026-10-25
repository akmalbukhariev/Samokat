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
    private bool returnInProgress;
    [ObservableProperty] private DeliveryJob job = new();
    [ObservableProperty] private bool isLoading;

    public DeliveryDetailViewModel(DeliveryApiService api) => this.api = api;

    public bool ShowClaim => Job.IsWaiting;
    public bool ShowStart => Job.IsAccepted;
    public bool ShowComplete => Job.IsOnTheWay;
    public bool ShowContact => Job.IsActive && !string.IsNullOrWhiteSpace(Job.customerPhone);
    public bool ShowFailure => Job.IsActive;
    public bool ShowPartialReturn => Job.IsOnTheWay && Job.products.Sum(p=>(long)p.quantity)>1;
    public string PartialReturnTitle => Text("PartialReturnTitle");
    public string PartialRefundTitle => Text("PartialRefundTitle");
    public string ReturnPhotoTitle => Text("ReturnPhotoTitle");
    private static string Text(string key) => AppResource.ResourceManager.GetString(key, AppResource.Culture) ?? key;

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
        catch (Exception ex)
        {
            // Only a real network failure is "can't reach the server"; anything else is a general error (logged for diagnosis).
            Console.WriteLine($"[NinimumDelivery] detail {id} failed: {ex}");
            await AlertService.Show(AppResource.Error, ex is HttpRequestException or TaskCanceledException or System.Net.Sockets.SocketException ? AppResource.ConnectionError : AppResource.ActionFailed);
        }
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
    private Task Fail() => SubmitReturn(false);
    [RelayCommand]
    private Task PartialReturn() => SubmitReturn(true);
    private async Task SubmitReturn(bool partial)
    {
        if (returnInProgress || IsLoading || !Job.IsActive) return;
        returnInProgress = true;
        try
        {
            if (!await AlertService.Confirm(partial?Text("PartialReturnTitle"):AppResource.DeliveryFailedTitle,partial?Text("PartialReturnConfirm"):AppResource.DeliveryFailedMessage, AppResource.Yes, AppResource.No)) return;
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page == null) return;
            List<ReturnQuantity>? quantities=null;
            if(partial) { quantities=await NinimumDelivery.Views.PartialReturnPage.Select(page,Job.products);if(quantities==null)return; }
            var reasons = new[] { Text("ReturnNoCustomer"), Text("ReturnAddress"), Text("ReturnRefused") };
            var reason = await page.DisplayActionSheetAsync(Text("ReturnReasonTitle"), AppResource.Close, null, reasons);
            if (!reasons.Contains(reason)) return;
            var sources = new[] { Text("ReturnPhotoCamera"), Text("ReturnPhotoGallery") };
            var source = await page.DisplayActionSheetAsync(Text("ReturnPhotoRequired"), AppResource.Close, null,
                MediaPicker.Default.IsCaptureSupported ? sources : new[] { sources[1] });
            if (!sources.Contains(source)) return;
            var options = new MediaPickerOptions
            {
                Title = Text("ReturnPhotoTitle"), MaximumWidth = 1024, MaximumHeight = 1024,
                CompressionQuality = 80, RotateImage = true, PreserveMetaData = false
            };
            var photo = source == sources[0] ? await MediaPicker.Default.CapturePhotoAsync(options)
                : await MediaPicker.Default.PickPhotoAsync(options);
            if (photo == null) return;
            using var input = await photo.OpenReadAsync();
            using var bytes = new MemoryStream();
            var buffer = new byte[81920];
            int read;
            while ((read = await input.ReadAsync(buffer)) > 0)
            {
                if (bytes.Length + read > 10 * 1024 * 1024)
                {
                    await AlertService.Show(AppResource.Error, Text("RETURN_PHOTO_TOO_LARGE"));
                    return;
                }
                await bytes.WriteAsync(buffer.AsMemory(0, read));
            }
            if (bytes.Length == 0)
            {
                await AlertService.Show(AppResource.Error, Text("RETURN_PHOTO_INVALID"));
                return;
            }
            await RunAction(() => api.UpdateStatus(jobId, partial?"PARTIAL_RETURN":"FAILED", reason, Convert.ToBase64String(bytes.ToArray()),quantities), reload: true);
        }
        catch (PermissionException) { await AlertService.Show(AppResource.Error, Text("ReturnPhotoPermission")); }
        catch { await AlertService.Show(AppResource.Error, Text("ReturnPhotoError")); }
        finally { returnInProgress = false; }
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
                var message = response?.resultMsg ?? AppResource.ActionFailed;
                await AlertService.Show(AppResource.Error, message.StartsWith("RETURN_PHOTO_") || message.StartsWith("PARTIAL_RETURN_") ? Text(message) : message);
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
        OnPropertyChanged(nameof(ReturnPhotoTitle));
        OnPropertyChanged(nameof(ShowPartialReturn));
    }
}
