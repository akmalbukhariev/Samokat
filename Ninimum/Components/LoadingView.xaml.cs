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
	}
     
	public void ChangeColor(Color color)
	{
		loading.Color = color;
	}

	private static void OnShowLoadingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (LoadingView)bindable;
        bool isLoading = (bool)newValue;

        // Spinner
        control.loading.IsRunning = isLoading;

        // Overlay content
        control.overlayLoading.IsVisible = isLoading;

        // Whole control behavior
        control.IsVisible = isLoading;
        control.InputTransparent = !isLoading; // when loading = true → block touches
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
	{
	}
}