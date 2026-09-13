using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Ninimum.Services;
using Ninimum.Views.Authorization;
using Ninimum.Views.ChangePhoneNumber;
using Ninimum.Views.DetailProduct;
using Ninimum.Views.Formalization;
using Ninimum.Views.LoginRegister;
using Ninimum.Views.Main;
using Ninimum.Views.MyTariff;
using Ninimum.Views.Orders;
using Ninimum.Views.Payment;
using Ninimum.Views.PaymentCard;
using Ninimum.Views.Profile;
using Ninimum.Views.Search;
using Ninimum.Views.Startup;

namespace Ninimum;

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
        var window = new Window(new AppEntryShell());
        var connectionMonitor = AppService.Get<ConnectionMonitorService>();

        window.Activated += (_, _) => connectionMonitor?.Start();
        window.Resumed += (_, _) => connectionMonitor?.Start();
        window.Stopped += (_, _) => connectionMonitor?.Stop();
        window.Destroying += (_, _) => connectionMonitor?.Stop();

        return window;
    }

    private void RegisterRoutes()
    {
        #region Startup pages
        Routing.RegisterRoute(nameof(StartPage), typeof(StartPage));
        Routing.RegisterRoute(nameof(OnboardingPage), typeof(OnboardingPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
        Routing.RegisterRoute(nameof(AuthorizationPage), typeof(AuthorizationPage));
        Routing.RegisterRoute(nameof(AddressPage), typeof(AddressPage));
        Routing.RegisterRoute(nameof(ChangePasswordPage), typeof(ChangePasswordPage));
        Routing.RegisterRoute(nameof(ChangePhoneNumberPage), typeof(ChangePhoneNumberPage));
        Routing.RegisterRoute("ProfileChangePasswordPage", typeof(Ninimum.Views.ChangePassword.ChangePasswordPage));
        #endregion

        #region Main pages
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(MenuPage), typeof(MenuPage));
        Routing.RegisterRoute(nameof(SearchPage), typeof(SearchPage));
        Routing.RegisterRoute(nameof(DetailProductPage), typeof(DetailProductPage));
        Routing.RegisterRoute(nameof(ProductReviews), typeof(ProductReviews));
        Routing.RegisterRoute(nameof(LeaveCommentPage), typeof(LeaveCommentPage));
        Routing.RegisterRoute(nameof(ProductQuestionsPage), typeof(ProductQuestionsPage));
        Routing.RegisterRoute(nameof(AskProductQuestionPage), typeof(AskProductQuestionPage));
        Routing.RegisterRoute(nameof(FormalizationPage), typeof(FormalizationPage));
        Routing.RegisterRoute(nameof(AddPaymentCardPage), typeof(AddPaymentCardPage));
        Routing.RegisterRoute(nameof(CancelOrderPage), typeof(CancelOrderPage));
        Routing.RegisterRoute(nameof(PaymentCardPage), typeof(PaymentCardPage));
        Routing.RegisterRoute(nameof(PaymentPage), typeof(PaymentPage));
        Routing.RegisterRoute(nameof(OrdersPage), typeof(OrdersPage));
        Routing.RegisterRoute(nameof(MyTariffPage), typeof(MyTariffPage));
        Routing.RegisterRoute(nameof(TariffsPage), typeof(TariffsPage));
        Routing.RegisterRoute(nameof(DeleteAccountPage), typeof(DeleteAccountPage));
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

/*
1. After registration was successed then log in aoutomaticly and navigate to main page. Also, phone number is not passed to RegisterPage from AuthorizationPage.
2. Savatcha page is showing some tools even though there is no products in the cart. It should be hidden when there is no products in the cart. Instead of that, display some message for user friendly.
3. In the "Maxsulotlar Sharhlari" page, it supposed to display "Sharh qoldirish uchun ushbu mahsulotni kamida bir marta xarid qilgan bo'lishingiz kerak". Please check and fix. If the user did not buy this product then that message should display.
4. Savol yuborish button must clickable in "Savol berish" page. If text is emopty then it should display "Iltimos, savolni kiriting" message.
5. When I press the "Buyurtmani rasmiylashtirish" button, it is creating an order in the orders table and it opens payme page. So, it is ok, but, if I did not implement and go back to the "Rasmiylashtirish" page orders table still saving that order. What do you think should I delete it if payment is not implemented?
6. Please complete the ForgotPasswordPage. I should be able to send a temporary password to the user. Please implement it. when this page is appearing phone number should display. I mean user should not rnter again.
7. SmsCodePopup is working but, when time is up, then user had to press "Send again" text to send again. However, when I press the "Tasdiqlash" button it could send even time is up. Do something better.
8. When SmsCodePopup is  showed, user is not able to close it. Please do something to close it. I mean, user should be able to close it.
9. Add something user friendly that when user navigate to thr "Sevimlilar" page and there is no products in the list, it should display some message for user friendly.
*/