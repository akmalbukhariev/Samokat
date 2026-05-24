using System.Windows.Input;

namespace Ninimum.Views.Main.Components;

public partial class ProductItemView : ContentView
{
    public static readonly BindableProperty AddToCartCommandProperty =
        BindableProperty.Create(
            nameof(AddToCartCommand),
            typeof(ICommand),
            typeof(ProductItemView));

    public ICommand AddToCartCommand
    {
        get => (ICommand)GetValue(AddToCartCommandProperty);
        set => SetValue(AddToCartCommandProperty, value);
    }

    public ProductItemView()
    {
        InitializeComponent();
    }
}