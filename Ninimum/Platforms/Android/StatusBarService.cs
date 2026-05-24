using Ninimum.Platforms.Android;
using Color = Android.Graphics.Color;
using AndroidX.Core.View;
using Ninimum.Services.Interface;

[assembly: Dependency(typeof(StatusBarService))]
namespace Ninimum.Platforms.Android
{
    public class StatusBarService : IStatusBarService
    {
        public void SetStatusBarColor(string hexColor, bool darkStatusBarTint)
        {
            var activity = Platform.CurrentActivity;
            if (activity == null) return;

            var window = activity.Window;
            window.SetStatusBarColor(Color.ParseColor(hexColor));

            // Modern way using AndroidX
            var decorView = window.DecorView;
            var insetsController = ViewCompat.GetWindowInsetsController(decorView);

            if (insetsController != null)
            {
                insetsController.AppearanceLightStatusBars = darkStatusBarTint;
            }
        }
    }
}
