using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using NinimumDelivery.Services;
using NinimumDelivery.Views;

namespace NinimumDelivery;

public partial class App : Application
{
    public static event EventHandler? AppStopped;
    public static event EventHandler? AppResumed;

    public App(LanguageService languageService)
    {
        InitializeComponent();
        languageService.Init();
        Routing.RegisterRoute(nameof(DeliveryDetailPage), typeof(DeliveryDetailPage));
        Setting();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var appState = AppServices.GetRequired<AppState>();

        Page rootPage;

        if (appState.IsLoggedIn())
            rootPage = new AppShell();
        else
            rootPage = AppServices.GetRequired<LoginPage>();

        var window = new Window(rootPage);

        window.Stopped += (_, _) => AppStopped?.Invoke(this, EventArgs.Empty);
        window.Resumed += (_, _) => AppResumed?.Invoke(this, EventArgs.Empty);

        return window;
    }

    private void Setting()
    {
#if ANDROID
        EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, view) =>
        {
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
        });

        PickerHandler.Mapper.AppendToMapping(nameof(Picker), (handler, view) =>
        {
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
        });

        DatePickerHandler.Mapper.AppendToMapping(nameof(DatePicker), (handler, view) =>
        {
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
        });

        TimePickerHandler.Mapper.AppendToMapping(nameof(TimePicker), (handler, view) =>
        {
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
        });

        EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {
            if (handler.PlatformView is Android.Widget.EditText editText)
                editText.Background = null;
        });
#endif

#if IOS
        EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, view) =>
        {
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
            handler.PlatformView.Layer.BorderWidth = 0;
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
        });

        PickerHandler.Mapper.AppendToMapping(nameof(Picker), (handler, view) =>
        {
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
            handler.PlatformView.Layer.BorderWidth = 0;
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
        });

        DatePickerHandler.Mapper.AppendToMapping(nameof(DatePicker), (handler, view) =>
        {
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
            handler.PlatformView.Layer.BorderWidth = 0;
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
        });

        TimePickerHandler.Mapper.AppendToMapping(nameof(TimePicker), (handler, view) =>
        {
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
            handler.PlatformView.Layer.BorderWidth = 0;
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
        });

        EditorHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
        {
            var textView = handler.PlatformView;
            textView.Layer.BorderWidth = 0;
            textView.BackgroundColor = UIKit.UIColor.Clear;
        });
#endif
    }
}
