using NinimumDelivery.ViewModels;
namespace NinimumDelivery.Views;
public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel vm;
    private bool loading;
    public ProfilePage(ProfileViewModel vm)
    {
        InitializeComponent(); BindingContext = this.vm = vm;
        PushTitle.Text = global::NinimumDelivery.Resources.Languages.AppResource.ResourceManager.GetString("PushTitle", global::NinimumDelivery.Resources.Languages.AppResource.Culture);
        PushHint.Text = global::NinimumDelivery.Resources.Languages.AppResource.ResourceManager.GetString("PushHint", global::NinimumDelivery.Resources.Languages.AppResource.Culture);
        global::NinimumDelivery.Services.RedSwitch.Apply(PushSwitch);
        global::NinimumDelivery.Services.RedSwitch.Apply(OnlineSwitch);
        PushSwitch.IsToggled = global::NinimumDelivery.Services.PushRegistration.Enabled; pushReady = true;
    }
    protected override async void OnAppearing() { base.OnAppearing(); loading = true; await vm.LoadCommand.ExecuteAsync(null); loading = false; }
    // Push notifications on/off for this phone (on by default).
    private bool pushReady;
    private async void OnPushToggled(object? sender, ToggledEventArgs e)
    {
        if (!pushReady) return;
        await global::NinimumDelivery.Services.PushRegistration.SetEnabledAsync(e.Value);
    }
    private async void OnlineSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        if (loading) return;
        await vm.SetOnlineAsync(e.Value);
    }
}
