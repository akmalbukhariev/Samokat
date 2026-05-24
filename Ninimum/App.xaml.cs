using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Samokat.Views.DetailProduct;
using Samokat.Views.Formalization;
using Samokat.Views.LoginRegister;
using Samokat.Views.Main;
using Samokat.Views.PaymentCard;
using Samokat.Views.Search;
using Samokat.Views.Startup;

namespace Samokat;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        
        RegisterRoutes();
        Setting();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppEntryShell());
        //return new Window(new AppShell());
	}

    private void RegisterRoutes()
    {
        #region Startup pages
        Routing.RegisterRoute(nameof(StartPage), typeof(StartPage));
        Routing.RegisterRoute(nameof(OnboardingPage), typeof(OnboardingPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
        #endregion

        #region Main pages
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(MenuPage), typeof(MenuPage));
        Routing.RegisterRoute(nameof(SearchPage), typeof(SearchPage));
        Routing.RegisterRoute(nameof(DetailProductPage), typeof(DetailProductPage));
        Routing.RegisterRoute(nameof(ProductReviews), typeof(ProductReviews));
        Routing.RegisterRoute(nameof(LeaveCommentPage), typeof(LeaveCommentPage));
        Routing.RegisterRoute(nameof(FormalizationPage), typeof(FormalizationPage));
        Routing.RegisterRoute(nameof(AddPaymentCardPage), typeof(AddPaymentCardPage));
        Routing.RegisterRoute(nameof(CancelOrderPage), typeof(CancelOrderPage));
         Routing.RegisterRoute(nameof(PaymentCardPage), typeof(PaymentCardPage));
        #endregion
    }

	private void Setting()
    {
#if ANDROID
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, view) =>
        {
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
        });

        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping(nameof(Picker), (handler, view) =>
        {
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
        });

        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping(nameof(DatePicker), (handler, view) =>
        {
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
        });

        Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping(nameof(TimePicker), (handler, view) =>
        {
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
        });

        EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {
            if (handler.PlatformView is Android.Widget.EditText editText)
            {
                editText.Background = null; // Removes underline
            }
        });
#endif

#if IOS    
        EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, view) =>
        {
            handler.PlatformView.BorderStyle = UITextBorderStyle.None;
            handler.PlatformView.Layer.BorderWidth = 0;
            handler.PlatformView.BackgroundColor = UIColor.Clear;
        });

        PickerHandler.Mapper.AppendToMapping(nameof(Picker), (handler, view) =>
        {
            handler.PlatformView.BorderStyle = UITextBorderStyle.None;
            handler.PlatformView.Layer.BorderWidth = 0;
            handler.PlatformView.BackgroundColor = UIColor.Clear;
        });

        DatePickerHandler.Mapper.AppendToMapping(nameof(DatePicker), (handler, view) =>
        {
            handler.PlatformView.BorderStyle = UITextBorderStyle.None;
            handler.PlatformView.Layer.BorderWidth = 0;
            handler.PlatformView.BackgroundColor = UIColor.Clear;
        });

        TimePickerHandler.Mapper.AppendToMapping(nameof(TimePicker), (handler, view) =>
        {
            handler.PlatformView.BorderStyle = UITextBorderStyle.None;
            handler.PlatformView.Layer.BorderWidth = 0;
            handler.PlatformView.BackgroundColor = UIColor.Clear;
        });

        EditorHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
        {
            // Editor is UITextView on iOS
            var textView = handler.PlatformView;
            textView.Layer.BorderWidth = 0;
            textView.BackgroundColor = UIColor.Clear;
        });
#endif
    }
}