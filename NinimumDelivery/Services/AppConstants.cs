namespace NinimumDelivery.Services;

public static class AppConstants
{
    // Local Mac backend for development. For Ubuntu release, switch Server to 95.182.118.233 and remove :8083.
    public const string Server = "192.168.219.105";
    public const string BaseUrl = $"http://{Server}:8083/ninimum/api/v1/";
    public const string TokenKey = "delivery_auth_token";
    public const string LoggedInKey = "delivery_logged_in";
    public const string WorkerKey = "delivery_worker";
    public const string LanguageKey = "delivery_language";
}
