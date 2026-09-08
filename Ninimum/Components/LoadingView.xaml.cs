using System.ComponentModel;
using Ninimum.Services;

namespace Ninimum.Components;

public partial class LoadingView : ContentView
{
    public static readonly BindableProperty ShowLoadingProperty =
        BindableProperty.Create(
            nameof(ShowLoading),
            typeof(bool),
            typeof(LoadingView),
            false,
            propertyChanged: OnShowLoadingChanged);

    private ConnectionMonitorService? connectionMonitor;
    private bool monitorAttached;

    public bool ShowLoading
    {
        get => (bool)GetValue(ShowLoadingProperty);
        set => SetValue(ShowLoadingProperty, value);
    }

    public LoadingView()
    {
        InitializeComponent();

        IsVisible = false;
        InputTransparent = true;
        loading.IsRunning = false;
        overlayLoading.IsVisible = false;

        connectionMonitor = AppService.Get<ConnectionMonitorService>();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (Parent != null)
            AttachMonitor();
        else
            DetachMonitor();

        UpdateLoadingState();
    }

    public void ChangeColor(Color color)
    {
        loading.Color = color;
    }

    private static void OnShowLoadingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((LoadingView)bindable).UpdateLoadingState();
    }

    private void AttachMonitor()
    {
        if (monitorAttached || connectionMonitor == null)
            return;

        connectionMonitor.PropertyChanged += OnConnectionMonitorPropertyChanged;
        monitorAttached = true;
    }

    private void DetachMonitor()
    {
        if (!monitorAttached || connectionMonitor == null)
            return;

        connectionMonitor.PropertyChanged -= OnConnectionMonitorPropertyChanged;
        monitorAttached = false;
    }

    private void OnConnectionMonitorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ConnectionMonitorService.IsBannerVisible)
            or nameof(ConnectionMonitorService.State))
        {
            if (MainThread.IsMainThread)
                UpdateLoadingState();
            else
                MainThread.BeginInvokeOnMainThread(UpdateLoadingState);
        }
    }

    private void UpdateLoadingState()
    {
        // A full-screen spinner and a connection problem banner at the same time are
        // confusing. While disconnected/reconnecting, the compact global connection
        // banner becomes the only waiting state. The original ShowLoading value is
        // kept, so normal loading can continue automatically when appropriate.
        bool connectionProblemVisible = connectionMonitor?.IsBannerVisible == true;
        bool shouldShow = ShowLoading && !connectionProblemVisible;

        loading.IsRunning = shouldShow;
        overlayLoading.IsVisible = shouldShow;
        IsVisible = shouldShow;
        InputTransparent = !shouldShow;
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
    }
}
