namespace Ninimum.Components;

public partial class CustomLikedView : ContentView
{
    public static readonly BindableProperty ShowLikedProperty =
     BindableProperty.Create(
         nameof(ShowLiked),
         typeof(bool),
         typeof(CustomLikedView),
         false,
         propertyChanged: ShowLikedChanged);

    public bool ShowLiked
    {
        get => (bool)GetValue(ShowLikedProperty);
        set => SetValue(ShowLikedProperty, value);
    }

    public CustomLikedView()
    {
        InitializeComponent(); 
    }

     private static void ShowLikedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (CustomLikedView)bindable;
        control.UpdateLikedState((bool)newValue);
    }

    private void UpdateLikedState(bool show)
    {
        imLiked.IsVisible = show;
        imUnLiked.IsVisible = !show;
    }

    public async Task DisplayAsAnimation()
    {
        IsVisible = true;
         
        Opacity = 0;
        Scale = 0.5;
         
        await Task.WhenAll(
            this.FadeTo(1, 150),
            this.ScaleTo(1.2, 150, Easing.SinInOut)
        );
        await this.ScaleTo(1.0, 50, Easing.SinInOut);

        await Task.Delay(500);

        IsVisible = false;
    }
}