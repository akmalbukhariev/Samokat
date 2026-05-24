using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Samokat.Models;
using Samokat.Models.Main;
using System.Collections.ObjectModel;

namespace Samokat.ViewModels;

public partial class BasketProductPageViewModel : ObservableObject
{
    public ObservableCollection<BasketProductItemModel> BasketProducts { get; } = new();

    public BasketProductPageViewModel()
    {
        BasketProducts.Add(new BasketProductItemModel
        {
            ProductImageSource = "product_1.png",
            Title = "Kabrita 3 GOLD echki sutiga asoslangan kukunli sutli ichimlik, 12+ oy, 800 g",
            OldPrice = "545 000",
            NewPrice = "486 000",
            NewPriceValue = 486000,
            IsChecked = true,
            Quantity = 1,
            ParentChangedCommand = new RelayCommand(UpdateSummary)
        });

        BasketProducts.Add(new BasketProductItemModel
        {
            ProductImageSource = "product_2.png",
            Title = "Frigg Daisy emzik, 0-6 oy.",
            OldPrice = "66 000",
            NewPrice = "60 000",
            NewPriceValue = 60000,
            IsChecked = false,
            Quantity = 1,
            ParentChangedCommand = new RelayCommand(UpdateSummary)
        });

        UpdateSummary();
    }

    public string BasketCountText => $"Savatchadagi mahsulotlar soni {BasketProducts.Count} ta";

    [ObservableProperty]
    private string selectAllIcon = "ic_uncheck.png";
    
    [ObservableProperty]
    private string summaryTopText = "Savatchadagi tanlangan 0 ta mahsulotni";

    [ObservableProperty]
    private string totalSelectedPrice = "0";

    [RelayCommand]
    private void ToggleSelectAll()
    {
        bool shouldSelectAll = BasketProducts.Any(x => !x.IsChecked);

        foreach (var item in BasketProducts)
            item.IsChecked = shouldSelectAll;

        UpdateSummary();
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        var selected = BasketProducts.Where(x => x.IsChecked).ToList();

        foreach (var item in selected)
            BasketProducts.Remove(item);

        OnPropertyChanged(nameof(BasketCountText));
        UpdateSummary();
    }
    
    [RelayCommand]
    private void JoinTariff()
    {
        // later navigation or popup
    }

    private void UpdateSummary()
    {
        int selectedCount = BasketProducts.Count(x => x.IsChecked);
        int total = BasketProducts
            .Where(x => x.IsChecked)
            .Sum(x => x.NewPriceValue * x.Quantity);

        SummaryTopText = $"Savatchadagi tanlangan {selectedCount} ta mahsulotni";
        TotalSelectedPrice = FormatPrice(total);
        SelectAllIcon = BasketProducts.Any() && BasketProducts.All(x => x.IsChecked)
            ? "ic_check.png"
            : "ic_uncheck.png";

        OnPropertyChanged(nameof(BasketCountText));
    }

    private string FormatPrice(int value)
    {
        return string.Format("{0:N0}", value).Replace(",", " ");
    }
}