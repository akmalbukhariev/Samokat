using Ninimum.Resources.Languages;
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
    private const double ShahrisabzLatitude = 39.0578;
    private const double ShahrisabzLongitude = 66.8342;
    private const double DefaultMapRadiusKm = 3;
    private const double AddressMapRadiusKm = 1;
    private const double MaxMapRadiusKm = 5;

    // The existing Shahrisabz delivery area is also the address selection boundary.
    private const double ShahrisabzMinLatitude = 39.0000;
    private const double ShahrisabzMaxLatitude = 39.1500;
    private const double ShahrisabzMinLongitude = 66.7500;
    private const double ShahrisabzMaxLongitude = 66.9500;
    private bool isMapInitialized;

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

        // Initialize when the map itself is attached. A constructor-time city move
        // can be applied late by the native map and overwrite the saved position.
        map.Loaded += AddressPage_Loaded;
        map.Unloaded += AddressPage_Unloaded;

        ShowMapMode(false);
    }

    private async void AddressPage_Loaded(object sender, EventArgs e)
    {
        isMapInitialized = false;
        map.PropertyChanged -= Map_PropertyChanged;
        try
        {
            await InitializeMapAsync();
            isMapInitialized = true;
            map.PropertyChanged += Map_PropertyChanged;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Address map initialization failed: {ex}");
            PickupButton.IsEnabled = false;
            await AlertService.ShowAlertAsync(AppResource.Address, AppResource.PleaseSelectADeliveryAddress);
        }
    }

    private void AddressPage_Unloaded(object sender, EventArgs e)
    {
        isMapInitialized = false;
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
            IsInsideShahrisabz(initialLatitude, initialLongitude))
        {
            await MoveMapToAsync(
                initialLatitude,
                initialLongitude,
                AddressMapRadiusKm,
                navigationData.AddressText);
            return;
        }

        // 2. Older users may have address text but no usable coordinates. Try to resolve the
        //    existing address inside Shahrisabz before falling back to Shahrisabz.
        if (!string.IsNullOrWhiteSpace(navigationData?.AddressText))
        {
            var resolved = await apiService.SearchAddressInShahrisabzAsync(navigationData.AddressText);

            if (resolved != null && IsInsideShahrisabz(resolved.Latitude, resolved.Longitude))
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

            var result = await apiService.SearchAddressInShahrisabzAsync(query);

            if (result == null || !IsInsideShahrisabz(result.Latitude, result.Longitude))
            {
                await AlertService.ShowAlertAsync(
                    AppResource.AddressNotFound,
                    AppResource.EnterADistrictStreetHouseOrLandmarkIn);
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
                AppResource.Error,
                AppResource.CouldNotSearchForTheAddressPleaseTry);

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
        if (!isMapInitialized || e.PropertyName != nameof(map.VisibleRegion) || map.VisibleRegion == null)
            return;

        if (isUpdatingMapProgrammatically)
            return;

        double centerLatitude = map.VisibleRegion.Center.Latitude;
        double centerLongitude = map.VisibleRegion.Center.Longitude;
        double radiusKm = map.VisibleRegion.Radius.Kilometers;

        // Keep both panning and excessive zoom-out inside the intended service region.
        if (!IsInsideShahrisabz(centerLatitude, centerLongitude) || radiusKm > MaxMapRadiusKm)
        {
            double clampedLatitude = Math.Clamp(
                centerLatitude,
                ShahrisabzMinLatitude,
                ShahrisabzMaxLatitude);

            double clampedLongitude = Math.Clamp(
                centerLongitude,
                ShahrisabzMinLongitude,
                ShahrisabzMaxLongitude);

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
        SetAddressTextFromMap(string.Empty);

        if (!isMapMoving)
        {
            isMapMoving = true;
            await CenterPin.TranslateTo(0, -10, 100, Easing.CubicOut);
        }

        mapMoveCts?.Cancel();
        mapMoveCts = new CancellationTokenSource();
        var token = mapMoveCts.Token;

        try
        {
            await Task.Delay(700, token);

            isMapMoving = false;
            await CenterPin.TranslateTo(0, 0, 100, Easing.CubicOut);

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
                    AppResource.Location,
                    AppResource.CouldNotDetermineYourCurrentLocation);
                return;
            }

            if (!IsInsideShahrisabz(location.Latitude, location.Longitude))
            {
                await AlertService.ShowAlertAsync(
                    AppResource.QashqadaryoRegion,
                    AppResource.ForNowAnAddressCanOnlyBeSelected);
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
                AppResource.LocationPermission,
                AppResource.LocationPermissionMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            await AlertService.ShowAlertAsync(
                AppResource.Location,
                AppResource.CouldNotDetermineYourCurrentLocation);
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
        mapMoveCts?.Cancel();
        if (!IsInsideShahrisabz(targetLatitude, targetLongitude))
        {
            targetLatitude = ShahrisabzLatitude;
            targetLongitude = ShahrisabzLongitude;
            radiusKm = DefaultMapRadiusKm;
            knownAddress = null;
        }

        latitude = targetLatitude;
        longitude = targetLongitude;
        SetAddressTextFromMap(string.Empty);

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

            var region = MapSpan.FromCenterAndRadius(
                new Location(targetLatitude, targetLongitude),
                Distance.FromKilometers(Math.Min(radiusKm, MaxMapRadiusKm)));
#if ANDROID
            // Loaded does not mean GoogleMap is ready. Wait for both the native
            // map and its measured viewport, then move without camera animation.
            var deadline = DateTime.UtcNow.AddSeconds(5);
            while ((map.Handler is not Microsoft.Maui.Maps.Handlers.MapHandler ready ||
                    ready.Map == null || ready.PlatformView.Width <= 0 || ready.PlatformView.Height <= 0)
                   && DateTime.UtcNow < deadline)
                await Task.Delay(50);

            if (map.Handler is not Microsoft.Maui.Maps.Handlers.MapHandler handler || handler.Map == null ||
                handler.PlatformView.Width <= 0 || handler.PlatformView.Height <= 0)
                throw new InvalidOperationException("Native map is not ready");

            var bounds = new Android.Gms.Maps.Model.LatLngBounds(
                new Android.Gms.Maps.Model.LatLng(targetLatitude - region.LatitudeDegrees / 2,
                    targetLongitude - region.LongitudeDegrees / 2),
                new Android.Gms.Maps.Model.LatLng(targetLatitude + region.LatitudeDegrees / 2,
                    targetLongitude + region.LongitudeDegrees / 2));
            handler.Map.StopAnimation();
            handler.Map.MoveCamera(Android.Gms.Maps.CameraUpdateFactory.NewLatLngBounds(
                bounds, handler.PlatformView.Width, handler.PlatformView.Height, 0));
#else
            map.MoveToRegion(region);
            await Task.Delay(350);
#endif
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

            // Ignore a response for a point the user has already moved away from.
            if (latitude == targetLatitude && longitude == targetLongitude && !hasPendingAddressSearch)
            {
                SetAddressTextFromMap(resolvedAddress);
                UpdateDeliveryState();
            }
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

    private static bool IsInsideShahrisabz(double lat, double lon)
    {
        return lat >= ShahrisabzMinLatitude &&
               lat <= ShahrisabzMaxLatitude &&
               lon >= ShahrisabzMinLongitude &&
               lon <= ShahrisabzMaxLongitude;
    }

    private bool CheckDeliveryAvailability(double lat, double lon)
    {
        return IsInsideShahrisabz(lat, lon);
    }

    private void OnConfirmAddress()
    {
        AddressEntry.Unfocus();

        Console.WriteLine($"Address: {AddressText}");
        Console.WriteLine($"Lat: {latitude}, Lon: {longitude}");
    }

    private async void PickupButton_Clicked(object sender, EventArgs e)
    {
        if (hasPendingAddressSearch && !await SearchTypedAddressAsync())
            return;

        // Saving uses the visible center, so it cannot retain coordinates from an
        // earlier viewport while the native map is still completing a move.
        if (map.VisibleRegion != null && !isUpdatingMapProgrammatically)
        {
            var center = map.VisibleRegion.Center;
            if (Math.Abs(latitude - center.Latitude) > 0.00001 ||
                Math.Abs(longitude - center.Longitude) > 0.00001)
            {
                latitude = center.Latitude;
                longitude = center.Longitude;
                SetAddressTextFromMap(string.Empty);
                await RefreshAddressForCoordinatesAsync(latitude, longitude);
            }
        }

        if (!isMapInitialized || !IsInsideShahrisabz(latitude, longitude) ||
            string.IsNullOrWhiteSpace(AddressText))
        {
            await AlertService.ShowAlertAsync(
                AppResource.Address,
                AppResource.PleaseSelectADeliveryAddress);
            return;
        }

        bool result = await DisplayAlert(
            AppResource.ConfirmAddress,
            AppResource.IsThisTheCorrectDeliveryAddress,
            AppResource.Yes,
            AppResource.No);

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
                FormalizationNavigationStore.SaveAddress(formalizationData.UserId, selectedAddress);
                formalizationData.AddressId = null;
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
