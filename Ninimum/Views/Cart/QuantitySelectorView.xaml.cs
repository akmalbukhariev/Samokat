using System.Windows.Input;
using Ninimum.Services;

namespace Ninimum.Views.Cart;

public partial class QuantitySelectorView : ContentView
{
    public QuantitySelectorView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty QuantityProperty =
        BindableProperty.Create(
            nameof(Quantity),
            typeof(int),
            typeof(QuantitySelectorView),
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
            typeof(QuantitySelectorView),
            default(ICommand));

    public ICommand QuantityChangedCommand
    {
        get => (ICommand)GetValue(QuantityChangedCommandProperty);
        set => SetValue(QuantityChangedCommandProperty, value);
    }

    // Stock left; 0 = no limit. "+" at the limit tells the customer how many are left instead of adding.
    public static readonly BindableProperty MaxQuantityProperty =
        BindableProperty.Create(nameof(MaxQuantity), typeof(int), typeof(QuantitySelectorView), 0);

    public int MaxQuantity
    {
        get => (int)GetValue(MaxQuantityProperty);
        set => SetValue(MaxQuantityProperty, value);
    }

    public event EventHandler<int>? QuantityChanged;

    private void OnDecreaseTapped(object? sender, TappedEventArgs e)
    {
        AppVibrationService.Like();
        
        if (Quantity > 1)
        {
            Quantity--;
            NotifyQuantityChanged();
        }
    }

    private void OnIncreaseTapped(object? sender, TappedEventArgs e)
    {
        AppVibrationService.Like();
        if (MaxQuantity > 0 && Quantity >= MaxQuantity)
        {
            _ = ShowStockLimitAsync(MaxQuantity);
            return;
        }
        Quantity++;
        NotifyQuantityChanged();
    }

    private static async Task ShowStockLimitAsync(int left)
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null) return;
        var lang = (Ninimum.Resources.Languages.AppResource.Culture ?? System.Globalization.CultureInfo.CurrentUICulture).TwoLetterISOLanguageName;
        string title = lang == "ru" ? "Больше нет" : lang == "en" ? "No more in stock" : "Boshqa qolmadi";
        string text = lang == "ru" ? $"На складе осталось только {left} шт. этого товара."
            : lang == "en" ? $"Only {left} of this product left in stock."
            : $"Bu mahsulotdan omborda faqat {left} dona qolgan.";
        await page.DisplayAlert(title, text, Ninimum.Resources.Languages.AppResource.Ok);
    }

    private void NotifyQuantityChanged()
    {
        QuantityChanged?.Invoke(this, Quantity);

        if (QuantityChangedCommand?.CanExecute(Quantity) == true)
            QuantityChangedCommand.Execute(Quantity);
    }
}