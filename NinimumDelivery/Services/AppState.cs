using NinimumDelivery.Models;
using NinimumDelivery.Views;

namespace NinimumDelivery.Services;

public class AppState
{
    private readonly DeliveryApiService api;
    private readonly AppStoreService store;
    public DeliveryWorker? Worker { get; private set; }

    public AppState(DeliveryApiService api, AppStoreService store)
    {
        this.api = api;
        this.store = store;
    }

    public bool IsLoggedIn()
    {
        var loggedIn = store.Get(AppConstants.LoggedInKey, false);
        var token = store.Get<string>(AppConstants.TokenKey, string.Empty);

        if (!loggedIn || string.IsNullOrWhiteSpace(token))
            return false;

        Worker = store.Get<DeliveryWorker?>(AppConstants.WorkerKey, null);

        return true;
    }

    /// <summary>
    /// Restores the locally saved session without making any network request.
    /// This keeps app startup instant and prevents the native splash screen from
    /// waiting for the backend/network.
    /// </summary>
    public bool RestoreSavedSession()
    {
        var loggedIn = store.Get(AppConstants.LoggedInKey, false);
        var token = store.Get<string>(AppConstants.TokenKey, string.Empty);

        if (!loggedIn || string.IsNullOrWhiteSpace(token))
        {
            Worker = null;
            api.Logout();
            store.Remove(AppConstants.WorkerKey);
            return false;
        }

        Worker = store.Get<DeliveryWorker?>(AppConstants.WorkerKey, null);
        return true;
    }

    public async Task<bool> LoginAsync(string workerId, string password)
    {
        var response = await api.Login(workerId, password);
        if (response?.resultCode != "100" || response.resultData == null)
            return false;

        Worker = response.resultData;
        store.Set(AppConstants.LoggedInKey, true);
        store.Set(AppConstants.WorkerKey, Worker);
        SetRoot(new AppShell());
        return true;
    }

    public void Logout()
    {
        Worker = null;
        api.Logout();
        store.Remove(AppConstants.WorkerKey);
        SetRoot(AppServices.GetRequired<LoginPage>());
    }

    public async Task RefreshWorkerAsync()
    {
        var response = await api.Me();
        if (response?.resultCode == "100" && response.resultData != null)
        {
            Worker = response.resultData;
            store.Set(AppConstants.WorkerKey, Worker);
        }
    }

    public void ReloadForLanguage()
    {
        SetRoot(Worker == null ? AppServices.GetRequired<LoginPage>() : new AppShell());
    }

    private static void SetRoot(Page page)
    {
        void Apply()
        {
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window != null)
                window.Page = page;
        }

        if (MainThread.IsMainThread)
            Apply();
        else
            MainThread.BeginInvokeOnMainThread(Apply);
    }
}
