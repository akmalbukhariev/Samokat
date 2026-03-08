namespace Samokat.Components;

public partial class SelectionPopupItemView : ContentView
{
    public SelectionPopupItemView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(SelectionPopupItemView),
            string.Empty);

    public static readonly BindableProperty LeftImageProperty =
        BindableProperty.Create(
            nameof(LeftImage),
            typeof(string),
            typeof(SelectionPopupItemView),
            string.Empty,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                ((SelectionPopupItemView)bindable).OnPropertyChanged(nameof(HasLeftImage));
            });

    public static readonly BindableProperty RightImageProperty =
        BindableProperty.Create(
            nameof(RightImage),
            typeof(string),
            typeof(SelectionPopupItemView),
            string.Empty,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                ((SelectionPopupItemView)bindable).OnPropertyChanged(nameof(HasRightImage));
            });

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string LeftImage
    {
        get => (string)GetValue(LeftImageProperty);
        set => SetValue(LeftImageProperty, value);
    }

    public string RightImage
    {
        get => (string)GetValue(RightImageProperty);
        set => SetValue(RightImageProperty, value);
    }

    public bool HasLeftImage => !string.IsNullOrWhiteSpace(LeftImage);
    public bool HasRightImage => !string.IsNullOrWhiteSpace(RightImage);
}