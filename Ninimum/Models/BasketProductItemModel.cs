using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace Ninimum.Models;

public partial class BasketProductItemModel : ObservableObject
{
    [ObservableProperty]
    private string productImageSource = string.Empty;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string oldPrice = string.Empty;

    [ObservableProperty]
    private string newPrice = string.Empty;

    [ObservableProperty]
    private bool isChecked;

    [ObservableProperty]
    private int quantity = 1;

    public int NewPriceValue { get; set; }

    public ICommand? ParentChangedCommand { get; set; }

    public IRelayCommand ToggleCheckedCommand { get; }
    public IRelayCommand<int> QuantityChangedCommand { get; }

    public BasketProductItemModel()
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