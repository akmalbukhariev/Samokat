using System.ComponentModel;
using System.Runtime.CompilerServices;
using Utils;

namespace Ninimum.Services;

public enum ServerConnectionState
{
    Connected,
    Checking,
    NoInternet,
    ServerUnavailable
}

public sealed class ConnectionMonitorService : INotifyPropertyChanged, IDisposable
{
    private const int BackgroundFailuresBeforeBanner = 3;
    private static readonly TimeSpan ConnectedCheckInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan SuspectedFailureCheckInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan DisconnectedCheckInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan RecentApiSuccessGrace = TimeSpan.FromSeconds(8);
    private static readonly TimeSpan FailureConfirmationDelay = TimeSpan.FromMilliseconds(700);

    private readonly HttpClient httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(3)
    };

    private readonly SemaphoreSlim checkLock = new(1, 1);
    private readonly object connectionSignalLock = new();
    private readonly object failureConfirmationLock = new();

    private CancellationTokenSource? monitorCts;
    private TaskCompletionSource<bool> connectedSignal = CreateCompletedSignal();
    private Task<bool>? activeFailureConfirmation;
    private ServerConnectionState state = ServerConnectionState.Connected;
    private int consecutiveBackgroundFailures;
    private DateTimeOffset lastServerReachableAt = DateTimeOffset.MinValue;
    private bool started;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? ConnectionRestored;

    public ServerConnectionState State => state;
    public bool IsConnected => state == ServerConnectionState.Connected;
    public bool IsBannerVisible => state != ServerConnectionState.Connected;
    public bool ShowSpinner => state is ServerConnectionState.Checking or ServerConnectionState.ServerUnavailable;
    public bool ShouldSkipApiRequest => state is ServerConnectionState.NoInternet or ServerConnectionState.ServerUnavailable or ServerConnectionState.Checking;

    public string StatusText => state switch
    {
        ServerConnectionState.NoInternet => "Internet aloqasi yo'q",
        ServerConnectionState.Checking => "Serverga ulanmoqda...",
        ServerConnectionState.ServerUnavailable => "Server bilan aloqa yo'q. Qayta ulanmoqda...",
        _ => string.Empty
    };

    public Color BannerColor => state == ServerConnectionState.NoInternet
        ? Color.FromArgb("#E5484D")
        : Color.FromArgb("#F59E0B");

    public void Start()
    {
        if (started)
            return;

        started = true;
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        monitorCts = new CancellationTokenSource();
        _ = MonitorLoopAsync(monitorCts.Token);
    }

    public void Stop()
    {
        if (!started)
            return;

        started = false;
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;

        try { monitorCts?.Cancel(); }
        catch { }
        finally
        {
            monitorCts?.Dispose();
            monitorCts = null;
        }
    }

    /// <summary>
    /// Performs a foreground check. Two failed probes are required before the server is
    /// declared unavailable, which avoids a banner flash caused by one temporary timeout.
    /// </summary>
    public async Task<bool> CheckNowAsync(CancellationToken cancellationToken = default)
    {
        Start();

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            consecutiveBackgroundFailures = BackgroundFailuresBeforeBanner;
            SetState(ServerConnectionState.NoInternet);
            return false;
        }

        if (await ProbeServerAsync(cancellationToken))
            return true;

        try
        {
            await Task.Delay(FailureConfirmationDelay, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return IsConnected;
        }

        if (await ProbeServerAsync(cancellationToken))
            return true;

        consecutiveBackgroundFailures = BackgroundFailuresBeforeBanner;
        SetState(ServerConnectionState.ServerUnavailable);
        return false;
    }

    /// <summary>
    /// Called after a real API transport failure. The failure is confirmed with the health
    /// endpoint before ConnectionStatusView is shown. Multiple simultaneous API failures
    /// share the same confirmation task so navigation does not cause competing checks.
    /// </summary>
    public Task<bool> ConfirmServerReachableAfterRequestFailureAsync(CancellationToken cancellationToken = default)
    {
        lock (failureConfirmationLock)
        {
            if (activeFailureConfirmation is { IsCompleted: false })
                return activeFailureConfirmation;

            activeFailureConfirmation = ConfirmServerReachableAfterRequestFailureCoreAsync(cancellationToken);
            return activeFailureConfirmation;
        }
    }

    /// <summary>
    /// Compatibility helper for callers that cannot await the confirmation directly.
    /// It deliberately does not change the banner immediately.
    /// </summary>
    public void ReportApiFailure()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            SetState(ServerConnectionState.NoInternet);
            return;
        }

        _ = ConfirmServerReachableAfterRequestFailureAsync();
    }

    public void ReportApiSuccess()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return;

        MarkServerReachable();
    }

    /// <summary>
    /// Waits without blocking the UI until the monitor confirms that the server is back.
    /// </summary>
    public async Task WaitUntilConnectedAsync(CancellationToken cancellationToken = default)
    {
        Start();

        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet && IsConnected)
            return;

        Task waitTask;
        lock (connectionSignalLock)
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet && IsConnected)
                return;

            waitTask = connectedSignal.Task;
        }

        await waitTask.WaitAsync(cancellationToken);
    }

    public void Retry()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            SetState(ServerConnectionState.NoInternet);
            return;
        }

        if (!IsConnected)
            SetState(ServerConnectionState.Checking);

        _ = CheckNowAsync();
    }

    private async Task<bool> ConfirmServerReachableAfterRequestFailureCoreAsync(CancellationToken cancellationToken)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            SetState(ServerConnectionState.NoInternet);
            return false;
        }

        // A normal API request already failed. Still require health confirmation before
        // showing a global connection problem, because an individual endpoint may timeout
        // while the server itself remains reachable.
        if (await ProbeServerAsync(cancellationToken))
            return true;

        try
        {
            await Task.Delay(FailureConfirmationDelay, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return IsConnected;
        }

        if (await ProbeServerAsync(cancellationToken))
            return true;

        SetState(ServerConnectionState.ServerUnavailable);
        return false;
    }

    /// <summary>
    /// A heartbeat is a reachability probe, not a business-health decision. Any HTTP
    /// response proves that the app can reach the backend process. This prevents a 4xx/5xx
    /// health response from fighting with successful normal API calls and flashing the banner.
    /// </summary>
    private async Task<bool> ProbeServerAsync(CancellationToken cancellationToken)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return false;

        try
        {
            await checkLock.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, AppConstants.HEALTH_URL);
            request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true
            };

            using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            // Receiving HTTP headers means the network path and backend HTTP server are alive.
            // Do not use IsSuccessStatusCode here; ConnectionStatusView represents connection,
            // not a temporary application/business error returned by the server.
            MarkServerReachable();
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch
        {
            return false;
        }
        finally
        {
            checkLock.Release();
        }
    }

    private void MarkServerReachable()
    {
        lastServerReachableAt = DateTimeOffset.UtcNow;
        consecutiveBackgroundFailures = 0;
        SetState(ServerConnectionState.Connected);
    }

    private bool HasRecentSuccessfulConnection()
    {
        if (lastServerReachableAt == DateTimeOffset.MinValue)
            return false;

        return DateTimeOffset.UtcNow - lastServerReachableAt <= RecentApiSuccessGrace;
    }

    private async Task MonitorLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                consecutiveBackgroundFailures = BackgroundFailuresBeforeBanner;
                SetState(ServerConnectionState.NoInternet);
            }
            else
            {
                bool reachable = await ProbeServerAsync(cancellationToken);

                if (!reachable)
                {
                    consecutiveBackgroundFailures++;

                    // Do not let a background heartbeat overwrite a very recent successful
                    // real API response. The real request is stronger proof that the backend
                    // is reachable than one timed-out monitoring request.
                    bool recentlyReachable = HasRecentSuccessfulConnection();
                    if (!recentlyReachable && consecutiveBackgroundFailures >= BackgroundFailuresBeforeBanner)
                        SetState(ServerConnectionState.ServerUnavailable);
                }
            }

            try
            {
                TimeSpan delay;

                if (!IsConnected)
                    delay = DisconnectedCheckInterval;
                else if (consecutiveBackgroundFailures > 0)
                    delay = SuspectedFailureCheckInterval;
                else
                    delay = ConnectedCheckInterval;

                await Task.Delay(delay, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess != NetworkAccess.Internet)
        {
            consecutiveBackgroundFailures = BackgroundFailuresBeforeBanner;
            SetState(ServerConnectionState.NoInternet);
            return;
        }

        consecutiveBackgroundFailures = 0;

        // Only show "connecting" when we were genuinely disconnected. Android can emit
        // repeated connectivity events while the connection is already healthy.
        if (!IsConnected)
            SetState(ServerConnectionState.Checking);

        _ = CheckNowAsync();
    }

    private void SetState(ServerConnectionState newState)
    {
        if (state == newState)
            return;

        var previousState = state;
        state = newState;

        lock (connectionSignalLock)
        {
            if (newState == ServerConnectionState.Connected)
            {
                connectedSignal.TrySetResult(true);
            }
            else if (previousState == ServerConnectionState.Connected || connectedSignal.Task.IsCompleted)
            {
                connectedSignal = CreatePendingSignal();
            }
        }

        void NotifyUi()
        {
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(IsBannerVisible));
            OnPropertyChanged(nameof(ShowSpinner));
            OnPropertyChanged(nameof(ShouldSkipApiRequest));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(BannerColor));

            if (newState == ServerConnectionState.Connected && previousState != ServerConnectionState.Connected)
                ConnectionRestored?.Invoke(this, EventArgs.Empty);
        }

        if (MainThread.IsMainThread)
            NotifyUi();
        else
            MainThread.BeginInvokeOnMainThread(NotifyUi);
    }

    private static TaskCompletionSource<bool> CreatePendingSignal()
    {
        return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private static TaskCompletionSource<bool> CreateCompletedSignal()
    {
        var signal = CreatePendingSignal();
        signal.TrySetResult(true);
        return signal;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        Stop();
        httpClient.Dispose();
        checkLock.Dispose();
    }
}
