using NinimumDelivery.Services;
using NinimumDelivery.Views;

namespace NinimumDelivery;

public partial class App : Application
{
    public App(LanguageService languageService)
    {
        InitializeComponent();
        languageService.Init();
        Routing.RegisterRoute(nameof(DeliveryDetailPage), typeof(DeliveryDetailPage));
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var appState = AppServices.GetRequired<AppState>();

        Page rootPage;

        if (appState.IsLoggedIn())
            rootPage = new AppShell();
        else
            rootPage = AppServices.GetRequired<LoginPage>();

        return new Window(rootPage);
    }
}
