using Ninimum.Services;

namespace Ninimum;

public partial class AppShell : Shell
{
	private bool _firstTabAppearing = true;
	public AppShell()
	{
		InitializeComponent();
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
