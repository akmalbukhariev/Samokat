using Ninimum.Resources.Languages;
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
    [ObservableProperty] private bool isRefreshing;
    // An order's products never change, so they are fetched once per app session.
    private readonly Dictionary<long, List<OrderProductItemModel>> productCache = new();
    private bool loadRunning;
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

    [RelayCommand]
    private async Task Refresh()
    {
        try { await LoadOrdersAsync(); }
        finally { IsRefreshing = false; }
    }

    public async Task LoadOrdersAsync()
    {
        if (loadRunning) return;
        loadRunning = true;
        try
        {
            // The spinner only covers an empty screen; later reloads keep the current list visible.
            IsLoading = ActiveOrders.Count == 0 && CompletedOrders.Count == 0 && !IsRefreshing;

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
                    PartialReturnStatus = order.partialReturnStatus,
                    ProductCount = order.productCount,
                    TotalPriceValue = order.totalPrice,
                    TotalPrice = string.Format(AppResource.UZS_02854f, order.totalPrice).Replace(",", " "),
                    DeliveryMethod = order.deliveryMethod,
                    TaxiCompany = order.taxiCompany,
                    TaxiDriverName = order.taxiDriverName,
                    TaxiDriverPhone = order.taxiDriverPhone,
                    TaxiCarNumber = order.taxiCarNumber,
                    TaxiFare = order.taxiFare,
                    OrderDate = FormatDate(order.orderedAt)
                };

                if (isCompleted)
                    CompletedOrders.Add(item);
                else
                    ActiveOrders.Add(item);
            }

            ActiveCountText = ActiveOrders.Count.ToString();
            CompletedCountText = CompletedOrders.Count.ToString();

            // Show the list now; product rows fill in while the customer looks at it.
            IsLoading = false;
            await LoadProductsAsync(ActiveOrders.Concat(CompletedOrders).ToList());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] LoadOrdersAsync => {ex}");
        }
        finally
        {
            IsLoading = false;
            loadRunning = false;
        }
    }

    /// <summary>Cached products at once; the rest 4 orders at a time instead of one after another.</summary>
    private async Task LoadProductsAsync(List<OrderItemModel> orders)
    {
        var missing = new List<OrderItemModel>();
        foreach (var order in orders)
        {
            if (productCache.TryGetValue(order.OrderId, out var cached))
            {
                order.Products.Clear();
                foreach (var p in cached) order.Products.Add(p);
            }
            else missing.Add(order);
        }

        using var gate = new SemaphoreSlim(4);
        await Task.WhenAll(missing.Select(async order =>
        {
            await gate.WaitAsync();
            try { await LoadOrderProductsAsync(order); }
            finally { gate.Release(); }
        }));
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

            var products = response.resultData.Select(product => new OrderProductItemModel
            {
                ProductId = product.productId,
                ProductName = product.productName,
                ProductImageUrl = product.productImageUrl,
                UnitPrice = product.unitPrice,
                Quantity = product.quantity
            }).ToList();
            productCache[order.OrderId] = products;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                order.Products.Clear();
                foreach (var product in products) order.Products.Add(product);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] LoadOrderProductsAsync => {ex}");
        }
    }

    public async Task RefreshActiveOrderStatusesAsync()
    {
        if (IsLoading || loadRunning || ActiveOrders.Count == 0)
            return;

        var orders = ActiveOrders.ToList();
        bool completedStateChanged = false;

        foreach (var order in orders)
        {
            if (order.IsLoading)
                continue;

            bool wasCompleted = IsCompleted(order.Status);
            await LoadOrderProcessAsync(order);

            if (wasCompleted != IsCompleted(order.Status))
                completedStateChanged = true;
        }

        // When a courier finishes a delivery, move the order to the Completed tab.
        if (completedStateChanged)
            await LoadOrdersAsync();
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

            order.PartialReturnStatus = response.resultData.partialReturnStatus;
            order.Status = response.resultData.status ?? order.Status;
            order.DeliveryMethod = response.resultData.deliveryMethod ?? order.DeliveryMethod;
            order.TaxiCompany = response.resultData.taxiCompany;
            order.TaxiDriverName = response.resultData.taxiDriverName;
            order.TaxiDriverPhone = response.resultData.taxiDriverPhone;
            order.TaxiCarNumber = response.resultData.taxiCarNumber;
            order.TaxiFare = response.resultData.taxiFare;

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

        string productName = order.Products.FirstOrDefault()?.ProductName ?? AppResource.Orders_8b489f;

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
            AppResource.DeleteFromOrderHistory,
            AppResource.ThisOrderWillBeRemovedFromOrderHistory,
            AppResource.Delete,
            AppResource.Close);

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
                    AppResource.Error,
                    AppResource.CouldNotDeleteOrderHistory,
                    AppResource.Close);

                return;
            }

            CompletedOrders.Remove(order);

            CompletedCountText = CompletedOrders.Count.ToString();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] DeleteCompletedOrder => {ex}");

            await AlertService.ShowAlertAsync(
                AppResource.Error,
                AppResource.CouldNotDeleteOrderHistory,
                AppResource.Close);
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
        return string.Equals(status, "RETURNED", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(status, "DELIVERED", StringComparison.OrdinalIgnoreCase) ||
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