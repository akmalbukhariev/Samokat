#if ANDROID
using Plugin.Firebase.CloudMessaging;
#endif

namespace NinimumWarehouse.Services;

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
        try
        {
            if (!listening)
            {
                listening = true;
                CrossFirebaseCloudMessaging.Current.TokenChanged += async (_, e) =>
                {
                    lastToken = e.Token;
                    if (Enabled && sender is not null && !string.IsNullOrWhiteSpace(e.Token)) await Safe(() => sender(e.Token));
                };
            }
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
                await MainThread.InvokeOnMainThreadAsync(() => Permissions.RequestAsync<Permissions.PostNotifications>());
            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
            var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token)) return;
            lastToken = token;
            await send(token);
        }
        catch (Exception ex) { Console.WriteLine($"[Push] {ex.Message}"); }
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

    private static async Task Safe(Func<Task> call) { try { await call(); } catch (Exception ex) { Console.WriteLine($"[Push] {ex.Message}"); } }
}
