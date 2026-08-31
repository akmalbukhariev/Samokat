using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Api.Services;
using Models.Requests;
using Models.Responses;
using Ninimum.Models.Tariff;
using Ninimum.Services;
using Utils;

namespace Ninimum.Views.MyTariff;

public partial class MyTariffPage : BasePage, INotifyPropertyChanged
{
    private readonly UserApiService apiService;
    private readonly AppControl appControl;
    private bool _isHistoryVisible = true;
    private bool _hasActiveSubscription;
    private string _currentTariffName = string.Empty;
    private string _currentStartDate = string.Empty;
    private string _currentEndDate = string.Empty;
    private string _currentRemainingText = string.Empty;
    private string _currentPrice = string.Empty;

    public ObservableCollection<TariffItem> TariffHistory { get; } = new();
    public bool HasHistory => TariffHistory.Any();

    public new event PropertyChangedEventHandler? PropertyChanged;

    public bool HasActiveSubscription
    {
        get => _hasActiveSubscription;
        set
        {
            if (_hasActiveSubscription == value)
                return;

            _hasActiveSubscription = value;
            OnPropertyChanged();
        }
    }

    public string CurrentTariffName
    {
        get => _currentTariffName;
        set { _currentTariffName = value; OnPropertyChanged(); }
    }

    public string CurrentStartDate
    {
        get => _currentStartDate;
        set { _currentStartDate = value; OnPropertyChanged(); }
    }

    public string CurrentEndDate
    {
        get => _currentEndDate;
        set { _currentEndDate = value; OnPropertyChanged(); }
    }

    public string CurrentRemainingText
    {
        get => _currentRemainingText;
        set { _currentRemainingText = value; OnPropertyChanged(); }
    }

    public string CurrentPrice
    {
        get => _currentPrice;
        set { _currentPrice = value; OnPropertyChanged(); }
    }

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

    public string HistoryArrowIcon => IsHistoryVisible ? "ic_arrow_up.png" : "ic_arrow_down.png";

    public ICommand ToggleHistoryCommand { get; }
    public ICommand JoinTariffCommand { get; }

    public MyTariffPage(UserApiService apiService, AppControl appControl)
    {
        InitializeComponent();

        this.apiService = apiService;
        this.appControl = appControl;

        ToggleHistoryCommand = new Command(() => IsHistoryVisible = !IsHistoryVisible);
        JoinTariffCommand = new Command(async () => await AppNavigatorService.NavigateTo(nameof(TariffsPage)));

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!await appControl.EnsureAuthenticatedAsync(true))
            return;

        await LoadTariffAsync();
    }

    private async Task LoadTariffAsync()
    {
        try
        {
            loading.ShowLoading = true;

            var request = new ActiveSubscriptionRequest
            {
                userId = appControl.userDto.id ?? 0
            };

            ActiveSubscriptionResponse activeResponse = await apiService.GetActiveSubscription(request);
            var active = activeResponse.resultData;

            HasActiveSubscription =
                activeResponse.resultCode == ApiResult.SUCCESS.GetCodeToString() &&
                active != null &&
                string.Equals(active.subscriptionStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase);

            if (HasActiveSubscription)
            {
                CurrentTariffName = active!.tariffName;
                CurrentStartDate = FormatDate(active.startDate);
                CurrentEndDate = FormatDate(active.endDate);
                CurrentRemainingText = GetRemainingDaysText(active.endDate);
                CurrentPrice = FormatPrice(active.price);
            }
            else
            {
                CurrentTariffName = string.Empty;
                CurrentStartDate = string.Empty;
                CurrentEndDate = string.Empty;
                CurrentRemainingText = string.Empty;
                CurrentPrice = string.Empty;
            }

            await LoadHistoryAsync(request);
        }
        catch
        {
            HasActiveSubscription = false;
            TariffHistory.Clear();
            OnPropertyChanged(nameof(HasHistory));
        }
        finally
        {
            loading.ShowLoading = false;
        }
    }

    private async Task LoadHistoryAsync(ActiveSubscriptionRequest request)
    {
        SubscriptionListResponse historyResponse = await apiService.GetSubscriptionList(request);

        TariffHistory.Clear();

        if (historyResponse.resultCode == ApiResult.SUCCESS.GetCodeToString() && historyResponse.resultData != null)
        {
            foreach (var item in historyResponse.resultData)
            {
                TariffHistory.Add(new TariffItem
                {
                    Plan = item.tariffName,
                    StartDate = FormatDate(item.startDate),
                    EndDate = FormatDate(item.endDate),
                    Price = FormatPrice(item.price)
                });
            }
        }

        OnPropertyChanged(nameof(HasHistory));
    }

    private static string FormatDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "-";

        if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            return date.ToString("dd.MM.yyyy");

        return value;
    }

    private static string GetRemainingDaysText(string endDate)
    {
        if (!DateTime.TryParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime end))
            return string.Empty;

        int days = Math.Max(0, (end.Date - DateTime.Today).Days);
        return $"{days} kun qoldi";
    }

    private static string FormatPrice(int price)
    {
        return $"{price:N0} so’m".Replace(",", " ");
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
