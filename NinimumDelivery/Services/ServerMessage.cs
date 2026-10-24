using System.Globalization;
using System.Text.RegularExpressions;
using NinimumDelivery.Resources.Languages;

namespace NinimumDelivery.Services;

/// <summary>
/// What the courier sees for a failed request: server sentences come in Uzbek already, codes are translated here,
/// and technical or unknown codes become a plain "action failed" text.
/// </summary>
public static class ServerMessage
{
    private static readonly Dictionary<string,(string uz,string ru,string en)> Codes = new()
    {
        ["PARTIAL_RETURN_ALREADY_SAVED"] = ("Qisman qaytarish allaqachon saqlangan.","Частичный возврат уже сохранён.","The partial return is already saved."),
        ["PARTIAL_RETURN_INVALID"] = ("Qaytariladigan mahsulot va sonini tekshiring.","Проверьте товары и количество для возврата.","Check the returned products and quantities."),
        ["PARTIAL_RETURN_UNAVAILABLE"] = ("Bu buyurtmada qisman qaytarish mumkin emas.","Для этого заказа частичный возврат недоступен.","A partial return isn’t possible for this order."),
        ["RETURN_PHOTO_ALREADY_SAVED"] = ("Rasm allaqachon saqlangan.","Фото уже сохранено.","The photo is already saved."),
        ["RETURN_PHOTO_INVALID"] = ("Rasm noto‘g‘ri. JPEG yoki PNG rasm tanlang.","Неверное фото. Выберите JPEG или PNG.","Invalid photo. Choose a JPEG or PNG."),
        ["RETURN_PHOTO_REQUIRED"] = ("Rasm qo‘shing.","Добавьте фото.","Add a photo."),
        ["RETURN_PHOTO_TOO_LARGE"] = ("Rasm juda katta.","Фото слишком большое.","The photo is too large."),
        ["RETURN_REASON_REQUIRED"] = ("Sababni yozing.","Укажите причину.","Enter the reason."),
        ["SESSION_REPLACED"] = ("Boshqa qurilmada kirildi. Qaytadan kiring.","Выполнен вход на другом устройстве. Войдите снова.","Signed in on another device. Please sign in again."),
    };

    public static string? Show(string? resultCode, string? raw)
    {
        if (resultCode == "100" || string.IsNullOrWhiteSpace(raw)) return raw;
        var text = raw.Trim();
        if (Codes.TryGetValue(text, out var t))
        {
            var lang = (AppResource.Culture ?? CultureInfo.CurrentUICulture).TwoLetterISOLanguageName;
            return lang == "ru" ? t.ru : lang == "en" ? t.en : t.uz;
        }
        // Unknown codes and English status texts from an older server are never shown raw.
        if (Regex.IsMatch(text, "^[A-Z0-9_]+$") || text is "Success." or "Failed.") return AppResource.ActionFailed;
        return raw;
    }
}
