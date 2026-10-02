using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Ninimum.Services;
using UIKit;

namespace Ninimum.Platforms.iOS;

public sealed class StoreShellRenderer : ShellRenderer
{
    protected override IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker()
        => new StoreTabAppearance();
}

internal sealed class StoreTabAppearance : ShellTabBarAppearanceTracker
{
    private readonly CartBadgeService badge = AppService.GetRequired<CartBadgeService>();
    private UITabBarController? controller;
    private bool disposed;
    public StoreTabAppearance() { badge.Changed += UpdateTabs; }
    public override void SetAppearance(UITabBarController controller, ShellAppearance appearance)
    {
        base.SetAppearance(controller, appearance);
        this.controller = controller;
        UpdateTabs();
    }
    public override void UpdateLayout(UITabBarController controller)
    {
        base.UpdateLayout(controller);
        this.controller = controller;
        UpdateTabs();
    }
    private void UpdateTabs()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (disposed || controller?.TabBar.Items is not { } items) return;
            var tabs = Shell.Current?.CurrentItem?.Items;
            if (tabs == null) return;
            for (int i = 0; i < items.Length && i < tabs.Count; i++)
            {
                var item = items[i];
                item.AccessibilityLabel = tabs[i].Title;
                item.Title = null;
                item.ImageInsets = new UIEdgeInsets(6, 0, -6, 0);
                if (Routing.GetRoute(tabs[i]) == "CartTab")
                {
                    item.BadgeValue = badge.Count == 0 ? null : badge.Count > 99 ? "99+" : badge.Count.ToString();
                    item.BadgeColor = UIColor.FromRGB(72, 107, 255);
                }
            }
        });
    }
    protected override void Dispose(bool disposing)
    {
        disposed = true;
        if (disposing) { badge.Changed -= UpdateTabs; controller = null; }
        base.Dispose(disposing);
    }
}
