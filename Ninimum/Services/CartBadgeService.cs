using Api.Services;
using Models.Requests;
using Utils;

namespace Ninimum.Services;

// Counts the complete cart, rather than just the rows loaded by its paginated page.
public sealed class CartBadgeService
{
    private readonly UserApiService api;
    private readonly AppControl app;
    private bool running;
    private int version;
    private int userId;
    public int Count { get; private set; }
    public event Action? Changed;

    public CartBadgeService(UserApiService api, AppControl app)
    {
        this.api = api;
        this.app = app;
        PageDataRefreshState.MarkedDirty += key =>
        {
            if (key == PageDataRefreshState.Cart) RequestRefresh();
        };
    }

    public void RequestRefresh() => MainThread.BeginInvokeOnMainThread(async () => await RefreshAsync());

    public async Task RefreshAsync()
    {
        int requestedUser = app.IsAuthenticated ? app.CurrentUserId : 0;
        if (requestedUser != userId)
        {
            userId = requestedUser;
            SetCount(0);
        }
        version++;
        if (running) return;
        running = true;
        try
        {
            int snapshot;
            do
            {
                snapshot = version;
                int owner = userId;
                if (owner == 0) { SetCount(0); continue; }
                var ids = new HashSet<int>();
                const int pageSize = 100;
                int offset = 0;
                bool success = true;
                while (snapshot == version)
                {
                    var response = await api.GetCartList(new CartListRequest
                    {
                        user_id = owner, pageSize = pageSize, offset = offset
                    });
                    if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
                    { success = false; break; }
                    var rows = response.resultData;
                    if (rows == null || rows.Count == 0) break;
                    int previous = ids.Count;
                    foreach (var row in rows) ids.Add(row.cart_id);
                    if (rows.Count < pageSize) break;
                    // Do not loop forever if a server ignores pagination.
                    if (ids.Count == previous) { success = false; break; }
                    offset += rows.Count;
                }
                if (success && snapshot == version && app.IsAuthenticated && app.CurrentUserId == owner)
                    SetCount(ids.Count);
            } while (snapshot != version);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cart badge refresh failed: {ex.Message}");
        }
        finally { running = false; }
    }

    private void SetCount(int count)
    {
        if (Count == count) return;
        Count = count;
        Changed?.Invoke();
    }
}
