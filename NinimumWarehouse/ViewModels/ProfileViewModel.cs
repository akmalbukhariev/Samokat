using CommunityToolkit.Mvvm.Input;
using NinimumWarehouse.Services;
namespace NinimumWarehouse.ViewModels;
public sealed class ProfileViewModel : ViewModelBase
{
    public string WorkerName => string.IsNullOrWhiteSpace(Api.WorkerName) ? Api.WorkerCode : Api.WorkerName;
    public string WorkerCode => Api.WorkerCode;
    public IAsyncRelayCommand LogoutCommand { get; }
    public ProfileViewModel(WarehouseApi api,Func<Task<bool>> confirmLogout) : base(api)
    {
        LogoutCommand = new AsyncRelayCommand(() => RunAsync(async () => { if(await confirmLogout()) { await Api.Logout(); App.CurrentApp.ShowLogin(); } }));
    }
}
