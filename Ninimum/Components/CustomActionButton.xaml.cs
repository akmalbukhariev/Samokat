using System.Windows.Input;

namespace Samokat.Components;

public partial class CustomActionButton : ContentView
{
    private bool _isAnimating;

    public CustomActionButton()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ButtonTextProperty =
        BindableProperty.Create(
            nameof(ButtonText),
            typeof(string),
            typeof(CustomActionButton),
            "Davom etish");

    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public static readonly BindableProperty ButtonBackgroundColorProperty =
        BindableProperty.Create(
            nameof(ButtonBackgroundColor),
            typeof(Color),
            typeof(CustomActionButton),
            Color.FromArgb("#FF4B4B"));

    public Color ButtonBackgroundColor
    {
        get => (Color)GetValue(ButtonBackgroundColorProperty);
        set => SetValue(ButtonBackgroundColorProperty, value);
    }

    public static readonly BindableProperty ButtonTextColorProperty =
        BindableProperty.Create(
            nameof(ButtonTextColor),
            typeof(Color),
            typeof(CustomActionButton),
            Colors.White);

    public Color ButtonTextColor
    {
        get => (Color)GetValue(ButtonTextColorProperty);
        set => SetValue(ButtonTextColorProperty, value);
    }

    public static readonly BindableProperty ButtonImageSourceProperty =
        BindableProperty.Create(
            nameof(ButtonImageSource),
            typeof(ImageSource),
            typeof(CustomActionButton),
            default(ImageSource));

    public ImageSource ButtonImageSource
    {
        get => (ImageSource)GetValue(ButtonImageSourceProperty);
        set => SetValue(ButtonImageSourceProperty, value);
    }

    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(
            nameof(TapCommand),
            typeof(ICommand),
            typeof(CustomActionButton),
            default(ICommand));

    public ICommand TapCommand
    {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public static readonly BindableProperty TapCommandParameterProperty =
        BindableProperty.Create(
            nameof(TapCommandParameter),
            typeof(object),
            typeof(CustomActionButton),
            default(object));

    public object TapCommandParameter
    {
        get => GetValue(TapCommandParameterProperty);
        set => SetValue(TapCommandParameterProperty, value);
    }

    public static readonly BindableProperty EnableTapAnimationProperty =
        BindableProperty.Create(
            nameof(EnableTapAnimation),
            typeof(bool),
            typeof(CustomActionButton),
            true);

    public bool EnableTapAnimation
    {
        get => (bool)GetValue(EnableTapAnimationProperty);
        set => SetValue(EnableTapAnimationProperty, value);
    }

    private async void OnTapped(object sender, TappedEventArgs e)
    {
        if (_isAnimating)
            return;

        if (EnableTapAnimation)
        {
            try
            {
                _isAnimating = true;

                await RootBorder.ScaleTo(0.96, 100, Easing.CubicOut);
                await RootBorder.ScaleTo(1.0, 100, Easing.CubicIn);
            }
            finally
            {
                _isAnimating = false;
            }
        }

        if (TapCommand?.CanExecute(TapCommandParameter) == true)
            TapCommand.Execute(TapCommandParameter);
    }
}