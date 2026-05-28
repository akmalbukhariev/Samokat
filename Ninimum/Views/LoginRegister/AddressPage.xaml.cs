using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Api.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Ninimum.Models;
using Ninimum.Services;

namespace Ninimum.Views.LoginRegister;

public partial class AddressPage : BasePage, INotifyPropertyChanged
{
    private double latitude;
    private double longitude;
    private string addressText = string.Empty;
    private double panStartHeight;  
    private bool isDeliveryAvailable;

    private CancellationTokenSource? mapMoveCts;
    private bool isMapMoving;
    private bool isUpdatingMapProgrammatically;

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
        await MoveToCurrentLocation();
    }

    private void AddressPage_Unloaded(object sender, EventArgs e)
    {
        map.PropertyChanged -= Map_PropertyChanged;
        mapMoveCts?.Cancel();
    }

    private async void MapButton_Tapped(object sender, TappedEventArgs e)
    {
        keyboardHelper.HideKeyboard();
        AddressEntry.Unfocus();
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
        // Do nothing here.
        // If we reduce panel here, keyboard closing can look strange.
    }

    private async void AddressEntry_Completed(object sender, EventArgs e)
    {
        AddressEntry.Unfocus();
        await ShowMapMode(true);
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
        if (e.PropertyName != nameof(map.VisibleRegion))
            return;

        if (map.VisibleRegion == null)
            return;

        if (isUpdatingMapProgrammatically)
            return;

        latitude = map.VisibleRegion.Center.Latitude;
        longitude = map.VisibleRegion.Center.Longitude;

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

            IsBusy = true;
            AddressText = await apiService.GetAddressFromYandexAsync(latitude, longitude);

            isDeliveryAvailable = CheckDeliveryAvailability(latitude, longitude);

            NoDeliveryLabel.IsVisible = !isDeliveryAvailable;
            PickupButton.IsVisible = true;

            await ShowMapMode(true);
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
    //location_latitude
    //location_longitude
    private async Task MoveToCurrentLocation()
    {
        try
        {
            var request = new GeolocationRequest(
                GeolocationAccuracy.High,
                TimeSpan.FromSeconds(10));

            var location = await Geolocation.GetLocationAsync(request);

            if (location == null)
                return;

            latitude = location.Latitude;
            longitude = location.Longitude;

            isUpdatingMapProgrammatically = true;

            map.MoveToRegion(
                MapSpan.FromCenterAndRadius(
                    new Location(latitude, longitude),
                    Distance.FromKilometers(1)));

            await Task.Delay(500);

            isUpdatingMapProgrammatically = false;

            IsBusy = true;
            AddressText = await apiService.GetAddressFromYandexAsync(latitude, longitude);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            await AlertService.ShowAlertAsync(
                "Location",
                "Location not found");
        }
        finally
        {
            IsBusy = false;
            isUpdatingMapProgrammatically = false;
        }
    }

    private bool CheckDeliveryAvailability(double lat, double lon)
    {
        // Example coordinates for Shahrisabz area.
        // You can later expand this territory.

        double minLat = 39.0000;
        double maxLat = 39.1500;

        double minLon = 66.7500;
        double maxLon = 66.9500;

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
        bool result = await DisplayAlert(
            "Manzilni tasdiqlash",
            "Bu haqiqiy yetkazib berish manzilimi?",
            "Ha",
            "Yo‘q");

        if (!result)
            return;

        MessagingCenter.Send(this, "SelectedAddress", new SelectedAddressModel
        {
            Address = AddressText,
            Latitude = latitude,
            Longitude = longitude
        });

        await AppNavigatorService.NavigateTo("..");
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}