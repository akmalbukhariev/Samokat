namespace NinimumWarehouse.Services;
public static class ProductImageSource
{
    public static string Resolve(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "warehouse_brand.png";
        if (Uri.TryCreate(path.Trim(), UriKind.Absolute, out var absolute) &&
            (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
        {
            // Stored uploads may retain a local development address or the internal backend port.
            var gateway = new Uri(AppConstants.BaseUrl);
            var localHost = absolute.IsLoopback || absolute.Host.StartsWith("192.168.", StringComparison.Ordinal) ||
                absolute.Host.StartsWith("10.", StringComparison.Ordinal);
            if (absolute.AbsolutePath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) &&
                (localHost || (absolute.Host == gateway.Host && absolute.Port != gateway.Port)))
                return new Uri(new Uri(gateway.GetLeftPart(UriPartial.Authority)), absolute.PathAndQuery).AbsoluteUri;
            return absolute.AbsoluteUri;
        }
        // Upload paths are relative to the server, not the warehouse API route.
        var server = new Uri(new Uri(AppConstants.BaseUrl).GetLeftPart(UriPartial.Authority) + "/");
        var relative = path.Trim().TrimStart('/');
        // Product image values use the same storage-relative paths as the Admin catalog.
        if (relative.StartsWith("products/", StringComparison.OrdinalIgnoreCase) ||
            relative.StartsWith("reviews/", StringComparison.OrdinalIgnoreCase))
            relative = "uploads/" + relative;
        else if (!relative.Contains('/'))
            relative = "uploads/products/" + relative;
        return Uri.TryCreate(server, relative, out var image) ? image.AbsoluteUri : "warehouse_brand.png";
    }
}
