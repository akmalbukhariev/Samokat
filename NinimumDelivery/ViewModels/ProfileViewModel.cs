using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NinimumDelivery.Models;
using NinimumDelivery.Resources.Languages;
using NinimumDelivery.Services;

namespace NinimumDelivery.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly AppState state;
    private readonly DeliveryApiService api;
    private readonly LanguageService language;
    [ObservableProperty] private DeliveryWorker worker = new();
    [ObservableProperty] private bool isLoading;

    public ProfileViewModel(AppState state, DeliveryApiService api, LanguageService language)
    { this.state = state; this.api = api; this.language = language; }

    [RelayCommand]
    public async Task Load()
    {
        await state.RefreshWorkerAsync();
        Worker = state.Worker ?? new DeliveryWorker();
    }

    public async Task SetOnlineAsync(bool online)
    {
        if (IsLoading || Worker.online == online) return;
        try
        {
            IsLoading = true;
            var response = await api.SetOnline(online);
            if (response?.resultCode == "100")
            {
                Worker.online = online;
                OnPropertyChanged(nameof(Worker));
            }
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(Worker));
        }
    }

    [RelayCommand]
    private async Task Logout()
    {
        if (!await AlertService.Confirm(AppResource.LogoutConfirmTitle, AppResource.LogoutConfirmMessage, AppResource.Logout, AppResource.Close)) return;
        state.Logout();
    }

    [RelayCommand]
    private void SetLanguage(string code)
    {
        language.Set(code);
        state.ReloadForLanguage();
    }
}
