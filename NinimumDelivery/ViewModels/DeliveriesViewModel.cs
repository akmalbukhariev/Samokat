using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NinimumDelivery.Models;
using NinimumDelivery.Resources.Languages;
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
    public string WorkerGreeting => string.Format(AppResource.Hello, WorkerName);
    public int SelectedAvailableCount => AvailableJobs.Count(x => x.IsSelected);
    public bool HasSelectedAvailableJobs => SelectedAvailableCount > 0;
    public bool HasAcceptedJobs => ActiveJobs.Any(x => x.IsAccepted);
    public bool HasMultipleActiveJobs => ActiveJobs.Count > 1;

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
        // Capture before publishing; only this reload owns disposal of this source.
        var cancellationToken = currentCancellation.Token;
        var previousCancellation = Interlocked.Exchange(ref loadCancellation, currentCancellation);

        if (previousCancellation != null)
        {
            try { previousCancellation.Cancel(); }
            catch (ObjectDisposedException) { }
        }

        IsLoading = true;
        try
        {
            await state.RefreshWorkerAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!state.IsLoggedIn() || version != Volatile.Read(ref loadVersion)) return;

            OnPropertyChanged(nameof(WorkerName));
            OnPropertyChanged(nameof(WorkerGreeting));

            var dashboardTask = api.Dashboard(cancellationToken);
            var availableTask = api.Available(cancellationToken);
            var activeTask = api.Active(cancellationToken);
            await Task.WhenAll(dashboardTask, availableTask, activeTask);
            cancellationToken.ThrowIfCancellationRequested();
            if (version != Volatile.Read(ref loadVersion)) return;

            var dashboardResponse = await dashboardTask;
            var availableResponse = await availableTask;
            var activeResponse = await activeTask;

            if (dashboardResponse?.resultCode == "100" && dashboardResponse.resultData != null)
                Dashboard = dashboardResponse.resultData;

            AvailableJobs.Clear();
            if (availableResponse?.resultCode == "100" && availableResponse.resultData != null)
            {
                foreach (var item in availableResponse.resultData)
                {
                    item.PropertyChanged += OnAvailableJobPropertyChanged;
                    AvailableJobs.Add(item);
                }
            }

            ActiveJobs.Clear();
            if (activeResponse?.resultCode == "100" && activeResponse.resultData != null)
            {
                foreach (var item in activeResponse.resultData) ActiveJobs.Add(item);
                await ApplySuggestedRouteAsync();
            }

            NotifySelectionState();
            NotifyActiveState();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[Deliveries] Load failed: {ex}"); }
        finally
        {
            Interlocked.CompareExchange(ref loadCancellation, null, currentCancellation);
            currentCancellation.Dispose();
            if (version == Volatile.Read(ref loadVersion))
                IsLoading = false;
        }
    }

    private void OnAvailableJobPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DeliveryJob.IsSelected)) NotifySelectionState();
    }

    private void NotifySelectionState()
    {
        OnPropertyChanged(nameof(SelectedAvailableCount));
        OnPropertyChanged(nameof(HasSelectedAvailableJobs));
        ClaimSelectedCommand.NotifyCanExecuteChanged();
    }

    private void NotifyActiveState()
    {
        OnPropertyChanged(nameof(HasAcceptedJobs));
        OnPropertyChanged(nameof(HasMultipleActiveJobs));
        StartRouteCommand.NotifyCanExecuteChanged();
    }

    public void CancelLoading()
    {
        Interlocked.Increment(ref loadVersion);
        var cancellation = Interlocked.Exchange(ref loadCancellation, null);
        if (cancellation != null)
        {
            try { cancellation.Cancel(); }
            catch (ObjectDisposedException) { }
        }
        IsLoading = false;
    }

    [RelayCommand]
    private Task Open(DeliveryJob job) => Shell.Current.GoToAsync($"{nameof(DeliveryDetailPage)}?jobId={job.jobId}");

    [RelayCommand(CanExecute = nameof(HasSelectedAvailableJobs))]
    private async Task ClaimSelected()
    {
        var selected = AvailableJobs.Where(x => x.IsSelected).Select(x => x.jobId).ToArray();
        if (selected.Length == 0) return;

        var confirm = await AlertService.Confirm("Buyurtmalarni olish", $"{selected.Length} ta buyurtmani birga yetkazish uchun olasizmi?", "Ha", "Yo‘q");
        if (!confirm) return;

        await RunBatchAction(() => api.ClaimBatch(selected));
    }

    [RelayCommand(CanExecute = nameof(HasAcceptedJobs))]
    private async Task StartRoute()
    {
        var accepted = ActiveJobs.Where(x => x.IsAccepted).OrderBy(x => x.RouteOrder).Select(x => x.jobId).ToArray();
        if (accepted.Length == 0) return;

        var confirm = await AlertService.Confirm("Yetkazishni boshlash", $"{accepted.Length} ta buyurtma uchun yetkazish yo‘lini boshlaysizmi?", "Boshlash", "Bekor qilish");
        if (!confirm) return;

        await RunBatchAction(() => api.StartBatch(accepted));
    }

    [RelayCommand]
    private async Task OpenRouteMap()
    {
        var stops = ActiveJobs.Where(HasCoordinates).OrderBy(x => x.RouteOrder).ToList();
        if (stops.Count == 0)
        {
            await AlertService.Show("Xarita", "Buyurtmalarda koordinata mavjud emas.");
            return;
        }

        var culture = System.Globalization.CultureInfo.InvariantCulture;
        var destination = $"{stops[^1].locationLatitude!.Value.ToString(culture)},{stops[^1].locationLongitude!.Value.ToString(culture)}";
        var waypoints = stops.Count > 1
            ? string.Join("%7C", stops.Take(stops.Count - 1).Select(x => $"{x.locationLatitude!.Value.ToString(culture)},{x.locationLongitude!.Value.ToString(culture)}"))
            : string.Empty;

        var url = $"https://www.google.com/maps/dir/?api=1&destination={destination}&travelmode=driving";
        if (!string.IsNullOrWhiteSpace(waypoints)) url += $"&waypoints={waypoints}";
        await Launcher.Default.OpenAsync(new Uri(url));
    }

    private async Task RunBatchAction(Func<Task<ApiResponse?>> action)
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
        await ReloadAsync();
    }

    private async Task ApplySuggestedRouteAsync()
    {
        foreach (var job in ActiveJobs)
        {
            job.RouteOrder = 0;
            job.DistanceFromPreviousKm = null;
        }

        var located = ActiveJobs.Where(HasCoordinates).ToList();
        if (located.Count == 0) return;

        (double lat, double lon)? origin = await TryGetCourierLocationAsync();
        var route = BuildNearestRoute(located, origin);

        for (var i = 0; i < route.Count; i++)
        {
            route[i].RouteOrder = i + 1;
            if (i == 0 && origin.HasValue)
                route[i].DistanceFromPreviousKm = DistanceKm(origin.Value.lat, origin.Value.lon, route[i].locationLatitude!.Value, route[i].locationLongitude!.Value);
            else if (i > 0)
                route[i].DistanceFromPreviousKm = DistanceKm(route[i - 1].locationLatitude!.Value, route[i - 1].locationLongitude!.Value, route[i].locationLatitude!.Value, route[i].locationLongitude!.Value);
        }

        var ordered = ActiveJobs.OrderBy(x => x.RouteOrder == 0 ? int.MaxValue : x.RouteOrder).ThenBy(x => x.createdAt).ToList();
        ActiveJobs.Clear();
        foreach (var job in ordered) ActiveJobs.Add(job);
    }

    private static bool HasCoordinates(DeliveryJob job) =>
        job.locationLatitude.HasValue && job.locationLongitude.HasValue &&
        Math.Abs(job.locationLatitude.Value) > 0.000001 && Math.Abs(job.locationLongitude.Value) > 0.000001;

    private static async Task<(double lat, double lon)?> TryGetCourierLocationAsync()
    {
        try
        {
            var location = await Microsoft.Maui.Devices.Sensors.Geolocation.Default.GetLastKnownLocationAsync();
            location ??= await Microsoft.Maui.Devices.Sensors.Geolocation.Default.GetLocationAsync(
                new Microsoft.Maui.Devices.Sensors.GeolocationRequest(Microsoft.Maui.Devices.Sensors.GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5)));
            return location == null ? null : (location.Latitude, location.Longitude);
        }
        catch { return null; }
    }

    private static List<DeliveryJob> BuildNearestRoute(List<DeliveryJob> jobs, (double lat, double lon)? origin)
    {
        if (jobs.Count <= 1) return jobs.ToList();

        if (origin.HasValue) return BuildFromStart(jobs, origin.Value.lat, origin.Value.lon);

        // No courier GPS: try every customer as the first stop and keep the shortest path.
        List<DeliveryJob>? best = null;
        double bestDistance = double.MaxValue;
        foreach (var candidate in jobs)
        {
            var route = BuildFromFirstJob(jobs, candidate);
            var distance = TotalRouteDistance(route);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = route;
            }
        }
        return best ?? jobs.ToList();
    }

    private static List<DeliveryJob> BuildFromStart(List<DeliveryJob> jobs, double lat, double lon)
    {
        var remaining = jobs.ToList();
        var route = new List<DeliveryJob>();
        var currentLat = lat;
        var currentLon = lon;
        while (remaining.Count > 0)
        {
            var next = remaining.OrderBy(x => DistanceKm(currentLat, currentLon, x.locationLatitude!.Value, x.locationLongitude!.Value)).First();
            route.Add(next);
            remaining.Remove(next);
            currentLat = next.locationLatitude!.Value;
            currentLon = next.locationLongitude!.Value;
        }
        return route;
    }

    private static List<DeliveryJob> BuildFromFirstJob(List<DeliveryJob> jobs, DeliveryJob first)
    {
        var remaining = jobs.Where(x => x != first).ToList();
        var route = new List<DeliveryJob> { first };
        var current = first;
        while (remaining.Count > 0)
        {
            var next = remaining.OrderBy(x => DistanceKm(current.locationLatitude!.Value, current.locationLongitude!.Value, x.locationLatitude!.Value, x.locationLongitude!.Value)).First();
            route.Add(next);
            remaining.Remove(next);
            current = next;
        }
        return route;
    }

    private static double TotalRouteDistance(IReadOnlyList<DeliveryJob> route)
    {
        double total = 0;
        for (var i = 1; i < route.Count; i++)
            total += DistanceKm(route[i - 1].locationLatitude!.Value, route[i - 1].locationLongitude!.Value, route[i].locationLatitude!.Value, route[i].locationLongitude!.Value);
        return total;
    }

    private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double DegreesToRadians(double value) => value * Math.PI / 180.0;
}
