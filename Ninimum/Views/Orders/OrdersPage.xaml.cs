using Ninimum.ViewModels;
using Ninimum.Services;
using Ninimum.Models;

namespace Ninimum.Views.Orders;

public partial class OrdersPage : BasePage
{
    private readonly OrdersPageViewModel viewModel;

    public OrdersPage(OrdersPageViewModel vm, AppControl appControl)
    {
        InitializeComponent();

        viewModel = vm;
        this.appControl = appControl;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!await appControl.EnsureAuthenticatedAsync(true))
            return;

        await viewModel.LoadOrdersAsync();
    }

    private async void Product_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is not VisualElement element ||
            element.BindingContext is not OrderProductItemModel product)
        return;

        await ClickGuard.RunAsync(element, async () =>
        {
            await element.ScaleTo(0.95, 100, Easing.CubicOut);
            await element.ScaleTo(1.0, 100, Easing.CubicIn);

            AppVibrationService.Like();
            
            if (viewModel.ProductClickedCommand.CanExecute(product))
                viewModel.ProductClickedCommand.Execute(product);
        });
    }
}
