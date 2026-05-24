using System.Windows.Input;

namespace Ninimum.Views.BasketProduct;

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

    public event EventHandler<int>? QuantityChanged;

    private void OnDecreaseTapped(object? sender, TappedEventArgs e)
    {
        if (Quantity > 1)
        {
            Quantity--;
            NotifyQuantityChanged();
        }
    }

    private void OnIncreaseTapped(object? sender, TappedEventArgs e)
    {
        Quantity++;
        NotifyQuantityChanged();
    }

    private void NotifyQuantityChanged()
    {
        QuantityChanged?.Invoke(this, Quantity);

        if (QuantityChangedCommand?.CanExecute(Quantity) == true)
            QuantityChangedCommand.Execute(Quantity);
    }
}