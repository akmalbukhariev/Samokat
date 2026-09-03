using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Api.Services;
using Models.Requests;
using Models.Responses;
using Ninimum.Services;
using Ninimum.Models.Startup;
using Ninimum.Views.Orders;
using Ninimum.Views.MyTariff;
using Utils;
using Ninimum.Views.PaymentCard;

namespace Ninimum.Views.Profile;

public partial class MyProfilePage : BasePage, INotifyPropertyChanged
{
    #region Commands
    public ICommand OrderCommand { get; }
    public ICommand ReviewCommand { get; }
    public ICommand PaymentCardCommand { get; }
    public ICommand MessageCommand { get; }
    public ICommand NotificationCommand { get; }
    public ICommand ToggleSettingsCommand { get; }

    public ICommand RegionCommand { get; }
    public ICommand RegionSelectedCommand { get; }
    public ICommand LanguageCommand { get; }
    public ICommand LanguageSelectedCommand { get; }
    public ICommand ThemeCommand { get; }
    public ICommand ChangePhoneCommand { get; }
    public ICommand ChangePasswordCommand { get; }
    public ICommand MyTariffCommand { get; }
    public ICommand ChildrenCommand { get; }
    public ICommand DeleteAccountCommand { get; }
    public ICommand LogoutCommand { get; }
    #endregion

