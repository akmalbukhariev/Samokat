using Ninimum.Resources.Languages;
public static class AlertService
{
    public static async Task ShowAlertAsync(string title, string message, string? okText = null)
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, okText ?? AppResource.Ok);
        }
    }

    public static async Task<bool> ShowConfirmationAsync(string title, string message, string? accept = null, string? cancel = null)
    {
        if (Application.Current?.MainPage != null)
        {
            return await Application.Current.MainPage.DisplayAlert(title, message, accept ?? AppResource.Yes, cancel ?? AppResource.No);
        }
        return false;
    }
}
