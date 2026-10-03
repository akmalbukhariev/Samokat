namespace NinimumDelivery;
public partial class AppShell : Shell
{
    public AppShell() => InitializeComponent();

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        // Re-measure after Shell has restored the tab bar and the visible page.
        // This clears the stale bottom inset left by pages with a hidden tab bar.
        Dispatcher.Dispatch(() =>
        {
            if (CurrentPage is ContentPage page)
                page.Content?.InvalidateMeasure();
            CurrentPage?.InvalidateMeasure();
            InvalidateMeasure();
        });
    }

}
