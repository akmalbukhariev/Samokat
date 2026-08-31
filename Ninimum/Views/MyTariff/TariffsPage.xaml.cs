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
                await DisplayAlert("Xatolik", response.resultMsg ?? "Tariflarni yuklab bo‘lmadi.", "Yopish");
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
                        ? "Amaldagi tarif"
                        : activeTariffId > 0
                            ? "Tarifni almashtirish"
                            : "Sotib olish"
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
            await DisplayAlert("Xatolik", "Tariflarni yuklab bo‘lmadi.", "Yopish");
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
                await DisplayAlert("Tarif", $"{tariff.Name} hozirgi amaldagi tarifingiz.", "Yopish");
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
                    "Tarifni almashtirish",
                    $"{currentName} tarifidan {tariff.Name} tarifiga o‘tmoqchimisiz? " +
                    "Yangi tarif to‘lovi tasdiqlangandan so‘ng darhol faol bo‘ladi.",
                    "Almashtirish",
                    "Bekor qilish");

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
                    "Xatolik",
                    checkoutResponse.resultMsg ?? "Tarif to‘lovini boshlash imkoni bo‘lmadi.",
                    "Yopish");
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
            await DisplayAlert("Xatolik", "Tarif to‘lovini boshlash imkoni bo‘lmadi.", "Yopish");
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
            return "Mahsulotni 1 soat davomida yetkazish";
        if (name.Contains("GOLD"))
            return "Mahsulotni 3 soat davomida yetkazish";
        if (name.Contains("SILVER"))
            return "Mahsulotni kun davomida yetkazish";

        return index switch
        {
            0 => "Mahsulotni kun davomida yetkazish",
            1 => "Mahsulotni 3 soat davomida yetkazish",
            _ => "Tezkor yetkazib berish"
        };
    }
}
