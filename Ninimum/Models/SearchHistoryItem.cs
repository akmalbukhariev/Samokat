using CommunityToolkit.Mvvm.ComponentModel;

namespace Ninimum.Models;

public partial class SearchHistoryItem: ObservableObject
{
    [ObservableProperty] private string searchedText = "";
}