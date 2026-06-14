using System.Windows.Input;

namespace Ninimum.Views.Cart;

public partial class CartItemView : ContentView
{
    public CartItemView()
    {
        InitializeComponent();
        UpdateCheckIcon();
    }

    public static readonly BindableProperty ProductImageSourceProperty =
        BindableProperty.Create(
            nameof(ProductImageSource),
            typeof(ImageSource),
            typeof(CartItemView),
            default(ImageSource));

    public ImageSource ProductImageSource
    {
        get => (ImageSource)GetValue(ProductImageSourceProperty);
        set => SetValue(ProductImageSourceProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(CartItemView),
            string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty PriceProperty =
        BindableProperty.Create(
            nameof(Price),
            typeof(string),
            typeof(CartItemView),
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
            typeof(CartItemView),
            string.Empty);

    public string SubscriptionPrice
    {
        get => (string)GetValue(SubscriptionPriceProperty);
        set => SetValue(SubscriptionPriceProperty, value);
    }

    public static readonly BindableProperty IsCheckedProperty =
        BindableProperty.Create(
            nameof(IsChecked),
            typeof(bool),
            typeof(CartItemView),
            false,
            BindingMode.TwoWay,
            propertyChanged: OnIsCheckedChanged);

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public static readonly BindableProperty CheckIconSourceProperty =
        BindableProperty.Create(
            nameof(CheckIconSource),
            typeof(string),
            typeof(CartItemView),
            "ic_uncheck.png");

    public string CheckIconSource
    {
        get => (string)GetValue(CheckIconSourceProperty);
        set => SetValue(CheckIconSourceProperty, value);
    }

    public static readonly BindableProperty QuantityProperty =
        BindableProperty.Create(
            nameof(Quantity),
            typeof(int),
            typeof(CartItemView),
            1,
            BindingMode.TwoWay);

    public int Quantity
    {
        get => (int)GetValue(QuantityProperty);
        set => SetValue(QuantityProperty, value);
    }

    public static readonly BindableProperty QuantityChangedCommandProperty =
        BindableProperty.Create(
            nameof(QuantityChangedCommand),
            typeof(ICommand),
            typeof(CartItemView));

    public ICommand QuantityChangedCommand
    {
        get => (ICommand)GetValue(QuantityChangedCommandProperty);
        set => SetValue(QuantityChangedCommandProperty, value);
    }

    public static readonly BindableProperty ToggleCheckedCommandProperty =
        BindableProperty.Create(
            nameof(ToggleCheckedCommand),
            typeof(ICommand),
            typeof(CartItemView));

    public ICommand ToggleCheckedCommand
    {
        get => (ICommand)GetValue(ToggleCheckedCommandProperty);
        set => SetValue(ToggleCheckedCommandProperty, value);
    }

    private static void OnIsCheckedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CartItemView view)
            view.UpdateCheckIcon();
    }

    private void UpdateCheckIcon()
    {
        CheckIconSource = IsChecked ? "ic_check.png" : "ic_uncheck.png";
    }
}