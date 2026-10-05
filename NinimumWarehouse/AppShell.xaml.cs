using NinimumWarehouse.Services;
using NinimumWarehouse.Views;
namespace NinimumWarehouse;
public partial class AppShell : Shell
{
    public AppShell(WarehouseApi api, bool showProfile = false)
    {
        InitializeComponent();
        QueueContent.ContentTemplate = new DataTemplate(() => new OrdersPage(api, AppConstants.QueueView));
        MineContent.ContentTemplate = new DataTemplate(() => new OrdersPage(api, AppConstants.MineView));
        HistoryContent.ContentTemplate = new DataTemplate(() => new OrdersPage(api, AppConstants.HistoryView));
        ProfileContent.ContentTemplate = new DataTemplate(() => new ProfilePage(api));
        if (showProfile) MainTabs.CurrentItem = ProfileTab;
    }
    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);
        Dispatcher.Dispatch(() => CurrentPage?.InvalidateMeasure());
    }
}
