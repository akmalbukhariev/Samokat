using Ninimum.Resources.Languages;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;
using Ninimum.Models.Main;
using Ninimum.Services;

namespace Ninimum.Views.Main.Components;

public partial class MainProductCardView : ContentView
{

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

    public static readonly BindableProperty PriceProperty =
        BindableProperty.Create(
            nameof(Price),
            typeof(string),
            typeof(MainProductCardView),
            string.Empty);

    public string Price
    {
        get => (string)GetValue(PriceProperty);
        set => SetValue(PriceProperty, value);
    }

    public static readonly BindableProperty SubscriptionPriceProperty =
        BindableProperty.Create(
            nameof(SubscriptionPrice),
            typeof(string),
            typeof(MainProductCardView),
            string.Empty);

    public string SubscriptionPrice
    {
        get => (string)GetValue(SubscriptionPriceProperty);
        set => SetValue(SubscriptionPriceProperty, value);
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
            AppResource.PlusTomorrow);

    public static readonly BindableProperty IsCartLoadingProperty =
    BindableProperty.Create(
        nameof(IsCartLoading),
        typeof(bool),
        typeof(MainProductCardView),
        false);

    public bool IsCartLoading
    {
        get => (bool)GetValue(IsCartLoadingProperty);
        set => SetValue(IsCartLoadingProperty, value);
    }

    public static readonly BindableProperty IsCartTextVisibleProperty =
    BindableProperty.Create(
        nameof(IsCartTextVisible),
        typeof(bool),
        typeof(MainProductCardView),
        true);

    public bool IsCartTextVisible
    {
        get => (bool)GetValue(IsCartTextVisibleProperty);
        set => SetValue(IsCartTextVisibleProperty, value);
    }

    public static readonly BindableProperty IsIndicatorVisibleProperty =
    BindableProperty.Create(
        nameof(IsIndicatorVisible),
        typeof(bool),
        typeof(MainProductCardView),
        true);

    public bool IsIndicatorVisible
    {
        get => (bool)GetValue(IsIndicatorVisibleProperty);
        set => SetValue(IsIndicatorVisibleProperty, value);
    }

    public static readonly BindableProperty LikedProperty =
     BindableProperty.Create(nameof(Liked), typeof(bool), typeof(MainProductCardView), false, propertyChanged: LikedChanged);

    public static readonly BindableProperty LikeCommandProperty =
        BindableProperty.Create(nameof(LikeCommand), typeof(ICommand), typeof(MainProductCardView));


    public static readonly BindableProperty ClickProductCommandProperty =
       BindableProperty.Create(nameof(ClickProductCommand), typeof(ICommand), typeof(MainProductCardView));

    public ICommand ClickProductCommand
    {
        get => (ICommand)GetValue(ClickProductCommandProperty);
        set => SetValue(ClickProductCommandProperty, value);
    }
    
    public static readonly BindableProperty ClickCartCommandProperty =
        BindableProperty.Create(nameof(ClickCartCommand), typeof(ICommand), typeof(MainProductCardView));

     public ICommand ClickCartCommand
    {
        get => (ICommand)GetValue(ClickCartCommandProperty);
        set => SetValue(ClickCartCommandProperty, value);
    }

    public bool Liked
    {
        get => (bool)GetValue(LikedProperty);
        set => SetValue(LikedProperty, value);
    }

    public ICommand LikeCommand
    {
        get => (ICommand)GetValue(LikeCommandProperty);
        set => SetValue(LikeCommandProperty, value);
    }

    public string ActionButtonText
    {
        get => (string)GetValue(ActionButtonTextProperty);
        set => SetValue(ActionButtonTextProperty, value);
    }

    public string RatingText => Rating.ToString("0.0");

    // Match the five filled/gray stars used on the product detail page.
    public IReadOnlyList<string> Stars => Enumerable.Range(0, 5)
        .Select(index => index < Math.Round(Rating) ? "star.png" : "star_gray.png")
        .ToArray();

    public string ReviewCountText => $"({ReviewCount})";

    private static void OnImagesSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (MainProductCardView)bindable;

        if (oldValue is INotifyCollectionChanged oldCollection)
            oldCollection.CollectionChanged -= view.OnImagesCollectionChanged;

        if (newValue is INotifyCollectionChanged newCollection)
            newCollection.CollectionChanged += view.OnImagesCollectionChanged;

        view.UpdateImageMode();
        view.UpdateCustomIndicator(0);
    }

    private void OnImagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateImageMode();
            UpdateCustomIndicator(0);
        });
    }

    private static void OnRatingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (MainProductCardView)bindable;
        view.OnPropertyChanged(nameof(RatingText));
        view.OnPropertyChanged(nameof(Stars));
    }

    private static void OnReviewCountChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (MainProductCardView)bindable;
        view.OnPropertyChanged(nameof(ReviewCountText));
    }

    private void UpdateImageMode()
    {
        bool hasImage = (ImagesSource?.Count ?? 0) > 0 && ImagesSource![0] is MainProductImageItem;
        SingleImage.IsVisible = hasImage;
        MainImageSource = hasImage ? ((MainProductImageItem)ImagesSource![0]!).ImageSource : string.Empty;
    }

    private void UpdateCustomIndicator(int position)
    {
        // The card shows only the first picture, so there are no image dots to draw.
        CustomIndicatorLayout.Children.Clear();
    }

    private static void LikedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (MainProductCardView)bindable;
        bool liked = (bool)newValue;

        control.imLiked.Source = liked ? "liked.png" : "unliked.png";
    }

    private async void Like_Tapped(object sender, TappedEventArgs e)
    {
        await ClickGuard.RunAsync((VisualElement)sender, async () =>
        {
            AppVibrationService.Like();

            await brdLiked.ScaleTo(1.3, 100, Easing.CubicOut);
            await brdLiked.ScaleTo(1.0, 100, Easing.CubicIn);

            if (BindingContext is MainProductCardItem product &&
                LikeCommand?.CanExecute(product) == true)
            {
                LikeCommand.Execute(product);
            }
        });
    }

    private async void Product_Tapped(object sender, TappedEventArgs e)
    {
        await ClickGuard.RunAsync(mainStack, async () =>
        {
            await mainStack.ScaleTo(0.95, 100, Easing.CubicOut);
            await mainStack.ScaleTo(1.0, 100, Easing.CubicIn);

            if (BindingContext is MainProductCardItem product &&  ClickProductCommand?.CanExecute(product) == true)
            {
                ClickProductCommand.Execute(product);
            }
        });
    }

    private async void Cart_Tapped(object sender, TappedEventArgs e)
    {
        await ClickGuard.RunAsync(brdAction, async () =>
        {
            AppVibrationService.Like();
            
            await brdAction.ScaleTo(0.95, 100, Easing.CubicOut);
            await brdAction.ScaleTo(1.0, 100, Easing.CubicIn);

            if (BindingContext is MainProductCardItem product && ClickCartCommand?.CanExecute(product) == true)
            {
                ClickCartCommand.Execute(product);
            }
        });
    }
}