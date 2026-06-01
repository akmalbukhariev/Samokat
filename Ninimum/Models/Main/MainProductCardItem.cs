using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ninimum.Models.Main;

public partial class MainProductCardItem : ObservableObject
{
    [ObservableProperty] private ObservableCollection<MainProductImageItem> images = new();

    [ObservableProperty] private bool isFavorite;

    [ObservableProperty] private string price = string.Empty;
    [ObservableProperty] private string subscription_price = string.Empty;

    [ObservableProperty] private string title = string.Empty;

    [ObservableProperty] private double rating;
    [ObservableProperty] private int reviewCount;

    [ObservableProperty] private string actionText = "Ertaga";
}