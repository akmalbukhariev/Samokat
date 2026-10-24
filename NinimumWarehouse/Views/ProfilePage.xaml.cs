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
        ErrorDialogs.Attach(this, (ProfileViewModel)BindingContext);
    }
    private void OnLanguageChanged(object? sender,EventArgs e)
    {
        if(initializing || LanguagePicker.SelectedIndex<0 || LanguagePicker.SelectedIndex==LanguageService.SelectedIndex) return;
        LanguageService.Set(LanguagePicker.SelectedIndex);
        // Rebuild the screens only after the picker has closed; replacing them inside its callback crashes the app.
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () => App.CurrentApp.ShowHome(showProfile:true));
    }
}
