using NinimumDelivery.ViewModels;
namespace NinimumDelivery.Views;
public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel vm;
    private bool loading;
    public ProfilePage(ProfileViewModel vm) { InitializeComponent(); BindingContext = this.vm = vm; }
    protected override async void OnAppearing() { base.OnAppearing(); loading = true; await vm.LoadCommand.ExecuteAsync(null); loading = false; }
    private async void OnlineSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        if (loading) return;
        await vm.SetOnlineAsync(e.Value);
    }
}
