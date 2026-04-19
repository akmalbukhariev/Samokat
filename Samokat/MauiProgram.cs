using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Samokat.Services;
using Samokat.Services.Interface;
using Xe.AcrylicView;
using Samokat.ViewModels;
using Samokat.Views.Main;
using Samokat.Views.Search;
using Samokat.Views.DetailProduct;


#if ANDROID

#elif IOS
using UIKit;
using Foundation;
#endif

namespace Samokat;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseAcrylicView()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

				fonts.AddFont("SFPRODISPLAYREGULAR.OTF", "SFRegular");
				fonts.AddFont("SFPRODISPLAYMEDIUM.OTF", "SFMedium");
				fonts.AddFont("SFPRODISPLAYBOLD.OTF", "SFBold");
				fonts.AddFont("SansitaOne.ttf", "SansitaOne");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
		RegisterSingleton(builder);
		RegisterTransient(builder);

		var mauiApp = builder.Build();
		AppService.Init(mauiApp.Services);

		return builder.Build();
	}

	private static void RegisterSingleton(MauiAppBuilder builder)
	{
		builder.Services.AddSingleton<AppStoreService>();
		builder.Services.AddSingleton<AppControl>();

#if ANDROID
		builder.Services.AddSingleton<IStatusBarService, Samokat.Platforms.Android.StatusBarService>();
		//builder.Services.AddSingleton<INotificationService, NotificationService>();
		//builder.Services.AddSingleton<IKeyboardHelper, KeyboardHelper>();
#endif

#if IOS
		builder.Services.AddSingleton<IStatusBarService, Samokat.Platforms.iOS.StatusBarService>();
		//builder.Services.AddSingleton<IKeyboardHelper, EcoPlatesMobile.Platforms.iOS.KeyboardHelper>();
#endif
	}

	private static void RegisterTransient(MauiAppBuilder builder)
	{
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<MenuPage>();
		builder.Services.AddTransient<SearchPage>();
		builder.Services.AddTransient<DetailProductPage>();
		builder.Services.AddTransient<ProductReviews>();

		builder.Services.AddTransient<MainPageViewModel>();
		builder.Services.AddTransient<MenuPageViewModel>();
		builder.Services.AddTransient<SearchPageViewModel>();
		builder.Services.AddTransient<DetailProductPageViewModel>();
		builder.Services.AddTransient<ProductReviewsViewModel>();
	}
}
