using Microsoft.Maui.Graphics.Platform;
using Newtonsoft.Json;
using RestSharp;
using SkiaSharp;
using System.Text;

namespace Api.Services
{
    public class ApiService
    {
        private readonly RestClient _client;
        protected string token = string.Empty;
        public ApiService(RestClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Stores the token securely
        /// </summary>
        public async Task SetTokenAsync(string token)
        {
            this.token = token;

            try
            {
                await SecureStorage.SetAsync("auth_token", token);
                System.Diagnostics.Debug.WriteLine("[SecureStorage] Token saved securely.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SecureStorage] Set failed: {ex.Message}");

                // Fallback for iOS simulator or unsigned builds
                Preferences.Set("auth_token", token);
                System.Diagnostics.Debug.WriteLine("[Preferences] Token saved in fallback storage.");
            }
        }

        /// <summary>
        /// Retrieves the stored token
        /// </summary>
        public async Task<string?> GetTokenAsync()
        {
            // 1️⃣ Fast path: already in RAM
            if (!string.IsNullOrEmpty(token))
                return token;

            // 2️⃣ Try secure storage first
            try
            {
                var secureToken = await SecureStorage.GetAsync("auth_token");
                if (!string.IsNullOrEmpty(secureToken))
                {
                    token = secureToken;
                    System.Diagnostics.Debug.WriteLine("[SecureStorage] Token loaded from secure storage.");
                    return secureToken;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SecureStorage] Get failed: {ex.Message}");
            }

            // 3️⃣ Fallback to local preferences
            if (Preferences.ContainsKey("auth_token"))
            {
                var prefToken = Preferences.Get("auth_token", null);
                token = prefToken ?? string.Empty;
                System.Diagnostics.Debug.WriteLine("[Preferences] Token loaded from fallback storage.");
                return prefToken;
            }

            System.Diagnostics.Debug.WriteLine("[Token] No saved token found.");
            return null;
        }

        /// <summary>
        /// Clears the stored token (for logout)
        /// </summary>
        public Task ClearTokenAsync()
        {
            try
            {
                SecureStorage.Remove("auth_token");
            }
            catch
            {
                // ignore secure failure
            }

            Preferences.Remove("auth_token");
            token = string.Empty;

            System.Diagnostics.Debug.WriteLine("[Token] Token cleared.");
            return Task.CompletedTask;
        }

        private async Task SetToken(RestRequest request)
        { 
            string? token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", $"Bearer {token}");
            }
        }

        private async Task<string> ExecuteRequestAsync(RestRequest request)
        {
            var response = await _client.ExecuteAsync(request);
            if (response.RawBytes != null && response.RawBytes.Length > 0)
            {
                var json = Encoding.UTF8.GetString(response.RawBytes);
                return json;
            }

            return response.Content ?? string.Empty;
        }

        public async Task<string> GetAsync(string endpoint, bool useToken = true)
        { 
            var request = new RestRequest(endpoint, Method.Get);
            if (useToken)
                await SetToken(request);

            return await ExecuteRequestAsync(request);
        }
        
        public async Task<string> PostAsync(string endpoint, object? data = null, bool addHeader = true, bool useToken = true)
        {         
            var request = new RestRequest(endpoint, Method.Post);
            if (addHeader)
                request.AddHeader("Content-Type", "application/json");

            if (useToken)
                await SetToken(request);

            if (data != null)
            {
                var json = JsonConvert.SerializeObject(data);
                request.AddJsonBody(json);
            }
            
            return await ExecuteRequestAsync(request);
        }

        public async Task<string> PutAsync(string endpoint, object? data = null)
        {
            var request = new RestRequest(endpoint, Method.Put);
            request.AddHeader("Content-Type", "application/json");
            await SetToken(request);

            if (data != null)
            {
                var json = JsonConvert.SerializeObject(data);
                request.AddJsonBody(json);
            }

            return await ExecuteRequestAsync(request);
        }

        public async Task<string> PostMultipartAsync(string endpoint, object data, IReadOnlyList<FileResult>? files = null)
        {
            var request = new RestRequest(endpoint, Method.Post);
            await SetToken(request);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("data", JsonConvert.SerializeObject(data));

            if (files != null)
            {
                foreach (var file in files)
                {
                    await using var stream = await file.OpenReadAsync();
                    var fileBytes = ResizeImage(stream);
                    var uploadFileName = $"review_{Guid.NewGuid():N}.jpg";
                    request.AddFile("images", fileBytes, uploadFileName, "image/jpeg");
                }
            }

            return await ExecuteRequestAsync(request);
        }

