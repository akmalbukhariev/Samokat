using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NinimumWarehouse.Models;
namespace NinimumWarehouse.Services;
public class ApiException(string code) : Exception(code);
public sealed class WarehouseApi
{
    private readonly HttpClient client = new() { BaseAddress = new Uri(AppConstants.BaseUrl), Timeout = AppConstants.RequestTimeout };
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    private string token = "";
    public string WorkerCode { get; private set; } = "";
    public string WorkerName { get; private set; } = "";
    public bool SignedIn => token.Length > 0;
    public event Action? SessionEnded;
    public async Task RestoreAsync()
    {
        try { token = await SecureStorage.Default.GetAsync(AppConstants.TokenKey) ?? ""; }
        catch { token = ""; SecureStorage.Default.Remove(AppConstants.TokenKey); }
        WorkerCode = Preferences.Default.Get(AppConstants.WorkerKey, "");
        WorkerName = Preferences.Default.Get(AppConstants.WorkerNameKey, "");
    }
    public async Task<T> Request<T>(string path, object? body = null, bool authenticated = true, CancellationToken ct = default)
    {
        var requestToken = token;
        using var request = new HttpRequestMessage(body is null ? HttpMethod.Get : HttpMethod.Post, path);
        if (body is not null) request.Content = JsonContent.Create(body);
        if (authenticated) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", requestToken);
        try
        {
            using var response = await client.SendAsync(request, ct);
            var raw = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            var code = root.GetProperty("resultCode").GetString() ?? "";
            if (authenticated && requestToken == token && (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || code == "WAREHOUSE_SESSION_REPLACED" || (code is "200" or "201" or "202" or "300" or "360")))
            {
                Clear(); SessionEnded?.Invoke(); throw new ApiException("SESSION");
            }
            if (!response.IsSuccessStatusCode || code != "100") throw new ApiException(code);
            var data = root.GetProperty("resultData");
            return data.ValueKind == JsonValueKind.Null ? default! : data.Deserialize<T>(Json)!;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (ApiException) { throw; }
        catch (Exception) { throw new ApiException("NETWORK"); }
    }
    public async Task Login(string code, string password)
    {
        var worker = await Request<Worker>("login", new { worker_code = code, password }, false);
        if (string.IsNullOrWhiteSpace(worker.Token)) throw new ApiException("WAREHOUSE_LOGIN_FAILED");
        await SecureStorage.Default.SetAsync(AppConstants.TokenKey, worker.Token);
        token = worker.Token; SetWorker(worker);
    }
    public async Task Logout()
    {
        try { await Request<object>("logout", new { }); } catch { }
        Clear();
    }
    public void SetWorker(Worker worker)
    {
        WorkerCode = worker.Code; WorkerName = worker.Name;
        Preferences.Default.Set(AppConstants.WorkerKey, WorkerCode);
        Preferences.Default.Set(AppConstants.WorkerNameKey, WorkerName);
    }
    private void Clear()
    {
        token = ""; WorkerCode = ""; WorkerName = "";
        SecureStorage.Default.Remove(AppConstants.TokenKey);
        Preferences.Default.Remove(AppConstants.WorkerKey);
        Preferences.Default.Remove(AppConstants.WorkerNameKey);
    }
    public Task<Order> Detail(long id) => Request<Order>($"orders/{id}");
    public Task<Order> Action(long id, string action, object? body = null) => Request<Order>($"orders/{id}/{action}", body ?? new { });
}
