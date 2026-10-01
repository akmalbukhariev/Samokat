namespace Ninimum.Resources.Languages;
public static class CatalogText
{
    private static string Get(string key) => AppResource.ResourceManager.GetString(key, AppResource.Culture) ?? key;
    public static string Title => Get("CatalogTitle");
    public static string EmptyCategories => Get("CatalogEmptyCategories");
    public static string EmptyProducts => Get("CatalogEmptyProducts");
    public static string LoadError => Get("CatalogLoadError");
    public static string Retry => Get("CatalogRetry");
}
