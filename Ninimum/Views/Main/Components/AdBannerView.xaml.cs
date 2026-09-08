using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;
using Ninimum.Models.Main;

namespace Ninimum.Views.Main.Components;

public partial class AdBannerView : ContentView
{
    private int _currentPosition;
    private IDispatcherTimer? _autoSlideTimer;
    private bool _viewLoaded;
    private const int AutoSlideSeconds = 4;

    public AdBannerView()
    {
        InitializeComponent();

        Loaded += OnViewLoaded;
        Unloaded += OnViewUnloaded;
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
        view._currentPosition = 0;
        view.CurrentPosition = 0;
        view.UpdateCustomIndicator(0);
        view.RestartAutoSlide();
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (ItemsSource == null || ItemsSource.Count == 0)
            {
                _currentPosition = 0;
                CurrentPosition = 0;
                CustomIndicatorLayout.Children.Clear();
                StopAutoSlide();
                return;
            }

            if (_currentPosition >= ItemsSource.Count)
            {
                _currentPosition = 0;
                CurrentPosition = 0;
            }

            UpdateCustomIndicator(_currentPosition);

            // ItemsSource is initially an empty ObservableCollection. Banners are
            // populated after the API response, so auto-slide must be started here
            // when the collection actually reaches 2+ items.
            RestartAutoSlide();
        });
    }

    private static void OnCurrentPositionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (AdBannerView)bindable;
        var position = (int)newValue;

        if (view.ItemsSource == null || view.ItemsSource.Count == 0)
            position = 0;
        else
            position = Math.Clamp(position, 0, view.ItemsSource.Count - 1);

        view._currentPosition = position;

        if (view.BannerCarousel.Position != position)
            view.BannerCarousel.Position = position;

        view.UpdateCustomIndicator(position);
    }

    private void OnCarouselPositionChanged(object? sender, PositionChangedEventArgs e)
    {
        _currentPosition = e.CurrentPosition;

        if (CurrentPosition != e.CurrentPosition)
            CurrentPosition = e.CurrentPosition;

        UpdateCustomIndicator(e.CurrentPosition);

        // Give the user a full interval after a manual swipe before moving again.
        RestartAutoSlide();
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

    private async void OnPurchaseTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not VisualElement element ||
            element.BindingContext is not AdBannerItem item)
            return;

        await AnimateElementScaleDown(element);

        PurchaseClicked?.Invoke(this, item);

        if (PurchaseCommand?.CanExecute(item) == true)
            PurchaseCommand.Execute(item);
    }

    private static async Task AnimateElementScaleDown(VisualElement element)
    {
        await element.ScaleTo(0.9, 100, Easing.CubicOut);
        await element.ScaleTo(1.0, 100, Easing.CubicIn);
    }

    private void OnViewLoaded(object? sender, EventArgs e)
    {
        _viewLoaded = true;
        RestartAutoSlide();
    }

    private void OnViewUnloaded(object? sender, EventArgs e)
    {
        _viewLoaded = false;
        StopAutoSlide();
    }

    private void RestartAutoSlide()
    {
        StopAutoSlide();
        StartAutoSlide();
    }

    private void StartAutoSlide()
    {
        if (!_viewLoaded || ItemsSource == null || ItemsSource.Count <= 1)
            return;

        _autoSlideTimer = Dispatcher.CreateTimer();
        _autoSlideTimer.Interval = TimeSpan.FromSeconds(AutoSlideSeconds);
        _autoSlideTimer.Tick += OnAutoSlideTimerTick;
        _autoSlideTimer.Start();
    }

    private void OnAutoSlideTimerTick(object? sender, EventArgs e)
    {
        if (ItemsSource == null || ItemsSource.Count <= 1)
        {
            StopAutoSlide();
            return;
        }

        int nextPosition = (_currentPosition + 1) % ItemsSource.Count;

        // ScrollTo gives us the same smooth animation as a finger swipe.
        // PositionChanged updates CurrentPosition + the custom indicator.
        BannerCarousel.ScrollTo(
            nextPosition,
            position: ScrollToPosition.Center,
            animate: true);
    }

    private void StopAutoSlide()
    {
        if (_autoSlideTimer == null)
            return;

        _autoSlideTimer.Stop();
        _autoSlideTimer.Tick -= OnAutoSlideTimerTick;
        _autoSlideTimer = null;
    }
}
