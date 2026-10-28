namespace NinimumDelivery.Services;

public static class AppConstants
{
    // HTTPS domain since 2026-10-28 (old address http://95.182.118.233:8083 still works for older versions).
    // Local Mac backend for development: http://192.168.219.105:8083/ninimum/api/v1/
    public const string Server = "ninimum.uz";
    public const string BaseUrl = $"https://{Server}/ninimum/api/v1/";
    public const string TokenKey = "delivery_auth_token";
    public const string LoggedInKey = "delivery_logged_in";
    public const string WorkerKey = "delivery_worker";
    public const string LanguageKey = "delivery_language";
}
