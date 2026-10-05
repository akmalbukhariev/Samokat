using CommunityToolkit.Mvvm.ComponentModel;
using NinimumWarehouse.Services;
namespace NinimumWarehouse.ViewModels;
public abstract class ViewModelBase : ObservableObject
{
    protected readonly WarehouseApi Api;
    public Func<string, Task>? ShowErrorAsync { get; set; }
    private bool isBusy;
    private string errorMessage = "";
    protected ViewModelBase(WarehouseApi api) => Api = api;
    public bool IsBusy { get => isBusy; private set { if(SetProperty(ref isBusy,value)) OnPropertyChanged(nameof(IsAvailable)); } }
    public bool IsAvailable => !IsBusy;
    public string ErrorMessage { get => errorMessage; protected set { if(SetProperty(ref errorMessage,value)) OnPropertyChanged(nameof(HasError)); } }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    protected async Task RunAsync(Func<Task> action, bool showErrors = true)
    {
        if(IsBusy) return;
        IsBusy = true; ErrorMessage = "";
        try { await action(); }
        catch(OperationCanceledException) { }
        catch(Exception ex)
        {
            if(ex.Message != "SESSION")
            {
                ErrorMessage = LocalizedMessages.Error(ex.Message);
                if (showErrors && ShowErrorAsync is not null) await ShowErrorAsync(ErrorMessage);
            }
        }
        finally { IsBusy = false; }
    }
    protected void Notify(params string[] names) { foreach(var name in names) OnPropertyChanged(name); }
}
