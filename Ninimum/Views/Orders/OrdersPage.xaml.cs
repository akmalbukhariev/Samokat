using Ninimum.ViewModels;

namespace Ninimum.Views.Orders;

public partial class OrdersPage : BasePage
{
    private readonly OrdersPageViewModel viewModel;

    public OrdersPage(OrdersPageViewModel vm)
    {
        InitializeComponent();

        viewModel = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadOrdersAsync();
    }
}