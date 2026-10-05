using NinimumWarehouse.Resources.Languages;
using NinimumWarehouse.Services;
using NinimumWarehouse.Views;
namespace NinimumWarehouse;
public partial class App : Application
{
    public static App CurrentApp => (App)Current!;
    private readonly WarehouseApi api = new();
    private Window? window;
    public App()
    {
        LanguageService.Init();
        InitializeComponent();
        UserAppTheme = AppTheme.Light;
        api.SessionEnded += () => MainThread.BeginInvokeOnMainThread(async () => {
            ShowLogin();
            if (window?.Page is not null)
                await window.Page.DisplayAlertAsync(AppResource.SignInAgain, LocalizedMessages.Error("SESSION"), AppResource.Ok);
        });
    }
    protected override Window CreateWindow(IActivationState? activationState)
    {
        window = new Window(new StartupPage());
        window.Created += async (_, _) => {
            await api.RestoreAsync();
            if (api.SignedIn) {
                try { api.SetWorker(await api.Request<Models.Worker>("me")); ShowHome(); }
                catch { ShowLogin(); }
            } else ShowLogin();
        };
        window.Resumed += async (_, _) => {
            if (window.Page is AppShell shell && shell.Navigation.ModalStack.Count == 0) {
                if (shell.CurrentPage is OrdersPage orders) await orders.RefreshAsync();
                else if (shell.CurrentPage is OrderDetailPage detail) await detail.RefreshAsync();
            }
        };
        return window;
    }
    public void ShowLogin(string workerId = "", string password = "")
    {
        if (window is not null) window.Page = new NavigationPage(new LoginPage(api, workerId, password));
    }
    public void ShowHome(bool showProfile = false)
    {
        if (window is not null) window.Page = new AppShell(api, showProfile);
    }
}
