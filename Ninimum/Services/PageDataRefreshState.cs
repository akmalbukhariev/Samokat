using System.Collections.Concurrent;

namespace Ninimum.Services;

/// <summary>
/// Keeps lightweight "dirty" flags for pages that already have data in memory.
/// A page should reload on return only when another action has changed data that
/// affects that page. Pull-to-refresh remains the user's explicit refresh path.
/// </summary>
public static class PageDataRefreshState
{
    private static readonly ConcurrentDictionary<string, byte> DirtyKeys = new();

    public const string Main = "Main";
    public const string Favorites = "Favorites";
    public const string Cart = "Cart";
    public const string Orders = "Orders";

    public static string ProductReviews(long productId) => $"ProductReviews:{productId}";
    public static string ProductQuestions(long productId) => $"ProductQuestions:{productId}";
    public static string DetailProduct(long productId) => $"DetailProduct:{productId}";

    public static void MarkDirty(string key)
    {
        if (!string.IsNullOrWhiteSpace(key))
            DirtyKeys[key] = 1;
    }

    public static bool ConsumeDirty(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        return DirtyKeys.TryRemove(key, out _);
    }
}
