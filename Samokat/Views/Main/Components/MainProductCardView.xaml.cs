using System.Collections;
using System.Collections.Specialized;
using Microsoft.Maui.Controls.Shapes;
using Samokat.Models.Main;

namespace Samokat.Views.Main.Components;

public partial class MainProductCardView : ContentView
{
    private int _currentPosition;

    public MainProductCardView()
    {
        InitializeComponent();
        UpdateImageMode();
        UpdateCustomIndicator(0);
    }

    public static readonly BindableProperty ImagesSourceProperty =
        BindableProperty.Create(
            nameof(ImagesSource),
            typeof(IList),
            typeof(MainProductCardView),
            default(IList),
            propertyChanged: OnImagesSourceChanged);

    public IList? ImagesSource
    {
        get => (IList?)GetValue(ImagesSourceProperty);
        set => SetValue(ImagesSourceProperty, value);
    }

    public static readonly BindableProperty MainImageSourceProperty =
        BindableProperty.Create(
            nameof(MainImageSource),
            typeof(string),
            typeof(MainProductCardView),
            string.Empty);

    public string MainImageSource
    {
        get => (string)GetValue(MainImageSourceProperty);
        set => SetValue(MainImageSourceProperty, value);
    }

    public static readonly BindableProperty OldPriceProperty =
        BindableProperty.Create(
            nameof(OldPrice),
            typeof(string),
            typeof(MainProductCardView),
            string.Empty);

    public string OldPrice
    {
        get => (string)GetValue(OldPriceProperty);
        set => SetValue(OldPriceProperty, value);
    }

    public static readonly BindableProperty NewPriceProperty =
        BindableProperty.Create(
            nameof(NewPrice),
            typeof(string),
            typeof(MainProductCardView),
            string.Empty);

    public string NewPrice
    {
        get => (string)GetValue(NewPriceProperty);
        set => SetValue(NewPriceProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(MainProductCardView),
            string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty RatingProperty =
        BindableProperty.Create(
            nameof(Rating),
            typeof(double),
            typeof(MainProductCardView),
            0.0,
            propertyChanged: OnRatingChanged);

    public double Rating
    {
        get => (double)GetValue(RatingProperty);
        set => SetValue(RatingProperty, value);
    }

    public static readonly BindableProperty ReviewCountProperty =
        BindableProperty.Create(
            nameof(ReviewCount),
            typeof(int),
            typeof(MainProductCardView),
            0,
            propertyChanged: OnReviewCountChanged);

    public int ReviewCount
    {
        get => (int)GetValue(ReviewCountProperty);
        set => SetValue(ReviewCountProperty, value);
    }

    public static readonly BindableProperty ActionButtonTextProperty =
        BindableProperty.Create(
            nameof(ActionButtonText),
            typeof(string),
            typeof(MainProductCardView),
            "+ Ertaga");

    public string ActionButtonText
    {
        get => (string)GetValue(ActionButtonTextProperty);
        set => SetValue(ActionButtonTextProperty, value);
    }

    public string RatingText => Rating.ToString("0.0");

    public string ReviewCountText => $"({ReviewCount})";

    private static void OnImagesSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (MainProductCardView)bindable;

        if (oldValue is INotifyCollectionChanged oldCollection)
            oldCollection.CollectionChanged -= view.OnImagesCollectionChanged;

        if (newValue is INotifyCollectionChanged newCollection)
            newCollection.CollectionChanged += view.OnImagesCollectionChanged;

        view._currentPosition = 0;
        view.UpdateImageMode();
        view.UpdateCustomIndicator(0);
    }

    private void OnImagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _currentPosition = 0;
            UpdateImageMode();
            UpdateCustomIndicator(0);
        });
    }

    private static void OnRatingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (MainProductCardView)bindable;
        view.OnPropertyChanged(nameof(RatingText));
    }

    private static void OnReviewCountChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (MainProductCardView)bindable;
        view.OnPropertyChanged(nameof(ReviewCountText));
    }

    private void UpdateImageMode()
    {
        int count = ImagesSource?.Count ?? 0;

        bool hasCarousel = count > 1;
        bool hasSingleImage = count == 1;

        ImageCarousel.IsVisible = hasCarousel;
        SingleImage.IsVisible = hasSingleImage;

        if (hasCarousel)
        {
            ImageCarousel.ItemsSource = ImagesSource;
            ImageCarousel.Position = 0;
            MainImageSource = string.Empty;
        }
        else if (hasSingleImage && ImagesSource?[0] is MainProductImageItem item)
        {
            MainImageSource = item.ImageSource;
        }
        else
        {
            MainImageSource = string.Empty;
        }
    }

    private void OnCarouselPositionChanged(object? sender, PositionChangedEventArgs e)
    {
        _currentPosition = e.CurrentPosition;
        UpdateCustomIndicator(_currentPosition);
    }

    private void UpdateCustomIndicator(int position)
    {
        CustomIndicatorLayout.Children.Clear();

        int count = ImagesSource?.Count ?? 0;

        if (count <= 1)
            return;

        for (int i = 0; i < count; i++)
        {
            bool isSelected = i == position;

            var indicator = new Border
            {
                StrokeThickness = 0,
                BackgroundColor = isSelected
                    ? Color.FromArgb("#95C11F")
                    : Color.FromArgb("#E5E5E5"),
                WidthRequest = isSelected ? 18 : 10,
                HeightRequest = 7,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(5)
                },
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            CustomIndicatorLayout.Children.Add(indicator);
        }
    }

    private async void Like_Tapped(object sender, TappedEventArgs e)
    {
        Image image = sender as Image;
        await image.ScaleTo(0.96, 100, Easing.CubicOut);
        await image.ScaleTo(1.0, 100, Easing.CubicIn);
    }
}