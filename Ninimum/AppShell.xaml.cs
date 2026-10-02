using Ninimum.Services;

namespace Ninimum;

public partial class AppShell : Shell
{
	private bool _firstTabAppearing = true;
    private bool openingSearch;
	public AppShell()
	{
		InitializeComponent();
        Loaded += (_, _) => AppService.GetRequired<CartBadgeService>().RequestRefresh();
        Navigated += (_, _) => AppService.GetRequired<CartBadgeService>().RequestRefresh();
	}
    
    protected override void OnNavigating(ShellNavigatingEventArgs args)
    {
        // Search is a tab-bar shortcut to the same pushed page used by Home.
        // Cancel the tab switch so Back returns to whichever tab opened Search.
        bool searchTab = args.Source == ShellNavigationSource.ShellSectionChanged &&
            args.Target.Location.OriginalString.Split('/').Contains("SearchTab");
        if (searchTab && args.CanCancel)
        {
            args.Cancel();
            if (!openingSearch)
            {
                openingSearch = true;
                Dispatcher.Dispatch(async () =>
                {
                    try
                    {
                        AppVibrationService.Click();
                        await AppNavigatorService.NavigateTo(nameof(Views.Search.SearchPage));
                    }
                    finally { openingSearch = false; }
                });
            }
            return;
        }
        base.OnNavigating(args);
    }

	private void Tab_Appearing(object sender, EventArgs e)
	{
		if (_firstTabAppearing)
		{
			_firstTabAppearing = false;
			return;
		}

		AppVibrationService.Click();
	}
}
