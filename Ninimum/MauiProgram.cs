using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Samokat.Services;
using Samokat.Services.Interface;
using Xe.AcrylicView;
using Samokat.ViewModels;
using Samokat.Views.Main;
using Samokat.Views.Search;
using Samokat.Views.DetailProduct;
using Samokat.Views.FavoriteProduct;
using Samokat.Views.BasketProduct;
using Samokat.Views.Formalization;
using Samokat.Views.PaymentCard;
using Samokat.Views.Children;
using Samokat.Views.Profile;
using Api.Services;
using Samokat.Views.LoginRegister;
using RestSharp;
using Utils;

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
		builder.Services.AddSingleton<LanguageService>();
		builder.Services.AddSingleton<UserApiService>();
		builder.Services.AddSingleton(sp =>
                new RestClient(new RestClientOptions(AppConstants.BASE_USER_URL)
                {
                    ThrowOnAnyError = false,
                    Timeout = TimeSpan.FromSeconds(30)
                }));

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
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<MenuPage>();
		builder.Services.AddTransient<SearchPage>();
		builder.Services.AddTransient<DetailProductPage>();
		builder.Services.AddTransient<ProductReviews>();
		builder.Services.AddTransient<FavoritePage>();
		builder.Services.AddTransient<BasketProductPage>();
		builder.Services.AddTransient<FormalizationPage>();
		builder.Services.AddTransient<PaymentCardPage>();
		builder.Services.AddTransient<ChildrenPage>();
		builder.Services.AddTransient<ChildInfoPage>();
		builder.Services.AddTransient<MyProfilePage>();

		builder.Services.AddTransient<MainPageViewModel>();
		builder.Services.AddTransient<MenuPageViewModel>();
		builder.Services.AddTransient<SearchPageViewModel>();
		builder.Services.AddTransient<DetailProductPageViewModel>();
		builder.Services.AddTransient<ProductReviewsViewModel>();
		builder.Services.AddTransient<FavoritePageViewModel>();
		builder.Services.AddTransient<BasketProductPageViewModel>();
		builder.Services.AddTransient<LoginPageViewModel>();
		builder.Services.AddTransient<RegisterPageViewModel>();

		builder.Services.AddTransient(sp =>
                new UserApiService(
                    sp.GetServices<RestClient>().First(rc => rc.Options.BaseUrl == new Uri(AppConstants.BASE_USER_URL))
                ));
	}
}
