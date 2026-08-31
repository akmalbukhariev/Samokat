using Api.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models.Requests;
using Ninimum.Models;
using Ninimum.Services;
using Ninimum.Views.DetailProduct;
using Ninimum.Views.PaymentCard;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Utils;

namespace Ninimum.ViewModels;

public partial class OrdersPageViewModel : ObservableObject
{
    private readonly UserApiService apiService;
    private readonly AppControl appControl;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool showActiveOrders = true;
    [ObservableProperty] private bool showCompletedOrders;
    [ObservableProperty] private string activeCountText = "0";
    [ObservableProperty] private string completedCountText = "0";
    [ObservableProperty] private Color activeTabBackground = Colors.White;
    [ObservableProperty] private Color completedTabBackground = Colors.Transparent;

    public ObservableCollection<OrderItemModel> ActiveOrders { get; } = new();
    public ObservableCollection<OrderItemModel> CompletedOrders { get; } = new();

    public OrdersPageViewModel(UserApiService apiService, AppControl appControl)
    {
        this.apiService = apiService;
        this.appControl = appControl;
    }

    public async Task LoadOrdersAsync()
    {
        try
        {
            IsLoading = true;

            var response = await apiService.GetOrderList(new OrderListRequest
            {
                userId = (long)appControl.userDto.id
            });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString() || response.resultData == null)
                return;

            ActiveOrders.Clear();
            CompletedOrders.Clear();

            foreach (var order in response.resultData)
            {
                bool isPaid = string.Equals(order.paymentStatus, "PAID", StringComparison.OrdinalIgnoreCase);
                bool isRefunded = string.Equals(order.paymentStatus, "REFUNDED", StringComparison.OrdinalIgnoreCase);
                bool isCompleted = IsCompleted(order.status);

                if ((!isCompleted && !isPaid) || (isCompleted && !isPaid && !isRefunded))
                    continue;

                var item = new OrderItemModel
                {
                    OrderId = order.orderId,
                    OrderNumber = order.orderNumber,
                    Status = order.status,
                    PaymentStatus = order.paymentStatus,
                    ProductCount = order.productCount,
                    TotalPriceValue = order.totalPrice,
                    TotalPrice = $"{FormatPrice(order.totalPrice)} so’m",
                    OrderDate = FormatDate(order.orderedAt)
                };

                if (isCompleted)
                    CompletedOrders.Add(item);
                else
                    ActiveOrders.Add(item);
            }

            ActiveCountText = ActiveOrders.Count.ToString();
            CompletedCountText = CompletedOrders.Count.ToString();

            foreach (var order in ActiveOrders)
                await LoadOrderProductsAsync(order);

            foreach (var order in CompletedOrders)
                await LoadOrderProductsAsync(order);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] LoadOrdersAsync => {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadOrderProductsAsync(OrderItemModel order)
    {
        try
        {
            var response = await apiService.GetOrderDetail(new OrderStatusRequest
            {
                orderId = order.OrderId,
                userId = (long)appControl.userDto.id
            });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString() || response.resultData == null)
                return;

            order.Products.Clear();

            foreach (var product in response.resultData)
            {
                order.Products.Add(new OrderProductItemModel
                {
                    ProductId = product.productId,
                    ProductName = product.productName,
                    ProductImageUrl = product.productImageUrl,
                    UnitPrice = product.unitPrice,
                    Quantity = product.quantity
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] LoadOrderProductsAsync => {ex}");
        }
    }

    private async Task LoadOrderProcessAsync(OrderItemModel order)
    {
        try
        {
            var response = await apiService.GetOrderProcess(new OrderStatusRequest
            {
                orderId = order.OrderId,
                userId = (long)appControl.userDto.id
            });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString() || response.resultData == null)
                return;

            order.Status = response.resultData.status ?? order.Status;

            Debug.WriteLine($"ORDER PROCESS => orderId={order.OrderId}, status={order.Status}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] LoadOrderProcessAsync => {ex}");
        }
    }

    [RelayCommand]
    private async Task ProductClicked(OrderProductItemModel product)
    {
        if (product == null || product.ProductId <= 0)
            return;

        await AppNavigatorService.NavigateTo(
            $"{nameof(DetailProductPage)}?productId={product.ProductId}");
    }

    [RelayCommand]
    private async Task ToggleOrder(OrderItemModel order)
    {
        if (order == null || order.IsLoading)
            return;

        if (order.IsExpanded)
        {
            order.IsExpanded = false;
            return;
        }

        order.IsExpanded = true;

        try
        {
            order.IsLoading = true;

            await LoadOrderProcessAsync(order);
        }
        finally
        {
            order.IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CancelOrder(OrderItemModel order)
    {
        if (order == null || !order.CanCancel)
            return;

        await LoadOrderProcessAsync(order);

        if (!order.CanCancel)
            return;

        string productName = order.Products.FirstOrDefault()?.ProductName ?? "Buyurtma";

        if (order.Products.Count > 1)
            productName = $"{productName} +{order.Products.Count - 1}";

        await AppNavigatorService.NavigateTo(nameof(CancelOrderPage), new Dictionary<string, object>
        {
            ["OrderId"] = order.OrderId,
            ["ProductName"] = productName,
            ["OrderNumber"] = order.DisplayOrderNumber,
            ["OrderDate"] = order.OrderDate,
            ["OrderAmount"] = order.TotalPrice
        });
    }

    [RelayCommand]
    private async Task DeleteCompletedOrder(OrderItemModel order)
    {
        if (order == null || order.IsLoading)
            return;

        bool confirmed = await AlertService.ShowConfirmationAsync(
            "Buyurtmalar tarixidan o‘chirish",
            "Bu buyurtma buyurtmalar tarixidan o‘chiriladi va uni qayta tiklab bo‘lmaydi. O‘chirmoqchimisiz?",
            "O‘chirish",
            "Yopish");

        if (!confirmed)
            return;

        try
        {
            order.IsLoading = true;
            IsLoading = true;

            var response = await apiService.DeleteOrderHistory(new OrderStatusRequest
            {
                orderId = order.OrderId,
                userId = (long)appControl.userDto.id
            });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                await AlertService.ShowAlertAsync(
                    "Xatolik",
                    "Buyurtmalar tarixini o‘chirib bo‘lmadi.",
                    "Yopish");

                return;
            }

            CompletedOrders.Remove(order);

            CompletedCountText = CompletedOrders.Count.ToString();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] DeleteCompletedOrder => {ex}");

            await AlertService.ShowAlertAsync(
                "Xatolik",
                "Buyurtmalar tarixini o‘chirib bo‘lmadi.",
                "Yopish");
        }
        finally
        {
            order.IsLoading = false;
            IsLoading = false;
        }
    }
    [RelayCommand]
    private void ShowActive()
    {
        ShowActiveOrders = true;
        ShowCompletedOrders = false;

        ActiveTabBackground = Colors.White;
        CompletedTabBackground = Colors.Transparent;
    }

    [RelayCommand]
    private void ShowCompleted()
    {
        ShowActiveOrders = false;
        ShowCompletedOrders = true;

        ActiveTabBackground = Colors.Transparent;
        CompletedTabBackground = Colors.White;
    }

    private bool IsCompleted(string status)
    {
        return string.Equals(status, "DELIVERED", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase);
    }

    private string FormatPrice(decimal value)
    {
        return string.Format("{0:N0}", value).Replace(",", " ");
    }

    private string FormatDate(string value)
    {
        return DateTime.TryParse(value, out var date) ? date.ToString("dd.MM.yy") : value;
    }
}