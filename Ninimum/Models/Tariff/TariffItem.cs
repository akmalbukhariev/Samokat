using CommunityToolkit.Mvvm.ComponentModel;

namespace Ninimum.Models.Tariff;

public partial class TariffItem : ObservableObject
{
    [ObservableProperty] private  string plan = "";
    [ObservableProperty] private  string startDate = "";
    [ObservableProperty] private  string endDate = "";
    [ObservableProperty] private  string price = "";
}