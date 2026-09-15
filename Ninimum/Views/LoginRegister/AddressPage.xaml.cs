using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Api.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Ninimum.Models;
using Ninimum.Services;
using Ninimum.Views.Formalization;

namespace Ninimum.Views.LoginRegister;

public partial class AddressPage : BasePage, INotifyPropertyChanged
{
    // The map is intentionally limited to Qashqadaryo for the first Ninimum delivery area.
    // Shahrisabz is the default focus when no previously saved address is available.
    private const double ShahrisabzLatitude = 39.0578;
    private const double ShahrisabzLongitude = 66.8342;
    private const double DefaultMapRadiusKm = 5;
    private const double AddressMapRadiusKm = 1;
    private const double MaxMapRadiusKm = 90;

    // A practical bounding box around Qashqadaryo viloyati.
    // It keeps the user from accidentally moving the map to another country/region.
    private const double QashqadaryoMinLatitude = 37.85;
    private const double QashqadaryoMaxLatitude = 39.70;
    private const double QashqadaryoMinLongitude = 64.10;
    private const double QashqadaryoMaxLongitude = 67.85;

    private double latitude;
    private double longitude;
    private string addressText = string.Empty;
    private double panStartHeight;
    private bool isDeliveryAvailable;

    private CancellationTokenSource? mapMoveCts;
    private bool isMapMoving;
    private bool isUpdatingMapProgrammatically;
    private bool isSettingAddressTextProgrammatically;
    private bool hasPendingAddressSearch;

    private const double MapModeHeight = 300;
    private const double SearchModeHeight = 620;
    private const double MapModeHeightWithWarning = 300;
    private const double MapModeHeightWithoutWarning = 220;

    public string AddressText
    {
        get => addressText;
        set
        {
            if (addressText == value)
                return;

            addressText = value;
            OnPropertyChanged();
        }
    }

    public ICommand ConfirmAddressCommand { get; }

    private readonly UserApiService apiService;
    private readonly IKeyboardHelper keyboardHelper;

    public AddressPage(UserApiService apiService, IKeyboardHelper keyboardHelper)
    {
        InitializeComponent();

        this.apiService = apiService;
        this.keyboardHelper = keyboardHelper;

        ConfirmAddressCommand = new Command(OnConfirmAddress);
        BindingContext = this;

        Loaded += AddressPage_Loaded;
        Unloaded += AddressPage_Unloaded;
        map.PropertyChanged += Map_PropertyChanged;

        ShowMapMode(false);
    }

    private async void AddressPage_Loaded(object sender, EventArgs e)
    {
        await InitializeMapAsync();
    }

    private void AddressPage_Unloaded(object sender, EventArgs e)
    {
        map.PropertyChanged -= Map_PropertyChanged;
        mapMoveCts?.Cancel();
    }

    private async Task InitializeMapAsync()
    {
        var navigationData = AddressSelectionNavigationStore.Data;

        // 1. Existing coordinates have priority. This is what makes checkout address editing
        //    open exactly where the user's current saved/selected address is.
        if (navigationData?.Latitude is double initialLatitude &&
            navigationData.Longitude is double initialLongitude &&
            IsInsideQashqadaryo(initialLatitude, initialLongitude))
        {
            await MoveMapToAsync(
                initialLatitude,
                initialLongitude,
                AddressMapRadiusKm,
                navigationData.AddressText);
            return;
        }

        // 2. Older users may have address text but no usable coordinates. Try to resolve the
        //    existing address inside Qashqadaryo before falling back to Shahrisabz.
        if (!string.IsNullOrWhiteSpace(navigationData?.AddressText))
        {
            var resolved = await apiService.SearchAddressInQashqadaryoAsync(navigationData.AddressText);

            if (resolved != null && IsInsideQashqadaryo(resolved.Latitude, resolved.Longitude))
            {
                await MoveMapToAsync(
                    resolved.Latitude,
                    resolved.Longitude,
                    AddressMapRadiusKm,
                    resolved.Address);
                return;
            }
        }

        // 3. Registration/new address starts from Shahrisabz instead of the phone's current
        //    GPS location. This prevents the map from opening in Korea or any unrelated place.
        await MoveMapToAsync(
            ShahrisabzLatitude,
            ShahrisabzLongitude,
            DefaultMapRadiusKm);
    }

