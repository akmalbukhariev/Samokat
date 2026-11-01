using Api.Services;
using Models.Requests;
using Models.Responses;
using Ninimum.Resources.Languages;
using Ninimum.Services;
using Utils;

namespace Ninimum.Views.MyTariff;

// Promo period: the customer's own code, a share button and how many friends have joined.
public partial class InvitePage : BasePage
{
    private readonly UserApiService apiService;
    private readonly AppControl appControl;

    public string PageTitle => PromoMode.InviteTitle;
    public string CodeTitle => PromoMode.L("Sizning promo kodingiz", "Ваш промокод", "Your promo code");
    public string ShareText => PromoMode.L("Do‘stlarga yuborish", "Отправить друзьям", "Send to friends");
    public string CopyText => PromoMode.L("Nusxa olish", "Скопировать", "Copy");
    public string Explanation { get; private set; } = string.Empty;
    public string Code { get; private set; } = "—";
    public bool HasCode { get; private set; }
    public string ProgressText { get; private set; } = string.Empty;
    public double Progress { get; private set; }
    public string TariffText { get; private set; } = string.Empty;
    public bool HasTariff => !string.IsNullOrEmpty(TariffText);

    public InvitePage(UserApiService apiService, AppControl appControl)
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
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            loading.ShowLoading = true;
            ReferralResponse response = await apiService.GetReferral();
            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString() || response.resultData == null)
            {
                await AlertService.ShowAlertAsync(AppResource.Error, response.resultMsg ?? AppResource.AnErrorOccurred);
                return;
            }
            Show(response.resultData);

            // "Mening tarifim" is hidden during the promo, so the end date is shown here.
            var active = await apiService.GetActiveSubscription(new ActiveSubscriptionRequest { userId = appControl.userDto.id ?? 0 });
            bool hasTariff = active.resultCode == ApiResult.SUCCESS.GetCodeToString() && active.resultData != null &&
                string.Equals(active.resultData.subscriptionStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase);
            TariffText = hasTariff
                ? PromoMode.L(
                    $"{active.resultData!.tariffName} tarifingiz {PromoMode.Day(active.resultData.endDate)} gacha faol",
                    $"Ваш тариф {active.resultData!.tariffName} активен до {PromoMode.Day(active.resultData.endDate)}",
                    $"Your {active.resultData!.tariffName} plan is active until {PromoMode.Day(active.resultData.endDate)}")
                : string.Empty;
            OnPropertyChanged(nameof(TariffText));
            OnPropertyChanged(nameof(HasTariff));
        }
        finally
        {
            loading.ShowLoading = false;
        }
    }

    private void Show(ReferralData data)
    {
        int needed = Math.Max(1, data.friends_needed);
        Code = data.code ?? "—";
        HasCode = !string.IsNullOrEmpty(data.code);
        Progress = Math.Min(1.0, (double)data.friends_count / needed);
        ProgressText = PromoMode.L($"{data.friends_count} / {needed} do‘st", $"Друзей: {data.friends_count} из {needed}", $"{data.friends_count} of {needed} friends");
        Explanation = data.rewarded
            ? PromoMode.L("Siz bir oylik tarifni oldingiz. Rahmat!", "Вы получили тариф на месяц. Спасибо!", "You have received your free month. Thank you!")
            : PromoMode.L(
                $"Chegirma narxida xarid qilish uchun {needed} ta do‘stingizni taklif qiling. Do‘stingiz ro‘yxatdan o‘tishda kodingizni kiritsa, 1 oy tarif unga bepul beriladi. {needed} ta do‘st kiritgach, 1 oy tarif sizga ham bepul beriladi.",
                $"Чтобы покупать со скидкой, пригласите друзей: {needed}. Друг вводит ваш код при регистрации и получает тариф на 1 месяц бесплатно. Когда код введут {needed} друзей, вы тоже получите 1 месяц бесплатно.",
                $"To buy at the discount price, invite {needed} friends. A friend who enters your code at registration gets 1 free month. Once {needed} friends have entered it, you get 1 free month too.");
        foreach (var name in new[] { nameof(Code), nameof(HasCode), nameof(Progress), nameof(ProgressText), nameof(Explanation) })
            OnPropertyChanged(name);
    }

    private async void OnShareClicked(object? sender, EventArgs e)
    {
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = PromoMode.InviteTitle,
            Text = PromoMode.L(
                $"Ninimum ilovasini o‘rnating va ro‘yxatdan o‘tishda {Code} promo kodini kiriting: 1 oy tarif bepul!",
                $"Установите приложение Ninimum и при регистрации введите промокод {Code}: тариф на 1 месяц бесплатно!",
                $"Install the Ninimum app and enter promo code {Code} at registration: 1 free month!")
        });
    }

    private async void OnCopyClicked(object? sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync(Code);
        await AlertService.ShowAlertAsync(AppResource.Information, PromoMode.L("Kod nusxalandi.", "Код скопирован.", "Code copied."));
    }
}
