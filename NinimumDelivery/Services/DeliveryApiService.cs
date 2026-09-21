using Newtonsoft.Json;
using NinimumDelivery.Models;
using RestSharp;

namespace NinimumDelivery.Services;

public class DeliveryApiService
{
    private readonly RestClient client;
    private readonly AppStoreService store;
    private string token = string.Empty;

    public DeliveryApiService(RestClient client, AppStoreService store)
    {
        this.client = client;
        this.store = store;
        token = store.Get<string>(AppConstants.TokenKey, string.Empty);
    }

    private async Task<RestResponse> Execute(RestRequest request, bool useToken = true)
    {
        if (useToken && !string.IsNullOrWhiteSpace(token))
            request.AddHeader("Authorization", $"Bearer {token}");
        return await client.ExecuteAsync(request);
    }

    private static T? Parse<T>(RestResponse response) where T : class =>
        string.IsNullOrWhiteSpace(response.Content) ? null : JsonConvert.DeserializeObject<T>(response.Content);

    public async Task<ApiResponse<DeliveryWorker>?> Login(string workerId, string password)
    {
        var req = new RestRequest("delivery-app/login", Method.Post).AddJsonBody(new { workerId, password });
        var res = await Execute(req, false);
        if (res.IsSuccessful)
        {
            var header = res.Headers?.FirstOrDefault(x => string.Equals(x.Name, "access-token", StringComparison.OrdinalIgnoreCase));
            if (header?.Value != null)
            {
                token = header.Value.ToString() ?? string.Empty;
                store.Set(AppConstants.TokenKey, token);
            }
        }
        return Parse<ApiResponse<DeliveryWorker>>(res);
    }

    public Task<ApiResponse<DeliveryWorker>?> Me() => Get<DeliveryWorker>("delivery-app/me");
    public Task<ApiResponse<DeliveryDashboard>?> Dashboard() => Get<DeliveryDashboard>("delivery-app/dashboard");
    public Task<ApiResponse<List<DeliveryJob>>?> Available() => Get<List<DeliveryJob>>("delivery-app/available");
    public Task<ApiResponse<List<DeliveryJob>>?> Active() => Get<List<DeliveryJob>>("delivery-app/active");
    public Task<ApiResponse<List<DeliveryJob>>?> History() => Get<List<DeliveryJob>>("delivery-app/history");

    public async Task<ApiResponse<DeliveryJob>?> Detail(long jobId)
    {
        var req = new RestRequest("delivery-app/detail", Method.Post).AddJsonBody(new { jobId });
        return Parse<ApiResponse<DeliveryJob>>(await Execute(req));
    }

    public async Task<ApiResponse?> Claim(long jobId)
    {
        var req = new RestRequest("delivery-app/claim", Method.Put).AddJsonBody(new { jobId });
        return Parse<ApiResponse>(await Execute(req));
    }

    public async Task<ApiResponse?> UpdateStatus(long jobId, string status, string? note = null)
    {
        var req = new RestRequest("delivery-app/status", Method.Put).AddJsonBody(new { jobId, status, note });
        return Parse<ApiResponse>(await Execute(req));
    }

    public async Task<ApiResponse?> SetOnline(bool online)
    {
        var req = new RestRequest("delivery-app/online", Method.Put).AddJsonBody(new { online });
        return Parse<ApiResponse>(await Execute(req));
    }

    private async Task<ApiResponse<T>?> Get<T>(string path)
    {
        var req = new RestRequest(path, Method.Get);
        return Parse<ApiResponse<T>>(await Execute(req));
    }

    public void Logout()
    {
        token = string.Empty;
        store.Remove(AppConstants.TokenKey);
        store.Remove(AppConstants.LoggedInKey);
    }
}
