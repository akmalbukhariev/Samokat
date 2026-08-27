using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Api.Services;
using Models.Requests;
using Models.Responses;
using Ninimum.Services;
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
    public ICommand LanguageCommand { get; }
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
    public MyProfilePage(AppControl appControl, UserApiService apiService)
    {
        InitializeComponent();
        this.appControl = appControl;
        this.apiService = apiService;

        OrderCommand = new Command(OnOrderClicked);
        ReviewCommand = new Command(OnReviewClicked);
        PaymentCardCommand = new Command(OnPaymentCardClicked);
        MessageCommand = new Command(OnMessageClicked);
        NotificationCommand = new Command(OnNotificationClicked);

        ToggleSettingsCommand = new Command(() =>
        {
            IsSettingsExpanded = !IsSettingsExpanded;
        });

        RegionCommand = new Command(OnRegionClicked);
        LanguageCommand = new Command(OnLanguageClicked);
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
        await DisplayAlert("Logout", "Logged out", "OK");
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
        await DisplayAlert("Clicked", "Yozishma", "OK");
        // await Navigation.PushAsync(new ChatListPage());
    }

    private async void OnNotificationClicked()
    {
        await DisplayAlert("Clicked", "Xabarnoma", "OK");
        // await Navigation.PushAsync(new NotificationPage());
    }

    private async void OnRegionClicked()
    {
        await DisplayAlert("Clicked", "Region", "OK");
    }

    private async void OnLanguageClicked()
    {
        var selected = await DisplayActionSheet(
            "Tilni tanlang",
            "Bekor qilish",
            null,
            "O‘zbek",
            "Русский",
            "English");

        if (selected == "O‘zbek")
            SelectedLanguageFlag = "flag_uz.png";
        else if (selected == "Русский")
            SelectedLanguageFlag = "flag_ru.png";
        else if (selected == "English")
            SelectedLanguageFlag = "flag_en.png";
    }

    private async void OnThemeClicked()
    {
        await DisplayAlert("Clicked", "Ko’rinish rejimi", "OK");
    }

    private async void OnChangePhoneClicked()
    {
        await DisplayAlert("Clicked", "Telefon raqamni o’zgartirish", "OK");
    }

    private async void OnChangePasswordClicked()
    {
        await DisplayAlert("Clicked", "Parolni o’zgartirish", "OK");
    }

    private async void OnMyTariffClicked()
    {
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
        await DisplayAlert("Clicked", "Akkauntni o’chirish", "OK");
    }
 
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}