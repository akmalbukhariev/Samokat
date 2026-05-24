namespace Ninimum.Views.Search;

public partial class SearchFilterBottomSheet : ContentView
{
    private double _panStartY;

    public SearchFilterBottomSheet()
    {
        InitializeComponent();

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (_, __) => await HideAsync();
        Backdrop.GestureRecognizers.Add(tapGesture);

        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;

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

    private async void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panStartY = SheetContainer.TranslationY;
                break;

            case GestureStatus.Running:
                if (e.TotalY > 0)
                    SheetContainer.TranslationY = _panStartY + e.TotalY;
                break;

            case GestureStatus.Completed:
                if (SheetContainer.TranslationY > 140)
                    await HideAsync();
                else
                    await SheetContainer.TranslateTo(0, 0, 180, Easing.CubicOut);
                break;
        }
    }
}