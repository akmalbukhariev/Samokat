using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Behaviors;
using Microsoft.Maui.Controls.Shapes;
using Ninimum.Services;

namespace Ninimum.Components;

/// <summary>
/// The app's own bottom tab bar. Android's Material bar allows only 5 tabs (more go under "More"),
/// and the app needs 6 (Bosh sahifa, Katalog, Qidiruv, Sevimli, Savatcha, Profil), so the Shell bar
/// stays hidden and each tab root page gets this bar under its content.
/// </summary>
public sealed class AppTabBar : ContentView
{
    public enum TabKind { Home, Catalog, Search, Favorites, Cart, Profile }

    // Shell routes of the tabs in AppShell.xaml; Search opens the pushed SearchPage instead of a tab.
    private static readonly (TabKind Kind, string Icon, string? Route)[] Tabs =
    {
        (TabKind.Home, "ic_home.png", "HomeTab"),
        (TabKind.Catalog, "ic_categories.png", "CategoryTab"),
        (TabKind.Search, "ic_serach.png", null),
        (TabKind.Favorites, "ic_favorite.png", "FavoritesTab"),
        (TabKind.Cart, "ic_basket.png", "CartTab"),
        (TabKind.Profile, "ic_user.png", "ProfileTab"),
    };

    private static readonly ConditionalWeakTable<Page, AppTabBar> bars = new();
    private static bool openingSearch;

    private readonly Border badge;
    private readonly Label badgeText;
    private readonly CartBadgeService? cartBadge;

    private AppTabBar(TabKind selected)
    {
        BackgroundColor = Colors.White;
        var grid = new Grid { HeightRequest = 56 };
        foreach (var _ in Tabs) grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        badgeText = new Label { TextColor = Colors.White, FontSize = 10, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
        badge = new Border
        {
            Content = badgeText,
            BackgroundColor = Color.FromRgb(72, 107, 255),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 9 },
            HeightRequest = 18,
            MinimumWidthRequest = 18,
            Padding = new Thickness(4, 0),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TranslationX = 14,
            TranslationY = -11,
            InputTransparent = true,
            IsVisible = false,
        };

        for (int i = 0; i < Tabs.Length; i++)
        {
            var tab = Tabs[i];
            var icon = new Image { Source = tab.Icon, WidthRequest = 24, HeightRequest = 24, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            icon.Behaviors.Add(new IconTintColorBehavior { TintColor = tab.Kind == selected ? Colors.Red : Colors.Gray });
            var cell = new Grid { BackgroundColor = Colors.Transparent };
            cell.Add(icon);
            if (tab.Kind == TabKind.Cart) cell.Add(badge);
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => Open(tab.Kind, tab.Route, selected);
            cell.GestureRecognizers.Add(tap);
            grid.Add(cell, i, 0);
        }

        Content = new VerticalStackLayout
        {
            Children = { new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0") }, grid }
        };

        cartBadge = AppService.Get<CartBadgeService>();
        if (cartBadge != null)
        {
            cartBadge.Changed += UpdateBadge;
            UpdateBadge();
        }
    }

    /// <summary>Puts the tab bar under the page content and hides the Shell bar on that page.</summary>
    public static void Attach(ContentPage page, TabKind selected)
    {
        Shell.SetTabBarIsVisible(page, false);
        var bar = new AppTabBar(selected);
        var content = page.Content;
        page.Content = null;
        var root = new Grid { RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
        if (content != null) root.Add(content, 0, 0);
        root.Add(bar, 0, 1);
        page.Content = root;
        bars.AddOrUpdate(page, bar);
    }

    /// <summary>Shows or hides the bar of a page that has one; false when the page has no bar.</summary>
    public static bool SetVisible(Page page, bool show)
    {
        if (!bars.TryGetValue(page, out var bar)) return false;
        bar.IsVisible = show;
        return true;
    }

    private static void Open(TabKind kind, string? route, TabKind current)
    {
        if (kind == current) return;
        if (route == null)
        {
            if (openingSearch) return;
            openingSearch = true;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    AppVibrationService.Click();
                    await AppNavigatorService.NavigateTo(nameof(Views.Search.SearchPage));
                }
                finally { openingSearch = false; }
            });
            return;
        }
        var item = Shell.Current?.CurrentItem;
        var section = item?.Items.FirstOrDefault(s => Routing.GetRoute(s) == route);
        if (item != null && section != null) item.CurrentItem = section;
    }

    private void UpdateBadge()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            int count = cartBadge?.Count ?? 0;
            badge.IsVisible = count > 0;
            badgeText.Text = count > 99 ? "99+" : count.ToString();
        });
    }
}
