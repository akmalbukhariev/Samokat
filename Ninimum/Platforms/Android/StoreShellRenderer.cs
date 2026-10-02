using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Ninimum.Services;

namespace Ninimum.Platforms.Android;

public sealed class StoreShellRenderer : ShellRenderer
{
    protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
        => new StoreTabAppearance(this, shellItem);
}

internal sealed class StoreTabAppearance : ShellBottomNavViewAppearanceTracker
{
    private readonly ShellItem shellItem;
    private readonly CartBadgeService badge;
    private BottomNavigationView? bottomView;
    private bool disposed;
    public StoreTabAppearance(IShellContext context, ShellItem item) : base(context, item)
    {
        shellItem = item;
        badge = AppService.GetRequired<CartBadgeService>();
        badge.Changed += UpdateBadge;
    }
    public override void SetAppearance(BottomNavigationView view, IShellAppearanceElement appearance)
    {
        base.SetAppearance(view, appearance);
        bottomView = view;
        Apply();
    }
    public override void ResetAppearance(BottomNavigationView view)
    {
        base.ResetAppearance(view);
        bottomView = view;
        Apply();
    }
    private void Apply()
    {
        if (disposed || bottomView == null) return;
        // Material's unlabeled mode hides titles but retains them for accessibility.
        bottomView.LabelVisibilityMode = 2;
        bottomView.ItemHorizontalTranslationEnabled = false;
        UpdateBadge();
    }
    private void UpdateBadge()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (disposed || bottomView == null) return;
            int index = shellItem.Items.ToList().FindIndex(x => Routing.GetRoute(x) == "CartTab");
            if (index < 0 || index >= bottomView.Menu.Size()) return;
            int id = bottomView.Menu.GetItem(index)!.ItemId;
            if (badge.Count == 0) { bottomView.RemoveBadge(id); return; }
            var drawable = bottomView.GetOrCreateBadge(id);
            drawable.Number = badge.Count;
            drawable.MaxCharacterCount = 3;
            drawable.BackgroundColor = global::Android.Graphics.Color.Rgb(72, 107, 255).ToArgb();
            drawable.BadgeTextColor = global::Android.Graphics.Color.White.ToArgb();
            drawable.SetVisible(true, false);
        });
    }
    protected override void Dispose(bool disposing)
    {
        disposed = true;
        if (disposing) { badge.Changed -= UpdateBadge; bottomView = null; }
        base.Dispose(disposing);
    }
}
