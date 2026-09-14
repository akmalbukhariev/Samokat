using Microsoft.Maui.Controls;

namespace Ninimum.Components;

public partial class ImagePreviewView : ContentView
{
    private const double MinScale = 1.0;
    private const double MaxScale = 4.0;
    private const double DoubleTapScale = 2.5;

    private bool _isAnimating;
    private double _pinchStartScale = MinScale;
    private double _panStartX;
    private double _panStartY;

    public ImagePreviewView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty PreviewImageSourceProperty =
        BindableProperty.Create(
            nameof(PreviewImageSource),
            typeof(ImageSource),
            typeof(ImagePreviewView),
            default(ImageSource));

    public ImageSource PreviewImageSource
    {
        get => (ImageSource)GetValue(PreviewImageSourceProperty);
        set => SetValue(PreviewImageSourceProperty, value);
    }

    public async Task ShowAsync(ImageSource imageSource)
    {
        if (_isAnimating || imageSource == null)
            return;

        _isAnimating = true;
        PreviewImageSource = imageSource;
        ResetZoom();

        InputTransparent = false;

        fullImage.TranslationY = -100;
        fullImage.Opacity = 0;
        fullImage.IsVisible = true;

        boxFullImage.IsVisible = true;
        boxFullImage.Opacity = 0.5;
        boxFullImage.InputTransparent = false;

        await Task.WhenAll(
            fullImage.TranslateTo(0, 0, 250, Easing.SinIn),
            fullImage.FadeTo(1, 250, Easing.SinIn));

        _isAnimating = false;
    }

    public async Task CloseAsync(bool swipeDown = true)
    {
        if (_isAnimating || !fullImage.IsVisible)
            return;

        _isAnimating = true;
        ResetZoom();

        double targetY = swipeDown ? 100 : -100;

        await Task.WhenAll(
            fullImage.TranslateTo(0, targetY, 250, Easing.SinOut),
            fullImage.FadeTo(0, 250, Easing.SinOut));

        FinishClosing();
        _isAnimating = false;
    }

    private void OnImagePinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (_isAnimating)
            return;

        switch (e.Status)
        {
            case GestureStatus.Started:
                _pinchStartScale = fullImage.Scale;
                break;

            case GestureStatus.Running:
                fullImage.Scale = Math.Clamp(_pinchStartScale * e.Scale, MinScale, MaxScale);

                if (fullImage.Scale <= MinScale + 0.01)
                {
                    fullImage.TranslationX = 0;
                    fullImage.TranslationY = 0;
                }
                else
                {
                    ClampImageTranslation();
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (fullImage.Scale < 1.05)
                    ResetZoom();
                else
                    ClampImageTranslation();
                break;
        }
    }

    private void OnImagePanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (_isAnimating || fullImage.Scale <= MinScale + 0.01)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panStartX = fullImage.TranslationX;
                _panStartY = fullImage.TranslationY;
                break;

            case GestureStatus.Running:
                fullImage.TranslationX = _panStartX + e.TotalX;
                fullImage.TranslationY = _panStartY + e.TotalY;
                ClampImageTranslation();
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                ClampImageTranslation();
                break;
        }
    }

    private async void OnImageDoubleTapped(object sender, TappedEventArgs e)
    {
        if (_isAnimating)
            return;

        _isAnimating = true;

        if (fullImage.Scale > MinScale + 0.01)
        {
            await Task.WhenAll(
                fullImage.ScaleTo(MinScale, 180, Easing.CubicOut),
                fullImage.TranslateTo(0, 0, 180, Easing.CubicOut));
        }
        else
        {
            await fullImage.ScaleTo(DoubleTapScale, 180, Easing.CubicOut);
            ClampImageTranslation();
        }

        _isAnimating = false;
    }

    private async void OnImageSwiped(object sender, SwipedEventArgs e)
    {
        // While zoomed, vertical gestures are used to inspect/pan the image.
        if (fullImage.Scale > MinScale + 0.01)
            return;

        if (e.Direction == SwipeDirection.Down)
            await CloseAsync(true);
        else if (e.Direction == SwipeDirection.Up)
            await CloseAsync(false);
    }

    private async void OnOverlayTapped(object sender, TappedEventArgs e)
    {
        await CloseWithoutAnimationAsync();
    }

    private Task CloseWithoutAnimationAsync()
    {
        if (_isAnimating)
            return Task.CompletedTask;

        ResetZoom();
        FinishClosing();
        return Task.CompletedTask;
    }

    private void ClampImageTranslation()
    {
        if (fullImage.Scale <= MinScale + 0.01)
        {
            fullImage.TranslationX = 0;
            fullImage.TranslationY = 0;
            return;
        }

        double maxX = Math.Max(0, fullImage.Width * (fullImage.Scale - 1) / 2);
        double maxY = Math.Max(0, fullImage.Height * (fullImage.Scale - 1) / 2);

        fullImage.TranslationX = Math.Clamp(fullImage.TranslationX, -maxX, maxX);
        fullImage.TranslationY = Math.Clamp(fullImage.TranslationY, -maxY, maxY);
    }

    private void ResetZoom()
    {
        _pinchStartScale = MinScale;
        _panStartX = 0;
        _panStartY = 0;
        fullImage.Scale = MinScale;
        fullImage.TranslationX = 0;
        fullImage.TranslationY = 0;
    }

    private void FinishClosing()
    {
        boxFullImage.IsVisible = false;
        boxFullImage.Opacity = 0;
        boxFullImage.InputTransparent = true;

        fullImage.IsVisible = false;
        fullImage.Opacity = 1;
        fullImage.TranslationX = 0;
        fullImage.TranslationY = 0;
        fullImage.Scale = MinScale;

        InputTransparent = true;
    }
}
