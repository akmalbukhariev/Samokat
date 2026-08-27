using Ninimum.ViewModels;
using Ninimum.Services;

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
}