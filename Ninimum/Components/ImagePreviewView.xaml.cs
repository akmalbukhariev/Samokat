using Microsoft.Maui.Controls;

namespace Ninimum.Components;

public partial class ImagePreviewView : ContentView
{
    private bool _isAnimating;

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

        // Let this overlay receive input now
        InputTransparent = false;

        // Prepare start state
        fullImage.TranslationY = -100;
        fullImage.Opacity = 0;
        fullImage.IsVisible = true;

        boxFullImage.IsVisible = true;
        boxFullImage.Opacity = 0.5;
        boxFullImage.InputTransparent = false;

        await Task.WhenAll(
            fullImage.TranslateTo(0, 0, 250, Easing.SinIn),
            fullImage.FadeTo(1, 250, Easing.SinIn)
        );

        _isAnimating = false;
    }

    public async Task CloseAsync(bool swipeDown = true)
    {
        if (_isAnimating || !fullImage.IsVisible)
            return;

        _isAnimating = true;

        double targetY = swipeDown ? 100 : -100;

        await Task.WhenAll(
            fullImage.TranslateTo(0, targetY, 250, Easing.SinOut),
            fullImage.FadeTo(0, 250, Easing.SinOut)
        );

        boxFullImage.IsVisible = false;
        boxFullImage.Opacity = 0;
        boxFullImage.InputTransparent = true;

        fullImage.IsVisible = false;
        fullImage.Opacity = 1;
        fullImage.TranslationY = 0;

        // Allow touches to go through when closed
        InputTransparent = true;

        _isAnimating = false;
    }

    private async void OnImageSwiped(object sender, SwipedEventArgs e)
    {
        if (e.Direction == SwipeDirection.Down)
        {
            await CloseAsync(true);
        }
        else if (e.Direction == SwipeDirection.Up)
        {
            await CloseAsync(false);
        }
    }

    private async void OnImageTapped(object sender, TappedEventArgs e)
    {
        await CloseWithoutAnimationAsync();
    }

    private async void OnOverlayTapped(object sender, TappedEventArgs e)
    {
        await CloseWithoutAnimationAsync();
    }

    private Task CloseWithoutAnimationAsync()
    {
        if (_isAnimating)
            return Task.CompletedTask;

        boxFullImage.IsVisible = false;
        boxFullImage.Opacity = 0;
        boxFullImage.InputTransparent = true;

        fullImage.IsVisible = false;
        fullImage.Opacity = 1;
        fullImage.TranslationY = 0;

        InputTransparent = true;

        return Task.CompletedTask;
    }
}