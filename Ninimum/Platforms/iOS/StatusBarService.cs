using Samokat.Platforms.iOS;
using UIKit;
using CoreGraphics;
using Samokat.Services.Interface;

[assembly: Dependency(typeof(StatusBarService))]
namespace Samokat.Platforms.iOS
{
    public class StatusBarService : IStatusBarService
    {
        UIView? _statusView;

        public void SetStatusBarColor(string hexColor, bool darkStatusBarTint)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var app = UIApplication.SharedApplication;

                // 1) Get key window
                var window = app.ConnectedScenes
                    .OfType<UIWindowScene>()
                    .SelectMany(s => s.Windows)
                    .FirstOrDefault(w => w.IsKeyWindow);

                if (window == null)
                    return;

                // make sure layout is up to date
                window.LayoutIfNeeded();

                // 2) Get status bar height
                nfloat topInset = window.SafeAreaInsets.Top;
                if (topInset <= 0)
                {
                    var statusFrame = window.WindowScene?.StatusBarManager?.StatusBarFrame
                                      ?? app.StatusBarFrame;
                    topInset = statusFrame.Height;
                }

                // 🔑 If still 0, we’re too early → retry once a bit later
                if (topInset <= 0)
                {
                    await Task.Delay(80);   // 50–100 ms is usually enough
                    SetStatusBarColor(hexColor, darkStatusBarTint);
                    return;
                }

                var frame = new CGRect(0, 0, window.Bounds.Width, topInset);

                if (_statusView == null)
                {
                    _statusView = new UIView(frame)
                    {
                        AutoresizingMask = UIViewAutoresizing.FlexibleWidth |
                                           UIViewAutoresizing.FlexibleBottomMargin
                    };
                    window.AddSubview(_statusView);
                }
                else
                {
                    _statusView.Frame = frame;
                }

                // keep it on top of everything
                window.BringSubviewToFront(_statusView);

                var mauiColor = Color.FromArgb(hexColor);
                _statusView.BackgroundColor = UIColor.FromRGBA(
                    (nfloat)mauiColor.Red,
                    (nfloat)mauiColor.Green,
                    (nfloat)mauiColor.Blue,
                    (nfloat)mauiColor.Alpha);

                // status bar icons color
                if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                {
                    app.StatusBarStyle = darkStatusBarTint
                        ? UIStatusBarStyle.DarkContent
                        : UIStatusBarStyle.LightContent;
                }
                else
                {
                    app.StatusBarStyle = darkStatusBarTint
                        ? UIStatusBarStyle.Default
                        : UIStatusBarStyle.LightContent;
                }
            });
        }
    }
}
