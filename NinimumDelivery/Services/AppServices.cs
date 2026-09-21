using Microsoft.Extensions.DependencyInjection;
namespace NinimumDelivery.Services;
public static class AppServices
{
    private static IServiceProvider? services;
    public static void Init(IServiceProvider provider) => services = provider;
    public static T GetRequired<T>() where T : class => services!.GetRequiredService<T>();
}
