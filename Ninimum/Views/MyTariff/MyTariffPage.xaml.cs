using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Samokat.Models;
using Samokat.Models.Tariff;

namespace Samokat.Views.MyTariff;

public partial class MyTariffPage : BasePage, INotifyPropertyChanged
{
    private bool _isHistoryVisible = true;

    public ObservableCollection<TariffItem> TariffHistory { get; set; } = new();

    public bool HasHistory => TariffHistory.Any();

    public new event PropertyChangedEventHandler? PropertyChanged;

    public bool IsHistoryVisible
    {
        get => _isHistoryVisible;
        set
        {
            if (_isHistoryVisible != value)
            {
                _isHistoryVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HistoryArrowIcon));
            }
        }
    }

    public string HistoryArrowIcon =>
        IsHistoryVisible ? "ic_arrow_up.png" : "ic_arrow_down.png";

    public ICommand ToggleHistoryCommand { get; }

    public MyTariffPage()
    {
        InitializeComponent();

        ToggleHistoryCommand = new Command(() =>
        {
            IsHistoryVisible = !IsHistoryVisible;
        });

        // TEST DATA (remove later)
        TariffHistory.Add(new TariffItem
        {
            Plan = "Gold",
            StartDate = "26.02.2026",
            EndDate = "25.03.2026",
            Price = "99 000 so’m"
        });

        TariffHistory.Add(new TariffItem
        {
            Plan = "Gold",
            StartDate = "26.01.2026",
            EndDate = "25.02.2026",
            Price = "99 000 so’m"
        });

        OnPropertyChanged(nameof(HasHistory));
        
        BindingContext = this;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}