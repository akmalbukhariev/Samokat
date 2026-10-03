using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using UIKit;

namespace NinimumDelivery.Platforms.iOS;

public sealed class DeliveryShellRenderer : ShellRenderer
{
    protected override IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker()
        => new DeliveryTabAppearance();
}

internal sealed class DeliveryTabAppearance : ShellTabBarAppearanceTracker
{
    public override void SetAppearance(UITabBarController controller, ShellAppearance appearance)
    {
        base.SetAppearance(controller, appearance);
        Apply(controller);
    }

    public override void UpdateLayout(UITabBarController controller)
    {
        base.UpdateLayout(controller);
        Apply(controller);
    }

    private static void Apply(UITabBarController controller)
    {
        var tabs = Shell.Current?.CurrentItem?.Items;
        if (tabs == null || controller.TabBar.Items is not { } items) return;
        for (int i = 0; i < items.Length && i < tabs.Count; i++)
        {
            items[i].AccessibilityLabel = tabs[i].Title;
            items[i].Title = null;
            items[i].ImageInsets = new UIEdgeInsets(6, 0, -6, 0);
        }
    }
}
