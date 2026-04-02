using System.Windows.Input;

namespace Samokat.Components;

public partial class CustomRadioButtonView : ContentView
{
    public CustomRadioButtonView()
    {
        InitializeComponent();
        UpdateVisualState();
    }

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(CustomRadioButtonView),
            string.Empty);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(
            nameof(IsSelected),
            typeof(bool),
            typeof(CustomRadioButtonView),
            false,
            propertyChanged: OnIsSelectedChanged);

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(
            nameof(Command),
            typeof(ICommand),
            typeof(CustomRadioButtonView));

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(
            nameof(CommandParameter),
            typeof(object),
            typeof(CustomRadioButtonView));

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly BindableProperty SelectedColorProperty =
        BindableProperty.Create(
            nameof(SelectedColor),
            typeof(Color),
            typeof(CustomRadioButtonView),
            Color.FromArgb("#21C940"));

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public static readonly BindableProperty UnselectedColorProperty =
        BindableProperty.Create(
            nameof(UnselectedColor),
            typeof(Color),
            typeof(CustomRadioButtonView),
            Color.FromArgb("#96979B"));

    public Color UnselectedColor
    {
        get => (Color)GetValue(UnselectedColorProperty);
        set => SetValue(UnselectedColorProperty, value);
    }

    public static readonly BindableProperty OuterStrokeColorProperty =
        BindableProperty.Create(
            nameof(OuterStrokeColor),
            typeof(Color),
            typeof(CustomRadioButtonView),
            Color.FromArgb("#96979B"));

    public Color OuterStrokeColor
    {
        get => (Color)GetValue(OuterStrokeColorProperty);
        set => SetValue(OuterStrokeColorProperty, value);
    }

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(
            nameof(TextColor),
            typeof(Color),
            typeof(CustomRadioButtonView),
            Color.FromArgb("#22292F"));

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(
            nameof(FontSize),
            typeof(double),
            typeof(CustomRadioButtonView),
            17d);

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(
            nameof(FontFamily),
            typeof(string),
            typeof(CustomRadioButtonView),
            "SF Pro");

    public string FontFamily
    {
        get => (string)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    private static void OnIsSelectedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CustomRadioButtonView view)
            view.UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        OuterStrokeColor = IsSelected ? SelectedColor : UnselectedColor;
    }

    private void OnTapped(object sender, TappedEventArgs e)
    {
        if (Command?.CanExecute(CommandParameter) == true)
            Command.Execute(CommandParameter);
    }
}