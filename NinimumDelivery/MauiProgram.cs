using Microsoft.Maui.LifecycleEvents;
using Microsoft.Extensions.Logging;
using NinimumDelivery.Services;
using NinimumDelivery.ViewModels;
using NinimumDelivery.Views;
using RestSharp;

namespace NinimumDelivery;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

#if ANDROID
		builder.ConfigureLifecycleEvents(events => events.AddAndroid(android => android.OnCreate((activity, _) =>
			Plugin.Firebase.Core.Platforms.Android.CrossFirebase.Initialize(activity))));
#endif
        builder.UseMauiApp<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
#if ANDROID
        builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<AppShell, NinimumDelivery.Platforms.Android.DeliveryShellRenderer>());
#elif IOS
        builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<AppShell, NinimumDelivery.Platforms.iOS.DeliveryShellRenderer>());
#endif
        builder.Services.AddSingleton<AppStoreService>();
        builder.Services.AddSingleton<LanguageService>();
        builder.Services.AddSingleton<DeliveryApiService>();
        builder.Services.AddSingleton<AppState>();
        builder.Services.AddSingleton(new RestClient(new RestClientOptions(AppConstants.BaseUrl)
        {
            ThrowOnAnyError = false,
            Timeout = TimeSpan.FromSeconds(30)
        }));

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<DeliveriesPage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<DeliveryDetailPage>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<DeliveriesViewModel>();
        builder.Services.AddTransient<HistoryViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<DeliveryDetailViewModel>();

        var app = builder.Build();
        AppServices.Init(app.Services);
        return app;
    }
}
