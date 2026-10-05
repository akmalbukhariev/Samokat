using Android.App;
using Android.Content.PM;
using Android.OS;

namespace NinimumWarehouse;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        AndroidX.AppCompat.App.AppCompatDelegate.DefaultNightMode =
            AndroidX.AppCompat.App.AppCompatDelegate.ModeNightNo;
        base.OnCreate(savedInstanceState);
        if (Window is not null)
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(35))
            {
                Window.SetStatusBarColor(Android.Graphics.Color.White);
                Window.SetNavigationBarColor(Android.Graphics.Color.White);
            }
            var controller = AndroidX.Core.View.WindowCompat.GetInsetsController(Window, Window.DecorView);
            if(controller is not null)
            {
                controller.AppearanceLightStatusBars = true;
                controller.AppearanceLightNavigationBars = true;
            }
        }
    }
}
