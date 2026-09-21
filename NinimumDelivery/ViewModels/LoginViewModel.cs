using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NinimumDelivery.Resources.Languages;
using NinimumDelivery.Services;

namespace NinimumDelivery.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AppState state;
    public LoginViewModel(AppState state) => this.state = state;

    [ObservableProperty] private string workerId = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private bool isLoading;

    [RelayCommand]
    private async Task Login()
    {
        if (IsLoading) return;
        var id = (WorkerId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(Password))
        {
            await AlertService.Show(AppResource.Error, AppResource.LoginFailed);
            return;
        }

        try
        {
            IsLoading = true;
            if (!await state.LoginAsync(id, Password))
                await AlertService.Show(AppResource.Error, AppResource.LoginFailed);
        }
        catch
        {
            await AlertService.Show(AppResource.Error, AppResource.ConnectionError);
        }
        finally { IsLoading = false; }
    }
}
