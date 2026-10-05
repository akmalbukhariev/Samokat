namespace NinimumWarehouse.Services;
public static class AppConstants
{
    public const string BaseUrl = "http://95.182.118.233:8083/ninimum/api/v1/warehouse-app/";
    public const string TokenKey = "warehouse.token";
    public const string WorkerKey = "warehouse.worker";
    public const string WorkerNameKey = "warehouse.workerName";
    public const string LanguageKey = "warehouse.language";
    public const string QueueView = "queue";
    public const string MineView = "mine";
    public const string HistoryView = "history";
    public const int PageSize = 20;
    public const int MaxReasonLength = 500;
    public static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(20);
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(25);
}
