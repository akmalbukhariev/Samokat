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
        LanguageService.Set(LanguagePicker.SelectedIndex); App.CurrentApp.ShowLogin(ViewModel.WorkerId,ViewModel.Password);
    }
    private void OnWorkerIdCompleted(object? sender,EventArgs e) => PasswordEntry.Focus();
}
