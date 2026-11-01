using Api.Services;
using Models.Responses;
using Ninimum.Resources.Languages;
using Ninimum.Views.MyTariff;
using Utils;

namespace Ninimum.Services;

// Promo period: tariffs are hidden and customers earn a free month by inviting friends.
// The owner switches it in web admin (Sozlamalar → Promo kod); nothing is deleted.
public static class PromoMode
{
    public static bool Enabled { get; private set; }
    public static int FriendsNeeded { get; private set; } = 5;

    private static UserApiService? Api => IPlatformApplication.Current?.Services.GetService<UserApiService>();

    public static string L(string uz, string ru, string en)
    {
        var lang = (AppResource.Culture ?? System.Globalization.CultureInfo.CurrentUICulture).TwoLetterISOLanguageName;
        return lang == "ru" ? ru : lang == "en" ? en : uz;
    }

    // "2026-12-01" from the server, shown as 01.12.2026.
    public static string Day(string? value) =>
        DateTime.TryParseExact(value, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var date)
            ? date.ToString("dd.MM.yyyy") : value ?? string.Empty;

    public static string InviteTitle => L("Do‘stlarni taklif qilish", "Пригласить друзей", "Invite friends");

    // A failed request keeps the last known value; the server refuses tariff purchases anyway.
    public static async Task RefreshAsync()
    {
        if (Api is not { } api) return;
        ReferralResponse response = await api.GetReferralSettings();
        if (response.resultCode != ApiResult.SUCCESS.GetCodeToString() || response.resultData == null) return;
        Enabled = response.resultData.promo_mode;
        FriendsNeeded = response.resultData.friends_needed;
    }

    public static async Task OpenTariffs()
    {
        await RefreshAsync();
        await AppNavigatorService.NavigateTo(Enabled ? nameof(InvitePage) : nameof(TariffsPage));
    }
}
