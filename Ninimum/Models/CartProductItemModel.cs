using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace Ninimum.Models;

public partial class CartProductItemModel : ObservableObject
{
    public int CartId { get; set; }
    public int ProductId { get; set; }
    
    [ObservableProperty] private string productImageSource = string.Empty;

    [ObservableProperty] private string title = string.Empty;

    [ObservableProperty] private string price = string.Empty;

    [ObservableProperty] private string subscriptionPrice = string.Empty;

    public int PriceValue { get; set; }
    public int SubscriptionPriceValue { get; set; }

    [ObservableProperty] private bool isChecked;
    
    [ObservableProperty] private int quantity = 1;
    
    public ICommand? ParentChangedCommand { get; set; }

    public IRelayCommand ToggleCheckedCommand { get; }
    public IRelayCommand<int> QuantityChangedCommand { get; }

    public CartProductItemModel()
    {
        ToggleCheckedCommand = new RelayCommand(OnToggleChecked);
        QuantityChangedCommand = new RelayCommand<int>(HandleQuantityChanged);
    }

    private void OnToggleChecked()
    {
        IsChecked = !IsChecked;
        ParentChangedCommand?.Execute(null);
    }

    private void HandleQuantityChanged(int qty)
    {
        Quantity = qty;
        ParentChangedCommand?.Execute(null);
    }

    partial void OnIsCheckedChanged(bool value)
    {
        ParentChangedCommand?.Execute(null);
    }

    partial void OnQuantityChanged(int value)
    {
        ParentChangedCommand?.Execute(null);
    }
}