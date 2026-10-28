namespace NinimumWarehouse.Services;
public static class AppConstants
{
    // HTTPS domain since 2026-10-28 (old address http://95.182.118.233:8083 still works for older versions).
    public const string BaseUrl = "https://ninimum.uz/ninimum/api/v1/warehouse-app/";
    public const string TokenKey = "warehouse.token";
    public const string WorkerKey = "warehouse.worker";
    public const string WorkerNameKey = "warehouse.workerName";
    public const string LanguageKey = "warehouse.language";
    public const string QueueView = "queue";
    public const string MineView = "mine";
    public const string HistoryView = "history";
    public const int PageSize = 20;
    public const int MaxReasonLength = 500;
    public static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(5);
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(25);
}
