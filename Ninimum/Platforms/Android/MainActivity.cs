using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace Ninimum;

// Payme sends the customer back with ninimum://payment-result after paying in the Payme app.
[IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataScheme = "ninimum", DataHost = "payment-result")]
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetUpPush();
        // A tap on a notification can start the app: hand that first intent to Firebase too.
        Ninimum.Services.PushRegistration.ListenForTaps();
        Plugin.Firebase.CloudMessaging.FirebaseCloudMessagingImplementation.OnNewIntent(Intent);

        //Window?.SetSoftInputMode(SoftInput.AdjustPan);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        Plugin.Firebase.CloudMessaging.FirebaseCloudMessagingImplementation.OnNewIntent(intent);
        if (intent?.Data?.Scheme == "ninimum" && intent.Data.Host == "payment-result")
            Ninimum.Services.PaymentReturn.Notify();
    }

    // Push notifications: one high-importance channel, a white bell as the small icon.
    private void SetUpPush()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            var channel = new Android.App.NotificationChannel("ninimum_orders", "Buyurtmalar", Android.App.NotificationImportance.High);
            channel.EnableVibration(true);
            ((Android.App.NotificationManager)GetSystemService(NotificationService)!).CreateNotificationChannel(channel);
        }
        Plugin.Firebase.CloudMessaging.FirebaseCloudMessagingImplementation.ChannelId = "ninimum_orders";
        Plugin.Firebase.CloudMessaging.FirebaseCloudMessagingImplementation.SmallIconRef = Resource.Drawable.ic_push;
    }
}
