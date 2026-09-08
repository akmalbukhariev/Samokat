using Ninimum.Services;
using Utils;

namespace Ninimum;

public partial class AppEntryShell : Shell
{
    private readonly AppStoreService appStoreService;
    private readonly AppControl appControl;
    private readonly ConnectionMonitorService connectionMonitor;
    private bool initialized;

    public AppEntryShell()
    {
        InitializeComponent();

        appStoreService = AppService.GetRequired<AppStoreService>();
        appControl = AppService.GetRequired<AppControl>();
        connectionMonitor = AppService.GetRequired<ConnectionMonitorService>();

        ShowLoadingPage();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        if (initialized)
            return;

        initialized = true;
        await InitializeSessionAsync();
    }

    private async Task InitializeSessionAsync()
    {
        bool hasCompletedOnboarding = appStoreService.Get(AppKeys.HasCompletedOnboarding, false);
        if (!hasCompletedOnboarding)
        {
            ShowFirstLaunchPage();
            return;
        }

        bool hasSavedLogin = appStoreService.Get(AppKeys.IsLoggedIn, false);

        if (hasSavedLogin)
        {
            string phoneNumber = appStoreService.Get(AppKeys.PhoneNumber, string.Empty);
            string password = appStoreService.Get(AppKeys.Password, string.Empty);

            if (!string.IsNullOrWhiteSpace(phoneNumber) && !string.IsNullOrWhiteSpace(password))
            {
                try
                {
                    bool serverAvailable = await connectionMonitor.CheckNowAsync();
                    if (!serverAvailable)
                    {
                        // Show the normal app immediately instead of trapping the user on a
                        // startup spinner. API reads will wait in the background and resume.
                        // Also keep a one-time session restore pending for when the server is UP.
                        await appControl.StartGuestMode();
                        _ = RestoreSavedSessionWhenConnectedAsync(phoneNumber, password);
                        return;
                    }

                    bool restored = await appControl.Login(phoneNumber, password);
                    if (restored)
                        return;
                }
                catch
                {
                    // Saved credentials are kept. If restoration fails for a real credential
                    // reason, the app remains usable as guest and the user can log in manually.
                }
            }
        }

        await appControl.StartGuestMode();
    }

    private async Task RestoreSavedSessionWhenConnectedAsync(string phoneNumber, string password)
    {
        try
        {
            await connectionMonitor.WaitUntilConnectedAsync();

            if (appControl.IsAuthenticated)
                return;

            await appControl.Login(phoneNumber, password);
        }
        catch
        {
            // Connection monitoring keeps running. A failed credential/session restore must
            // never make the app unusable; the user can still authenticate manually.
        }
    }

    private void ShowFirstLaunchPage()
    {
        Items.Clear();

        Items.Add(new ShellContent
        {
            Route = "FirstLaunch",
            Content = AppService.GetRequired<Ninimum.Views.Startup.StartPage>()
        });
    }

    private void ShowLoadingPage()
    {
        Items.Clear();

        Items.Add(new ShellContent
        {
            Content = new ContentPage
            {
                BackgroundColor = Colors.White,
                Content = new ActivityIndicator
                {
                    IsRunning = true,
                    Color = (Color)Application.Current!.Resources["PrimaryColor"],
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                }
            }
        });
    }
}
