using System.Runtime.CompilerServices;

using Microsoft.Maui.Controls.PlatformConfiguration;
using Ninimum.Components;
using Ninimum.Services;
using Ninimum.Services.Interface;

namespace Ninimum.Views
{
    public abstract class BasePage : ContentPage
    {
        protected CancellationTokenSource? cts;
        protected AppControl appControl;
        protected IStatusBarService statusBarService;
        private bool connectionStatusAttached;

        protected BasePage()
        {
            BackgroundColor = Colors.White;
            Shell.SetNavBarIsVisible(this, false);
            Shell.SetTabBarIsVisible(this, false);

            statusBarService = AppService.Get<IStatusBarService>();
            statusBarService.SetStatusBarColor(Colors.White.ToArgbHex(), false);

            // Attach the global connection banner from Loaded rather than relying only
            // on OnAppearing. Some pages override OnAppearing and may forget to call
            // base.OnAppearing(), but every BasePage still needs the connection state.
            Loaded += OnBasePageLoaded;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            EnsureConnectionStatusView();
        }

        private void OnBasePageLoaded(object? sender, EventArgs e)
        {
            EnsureConnectionStatusView();
        }

        private void EnsureConnectionStatusView()
        {
            if (connectionStatusAttached || Content == null)
                return;

            var monitor = AppService.Get<ConnectionMonitorService>();
            if (monitor == null)
                return;

            var pageContent = Content;
            Content = null;

            var root = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Star)
                }
            };

            var connectionStatus = new ConnectionStatusView
            {
                BindingContext = monitor
            };

            // Keep the current page visible while the server is unavailable, but do not
            // allow controls underneath to receive taps, swipes, pull-to-refresh or other
            // gestures. This avoids misleading empty-state actions and repeated API calls.
            // The global ConnectionStatusView above remains the single explanation of why
            // the page is temporarily unavailable.
            var contentHost = new Grid();
            contentHost.Children.Add(pageContent);

            var connectionInteractionBlocker = new Grid
            {
                BackgroundColor = new Color(0.96f, 0.96f, 0.96f, 0.42f),
                InputTransparent = false,
                ZIndex = 10000
            };

            // A visible background makes this element participate in hit testing on every
            // platform. The no-op recognizer is intentional: touches stop here instead of
            // reaching RefreshView, CollectionView, SwipeView, buttons or page gestures.
            connectionInteractionBlocker.GestureRecognizers.Add(new TapGestureRecognizer());
            connectionInteractionBlocker.SetBinding(
                IsVisibleProperty,
                new Binding(nameof(ConnectionMonitorService.IsBannerVisible), source: monitor));

            contentHost.Children.Add(connectionInteractionBlocker);

            Grid.SetRow(connectionStatus, 0);
            Grid.SetRow(contentHost, 1);
            root.Children.Add(connectionStatus);
            root.Children.Add(contentHost);

            Content = root;
            connectionStatusAttached = true;
            monitor.Start();
        }

        protected void CancelAndDisposeCts()
        {
            try { cts?.Cancel(); }
            catch { }
            finally { cts?.Dispose(); cts = null; }
        }
 
        protected Task AnimateElementScaleUp(VisualElement element)
        {
            return Task.Run(async () =>
            {
                await element.ScaleTo(1.3, 100, Easing.CubicOut);
                await element.ScaleTo(1.0, 100, Easing.CubicIn);
            });
        }

        protected Task AnimateElementScaleDown(VisualElement element)
        {
            return Task.Run(async () =>
            {
                await element.ScaleTo(0.9, 100, Easing.CubicOut);
                await element.ScaleTo(1.0, 100, Easing.CubicIn);
            });
        }

        private bool isAnimating;
        protected async Task AnimateSelectAllBarAsync(HorizontalStackLayout selectAllBar, bool show)
        {
            if (isAnimating) return;
            isAnimating = true;

            const uint duration = 220;
            var easing = Easing.SinOut;

            // Cancel any in-flight animations just in case
            selectAllBar.AbortAnimation("TranslationX");
            selectAllBar.AbortAnimation("FadeTo");

            if (show)
            {
                selectAllBar.IsVisible = true;
                selectAllBar.TranslationX = -60; // start a bit left
                selectAllBar.Opacity = 0;

                var fadeIn = selectAllBar.FadeTo(1, duration, easing);
                var slideIn = selectAllBar.TranslateTo(0, 0, duration, easing);

                await Task.WhenAll(fadeIn, slideIn);
            }
            else
            {
                // slide/fade out to the left, then hide
                var fadeOut = selectAllBar.FadeTo(0, duration, easing);
                var slideOut = selectAllBar.TranslateTo(-60, 0, duration, easing);

                await Task.WhenAll(fadeOut, slideOut);
                selectAllBar.IsVisible = false;
            }

            isAnimating = false;
        }

        protected async Task Back()
        {
            await AppNavigatorService.NavigateTo("..");
        }
    }
}
