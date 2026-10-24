using System.Globalization;
using System.Text.RegularExpressions;
using Ninimum.Resources.Languages;

namespace Utils
{
    /// <summary>
    /// What the customer sees for a failed request: server sentences arrive in Uzbek already, server codes are
    /// translated here, and technical texts (exceptions, JSON errors, English status messages) never reach the screen.
    /// </summary>
    public static class ServerMessage
    {
        private static readonly Dictionary<string, (string uz, string ru, string en)> Codes = new()
        {
            ["STOCK_UNAVAILABLE"] = ("Uzr, mahsulot yetarli emas.", "Извините, товара недостаточно.", "Sorry, not enough stock."),
            ["PRICE_CHANGED"] = ("Narx o‘zgardi. Savatni yangilab, qayta urinib ko‘ring.", "Цена изменилась. Обновите корзину и повторите.", "The price changed. Refresh the cart and try again."),
            ["PROVIDER_ERROR"] = ("To‘lov tizimi xato qaytardi. Qayta urinib ko‘ring.", "Платёжная система вернула ошибку. Повторите.", "The payment system returned an error. Try again."),
            ["PROVIDER_UNAVAILABLE"] = ("To‘lov tizimi vaqtincha ishlamayapti. Birozdan keyin urinib ko‘ring.", "Платёжная система временно недоступна.", "The payment system is temporarily unavailable."),
            ["NOT_CONFIGURED"] = ("Bu xizmat hozircha mavjud emas.", "Эта услуга пока недоступна.", "This service is not available yet."),
            ["SESSION_REPLACED"] = ("Hisobingizga boshqa qurilmada kirildi. Qaytadan kiring.", "В аккаунт вошли на другом устройстве. Войдите снова.", "Your account was opened on another device. Please sign in again."),
            ["LOGIN_LOCKED_15_MINUTES"] = ("Juda ko‘p urinish. 15 daqiqadan keyin qayta urinib ko‘ring.", "Слишком много попыток. Повторите через 15 минут.", "Too many attempts. Try again in 15 minutes."),
            ["PARTIAL_RETURN_UNAVAILABLE"] = ("Bu buyurtmada qisman qaytarish mumkin emas.", "Для этого заказа частичный возврат недоступен.", "A partial return isn’t possible for this order."),
            ["RETURN_PHOTO_REQUIRED"] = ("Rasm qo‘shing.", "Добавьте фото.", "Add a photo."),
            ["RETURN_PHOTO_TOO_LARGE"] = ("Rasm juda katta.", "Фото слишком большое.", "The photo is too large."),
            ["RETURN_PHOTO_INVALID"] = ("Rasm noto‘g‘ri.", "Неверное фото.", "Invalid photo."),
            ["RETURN_REASON_REQUIRED"] = ("Sababni yozing.", "Укажите причину.", "Enter the reason."),
            ["WAREHOUSE_INVALID_INPUT"] = ("Ma’lumotlarni tekshiring.", "Проверьте данные.", "Check the details."),
            ["WAREHOUSE_INVALID_STATE"] = ("Hozir bu amalni bajarib bo‘lmaydi.", "Сейчас это действие недоступно.", "This can’t be done right now."),
            ["WAREHOUSE_NOT_FOUND"] = ("Ma’lumot topilmadi.", "Данные не найдены.", "Not found."),
        };

        private static string Pick((string uz, string ru, string en) t)
        {
            var lang = (AppResource.Culture ?? CultureInfo.CurrentUICulture).TwoLetterISOLanguageName;
            return lang == "ru" ? t.ru : lang == "en" ? t.en : t.uz;
        }

        /// <summary>A failed call or app error in plain words: no connection, or a general error. Never the exception text.</summary>
        public static string Exception(System.Exception ex) =>
            ex is HttpRequestException or TaskCanceledException or System.IO.IOException or System.Net.WebException
                ? AppResource.NoInternetConnection : AppResource.AnErrorOccurred;

        public static string? Show(string? resultCode, string? raw)
        {
            if (resultCode == "100" || raw == null) return raw;
            var text = raw.Trim();
            if (text.Length == 0) return raw;
            if (Codes.TryGetValue(text, out var known)) return Pick(known);
            // Unknown code, client-side exception text or an English status message from an older server.
            if (Regex.IsMatch(text, "^[A-Z0-9_]+$") || text.StartsWith("API:") || text.StartsWith("JSON Parsing Error")
                || ResultExtensions.IsClientMessage(text))
                return AppResource.AnErrorOccurred;
            return raw;
        }
    }
}
