
namespace Utils
{
    internal class AppConstants
    {
#region Domen
        //public const string SERVER_DOMAIN = "95.182.118.233:8083";
        public const string SERVER_DOMAIN = "192.168.219.105:8083";
        public const string BASE_USER_URL = $"http://{SERVER_DOMAIN}/ninimum/api/v1/";
#endregion

        public static readonly string App_Url_PlayMarket = "";
        public static readonly string App_Url_AppStore = "";

        public static readonly string Version = AppInfo.Current.VersionString;     // e.g., "1.0"
        public static readonly string Build = AppInfo.Current.BuildString;         // e.g., "1.0.0"

        public static readonly string OsName = DeviceInfo.Current.Platform.ToString();      // Android, iOS, macOS, Windows
        public static readonly string OsVersion = DeviceInfo.Current.VersionString;         // OS version string

        public const int MaxRadius = 100;
        
        public const string FirstRunKey = "IsFirstRun";
         
        public const string UZ = "uz";
        public const string EN = "en";
        public const string RU = "ru";

        public const string ROLE_USER = "ROLE_USER";
        public const string ROLE_COMPANY = "ROLE_COMPANY";
        public const string SERVICE_COMPANY = "SERVICE_COMPANY";
        public const string SERVICE_USER = "SERVICE_USER";
        public const string SERVICE_CHAT = "SERVICE_CHAT";
        public const string SERVICE_MESSAGE = "SERVICE_MESSAGE";

        public const string NOTIFICATION_TITLE = "notification_title";
        public const string NOTIFICATION_BODY = "notification_body";
        public const string SEARCH_NOTIFICATION_FOR_USER = "SearchNotificationForUser";
        public const string SEARCH_NOTIFICATION_FOR_COMPANY = "SearchNotificationForCompany";

        public const string LAN_UZBEK = "O'zbekcha";
        public const string LAN_ENGLISH = "English";
        public const string LAN_RUSSIAN = "Русский";

        public const string LAN_ICON_UZBEK = "flag_uz.png";
        public const string LAN_ICON_ENGLISH = "flag_en.png";
        public const string LAN_ICON_RUSSIAN = "flag_ru.png";
        
    }
}