        public async Task<string> PostImageAsync(string endpoint, Stream imageStream, Dictionary<string, string>? additionalData = null, string streamName = "image_data")
        {
            var request = new RestRequest(endpoint, Method.Post);
            await SetToken(request);
            request.AlwaysMultipartFormData = true;

            if (additionalData != null)
            {
                foreach (var entry in additionalData)
                {
                    request.AddParameter(entry.Key, entry.Value);
                }
            }

            if (imageStream != null)
            {
                //var fileBytes = await ConvertStreamToByteArrayAsync(imageStream);
                var fileBytes = ResizeImage(imageStream);
                request.AddFile(streamName, fileBytes, "image.jpg", "image/jpeg");
            }

            return await ExecuteRequestAsync(request);
        }

        public async Task<string> DeleteAsync(string endpoint)
        {
            var request =  new RestRequest(endpoint, Method.Delete);
            await SetToken(request);

            return await ExecuteRequestAsync(request);
        }

        public async Task<string> DeleteAsync(string endpoint, object? data = null, bool addHeader = true, bool useToken = true)
        {         
            var request = new RestRequest(endpoint, Method.Delete);
            if (addHeader)
                request.AddHeader("Content-Type", "application/json");

            if (useToken)
                await SetToken(request);

            if (data != null)
            {
                var json = JsonConvert.SerializeObject(data);
                request.AddJsonBody(json);
            }
            
            return await ExecuteRequestAsync(request);
        }

        /// <summary>
        /// Normalizes EXIF orientation, constrains the image dimensions and encodes it as JPEG
        /// before it is uploaded. This prevents camera photos from appearing sideways after
        /// their EXIF metadata is removed by the server or another image decoder.
        /// </summary>
        public static byte[] ResizeImage(Stream imageStream, int maxWidth = 1024, int maxHeight = 1024, int quality = 80)
        {
            using var msOriginal = new MemoryStream();
            imageStream.CopyTo(msOriginal);
            byte[] imageBytes = msOriginal.ToArray();

            try
            {
                SKEncodedOrigin origin = SKEncodedOrigin.TopLeft;
                using (var skStream = new SKMemoryStream(imageBytes))
                using (var codec = SKCodec.Create(skStream))
                {
                    if (codec != null)
                        origin = codec.EncodedOrigin;
                }

                using var original = SKBitmap.Decode(imageBytes);
                if (original == null)
                    return imageBytes;

                SKBitmap oriented = ApplyExifOrientation(original, origin);
                try
                {
                    int originalWidth = oriented.Width;
                    int originalHeight = oriented.Height;
                    float ratio = Math.Min(1f, Math.Min((float)maxWidth / originalWidth, (float)maxHeight / originalHeight));
                    int newWidth = Math.Max(1, (int)Math.Round(originalWidth * ratio));
                    int newHeight = Math.Max(1, (int)Math.Round(originalHeight * ratio));

                    if (newWidth == originalWidth && newHeight == originalHeight)
                        return EncodeJpeg(oriented, quality);

                    var sampling = new SKSamplingOptions(SKFilterMode.Linear);
                    using var resized = oriented.Resize(new SKImageInfo(newWidth, newHeight), sampling);
                    return resized != null ? EncodeJpeg(resized, quality) : EncodeJpeg(oriented, quality);
                }
                finally
                {
                    if (!ReferenceEquals(oriented, original))
                        oriented.Dispose();
                }
            }
            catch
            {
                // Skia is normally used on both Android and iOS. If image decoding fails,
                // retain the previous Android fallback rather than blocking review upload.
                return ConvertToJpegAndroid(imageBytes, maxWidth, maxHeight, quality);
            }
        }

