#if ANDROID
using Plugin.Firebase.CloudMessaging;
#endif

namespace Ninimum.Services;

// Push notifications (Firebase): asks the notification permission once, then gives the server this phone's address.
// The user can switch them off on the profile page (on by default); off = the server forgets this phone.
public static class PushRegistration
{
    private const string EnabledKey = "push_enabled";
    private static string? lastToken;
    private static Func<string, Task>? sender, remover;
    private static bool listening;

    public static bool Enabled => Preferences.Default.Get(EnabledKey, true);

    public static async Task RegisterAsync(Func<string, Task> send, Func<string, Task> remove)
    {
        sender = send; remover = remove;
        if (!Enabled) return;
#if ANDROID
        // Right after start-up Firebase may not be ready yet: try a few times.
        for (int attempt = 0; attempt < 5; attempt++)
        try
        {
            if (!listening)
            {
                CrossFirebaseCloudMessaging.Current.TokenChanged += async (_, e) =>
                {
                    lastToken = e.Token;
                    if (Enabled && sender is not null && !string.IsNullOrWhiteSpace(e.Token)) await Safe(() => sender(e.Token));
                };
                listening = true;
            }
            if (attempt == 0 && OperatingSystem.IsAndroidVersionAtLeast(33))
                await MainThread.InvokeOnMainThreadAsync(() => Permissions.RequestAsync<Permissions.PostNotifications>());
            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
            var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token)) return;
            lastToken = token;
            await send(token);
            return;
        }
        catch (Exception ex) { Console.WriteLine($"[Push] {ex.Message}"); await Task.Delay(3000); }
#else
        await Task.CompletedTask;
#endif
    }

    // Profile switch. Kept per phone, so it also holds after signing in again.
    public static async Task SetEnabledAsync(bool on)
    {
        if (on == Enabled) return;
        Preferences.Default.Set(EnabledKey, on);
        if (on) { if (sender is not null && remover is not null) await RegisterAsync(sender, remover); }
        else await Forget();
    }

    // Sign-out: this phone should no longer get the account's notifications.
    public static async Task UnregisterAsync()
    {
        await Forget();
        sender = null; remover = null;
    }

    private static async Task Forget()
    {
        var token = lastToken; lastToken = null;
        if (!string.IsNullOrWhiteSpace(token) && remover is not null) await Safe(() => remover(token));
    }

    // Tapping an order notification opens that order. Kept until the customer is signed in and the app is ready.
    public static long PendingOrderId { get; private set; }
    private static string pendingStatus = "";
    private static bool tapsListening;

    public static void ListenForTaps()
    {
#if ANDROID
        if (tapsListening) return;
        tapsListening = true;
        CrossFirebaseCloudMessaging.Current.NotificationTapped += (_, e) =>
        {
            if (e.Notification?.Data != null && e.Notification.Data.TryGetValue("order_id", out var value) && long.TryParse(value, out var id))
            {
                PendingOrderId = id;
                pendingStatus = e.Notification.Data.TryGetValue("status", out var status) ? status ?? "" : "";
                MainThread.BeginInvokeOnMainThread(async () => await OpenPendingOrderAsync());
            }
        };
#endif
    }

    public static async Task OpenPendingOrderAsync()
    {
        long id = PendingOrderId;
        var app = AppService.Get<AppControl>();
        if (id <= 0 || app is null || !app.IsAuthenticated || Shell.Current is null) return;
        PendingOrderId = 0;
        // Absolute route: Profil → Buyurtmalar every time, instead of stacking another Buyurtmalar page.
        await Safe(() => AppNavigatorService.NavigateTo($"//ProfileTab/{nameof(Ninimum.Views.Orders.OrdersPage)}?openOrderId={id}&status={pendingStatus}"));
    }

    private static async Task Safe(Func<Task> call) { try { await call(); } catch (Exception ex) { Console.WriteLine($"[Push] {ex.Message}"); } }
}
