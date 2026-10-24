using NinimumWarehouse.Services;
using NinimumWarehouse.ViewModels;
namespace NinimumWarehouse.Views;
public partial class LoginPage : ContentPage
{
    private bool initializing = true;
    private LoginViewModel ViewModel => (LoginViewModel)BindingContext;
    public LoginPage(WarehouseApi api,string workerId="",string password="")
    {
        InitializeComponent(); BindingContext = new LoginViewModel(api) {WorkerId=workerId,Password=password};
        LanguagePicker.SelectedIndex = LanguageService.SelectedIndex; initializing = false;
        ErrorDialogs.Attach(this, (LoginViewModel)BindingContext);
    }
    private void OnLanguageChanged(object? sender,EventArgs e)
    {
        if(initializing || LanguagePicker.SelectedIndex<0 || LanguagePicker.SelectedIndex==LanguageService.SelectedIndex) return;
        LanguageService.Set(LanguagePicker.SelectedIndex);
        string worker = ViewModel.WorkerId, password = ViewModel.Password;
        // Rebuild the screen only after the picker has closed; replacing it inside its callback crashes the app.
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () => App.CurrentApp.ShowLogin(worker, password));
    }
    private void OnWorkerIdCompleted(object? sender,EventArgs e) => PasswordEntry.Focus();
}
