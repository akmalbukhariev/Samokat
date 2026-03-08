using System.Windows.Input;

namespace Samokat.Components;

public partial class InputView : ContentView
{
    public InputView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(
            nameof(TapCommand),
            typeof(ICommand),
            typeof(InputView));

    public ICommand TapCommand
    {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(InputView),
            string.Empty,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                ((InputView)bindable).OnPropertyChanged(nameof(HasTitle));
            });

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(InputView),
            string.Empty,
            BindingMode.TwoWay);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(string),
            typeof(InputView),
            string.Empty);

    public static readonly BindableProperty LeftIconProperty =
        BindableProperty.Create(
            nameof(LeftIcon),
            typeof(string),
            typeof(InputView),
            string.Empty,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                ((InputView)bindable).OnPropertyChanged(nameof(HasLeftIcon));
            });

    public static readonly BindableProperty MiddleImageProperty =
        BindableProperty.Create(
            nameof(MiddleImage),
            typeof(string),
            typeof(InputView),
            string.Empty,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                ((InputView)bindable).OnPropertyChanged(nameof(HasMiddleImage));
            });

    public static readonly BindableProperty RightIconProperty =
        BindableProperty.Create(
            nameof(RightIcon),
            typeof(string),
            typeof(InputView),
            string.Empty,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                ((InputView)bindable).OnPropertyChanged(nameof(HasRightIcon));
            });

    public static readonly BindableProperty IsReadOnlyProperty =
        BindableProperty.Create(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(InputView),
            false);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string LeftIcon
    {
        get => (string)GetValue(LeftIconProperty);
        set => SetValue(LeftIconProperty, value);
    }

    public string MiddleImage
    {
        get => (string)GetValue(MiddleImageProperty);
        set => SetValue(MiddleImageProperty, value);
    }

    public string RightIcon
    {
        get => (string)GetValue(RightIconProperty);
        set => SetValue(RightIconProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool HasLeftIcon => !string.IsNullOrWhiteSpace(LeftIcon);
    public bool HasMiddleImage => !string.IsNullOrWhiteSpace(MiddleImage);
    public bool HasRightIcon => !string.IsNullOrWhiteSpace(RightIcon);
    public bool HasTitle => !string.IsNullOrWhiteSpace(Title);
}