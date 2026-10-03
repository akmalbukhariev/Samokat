using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace NinimumDelivery.Platforms.Android;

public sealed class DeliveryShellRenderer : ShellRenderer
{
    protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
        => new DeliveryTabAppearance(this, shellItem);
}

internal sealed class DeliveryTabAppearance : ShellBottomNavViewAppearanceTracker
{
    public DeliveryTabAppearance(IShellContext context, ShellItem item) : base(context, item) { }

    public override void SetAppearance(BottomNavigationView view, IShellAppearanceElement appearance)
    {
        base.SetAppearance(view, appearance);
        Apply(view);
    }

    public override void ResetAppearance(BottomNavigationView view)
    {
        base.ResetAppearance(view);
        Apply(view);
    }

    private static void Apply(BottomNavigationView view)
    {
        // Keep the localized titles for accessibility, but display icons only.
        view.LabelVisibilityMode = 2;
        view.ItemHorizontalTranslationEnabled = false;
    }
}
