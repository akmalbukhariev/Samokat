using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
namespace NinimumWarehouse.Components;
public partial class ProductImage : ContentView
{
    public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(string), typeof(ProductImage), null,
        propertyChanged: (view, _, _) => ((ProductImage)view).StartLoading());
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(20) };
    private CancellationTokenSource? loading;
    private bool attached;
    public string? Source { get => (string?)GetValue(SourceProperty); set => SetValue(SourceProperty, value); }
    public ProductImage()
    {
        InitializeComponent();
        Loaded += (_, _) => { attached = true; StartLoading(); };
        Unloaded += (_, _) => { attached = false; loading?.Cancel(); };
    }
    private void StartLoading()
    {
        loading?.Cancel();
        if (!attached) return;
        Photo.IsVisible = false;
        var cancellation = new CancellationTokenSource();
        loading = cancellation;
        _ = LoadAsync(Source, cancellation);
    }
    private async Task LoadAsync(string? source, CancellationTokenSource cancellation)
    {
        var token = cancellation.Token;
        try
        {
            if (!Uri.TryCreate(source, UriKind.Absolute, out var url) || (url.Scheme != "http" && url.Scheme != "https")) return;
            var filename = Path.Combine(FileSystem.CacheDirectory, "product-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url.AbsoluteUri))) + ".img");
            if (!File.Exists(filename))
            {
                using var response = await Client.GetAsync(url, token);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[ProductImage] HTTP {(int)response.StatusCode}: {url}");
                    return;
                }
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType is not null && !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) && contentType != "application/octet-stream")
                {
                    Debug.WriteLine($"[ProductImage] Unexpected content type {contentType}: {url}");
                    return;
                }
                var bytes = await response.Content.ReadAsByteArrayAsync(token);
                if (bytes.Length == 0) return;
                // Only complete downloads enter the cache. A canceled page cannot leave a partial image.
                var temporary = filename + "." + Guid.NewGuid().ToString("N") + ".tmp";
                try { await File.WriteAllBytesAsync(temporary, bytes, token); File.Move(temporary, filename, overwrite: true); }
                finally { if (File.Exists(temporary)) File.Delete(temporary); }
            }
            token.ThrowIfCancellationRequested();
            if (!attached || !ReferenceEquals(loading, cancellation)) return;
            Photo.Source = ImageSource.FromFile(filename);
            Photo.IsVisible = true;
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { Debug.WriteLine($"[ProductImage] Download failed: {source}; {error.Message}"); }
        finally
        {
            if (ReferenceEquals(loading, cancellation)) loading = null;
            cancellation.Dispose();
        }
    }
}
