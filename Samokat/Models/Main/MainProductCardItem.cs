using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Samokat.Models.Main;

public partial class MainProductCardItem : ObservableObject
{
    [ObservableProperty] private ObservableCollection<MainProductImageItem> images = new();

    [ObservableProperty] private bool isFavorite;

    [ObservableProperty] private string oldPrice = string.Empty;
    [ObservableProperty] private string newPrice = string.Empty;

    [ObservableProperty] private string title = string.Empty;

    [ObservableProperty] private double rating;
    [ObservableProperty] private int reviewCount;

    [ObservableProperty] private string actionText = "Ertaga";
}