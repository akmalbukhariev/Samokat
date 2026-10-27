using Ninimum.Services;

namespace Ninimum;

public partial class AppShell : Shell
{
	private bool _firstTabAppearing = true;
	public AppShell()
	{
		InitializeComponent();
        Loaded += (_, _) => AppService.GetRequired<CartBadgeService>().RequestRefresh();
        Navigated += (_, _) => AppService.GetRequired<CartBadgeService>().RequestRefresh();
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
