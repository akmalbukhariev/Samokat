using CommunityToolkit.Mvvm.Input;
using NinimumWarehouse.Resources.Languages;
using NinimumWarehouse.Services;
namespace NinimumWarehouse.ViewModels;
public sealed class LoginViewModel : ViewModelBase
{
    private string workerId = "", password = "";
    public string WorkerId { get => workerId; set => SetProperty(ref workerId,value); }
    public string Password { get => password; set => SetProperty(ref password,value); }
    public IAsyncRelayCommand LoginCommand { get; }
    public LoginViewModel(WarehouseApi api) : base(api)
    {
        LoginCommand = new AsyncRelayCommand(() => RunAsync(async () => {
            if(string.IsNullOrWhiteSpace(WorkerId) || string.IsNullOrEmpty(Password)) throw new ApiException("LOGIN_REQUIRED");
            await Api.Login(WorkerId.Trim(),Password); App.CurrentApp.ShowHome();
        }));
    }
}
