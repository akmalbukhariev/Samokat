using System.Globalization;
using NinimumWarehouse.Resources.Languages;
namespace NinimumWarehouse.Services;
public static class LanguageService
{
    public static string[] Options => [AppResource.LanguageUz, AppResource.LanguageRu, AppResource.LanguageEn];
    public static string Current => Preferences.Default.Get(AppConstants.LanguageKey, "uz");
    public static int SelectedIndex => Current switch { "ru" => 1, "en" => 2, _ => 0 };
    public static void Init() => Apply(Current);
    public static void Set(int index)
    {
        string code = index switch { 1 => "ru", 2 => "en", _ => "uz" };
        Preferences.Default.Set(AppConstants.LanguageKey, code); Apply(code);
    }
    private static void Apply(string code)
    {
        var culture = CultureInfo.GetCultureInfo(code);
        AppResource.Culture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
}
