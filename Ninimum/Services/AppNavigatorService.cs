
public static class AppNavigatorService
{
    private const string HomeRoute = "//MainTabs/HomeTab/HomeRoot";

    public static async Task NavigateTo(string route, bool animate = true)
    {
        await Shell.Current.GoToAsync(route, animate);
    }

    public static async Task NavigateTo(string route, Dictionary<string, object> param, bool animate = true)
    {
        await Shell.Current.GoToAsync(route, animate, param);
    }

    /// <summary>
    /// Returns to the real Home Shell root and clears transient pages such as
    /// TariffsPage and PaymentPage from the navigation history.
    /// </summary>
    public static async Task NavigateHome(bool animate = true)
    {
        await Shell.Current.GoToAsync(HomeRoute, animate);
    }
}