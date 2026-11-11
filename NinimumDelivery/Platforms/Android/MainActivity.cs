using Android.App;
using Android.Content.PM;
using Android.OS;

namespace NinimumDelivery;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges =
        ConfigChanges.ScreenSize |
        ConfigChanges.Orientation |
        ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout |
        ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        SetTheme(Resource.Style.Delivery_MainTheme);
        base.OnCreate(savedInstanceState);
        // Courier GPS V1c: on a company phone set up as device owner, keep the app installed and GPS on.
        NinimumDelivery.Platforms.Android.DeviceLock.Apply(this);
        SetUpPush();
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

    protected override void OnNewIntent(Android.Content.Intent? intent)
    {
        base.OnNewIntent(intent);
        Plugin.Firebase.CloudMessaging.FirebaseCloudMessagingImplementation.OnNewIntent(intent);
    }
}
