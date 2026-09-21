using System.Globalization;
namespace NinimumDelivery.Services;
public class LanguageService
{
    private readonly AppStoreService store;
    public LanguageService(AppStoreService store) => this.store = store;
    public string Current => store.Get(AppConstants.LanguageKey, "uz");
    public void Init() => Apply(Current);
    public void Set(string code) { store.Set(AppConstants.LanguageKey, code); Apply(code); }
    private static void Apply(string code)
    {
        var culture = new CultureInfo(code);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
}
