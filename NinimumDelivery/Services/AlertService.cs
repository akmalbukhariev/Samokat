using NinimumDelivery.Resources.Languages;
namespace NinimumDelivery.Services;
public static class AlertService
{
    public static Task Show(string title, string message) =>
        Application.Current?.Windows.FirstOrDefault()?.Page?.DisplayAlert(title, message, AppResource.OK) ?? Task.CompletedTask;
    public static Task<bool> Confirm(string title, string message, string accept, string cancel) =>
        Application.Current?.Windows.FirstOrDefault()?.Page?.DisplayAlert(title, message, accept, cancel) ?? Task.FromResult(false);
}
