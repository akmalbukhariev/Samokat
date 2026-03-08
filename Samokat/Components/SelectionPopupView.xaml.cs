using System.Collections;
using System.Windows.Input;

namespace Samokat.Components;

public partial class SelectionPopupView : ContentView
{
    public SelectionPopupView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(SelectionPopupView),
            default(IEnumerable));

    public static readonly BindableProperty ItemTappedCommandProperty =
        BindableProperty.Create(
            nameof(ItemTappedCommand),
            typeof(ICommand),
            typeof(SelectionPopupView));

    public static readonly BindableProperty PopupWidthProperty =
        BindableProperty.Create(
            nameof(PopupWidth),
            typeof(double),
            typeof(SelectionPopupView),
            320d);

    public static readonly BindableProperty PopupMaxHeightProperty =
        BindableProperty.Create(
            nameof(PopupMaxHeight),
            typeof(double),
            typeof(SelectionPopupView),
            620d);

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public ICommand ItemTappedCommand
    {
        get => (ICommand)GetValue(ItemTappedCommandProperty);
        set => SetValue(ItemTappedCommandProperty, value);
    }

    public double PopupWidth
    {
        get => (double)GetValue(PopupWidthProperty);
        set => SetValue(PopupWidthProperty, value);
    }

    public double PopupMaxHeight
    {
        get => (double)GetValue(PopupMaxHeightProperty);
        set => SetValue(PopupMaxHeightProperty, value);
    }

    private void OnBackgroundTapped(object sender, TappedEventArgs e)
    {
        IsVisible = false;
    }
}