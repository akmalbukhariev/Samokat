namespace Ninimum.Services
{
    public static class AppService
    {
        private static IServiceProvider? _services;

        public static void Init(IServiceProvider serviceProvider)
        {
            _services = serviceProvider;
        }

        public static T? Get<T>() where T : class
        {
            return _services?.GetService<T>();
        }

        public static T GetRequired<T>() where T : class
        {
            return _services!.GetRequiredService<T>();
        }
    }
}