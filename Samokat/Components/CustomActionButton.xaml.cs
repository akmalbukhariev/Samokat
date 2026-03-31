using System.Windows.Input;

namespace Samokat.Components;

public partial class CustomActionButton : ContentView
{
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
}