    private async void MapButton_Tapped(object sender, TappedEventArgs e)
    {
        keyboardHelper.HideKeyboard();
        AddressEntry.Unfocus();

        if (hasPendingAddressSearch && !await SearchTypedAddressAsync())
        {
            await ShowSearchMode(false);
            return;
        }

        await ShowMapMode(true);
    }

    private async void MyLocation_Tapped(object sender, TappedEventArgs e)
    {
        keyboardHelper.HideKeyboard();
        AddressEntry.Unfocus();

        await ShowMapMode(true);
        await MoveToCurrentLocation();
    }

    private async void AddressEntry_Focused(object sender, FocusEventArgs e)
    {
        await ShowSearchMode(true);
    }

    private async void AddressEntry_Unfocused(object sender, FocusEventArgs e)
    {
        // Keep the expanded panel until the user explicitly switches back to the map.
    }

    private void AddressEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!isSettingAddressTextProgrammatically)
            hasPendingAddressSearch = true;
    }

    private async void AddressEntry_Completed(object sender, EventArgs e)
    {
        AddressEntry.Unfocus();
        keyboardHelper.HideKeyboard();

        if (await SearchTypedAddressAsync())
            await ShowMapMode(true);
        else
            await ShowSearchMode(false);
    }

    private async Task<bool> SearchTypedAddressAsync()
    {
        if (!hasPendingAddressSearch)
            return true;

        string query = AddressText?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(query))
            return false;

        try
        {
            IsBusy = true;

            var result = await apiService.SearchAddressInQashqadaryoAsync(query);

            if (result == null || !IsInsideQashqadaryo(result.Latitude, result.Longitude))
            {
                await AlertService.ShowAlertAsync(
                    "Manzil topilmadi",
                    "Qashqadaryo viloyatidagi tuman, ko‘cha, uy yoki mo‘ljalni kiriting.");
                return false;
            }

            hasPendingAddressSearch = false;

            await MoveMapToAsync(
                result.Latitude,
                result.Longitude,
                AddressMapRadiusKm,
                result.Address);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Address search error: {ex.Message}");

            await AlertService.ShowAlertAsync(
                "Xatolik",
                "Manzilni qidirib bo‘lmadi. Iltimos, qayta urinib ko‘ring.");

            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private double GetMapModeHeight()
    {
        return isDeliveryAvailable
            ? MapModeHeightWithoutWarning
            : MapModeHeightWithWarning;
    }

    private async Task ShowMapMode(bool animated)
    {
        MapButtonLabel.IsVisible = false;
        MapColumn.Width = new GridLength(0);

        HintLabel.IsVisible = false;
        MyLocationRow.IsVisible = false;
        DividerLine.IsVisible = false;

        NoDeliveryLabel.IsVisible = !isDeliveryAvailable;
        PickupButton.IsVisible = true;

        double targetHeight = GetMapModeHeight();

        if (animated)
            await AnimatePanelHeight(targetHeight);
        else
            BottomPanel.HeightRequest = targetHeight;
    }

    private async Task ShowSearchMode(bool animated)
    {
        MapButtonLabel.IsVisible = true;
        MapColumn.Width = new GridLength(80);

        NoDeliveryLabel.IsVisible = false;
        PickupButton.IsVisible = false;

        HintLabel.IsVisible = true;
        MyLocationRow.IsVisible = true;
        DividerLine.IsVisible = true;

        if (animated)
            await AnimatePanelHeight(SearchModeHeight);
        else
            BottomPanel.HeightRequest = SearchModeHeight;
    }

    private async Task AnimatePanelHeight(double targetHeight)
    {
        double startHeight = BottomPanel.Height <= 0
            ? BottomPanel.HeightRequest
            : BottomPanel.Height;

        if (startHeight <= 0)
            startHeight = MapModeHeight;

        var animation = new Animation(v =>
        {
            BottomPanel.HeightRequest = v;
        }, startHeight, targetHeight);

        animation.Commit(
            this,
            "BottomPanelHeightAnimation",
            length: 180,
            easing: Easing.CubicOut);

        await Task.Delay(190);
    }

    private async void BottomPanel_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                panStartHeight = BottomPanel.HeightRequest;
                break;

            case GestureStatus.Running:
                var newHeight = panStartHeight - e.TotalY;

                double minHeight = GetMapModeHeight();

                if (newHeight < minHeight)
                    newHeight = minHeight;

                if (newHeight > SearchModeHeight)
                    newHeight = SearchModeHeight;

                BottomPanel.HeightRequest = newHeight;
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (BottomPanel.HeightRequest > 450)
                {
                    await ShowSearchMode(true);
                }
                else
                {
                    keyboardHelper.HideKeyboard();
                    AddressEntry.Unfocus();
                    await ShowMapMode(true);
                }
                break;
        }
    }

    private async void CurrentLocation_Tapped(object sender, TappedEventArgs e)
    {
        await MoveToCurrentLocation();
    }

    private async void Map_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(map.VisibleRegion) || map.VisibleRegion == null)
            return;

        if (isUpdatingMapProgrammatically)
            return;

        double centerLatitude = map.VisibleRegion.Center.Latitude;
        double centerLongitude = map.VisibleRegion.Center.Longitude;
        double radiusKm = map.VisibleRegion.Radius.Kilometers;

        // Keep both panning and excessive zoom-out inside the intended service region.
        if (!IsInsideQashqadaryo(centerLatitude, centerLongitude) || radiusKm > MaxMapRadiusKm)
        {
            double clampedLatitude = Math.Clamp(
                centerLatitude,
                QashqadaryoMinLatitude,
                QashqadaryoMaxLatitude);

            double clampedLongitude = Math.Clamp(
                centerLongitude,
                QashqadaryoMinLongitude,
                QashqadaryoMaxLongitude);

            double clampedRadiusKm = Math.Min(
                Math.Max(radiusKm, AddressMapRadiusKm),
                MaxMapRadiusKm);

            await MoveMapOnlyAsync(
                clampedLatitude,
                clampedLongitude,
                clampedRadiusKm);

            centerLatitude = clampedLatitude;
            centerLongitude = clampedLongitude;
        }

        latitude = centerLatitude;
        longitude = centerLongitude;

        if (!isMapMoving)
        {
            isMapMoving = true;
            await CenterPin.TranslateTo(0, -55, 100, Easing.CubicOut);
        }

        mapMoveCts?.Cancel();
        mapMoveCts = new CancellationTokenSource();
        var token = mapMoveCts.Token;

        try
        {
            await Task.Delay(700, token);

            isMapMoving = false;
            await CenterPin.TranslateTo(0, -45, 100, Easing.CubicOut);

            await RefreshAddressForCoordinatesAsync(latitude, longitude);
            await ShowMapMode(true);
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async Task MoveToCurrentLocation()
    {
        try
        {
            IsBusy = true;

            var request = new GeolocationRequest(
                GeolocationAccuracy.High,
                TimeSpan.FromSeconds(10));

            var location = await Geolocation.GetLocationAsync(request);

            if (location == null)
            {
                await AlertService.ShowAlertAsync(
                    "Joylashuv",
                    "Joriy joylashuvni aniqlab bo‘lmadi.");
                return;
            }

            if (!IsInsideQashqadaryo(location.Latitude, location.Longitude))
            {
                await AlertService.ShowAlertAsync(
                    "Qashqadaryo hududi",
                    "Hozircha manzil faqat Qashqadaryo viloyatida tanlanadi. Xarita Qashqadaryo hududida qoladi.");
                return;
            }

            await MoveMapToAsync(
                location.Latitude,
                location.Longitude,
                AddressMapRadiusKm);
        }
        catch (PermissionException)
        {
            await AlertService.ShowAlertAsync(
                "Joylashuv ruxsati",
                "Joriy joylashuvdan foydalanish uchun ilovaga joylashuv ruxsatini bering.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            await AlertService.ShowAlertAsync(
                "Joylashuv",
                "Joriy joylashuvni aniqlab bo‘lmadi.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task MoveMapToAsync(
        double targetLatitude,
        double targetLongitude,
        double radiusKm,
        string? knownAddress = null)
    {
        if (!IsInsideQashqadaryo(targetLatitude, targetLongitude))
        {
            targetLatitude = ShahrisabzLatitude;
            targetLongitude = ShahrisabzLongitude;
            radiusKm = DefaultMapRadiusKm;
            knownAddress = null;
        }

        latitude = targetLatitude;
        longitude = targetLongitude;

        await MoveMapOnlyAsync(targetLatitude, targetLongitude, radiusKm);

        if (!string.IsNullOrWhiteSpace(knownAddress))
        {
            SetAddressTextFromMap(knownAddress);
            UpdateDeliveryState();
        }
        else
        {
            await RefreshAddressForCoordinatesAsync(targetLatitude, targetLongitude);
        }

        await ShowMapMode(false);
    }

    private async Task MoveMapOnlyAsync(double targetLatitude, double targetLongitude, double radiusKm)
    {
        try
        {
            isUpdatingMapProgrammatically = true;

            map.MoveToRegion(
                MapSpan.FromCenterAndRadius(
                    new Location(targetLatitude, targetLongitude),
                    Distance.FromKilometers(Math.Min(radiusKm, MaxMapRadiusKm))));

            await Task.Delay(350);
        }
        finally
        {
            isUpdatingMapProgrammatically = false;
        }
    }

    private async Task RefreshAddressForCoordinatesAsync(double targetLatitude, double targetLongitude)
    {
        try
        {
            IsBusy = true;

            string resolvedAddress = await apiService.GetAddressFromYandexAsync(
                targetLatitude,
                targetLongitude);

            if (!string.IsNullOrWhiteSpace(resolvedAddress))
                SetAddressTextFromMap(resolvedAddress);

            UpdateDeliveryState();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SetAddressTextFromMap(string value)
    {
        isSettingAddressTextProgrammatically = true;
        AddressText = value;
        hasPendingAddressSearch = false;
        isSettingAddressTextProgrammatically = false;
    }

    private void UpdateDeliveryState()
    {
        isDeliveryAvailable = CheckDeliveryAvailability(latitude, longitude);
        NoDeliveryLabel.IsVisible = !isDeliveryAvailable;
        PickupButton.IsVisible = true;
    }

    private static bool IsInsideQashqadaryo(double lat, double lon)
    {
        return lat >= QashqadaryoMinLatitude &&
               lat <= QashqadaryoMaxLatitude &&
               lon >= QashqadaryoMinLongitude &&
               lon <= QashqadaryoMaxLongitude;
    }

    private bool CheckDeliveryAvailability(double lat, double lon)
    {
        // Current first delivery zone: Shahrisabz and its nearby area.
        // The map itself can be explored throughout Qashqadaryo.
        const double minLat = 39.0000;
        const double maxLat = 39.1500;
        const double minLon = 66.7500;
        const double maxLon = 66.9500;

        return lat >= minLat &&
               lat <= maxLat &&
               lon >= minLon &&
               lon <= maxLon;
    }

    private void OnConfirmAddress()
    {
        AddressEntry.Unfocus();

        Console.WriteLine($"Address: {AddressText}");
        Console.WriteLine($"Lat: {latitude}, Lon: {longitude}");
    }

    private async void PickupButton_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AddressText))
        {
            await AlertService.ShowAlertAsync(
                "Manzil",
                "Iltimos, yetkazib berish manzilini tanlang.");
            return;
        }

        bool result = await DisplayAlert(
            "Manzilni tasdiqlash",
            "Bu haqiqiy yetkazib berish manzilimi?",
            "Ha",
            "Yo‘q");

        if (!result)
            return;

        var selectedAddress = new SelectedAddressModel
        {
            Address = AddressText,
            Latitude = latitude,
            Longitude = longitude
        };

        var navigationData = AddressSelectionNavigationStore.Data;
        AddressSelectionNavigationStore.UpdateSelection(
            selectedAddress.Address,
            selectedAddress.Latitude,
            selectedAddress.Longitude);

        if (navigationData?.Mode == AddressSelectionMode.Checkout)
        {
            var formalizationData = FormalizationNavigationStore.Data;

            if (formalizationData != null)
            {
                formalizationData.AddressText = selectedAddress.Address;
                formalizationData.AddressLatitude = selectedAddress.Latitude;
                formalizationData.AddressLongitude = selectedAddress.Longitude;
            }
        }
        else
        {
            WeakReferenceMessenger.Default.Send(
                selectedAddress,
                "SelectedAddress");
        }

        await AppNavigatorService.NavigateTo("..");
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
