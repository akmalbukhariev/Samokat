using Ninimum.Services;
using Utils;

namespace Ninimum;

public partial class AppEntryShell : Shell
{
    private readonly AppStoreService appStoreService;
    private readonly AppControl appControl;
    private bool initialized;

    public AppEntryShell()
    {
        InitializeComponent();

        appStoreService = AppService.GetRequired<AppStoreService>();
        appControl = AppService.GetRequired<AppControl>();

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
        bool hasSavedLogin = appStoreService.Get(AppKeys.IsLoggedIn, false);

        if (hasSavedLogin)
        {
            string phoneNumber = appStoreService.Get(AppKeys.PhoneNumber, string.Empty);
            string password = appStoreService.Get(AppKeys.Password, string.Empty);

            if (!string.IsNullOrWhiteSpace(phoneNumber) && !string.IsNullOrWhiteSpace(password))
            {
                try
                {
                    bool restored = await appControl.Login(phoneNumber, password);

                    if (restored)
                        return;
                }
                catch
                {
                    // If session restoration cannot be completed, open the app as guest.
                    // Saved credentials are kept so the user can still log in later.
                }
            }
        }

        await appControl.StartGuestMode();
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
