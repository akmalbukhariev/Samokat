namespace Ninimum.Models;

public static class CategoryDisplayName
{
    public static string Resolve(string? fallback, string? uzbek, string? russian, string language)
    {
        var translated = language == "uz" ? uzbek : language == "ru" ? russian : null;
        return string.IsNullOrWhiteSpace(translated) ? fallback ?? "" : translated.Trim();
    }
}