        private static byte[] EncodeJpeg(SKBitmap bitmap, int quality)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, ClampQuality(quality));
            return data?.ToArray() ?? Array.Empty<byte>();
        }

        private static byte[] ConvertToJpegAndroid(byte[] bytes, int maxW, int maxH, int quality)
        {
#if ANDROID
            try
            {
                var opts = new Android.Graphics.BitmapFactory.Options { InJustDecodeBounds = true };
                Android.Graphics.BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length, opts);

                if (opts.OutWidth <= 0 || opts.OutHeight <= 0)
                    return bytes;

                opts.InSampleSize = ComputeInSampleSize(opts.OutWidth, opts.OutHeight, maxW, maxH);
                opts.InJustDecodeBounds = false;
                opts.InPreferredConfig = Android.Graphics.Bitmap.Config.Argb8888;

                using var bmp = Android.Graphics.BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length, opts);
                if (bmp == null)
                    return bytes;

                int width = bmp.Width;
                int height = bmp.Height;
                float ratio = Math.Min(1f, Math.Min((float)maxW / width, (float)maxH / height));
                int newWidth = Math.Max(1, (int)Math.Round(width * ratio));
                int newHeight = Math.Max(1, (int)Math.Round(height * ratio));

                Android.Graphics.Bitmap? scaled = null;
                Android.Graphics.Bitmap finalBitmap = bmp;
                if (newWidth != width || newHeight != height)
                {
                    scaled = Android.Graphics.Bitmap.CreateScaledBitmap(bmp, newWidth, newHeight, true);
                    finalBitmap = scaled;
                }

                try
                {
                    using var ms = new MemoryStream();
                    finalBitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, ClampQuality(quality), ms);
                    return ms.ToArray();
                }
                finally
                {
                    scaled?.Dispose();
                }
            }
            catch
            {
                return bytes;
            }
#else
            return bytes;
#endif
        }

        private static int ComputeInSampleSize(int width, int height, int reqW, int reqH)
        {
            int inSample = 1;
            if (height > reqH || width > reqW)
            {
                int halfH = height / 2;
                int halfW = width / 2;
                while ((halfH / inSample) >= reqH && (halfW / inSample) >= reqW)
                    inSample *= 2;
            }
            return Math.Max(1, inSample);
        }

        private static int ClampQuality(int quality) => Math.Min(100, Math.Max(1, quality));

        /// <summary>
        /// Applies every EXIF orientation supported by SKCodec. The returned bitmap is a new
        /// bitmap only when a transform is required; TopLeft returns the original instance.
        /// </summary>
        private static SKBitmap ApplyExifOrientation(SKBitmap bitmap, SKEncodedOrigin origin)
        {
            if (origin == SKEncodedOrigin.TopLeft)
                return bitmap;

            bool swapDimensions = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
                or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;

            var result = new SKBitmap(
                swapDimensions ? bitmap.Height : bitmap.Width,
                swapDimensions ? bitmap.Width : bitmap.Height);

            using var canvas = new SKCanvas(result);

            switch (origin)
            {
                case SKEncodedOrigin.TopRight: // mirror horizontally
                    canvas.Translate(result.Width, 0);
                    canvas.Scale(-1, 1);
                    break;

                case SKEncodedOrigin.BottomRight: // 180 degrees
                    canvas.Translate(result.Width, result.Height);
                    canvas.RotateDegrees(180);
                    break;

                case SKEncodedOrigin.BottomLeft: // mirror vertically
                    canvas.Translate(0, result.Height);
                    canvas.Scale(1, -1);
                    break;

                case SKEncodedOrigin.LeftTop: // transpose
                    canvas.RotateDegrees(90);
                    canvas.Scale(1, -1);
                    break;

                case SKEncodedOrigin.RightTop: // 90 degrees clockwise
                    canvas.Translate(result.Width, 0);
                    canvas.RotateDegrees(90);
                    break;

                case SKEncodedOrigin.RightBottom: // transverse
                    canvas.Translate(result.Width, result.Height);
                    canvas.Scale(1, -1);
                    canvas.RotateDegrees(90);
                    break;

                case SKEncodedOrigin.LeftBottom: // 270 degrees clockwise
                    canvas.Translate(0, result.Height);
                    canvas.RotateDegrees(270);
                    break;
            }

            canvas.DrawBitmap(bitmap, 0, 0);
            canvas.Flush();
            return result;
        }

        /// <summary>
        /// Generic login method that allows different response types.
        /// </summary>
        public async Task<T?> LoginAsync<T>(string endpoint, object data) where T : class
        {
            try
            {
                var request = new RestRequest(endpoint, Method.Post);
                request.AddHeader("Content-Type", "application/json");
                var json = JsonConvert.SerializeObject(data);
                request.AddJsonBody(json);

                var request111 = _client.BuildUri(request);
                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrWhiteSpace(response.Content))
                {
                    var result = JsonConvert.DeserializeObject<T>(response.Content);

                    // Extract token from headers
                    if (response.Headers != null)
                    {
                        var tokenHeader = response.Headers.FirstOrDefault(h => h.Name == "access-token");
                        if (tokenHeader != null && tokenHeader.Value != null)
                        {
                            string token = tokenHeader.Value.ToString();
                            await SetTokenAsync(token);
                        }
                    }

                    return result;
                }
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON Parsing Error: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex.Message}");
            }

            return null;
        }
    }
}
