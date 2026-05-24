using System.Windows.Input;

namespace Samokat.Views.BasketProduct;

public partial class BasketCartItemView : ContentView
{
    public BasketCartItemView()
    {
        InitializeComponent();
        UpdateCheckIcon();
    }

    public static readonly BindableProperty ProductImageSourceProperty =
        BindableProperty.Create(
            nameof(ProductImageSource),
            typeof(ImageSource),
            typeof(BasketCartItemView),
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
            typeof(BasketCartItemView),
            string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty OldPriceProperty =
        BindableProperty.Create(
            nameof(OldPrice),
            typeof(string),
            typeof(BasketCartItemView),
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
            typeof(BasketCartItemView),
            string.Empty);

    public string NewPrice
    {
        get => (string)GetValue(NewPriceProperty);
        set => SetValue(NewPriceProperty, value);
    }

    public static readonly BindableProperty IsCheckedProperty =
        BindableProperty.Create(
            nameof(IsChecked),
            typeof(bool),
            typeof(BasketCartItemView),
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
            typeof(BasketCartItemView),
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
            typeof(BasketCartItemView),
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
            typeof(BasketCartItemView));

    public ICommand QuantityChangedCommand
    {
        get => (ICommand)GetValue(QuantityChangedCommandProperty);
        set => SetValue(QuantityChangedCommandProperty, value);
    }

    public static readonly BindableProperty ToggleCheckedCommandProperty =
        BindableProperty.Create(
            nameof(ToggleCheckedCommand),
            typeof(ICommand),
            typeof(BasketCartItemView));

    public ICommand ToggleCheckedCommand
    {
        get => (ICommand)GetValue(ToggleCheckedCommandProperty);
        set => SetValue(ToggleCheckedCommandProperty, value);
    }

    private static void OnIsCheckedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BasketCartItemView view)
            view.UpdateCheckIcon();
    }

    private void UpdateCheckIcon()
    {
        CheckIconSource = IsChecked ? "ic_check.png" : "ic_uncheck.png";
    }
}