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

    private async Task<RestResponse> Execute(RestRequest request, bool useToken = true, CancellationToken cancellationToken = default)
    {
        if (useToken && !string.IsNullOrWhiteSpace(token))
            request.AddHeader("Authorization", $"Bearer {token}");

        return await client.ExecuteAsync(request, cancellationToken);
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

    public Task<ApiResponse<DeliveryWorker>?> Me(CancellationToken cancellationToken = default) => Get<DeliveryWorker>("delivery-app/me", cancellationToken);
    public Task<ApiResponse<DeliveryDashboard>?> Dashboard(CancellationToken cancellationToken = default) => Get<DeliveryDashboard>("delivery-app/dashboard", cancellationToken);
    public Task<ApiResponse<List<DeliveryJob>>?> Available(CancellationToken cancellationToken = default) => Get<List<DeliveryJob>>("delivery-app/available", cancellationToken);
    public Task<ApiResponse<List<DeliveryJob>>?> Active(CancellationToken cancellationToken = default) => Get<List<DeliveryJob>>("delivery-app/active", cancellationToken);
    public Task<ApiResponse<List<DeliveryJob>>?> History(CancellationToken cancellationToken = default) => Get<List<DeliveryJob>>("delivery-app/history", cancellationToken);

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

    private async Task<ApiResponse<T>?> Get<T>(string path, CancellationToken cancellationToken = default)
    {
        var req = new RestRequest(path, Method.Get);
        return Parse<ApiResponse<T>>(await Execute(req, cancellationToken: cancellationToken));
    }

    public void Logout()
    {
        token = string.Empty;
        store.Remove(AppConstants.TokenKey);
        store.Remove(AppConstants.LoggedInKey);
    }
}
