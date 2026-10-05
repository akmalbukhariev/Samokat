using NinimumWarehouse.Resources.Languages;
using NinimumWarehouse.ViewModels;
namespace NinimumWarehouse.Services;
public static class ErrorDialogs
{
    public static void Attach(ContentPage page, ViewModelBase viewModel)
    {
        viewModel.ShowErrorAsync = message => page.Window is null ? Task.CompletedTask :
            page.DisplayAlertAsync(AppResource.Attention, message, AppResource.Ok);
    }
}