    private bool _isSettingsExpanded = true;
    private string _selectedLanguageFlag = "flag_uz.png";
    private string _currentTariffName = "Tarif yo‘q";
    private bool _isProfileBusy;
    private string _selectedRegionName = "Qashqadaryo";
    private bool _isRegionUpdating;

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    public bool IsSettingsExpanded
    {
        get => _isSettingsExpanded;
        set
        {
            if (_isSettingsExpanded != value)
            {
                _isSettingsExpanded = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SettingsArrowIcon));
            }
        }
    }

    public string SettingsArrowIcon => IsSettingsExpanded ? "ic_arrow_up.png" : "ic_arrow_down.png";

    public ObservableCollection<PopupItemModel> RegionItems => appControl.RegionItems;
    public ObservableCollection<PopupItemModel> LanguageItems { get; } = new();

    public string MaskedPhoneNumber
    {
        get
        {
            string digits = new string((appControl.userDto.phone_number ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digits.Length < 6)
                return appControl.userDto.phone_number ?? string.Empty;

            return $"...{digits[^6..]}";
        }
    }

    public bool IsProfileBusy
    {
        get => _isProfileBusy;
        set
        {
            if (_isProfileBusy == value)
                return;

            _isProfileBusy = value;
            OnPropertyChanged();
        }
    }

    public string SelectedRegionName
    {
        get => _selectedRegionName;
        set
        {
            if (_selectedRegionName != value)
            {
                _selectedRegionName = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsRegionUpdating
    {
        get => _isRegionUpdating;
        set
        {
            if (_isRegionUpdating != value)
            {
                _isRegionUpdating = value;
                OnPropertyChanged();
            }
        }
    }

    public string CurrentTariffName
    {
        get => _currentTariffName;
        set
        {
            if (_currentTariffName != value)
            {
                _currentTariffName = value;
                OnPropertyChanged();
            }
        }
    }

    public string SelectedLanguageFlag
    {
        get => _selectedLanguageFlag;
        set
        {
            if (_selectedLanguageFlag != value)
            {
                _selectedLanguageFlag = value;
                OnPropertyChanged();
            }
        }
    }
 
    private readonly AppControl appControl;
    private readonly UserApiService apiService;
    private readonly LanguageService languageService;

    public MyProfilePage(AppControl appControl, UserApiService apiService, LanguageService languageService)
    {
        InitializeComponent();
        this.appControl = appControl;
        this.apiService = apiService;
        this.languageService = languageService;

        OrderCommand = new Command(OnOrderClicked);
        ReviewCommand = new Command(OnReviewClicked);
        PaymentCardCommand = new Command(OnPaymentCardClicked);
        MessageCommand = new Command(OnMessageClicked);
        NotificationCommand = new Command(OnNotificationClicked);

        ToggleSettingsCommand = new Command(() =>
        {
            AppVibrationService.Like();
            IsSettingsExpanded = !IsSettingsExpanded;
        });

        RegionCommand = new Command(OnRegionClicked);
        RegionSelectedCommand = new Command<PopupItemModel>(OnRegionSelected);
        LanguageCommand = new Command(OnLanguageClicked);
        LanguageSelectedCommand = new Command<PopupItemModel>(OnLanguageSelected);
        ThemeCommand = new Command(OnThemeClicked);
        ChangePhoneCommand = new Command(OnChangePhoneClicked);
        ChangePasswordCommand = new Command(OnChangePasswordClicked);
        MyTariffCommand = new Command(OnMyTariffClicked);
        ChildrenCommand = new Command(OnChildrenClicked);
        DeleteAccountCommand = new Command(OnDeleteAccountClicked);
        LogoutCommand = new Command(OnLogoutClicked);

        logOutPopup.Confirmed += OnLogoutConfirmed;
        logOutPopup.Closed += (s, e) =>
        {
            appControl.ShowTabBar(true);
        };

        RegionPopup.Closed += (s, e) =>
        {
            appControl.ShowTabBar(true);
        };

        LanguagePopup.Closed += (s, e) =>
        {
            appControl.ShowTabBar(true);
        };

        PrepareLanguageItems();

        BindingContext = this;

        Shell.SetTabBarIsVisible(this, true);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!await appControl.EnsureAuthenticatedAsync(true))
            return;

        lbUserName.Text = appControl.userDto.first_name;
        lbPhoneNumber.Text = appControl.userDto.phone_number;
        OnPropertyChanged(nameof(MaskedPhoneNumber));

        ApplyLanguageSelection(languageService.GetCurrentLanguage(), persist: false);

        await appControl.LoadRegionsAsync();
        appControl.SelectRegion(appControl.userDto.region_id ?? appControl.SelectedRegionId);
        SelectedRegionName = appControl.CurrentRegionName;
        RegionPopup.Refresh();

        await LoadCurrentTariffAsync();
    }

    private void OnLogoutClicked()
    {
        appControl.ShowTabBar(false);
        logOutPopup.ShowConfirm();
    }

    private async void OnLogoutConfirmed(object? sender, EventArgs e)
    {
        logOutPopup.IsVisible = false;
        appControl.ShowTabBar(true);

        IsLoading = true;
        await appControl.Logout();
        IsLoading = false;
    }

    private async void OnOrderClicked()
    {
        AppVibrationService.Like();
        await AppNavigatorService.NavigateTo(nameof(OrdersPage));
    }

    private async void OnReviewClicked()
    {
        await DisplayAlert("Clicked", "Sharh", "OK");
        // await Navigation.PushAsync(new ReviewsPage());
    }

    private async void OnPaymentCardClicked()
    {
        //await DisplayAlert("Clicked", "To’lov karta", "OK");
        //await Navigation.PushAsync(new PaymentCardsPage());
        await AppNavigatorService.NavigateTo(nameof(PaymentCardPage));
    }

    private async void OnMessageClicked()
    {
        AppVibrationService.Like();

        await DisplayAlert("Clicked", "Yozishma", "OK");
        // await Navigation.PushAsync(new ChatListPage());
    }

    private async void OnNotificationClicked()
    {
        await DisplayAlert("Clicked", "Xabarnoma", "OK");
        // await Navigation.PushAsync(new NotificationPage());
    }

    private void OnRegionClicked()
    {
        AppVibrationService.Like();

        if (IsRegionUpdating)
            return;

        if (appControl.RegionItems.Count == 0)
            return;

        appControl.SelectRegion(
            appControl.userDto.region_id ?? appControl.SelectedRegionId);

        appControl.ShowTabBar(false);

        RegionPopup.IsVisible = true;
    }

    private async void OnRegionSelected(PopupItemModel item)
    {
        if (item == null || item.Id <= 0 || IsRegionUpdating)
            return;

        int currentRegionId = appControl.userDto.region_id ?? appControl.SelectedRegionId;

        if (item.Id == currentRegionId)
        {
            RegionPopup.IsVisible = false;
            appControl.ShowTabBar(true);
            return;
        }

        try
        {
            IsRegionUpdating = true;

            var response = await apiService.ChangeRegion(new ChangeRegionRequest
            {
                userId = appControl.CurrentUserId,
                regionId = item.Id
            });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                await DisplayAlert(
                    "Xatolik",
                    "Regionni o‘zgartirib bo‘lmadi. Iltimos, qayta urinib ko‘ring.",
                    "Yopish");
                return;
            }

            appControl.userDto.region_id = item.Id;
            appControl.SelectRegion(item.Id);
            SelectedRegionName = item.Text;

            RegionPopup.Refresh();
            RegionPopup.IsVisible = false;
            appControl.ShowTabBar(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] ChangeRegion => {ex}");

            await DisplayAlert(
                "Xatolik",
                "Regionni o‘zgartirib bo‘lmadi. Iltimos, qayta urinib ko‘ring.",
                "Yopish");
        }
        finally
        {
            IsRegionUpdating = false;
        }
    }

    private void OnLanguageClicked()
    {
        AppVibrationService.Like();
        RefreshLanguageSelection(languageService.GetCurrentLanguage());
        appControl.ShowTabBar(false);
        LanguagePopup.IsVisible = true;
    }

    private void OnLanguageSelected(PopupItemModel item)
    {
        if (item == null)
            return;

        string cultureCode = item.Id switch
        {
            2 => AppConstants.RU,
            3 => AppConstants.EN,
            _ => AppConstants.UZ
        };

        ApplyLanguageSelection(cultureCode, persist: true);
        LanguagePopup.Refresh();
        LanguagePopup.IsVisible = false;
        appControl.ShowTabBar(true);
    }

    private void PrepareLanguageItems()
    {
        if (LanguageItems.Count > 0)
            return;

        LanguageItems.Add(new PopupItemModel { Id = 1, Text = "O‘zbek", LeftImage = AppConstants.LAN_ICON_UZBEK });
        LanguageItems.Add(new PopupItemModel { Id = 2, Text = "Русский", LeftImage = AppConstants.LAN_ICON_RUSSIAN });
        LanguageItems.Add(new PopupItemModel { Id = 3, Text = "English", LeftImage = AppConstants.LAN_ICON_ENGLISH });

        RefreshLanguageSelection(languageService.GetCurrentLanguage());
    }

    private void ApplyLanguageSelection(string cultureCode, bool persist)
    {
        // Translation resources can be connected here later. For now this method
        // stores the selected culture and updates the profile language indicator.
        if (persist)
            languageService.SetCulture(cultureCode);

        SelectedLanguageFlag = cultureCode switch
        {
            AppConstants.RU => AppConstants.LAN_ICON_RUSSIAN,
            AppConstants.EN => AppConstants.LAN_ICON_ENGLISH,
            _ => AppConstants.LAN_ICON_UZBEK
        };

        RefreshLanguageSelection(cultureCode);
        OnLanguageChanged(cultureCode);
    }

    private void RefreshLanguageSelection(string cultureCode)
    {
        int selectedId = cultureCode switch
        {
            AppConstants.RU => 2,
            AppConstants.EN => 3,
            _ => 1
        };

        foreach (var item in LanguageItems)
            item.RightImage = item.Id == selectedId ? "check_gray.png" : string.Empty;
    }

    private void OnLanguageChanged(string cultureCode)
    {
        System.Diagnostics.Debug.WriteLine($"LANGUAGE CHANGED => {cultureCode}");
        // Add resource refresh/reload logic here when translation files are ready.
    }

    private async void OnThemeClicked()
    {
        await DisplayAlert("Clicked", "Ko’rinish rejimi", "OK");
    }

    private async void OnChangePhoneClicked()
    {
        AppVibrationService.Like();
        await AppNavigatorService.NavigateTo(nameof(Ninimum.Views.ChangePhoneNumber.ChangePhoneNumberPage));
    }

    private async void OnChangePasswordClicked()
    {
        AppVibrationService.Like();
        await AppNavigatorService.NavigateTo("ProfileChangePasswordPage");
    }

    private async void OnMyTariffClicked()
    {
        AppVibrationService.Like();
        await AppNavigatorService.NavigateTo(nameof(MyTariffPage));
    }

    private async Task LoadCurrentTariffAsync()
    {
        try
        {
            ActiveSubscriptionResponse response = await apiService.GetActiveSubscription(new ActiveSubscriptionRequest
            {
                userId = appControl.userDto.id ?? 0
            });

            var subscription = response.resultData;
            bool isActive =
                response.resultCode == ApiResult.SUCCESS.GetCodeToString() &&
                subscription != null &&
                string.Equals(subscription.subscriptionStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase);

            CurrentTariffName = isActive ? subscription!.tariffName : "Tarif yo‘q";
        }
        catch
        {
            CurrentTariffName = "Tarif yo‘q";
        }
    }

    private async void OnChildrenClicked()
    {
        await DisplayAlert("Clicked", "Farzandlarim", "OK");
    }

    private async void OnDeleteAccountClicked()
    {
        AppVibrationService.Like();

        if (IsProfileBusy)
            return;

        await AppNavigatorService.NavigateTo(nameof(DeleteAccountPage));
    }
    
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}