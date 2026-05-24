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
          
        public static byte[] ResizeImage(Stream imageStream, int maxWidth = 1024, int maxHeight = 1024, int quality = 80)
        {
            // Copy stream to byte array
            using var msOriginal = new MemoryStream();
            imageStream.CopyTo(msOriginal);
            var imageBytes = msOriginal.ToArray();

            try
            {
                // Decode bitmap
                using var original = SKBitmap.Decode(imageBytes);
                if (original == null)
                {
                    // Return original bytes if decode fails
                    return imageBytes;
                }

                // Try to read EXIF orientation
                SKEncodedOrigin origin = SKEncodedOrigin.TopLeft;
                try
                {
                    using var skStream = new SKMemoryStream(imageBytes);
                    using var codec = SKCodec.Create(skStream);
                    if (codec != null)
                        origin = codec.EncodedOrigin;
                }
                catch
                {
                    // Ignore codec errors, just use default orientation
                }

                using var orientedBitmap = ApplyExifOrientation(original, origin);

                int ow = orientedBitmap.Width, oh = orientedBitmap.Height;

                // If already small, just return JPEG directly
                if (ow <= maxWidth && oh <= maxHeight)
                {
                    using var ms = new MemoryStream();
                    using var img = SKImage.FromBitmap(orientedBitmap);
                    using var data = img.Encode(SKEncodedImageFormat.Jpeg, quality);
                    data?.SaveTo(ms);
                    return ms.ToArray();
                }

                // Resize
                float ratio = Math.Min((float)maxWidth / ow, (float)maxHeight / oh);
                int nw = Math.Max(1, (int)(ow * ratio));
                int nh = Math.Max(1, (int)(oh * ratio));

                var sampling = new SKSamplingOptions(SKFilterMode.Linear);
                using var resized = orientedBitmap.Resize(new SKImageInfo(nw, nh), sampling);
                if (resized == null)
                {
                    // Fallback: return original bytes
                    return imageBytes;
                }

                using var image = SKImage.FromBitmap(resized);
                using var msFinal = new MemoryStream();
                using var dataFinal = image.Encode(SKEncodedImageFormat.Jpeg, quality);
                dataFinal?.SaveTo(msFinal);

                return msFinal.ToArray();
            }
            catch
            {
                // Skia failed → Android native fallback
                return ConvertToJpegAndroid(imageBytes, maxWidth, maxHeight, quality);
            }
        }

        private static byte[] ConvertToJpegAndroid(byte[] bytes, int maxW, int maxH, int quality)
        {
            #if ANDROID
            try
            {
                // Probe image bounds without loading full bitmap
                var opts = new Android.Graphics.BitmapFactory.Options { InJustDecodeBounds = true };
                Android.Graphics.BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length, opts);

                if (opts.OutWidth <= 0 || opts.OutHeight <= 0)
                    return bytes;

                // Compute sampling to roughly fit within target
                opts.InSampleSize = ComputeInSampleSize(opts.OutWidth, opts.OutHeight, maxW, maxH);
                opts.InJustDecodeBounds = false;
                opts.InPreferredConfig = Android.Graphics.Bitmap.Config.Argb8888;

                using var bmp = Android.Graphics.BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length, opts);
                if (bmp == null) return bytes;

                // Scale again if still larger
                int w = bmp.Width, h = bmp.Height;
                float ratio = Math.Min((float)maxW / w, (float)maxH / h);

                using var finalBmp = (ratio < 1f)
                    ? Android.Graphics.Bitmap.CreateScaledBitmap(bmp,
                            Math.Max(1, (int)(w * ratio)),
                            Math.Max(1, (int)(h * ratio)),
                            true)
                    : bmp;

                using var ms = new MemoryStream();
                finalBmp.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, ClampQuality(quality), ms);
                return ms.ToArray();
            }
            catch
            {
                // Last resort: return original
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

        private static int ClampQuality(int q) => Math.Min(100, Math.Max(1, q));

        private static SKBitmap ApplyExifOrientation(SKBitmap bitmap, SKEncodedOrigin origin)
        {
            SKBitmap rotated;

            switch (origin)
            {
                case SKEncodedOrigin.BottomRight: // 180°
                    rotated = new SKBitmap(bitmap.Width, bitmap.Height);
                    using (var canvas = new SKCanvas(rotated))
                    {
                        canvas.RotateDegrees(180, bitmap.Width / 2, bitmap.Height / 2);
                        canvas.DrawBitmap(bitmap, 0, 0);
                    }
                    break;

                case SKEncodedOrigin.RightTop: // 90° CW
                    rotated = new SKBitmap(bitmap.Height, bitmap.Width);
                    using (var canvas = new SKCanvas(rotated))
                    {
                        canvas.Translate(rotated.Width, 0);
                        canvas.RotateDegrees(90);
                        canvas.DrawBitmap(bitmap, 0, 0);
                    }
                    break;

                case SKEncodedOrigin.LeftBottom: // 270° CW
                    rotated = new SKBitmap(bitmap.Height, bitmap.Width);
                    using (var canvas = new SKCanvas(rotated))
                    {
                        canvas.Translate(0, rotated.Height);
                        canvas.RotateDegrees(270);
                        canvas.DrawBitmap(bitmap, 0, 0);
                    }
                    break;

                default:
                    // No rotation needed
                    return bitmap;
            }

            return rotated;
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
