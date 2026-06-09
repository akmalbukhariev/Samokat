namespace Ninimum.Components;

public partial class DescriptionPopupView : ContentView
{
    private const double ClosedY = 600;
    private const double CloseThreshold = 120;

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(DescriptionPopupView),
            "Tavsiflar");

    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(
            nameof(Description),
            typeof(string),
            typeof(DescriptionPopupView),
            string.Empty);

    private bool _isAnimating;

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public DescriptionPopupView()
    {
        InitializeComponent();
    }

    public async Task ShowAsync()
    {
        if (_isAnimating)
            return;

        _isAnimating = true;

        IsVisible = true;
        PopupBody.TranslationY = ClosedY;

        await PopupBody.TranslateTo(0, 0, 250, Easing.CubicOut);

        _isAnimating = false;
    }

    public async Task CloseAsync()
    {
        if (_isAnimating || !IsVisible)
            return;

        _isAnimating = true;

        await PopupBody.TranslateTo(0, ClosedY, 200, Easing.CubicIn);

        IsVisible = false;

        _isAnimating = false;
    }

    private async void Close_Tapped(object sender, TappedEventArgs e)
    {
        await CloseAsync();
    }

    private async void Overlay_Tapped(object sender, TappedEventArgs e)
    {
        await CloseAsync();
    }

    private async void PopupBody_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (_isAnimating)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Running:
                if (e.TotalY > 0)
                    PopupBody.TranslationY = e.TotalY;
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (PopupBody.TranslationY > CloseThreshold)
                {
                    await CloseAsync();
                }
                else
                {
                    await PopupBody.TranslateTo(0, 0, 150, Easing.CubicOut);
                }
                break;
        }
    }
}