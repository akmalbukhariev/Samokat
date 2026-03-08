
public static class AppNavigatorService
{
    public static async Task NavigateTo(string route, bool animate = true)
    {
        await Shell.Current.GoToAsync(route, animate);
    }

    public static async Task NavigateTo(string route, Dictionary<string, object> param, bool animate = true)
    {
        await Shell.Current.GoToAsync(route, animate, param);
    }
}