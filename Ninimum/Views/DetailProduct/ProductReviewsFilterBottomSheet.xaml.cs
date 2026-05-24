namespace Ninimum.Views.DetailProduct;

public partial class ProductReviewsFilterBottomSheet : ContentView
{
    private double _panStartY;

    public ProductReviewsFilterBottomSheet()
    {
        InitializeComponent();

        var backdropTapGesture = new TapGestureRecognizer();
        backdropTapGesture.Tapped += async (_, __) => await HideAsync();
        Backdrop.GestureRecognizers.Add(backdropTapGesture);

        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;

        // Use the whole sheet for dragging
        SheetContainer.GestureRecognizers.Add(panGesture);
    }

    public async Task ShowAsync()
    {
        if (IsVisible)
            return;

        IsVisible = true;
        Backdrop.Opacity = 0;
        SheetContainer.TranslationY = 700;

        await Task.WhenAll(
            Backdrop.FadeTo(1, 220, Easing.CubicOut),
            SheetContainer.TranslateTo(0, 0, 280, Easing.CubicOut)
        );
    }

    public async Task HideAsync()
    {
        if (!IsVisible)
            return;

        await Task.WhenAll(
            Backdrop.FadeTo(0, 180, Easing.CubicIn),
            SheetContainer.TranslateTo(0, 700, 220, Easing.CubicIn)
        );

        IsVisible = false;
    }

    private void SheetContainer_Tapped(object sender, TappedEventArgs e)
    {
        // absorb tap so it does not close through Backdrop
    }

    private async void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panStartY = SheetContainer.TranslationY;
                break;

            case GestureStatus.Running:
                if (e.TotalY > 0)
                {
                    var newY = _panStartY + e.TotalY;
                    SheetContainer.TranslationY = Math.Max(0, newY);
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (SheetContainer.TranslationY > 120)
                    await HideAsync();
                else
                    await SheetContainer.TranslateTo(0, 0, 180, Easing.CubicOut);
                break;
        }
    }
}