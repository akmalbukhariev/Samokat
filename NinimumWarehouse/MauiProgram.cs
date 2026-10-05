using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

namespace NinimumWarehouse;

public static class MauiProgram
{
#if ANDROID
    private static void RemoveUnderline(global::Android.Views.View view)
    {
        view.BackgroundTintList = global::Android.Content.Res.ColorStateList.ValueOf(global::Android.Graphics.Color.Transparent);
        view.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
    }
#endif
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseBarcodeReader()
            .ConfigureMauiHandlers(handlers => {
#if ANDROID
                handlers.AddHandler<Shell, Platforms.Android.WarehouseShellRenderer>();
#elif IOS
                handlers.AddHandler<Shell, Platforms.iOS.WarehouseShellRenderer>();
#endif
            })
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if ANDROID
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("RoundedInput", (handler, _) => RemoveUnderline(handler.PlatformView));
        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("RoundedInput", (handler, _) => RemoveUnderline(handler.PlatformView));
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("RoundedInput", (handler, _) => RemoveUnderline(handler.PlatformView));
#endif
#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
