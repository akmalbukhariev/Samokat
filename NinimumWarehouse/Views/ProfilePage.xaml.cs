using NinimumWarehouse.Resources.Languages;
using NinimumWarehouse.Services;
using NinimumWarehouse.ViewModels;
namespace NinimumWarehouse.Views;
public partial class ProfilePage : ContentPage
{
    private bool initializing=true;
    public ProfilePage(WarehouseApi api)
    {
        InitializeComponent(); BindingContext = new ProfileViewModel(api,() => DisplayAlertAsync(AppResource.SignOutQuestion,AppResource.SignOutMessage,AppResource.SignOut,AppResource.Cancel));
        LanguagePicker.SelectedIndex = LanguageService.SelectedIndex; initializing=false;
        PushTitle.Text = AppResource.ResourceManager.GetString("PushTitle", AppResource.Culture);
        PushHint.Text = AppResource.ResourceManager.GetString("PushHint", AppResource.Culture);
        PushSwitch.IsToggled = PushRegistration.Enabled; pushReady = true;
        ErrorDialogs.Attach(this, (ProfileViewModel)BindingContext);
    }
    // Push notifications on/off for this phone (on by default).
    private bool pushReady;
    private async void OnPushToggled(object? sender, ToggledEventArgs e)
    {
        if (!pushReady) return;
        await PushRegistration.SetEnabledAsync(e.Value);
    }
    private void OnLanguageChanged(object? sender,EventArgs e)
    {
        if(initializing || LanguagePicker.SelectedIndex<0 || LanguagePicker.SelectedIndex==LanguageService.SelectedIndex) return;
        LanguageService.Set(LanguagePicker.SelectedIndex);
        // Rebuild the screens only after the picker has closed; replacing them inside its callback crashes the app.
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () => App.CurrentApp.ShowHome(showProfile:true));
    }
}
