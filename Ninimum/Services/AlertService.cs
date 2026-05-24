public static class AlertService
{
    public static async Task ShowAlertAsync(string title, string message, string okText = "OK")
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, okText);
        }
    }

    public static async Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No")
    {
        if (Application.Current?.MainPage != null)
        {
            return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }
        return false;
    }
}
