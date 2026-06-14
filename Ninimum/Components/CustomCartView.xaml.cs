namespace Ninimum.Components;

public partial class CustomCartView : ContentView
{
    public CustomCartView()
    {
        InitializeComponent();
    }

    public async Task DisplayAsAnimation()
    {
        IsVisible = true;

        Opacity = 0;
        Scale = 0.5;
        TranslationY = 20;

        await Task.WhenAll(
            this.FadeTo(1, 150),
            this.ScaleTo(1.2, 150, Easing.SinInOut),
            this.TranslateTo(0, 0, 150, Easing.SinInOut)
        );

        await this.ScaleTo(1.0, 50, Easing.SinInOut);

        await Task.Delay(500);

        await Task.WhenAll(
            this.FadeTo(0, 150),
            this.ScaleTo(0.7, 150, Easing.SinInOut),
            this.TranslateTo(0, 20, 150, Easing.SinInOut)
        );

        IsVisible = false;
        Opacity = 1;
        Scale = 1;
        TranslationY = 0;
    }
}