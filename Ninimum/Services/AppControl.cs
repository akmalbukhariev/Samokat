using System.Globalization;
using System.Text.RegularExpressions;
using Api.Services;
using Models.Dto;
using Models.Requests;
using Models.Responses;
using Utils;

namespace Ninimum.Services
{
    public class AppControl
    {
        public bool IsBlocked = false;
        public UserDto userDto { get; set; } = new UserDto();
        private readonly LanguageService lang;
        private readonly UserApiService apiService;
        private AppStoreService storeService;

        public AppControl(
            LanguageService lang,
            AppStoreService storeService,
            UserApiService apiService)
        {
            this.lang = lang;
            this.storeService = storeService;
            this.apiService = apiService;
        }

        public bool IsConnectedToWifi()
        {
            return Connectivity.NetworkAccess == NetworkAccess.Internet;
        }

        public async Task Login(string phoneNumber, string password)
        {
            var request = new LoginUserRequest
            {
                phone_number = phoneNumber,
                password = password
            };
            LoginUserResponse response = await apiService.Login(request);
            if (response.resultCode == ApiResult.SUCCESS.GetCodeToString())
            {
                userDto = response.resultData;

                storeService.Set(AppKeys.IsLoggedIn, true);
                storeService.Set(AppKeys.PhoneNumber, phoneNumber);
                storeService.Set(AppKeys.Password, password);

                SetRootPage(new ContentPage
                {
                    BackgroundColor = Colors.White,
                    Content = new ActivityIndicator
                    {
                        IsRunning = true,
                        Color = AppConstants.COLOR_USER,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center
                    }
                });

                await Task.Delay(100);
                
                SetRootPage(new AppShell());
            }
        }

        public async Task Logout(bool isBlocked = true)
        {
            IsBlocked = isBlocked;

            Response response = await apiService.LogOut();
            if (response.resultCode == ApiResult.SUCCESS.GetCodeToString())
            {
                //do something
            }

            storeService.Remove(AppKeys.IsLoggedIn);
            storeService.Remove(AppKeys.PhoneNumber);
            storeService.Remove(AppKeys.Password);

            await apiService.ClearTokenAsync();

            SetRootPage(new AppEntryShell());
        }

        public void SetRootPage(Page page)
        {
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window == null) return;

            // If you call this from a background thread, force UI thread:
            MainThread.BeginInvokeOnMainThread(() =>
            {
                window.Page = page;
            });
        }

        public void ShowTabBar(bool show)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var page = Shell.Current?.CurrentPage;

                if (page != null)
                {
                    Shell.SetTabBarIsVisible(page, show);
                }
            });
        }

        public bool IsValidUzbekistanPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            phoneNumber = phoneNumber.Trim();

            const string PHONE_PATTERN = @"^998(90|91|93|94|50|77|95|99|87|88|97|98|33|20)\d{7}$";

            return Regex.IsMatch(phoneNumber, PHONE_PATTERN);
        }
         
        public string GetUzbCurrency(decimal? price)
        {
            var uz = new CultureInfo("uz-UZ");
            return string.Format(uz, "{0:N0} so'm", price);
        }

#region Check url image
        private readonly string[] AllowedBases =
        {
            $"http://{AppConstants.SERVER_DOMAIN}/uploads-user/profile-pictures/",
            $"http://{AppConstants.SERVER_DOMAIN}/uploads-company/profile-pictures/",
            $"http://{AppConstants.SERVER_DOMAIN}/uploads-company/poster-pictures/"
        };

        public string GetImageUrlOrFallback(string? imageUrl, string fallback = "no_image.png")
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return fallback;

            // Must be a valid absolute http/https URL
            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return fallback;
            }

            // Normalize for comparison (ignore trailing '/')
            string normUrl = TrimEndSlash(uri.ToString());
            var normBases = AllowedBases.Select(TrimEndSlash).ToArray();

            // Case 1: exactly the base path (no filename) -> fallback
            if (normBases.Any(b => string.Equals(normUrl, b, StringComparison.OrdinalIgnoreCase)))
                return fallback;

            // Case 2: must start with one of the bases AND have something after it (a file part)
            bool hasAllowedPrefixWithFile = normBases.Any(b =>
                normUrl.StartsWith(b, StringComparison.OrdinalIgnoreCase) &&
                normUrl.Length > b.Length); // ensures there's more than the base

            if (!hasAllowedPrefixWithFile)
                return fallback;

            return imageUrl;
        }

        private string TrimEndSlash(string s) => s.TrimEnd('/');
#endregion
        
#region Photo
        public async Task<FileResult?> TryPickPhotoAsync()
        {
            try
            {   
                if (!await EnsureGalleryPermissionAsync())
                    return null;

                return await MediaPicker.PickPhotoAsync();
            }
            catch (OperationCanceledException) { return null; }
            catch (PermissionException) { return null; }
        }

        public async Task<FileResult?> TryCapturePhotoAsync()
        {
            try
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                    return null;

                // ✅ Ensure permission first (prevents crash + gives user guidance)
                if (!await EnsureCameraPermissionAsync())
                    return null;

                return await MediaPicker.CapturePhotoAsync();
            }
            catch (OperationCanceledException) { return null; }
            catch (PermissionException) { return null; }
        }

        public async Task<bool> EnsureGalleryPermissionAsync()
        {
        #if IOS
            var status = await Permissions.CheckStatusAsync<Permissions.Photos>();

            if (status == PermissionStatus.Granted)
                return true;

            if (status == PermissionStatus.Limited)
            {
                await AskUserToGrantFullPhotoAccess();
                return false;
            }

            status = await Permissions.RequestAsync<Permissions.Photos>();

            if (status == PermissionStatus.Granted)
                return true;

            if (status == PermissionStatus.Limited)
            {
                await AskUserToGrantFullPhotoAccess();
                return false;
            }

            await AskUserToGrantFullPhotoAccess();
            return false;
        #else
            // Android: your existing logic
            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
            return status == PermissionStatus.Granted;
        #endif
        }

        private async Task AskUserToGrantFullPhotoAccess()
        {
            bool openSettings = await Shell.Current.DisplayAlert(
                "",//AppResource.PermissionRequired,
                "",//AppResource.MessagePermissionRequired,
                "",//AppResource.OpenSettings,
                "");//AppResource.Cancel);

            if (openSettings)
            {
                // This opens the app's settings page directly
                AppInfo.ShowSettingsUI();
            }
        }
        
        public async Task<bool> EnsureCameraPermissionAsync()
        {
        #if IOS
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

            // Already granted ✅
            if (status == PermissionStatus.Granted)
                return true;

            // If previously denied/restricted, no point re-asking — go straight to settings
            if (status == PermissionStatus.Denied || status == PermissionStatus.Restricted)
            {
                await AskUserToGrantFullPhotoAccess();
                return false;
            }

            // First-time/unknown → request it
            status = await Permissions.RequestAsync<Permissions.Camera>();

            if (status == PermissionStatus.Granted)
                return true;

            // Still not granted → ask user to open Settings
            await AskUserToGrantFullPhotoAccess();
            return false;

        #elif ANDROID
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (status == PermissionStatus.Granted)
                return true;

            status = await Permissions.RequestAsync<Permissions.Camera>();

            if (status == PermissionStatus.Granted)
                return true;

            // User denied (maybe checked "Don't ask again") → suggest opening settings
            await AskUserToGrantFullPhotoAccess();
            return false;

        #else
            // Other platforms: treat as granted
            return true;
        #endif
        }

#endregion
    }
}
