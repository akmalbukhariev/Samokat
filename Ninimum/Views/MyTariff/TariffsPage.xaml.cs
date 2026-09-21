using Ninimum.Resources.Languages;
using System.Collections.ObjectModel;
using Api.Services;
using Microsoft.Maui.Controls.Shapes;
using Models.Requests;
using Ninimum.Models.Tariff;
using Ninimum.Services;
using Ninimum.Views.Payment;
using Utils;

namespace Ninimum.Views.MyTariff;

public partial class TariffsPage : BasePage
{
    private readonly UserApiService apiService;
    private readonly AppControl appControl;
    private bool isBuying;
    private long activeTariffId;

    public ObservableCollection<TariffPlan> Tariffs { get; } = new();

    public TariffsPage(UserApiService apiService, AppControl appControl)
    {
        InitializeComponent();
        this.apiService = apiService;
        this.appControl = appControl;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!await appControl.EnsureAuthenticatedAsync(true))
            return;

        await LoadTariffsAsync();
    }

    private async Task LoadTariffsAsync()
    {
        try
        {
            loading.ShowLoading = true;

            // Read the active subscription first so TariffsPage can show which
            // plan is currently in use and disable buying that same plan again.
            activeTariffId = 0;

            var activeResponse = await apiService.GetActiveSubscription(new ActiveSubscriptionRequest
            {
                userId = appControl.CurrentUserId
            });

            if (activeResponse.resultCode == ApiResult.SUCCESS.GetCodeToString() &&
                activeResponse.resultData != null &&
                string.Equals(activeResponse.resultData.subscriptionStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase))
            {
                activeTariffId = activeResponse.resultData.tariffId;
            }

            var response = await apiService.GetTariffList();

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString() || response.resultData == null)
            {
                await DisplayAlert(AppResource.Error, response.resultMsg ?? AppResource.CouldNotLoadTariffs, AppResource.Close);
                return;
            }

            Tariffs.Clear();
            int activeIndex = -1;

            for (int i = 0; i < response.resultData.Count; i++)
            {
                var item = response.resultData[i];
                bool isCurrent = activeTariffId > 0 && item.tariffId == activeTariffId;

                if (isCurrent)
                    activeIndex = i;

                Tariffs.Add(new TariffPlan
                {
                    Id = item.tariffId,
                    Name = item.tariffName,
                    Price = FormatPrice(item.price),
                    DurationMonth = Math.Max(1, item.durationMonth),
                    Description = item.description ?? string.Empty,
                    Color = GetTariffColor(i),
                    DeliveryText = GetDeliveryText(item.tariffName, i),
                    PartnerIcon = i == 0 ? "ic_uncheck_circle.png" : "ic_check_circle.png",
                    IsCurrent = isCurrent,
                    CanPurchase = !isCurrent,
                    ActionText = isCurrent
                        ? AppResource.CurrentTariff
                        : activeTariffId > 0
                            ? AppResource.ChangeTariff
                            : AppResource.Buy
                });
            }

            int position = activeIndex >= 0 ? activeIndex : (Tariffs.Count > 0 ? 0 : -1);

            if (position >= 0)
                TariffCarousel.Position = position;

            UpdateCustomIndicator(position);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] LoadTariffsAsync => {ex}");
            await DisplayAlert(AppResource.Error, AppResource.CouldNotLoadTariffs, AppResource.Close);
        }
        finally
        {
            loading.ShowLoading = false;
        }
    }

    private async void OnBuyTariffClicked(object sender, EventArgs e)
    {
        if (isBuying ||
            sender is not Button button ||
            button.CommandParameter is not TariffPlan tariff ||
            tariff.Id <= 0 ||
            tariff.IsCurrent ||
            !tariff.CanPurchase)
        {
            return;
        }

        try
        {
            isBuying = true;
            loading.ShowLoading = true;

            // Re-check the active tariff just before checkout. The value may
            // have changed while this page was open (for example after payment
            // completed on another device/session).
            var activeResponse = await apiService.GetActiveSubscription(new ActiveSubscriptionRequest
            {
                userId = appControl.CurrentUserId
            });

            bool hasActiveTariff =
                activeResponse.resultCode == ApiResult.SUCCESS.GetCodeToString() &&
                activeResponse.resultData != null &&
                string.Equals(activeResponse.resultData.subscriptionStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase);

            if (hasActiveTariff && activeResponse.resultData!.tariffId == tariff.Id)
            {
                await DisplayAlert(AppResource.Tariff, string.Format(AppResource.CurrentTariffMessage, tariff.Name), AppResource.Close);
                await LoadTariffsAsync();
                return;
            }

            if (hasActiveTariff)
            {
                // Do not replace the existing tariff until Payme confirms the
                // new payment. The backend performs the actual switch only on
                // successful payment.
                loading.ShowLoading = false;

                string currentName = activeResponse.resultData!.tariffName;
                bool confirmed = await DisplayAlert(
                    AppResource.ChangeTariff,
                    string.Format(AppResource.DoYouWantToSwitchFromTo, currentName, tariff.Name) +
                    AppResource.TheNewTariffWillBecomeActiveImmediatelyAfter,
                    AppResource.Change,
                    AppResource.Cancel);

                if (!confirmed)
                    return;

                loading.ShowLoading = true;
            }

            var checkoutResponse = await apiService.CreateTariffCheckout(new CreateTariffCheckoutRequest
            {
                userId = appControl.CurrentUserId,
                tariffId = tariff.Id
            });

            if (checkoutResponse.resultCode != ApiResult.SUCCESS.GetCodeToString() ||
                checkoutResponse.resultData == null ||
                checkoutResponse.resultData.subscriptionId <= 0 ||
                string.IsNullOrWhiteSpace(checkoutResponse.resultData.paymentUrl))
            {
                await DisplayAlert(
                    AppResource.Error,
                    checkoutResponse.resultMsg ?? AppResource.CouldNotStartTariffPayment,
                    AppResource.Close);
                return;
            }

            await AppNavigatorService.NavigateTo(
                nameof(PaymentPage),
                new Dictionary<string, object>
                {
                    ["PaymentUrl"] = checkoutResponse.resultData.paymentUrl,
                    ["PaymentType"] = "TARIFF",
                    ["SubscriptionId"] = checkoutResponse.resultData.subscriptionId
                });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] BuyTariff => {ex}");
            await DisplayAlert(AppResource.Error, AppResource.CouldNotStartTariffPayment, AppResource.Close);
        }
        finally
        {
            loading.ShowLoading = false;
            isBuying = false;
        }
    }

    private void OnTariffPositionChanged(object sender, PositionChangedEventArgs e)
    {
        UpdateCustomIndicator(e.CurrentPosition);
    }

    private void UpdateCustomIndicator(int position)
    {
        CustomIndicatorLayout.Children.Clear();

        for (int i = 0; i < Tariffs.Count; i++)
        {
            bool isSelected = i == position;

            var indicator = new Border
            {
                StrokeThickness = 0,
                BackgroundColor = isSelected ? Color.FromArgb("#FF4B4B") : Color.FromArgb("#D8D8D8"),
                WidthRequest = isSelected ? 36 : 12,
                HeightRequest = 12,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(6) },
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            CustomIndicatorLayout.Children.Add(indicator);
        }
    }

    private static string FormatPrice(int price) => $"{price:N0}".Replace(",", " ");

    private static Color GetTariffColor(int index) => index switch
    {
        0 => Color.FromArgb("#76B900"),
        1 => Color.FromArgb("#FF8700"),
        _ => Color.FromArgb("#FF403B")
    };

    private static string GetDeliveryText(string tariffName, int index)
    {
        string name = tariffName?.ToUpperInvariant() ?? string.Empty;

        if (name.Contains("PLATINUM"))
            return AppResource.DeliveryWithinOneHour;
        if (name.Contains("GOLD"))
            return AppResource.DeliveryWithinThreeHours;
        if (name.Contains("SILVER"))
            return AppResource.DeliveryWithinTheDay;

        return index switch
        {
            0 => AppResource.DeliveryWithinTheDay,
            1 => AppResource.DeliveryWithinThreeHours,
            _ => AppResource.FastDelivery
        };
    }
}
