using Ninimum.Services;

namespace Ninimum.Components;

public partial class ConnectionStatusView : ContentView
{
    public ConnectionStatusView()
    {
        InitializeComponent();
    }

    private void OnRetryTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is ConnectionMonitorService monitor)
            monitor.Retry();
    }
}
