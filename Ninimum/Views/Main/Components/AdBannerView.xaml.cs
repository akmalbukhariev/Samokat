using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;
using Samokat.Models.Main;

namespace Samokat.Views.Main.Components;

public partial class AdBannerView : ContentView
{
    private int _currentPosition;

    public AdBannerView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IList),
            typeof(AdBannerView),
            default(IList),
            propertyChanged: OnItemsSourceChanged);

    public IList? ItemsSource
    {
        get => (IList?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly BindableProperty PurchaseCommandProperty =
        BindableProperty.Create(
            nameof(PurchaseCommand),
            typeof(ICommand),
            typeof(AdBannerView),
            default(ICommand));

    public ICommand? PurchaseCommand
    {
        get => (ICommand?)GetValue(PurchaseCommandProperty);
        set => SetValue(PurchaseCommandProperty, value);
    }

    public static readonly BindableProperty CurrentPositionProperty =
        BindableProperty.Create(
            nameof(CurrentPosition),
            typeof(int),
            typeof(AdBannerView),
            0,
            BindingMode.TwoWay,
            propertyChanged: OnCurrentPositionChanged);

    public int CurrentPosition
    {
        get => (int)GetValue(CurrentPositionProperty);
        set => SetValue(CurrentPositionProperty, value);
    }

    public event EventHandler<AdBannerItem>? PurchaseClicked;

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (AdBannerView)bindable;

        if (oldValue is INotifyCollectionChanged oldCollection)
            oldCollection.CollectionChanged -= view.OnItemsCollectionChanged;

        if (newValue is INotifyCollectionChanged newCollection)
            newCollection.CollectionChanged += view.OnItemsCollectionChanged;

        view.BannerCarousel.ItemsSource = view.ItemsSource;
        view.CurrentPosition = 0;
        view.UpdateCustomIndicator(0);
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (ItemsSource == null || ItemsSource.Count == 0)
            {
                CustomIndicatorLayout.Children.Clear();
                return;
            }

            if (_currentPosition >= ItemsSource.Count)
                _currentPosition = 0;

            UpdateCustomIndicator(_currentPosition);
        });
    }

    private static void OnCurrentPositionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (AdBannerView)bindable;
        var position = (int)newValue;

        view._currentPosition = position;

        if (view.BannerCarousel.Position != position)
            view.BannerCarousel.Position = position;

        view.UpdateCustomIndicator(position);
    }

    private void OnCarouselPositionChanged(object? sender, PositionChangedEventArgs e)
    {
        _currentPosition = e.CurrentPosition;
        CurrentPosition = e.CurrentPosition;
        UpdateCustomIndicator(e.CurrentPosition);
    }

    private void UpdateCustomIndicator(int position)
    {
        CustomIndicatorLayout.Children.Clear();

        if (ItemsSource == null || ItemsSource.Count == 0)
            return;

        for (int i = 0; i < ItemsSource.Count; i++)
        {
            bool isSelected = i == position;

            var indicator = new Border
            {
                StrokeThickness = 0,
                BackgroundColor = isSelected
                    ? Color.FromArgb("#FD473C")
                    : Color.FromArgb("#DADADA"),
                WidthRequest = isSelected ? 30 : 12,
                HeightRequest = 12,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = isSelected
                        ? new CornerRadius(6)
                        : new CornerRadius(999)
                },
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            CustomIndicatorLayout.Children.Add(indicator);
        }
    }

    private void OnPurchaseTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Element element && element.BindingContext is AdBannerItem item)
        {
            PurchaseClicked?.Invoke(this, item);

            if (PurchaseCommand?.CanExecute(item) == true)
                PurchaseCommand.Execute(item);
        }
    }
}