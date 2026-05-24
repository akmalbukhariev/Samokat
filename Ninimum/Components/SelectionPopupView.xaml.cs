using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;

namespace Ninimum.Components;

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
            default(IEnumerable),
            propertyChanged: OnItemsSourceChanged);

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

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (SelectionPopupView)bindable;

        if (oldValue is INotifyCollectionChanged oldCollection)
            oldCollection.CollectionChanged -= view.OnCollectionChanged;

        if (newValue is INotifyCollectionChanged newCollection)
            newCollection.CollectionChanged += view.OnCollectionChanged;

        view.BuildItems();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        BuildItems();
    }

    private void BuildItems()
    {
        ItemsHost.Children.Clear();

        if (ItemsSource == null)
            return;

        var list = new List<object>();

        foreach (var item in ItemsSource)
        {
            if (item != null)
                list.Add(item);
        }

        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            var type = item.GetType();

            var text = type.GetProperty("Text")?.GetValue(item)?.ToString() ?? string.Empty;
            var leftImage = type.GetProperty("LeftImage")?.GetValue(item)?.ToString() ?? string.Empty;
            var rightImage = type.GetProperty("RightImage")?.GetValue(item)?.ToString() ?? string.Empty;
            var showSeparator = i < list.Count - 1;

            var rowRoot = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = 72 },
                    new RowDefinition { Height = 1 }
                }
            };

            var rowContent = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 16,
                Padding = new Thickness(22, 0, 22, 0),
                HeightRequest = 72
            };

            var leftImageView = new Image
            {
                Source = leftImage,
                WidthRequest = 28,
                HeightRequest = 18,
                Aspect = Aspect.AspectFit,
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(leftImageView, 0);

            var textLabel = new Label
            {
                Text = text,
                FontSize = 17,
                TextColor = Color.FromArgb("#222222"),
                VerticalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.NoWrap
            };
            Grid.SetColumn(textLabel, 1);

            var rightImageView = new Image
            {
                Source = rightImage,
                WidthRequest = 18,
                HeightRequest = 18,
                Aspect = Aspect.AspectFit,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                IsVisible = !string.IsNullOrWhiteSpace(rightImage)
            };
            Grid.SetColumn(rightImageView, 2);

            rowContent.Children.Add(leftImageView);
            rowContent.Children.Add(textLabel);
            rowContent.Children.Add(rightImageView);

            Grid.SetRow(rowContent, 0);
            rowRoot.Children.Add(rowContent);

            var separator = new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = Color.FromArgb("#E5E5E5"),
                IsVisible = showSeparator
            };
            Grid.SetRow(separator, 1);
            rowRoot.Children.Add(separator);

            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                if (ItemTappedCommand?.CanExecute(item) == true)
                    ItemTappedCommand.Execute(item);
            };
            rowRoot.GestureRecognizers.Add(tap);

            ItemsHost.Children.Add(rowRoot);
        }
    }

    private void OnBackgroundTapped(object sender, TappedEventArgs e)
    {
        IsVisible = false;
    }
}