using Ninimum.Resources.Languages;
using Api.Services;
using Models.Requests;
using Models.Responses;
using Ninimum.Services;
using Ninimum.Views.Main;
using Ninimum.Views.MyTariff;
using System.Diagnostics;
using System.Windows.Input;
using Utils;

namespace Ninimum.Views.Payment;

public partial class PaymentPage : BasePage, IQueryAttributable
{
    private readonly UserApiService apiService;
    private readonly AppControl appControl;

    private long orderId;
    private long subscriptionId;
    private string paymentType = "ORDER";
    private string paymentUrl = string.Empty;

    private bool paymentReturnHandled;
    private int paymentFinished;

    private CancellationTokenSource? paymentStatusCts;
    private readonly SemaphoreSlim paymentStatusCheckLock = new(1, 1);
    private int backFlowRunning;

    public ICommand BackCommand { get; }

    public PaymentPage(UserApiService apiService, AppControl appControl)
    {
        InitializeComponent();

        this.apiService = apiService;
        this.appControl = appControl;

        BackCommand = new Command(async () => await HandleBackAsync());
        BindingContext = this;
}

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("PaymentUrl", out var paymentUrlValue))
            paymentUrl = paymentUrlValue?.ToString() ?? string.Empty;

        if (query.TryGetValue("OrderId", out var orderIdValue))
            long.TryParse(orderIdValue?.ToString(), out orderId);

        if (query.TryGetValue("SubscriptionId", out var subscriptionIdValue))
            long.TryParse(subscriptionIdValue?.ToString(), out subscriptionId);

        if (query.TryGetValue("PaymentType", out var paymentTypeValue) &&
            !string.IsNullOrWhiteSpace(paymentTypeValue?.ToString()))
            paymentType = paymentTypeValue!.ToString()!.ToUpperInvariant();

        Debug.WriteLine($"PAYMENT PAGE => type={paymentType}, orderId={orderId}, subscriptionId={subscriptionId}");

        if (string.IsNullOrWhiteSpace(paymentUrl))
            return;

        PaymeWebView.Source = paymentUrl;

        StartPaymentStatusChecking();
    }

    private void StartPaymentStatusChecking()
    {
        paymentStatusCts?.Cancel();
        paymentStatusCts?.Dispose();

        paymentStatusCts = new CancellationTokenSource();

        _ = MonitorPaymentStatusAsync(paymentStatusCts.Token);
    }

    private async Task MonitorPaymentStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested &&
                   Volatile.Read(ref paymentFinished) == 0)
            {
                await Task.Delay(2000, cancellationToken);

                bool finished = await CheckPaymentStatusOnceAsync(cancellationToken);

                if (finished)
                    return;
            }
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("PAYMENT STATUS MONITOR CANCELLED");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] MonitorPaymentStatusAsync => {ex}");
        }
    }

    private async Task<bool> CheckPaymentStatusOnceAsync(CancellationToken cancellationToken)
    {
        if (paymentType == "TARIFF" && subscriptionId <= 0)
            return false;

        if (paymentType != "TARIFF" && orderId <= 0)
            return false;

        bool lockTaken = false;

        try
        {
            await paymentStatusCheckLock.WaitAsync(cancellationToken);
            lockTaken = true;

            if (Volatile.Read(ref paymentFinished) == 1)
                return true;

            if (paymentType == "TARIFF")
            {
                var response = await apiService.GetTariffPaymentStatus(
                    new TariffPaymentStatusRequest
                    {
                        subscriptionId = subscriptionId,
                        userId = appControl.CurrentUserId
                    });

                if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
                {
                    Debug.WriteLine($"TARIFF PAYMENT STATUS API ERROR => {response.resultMsg}");
                    return false;
                }

                string paymentStatus = response.resultData?.paymentStatus ?? string.Empty;
                string subscriptionStatus = response.resultData?.subscriptionStatus ?? string.Empty;

                Debug.WriteLine($"TARIFF PAYMENT STATUS => subscriptionId={subscriptionId}, payment={paymentStatus}, subscription={subscriptionStatus}");

                if (paymentStatus.Equals("PAID", StringComparison.OrdinalIgnoreCase) ||
                    subscriptionStatus.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase))
                {
                    await FinishPaymentAsync("PAID");
                    return true;
                }

                if (paymentStatus.Equals("FAILED", StringComparison.OrdinalIgnoreCase) ||
                    paymentStatus.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase) ||
                    subscriptionStatus.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase))
                {
                    await FinishPaymentAsync(paymentStatus);
                    return true;
                }

                return false;
            }

            var orderResponse = await apiService.GetOrderPaymentStatus(
                new OrderStatusRequest
                {
                    orderId = orderId,
                    userId = appControl.CurrentUserId
                });

            if (orderResponse.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                Debug.WriteLine($"PAYMENT STATUS API ERROR => {orderResponse.resultMsg}");
                return false;
            }

            string orderPaymentStatus = orderResponse.resultData?.paymentStatus ?? string.Empty;

            Debug.WriteLine($"PAYMENT STATUS => orderId={orderId}, status={orderPaymentStatus}");

            if (orderPaymentStatus.Equals("PAID", StringComparison.OrdinalIgnoreCase) ||
                orderPaymentStatus.Equals("FAILED", StringComparison.OrdinalIgnoreCase) ||
                orderPaymentStatus.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                await FinishPaymentAsync(orderPaymentStatus);
                return true;
            }

            return false;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] CheckPaymentStatusOnceAsync => {ex}");
            return false;
        }
        finally
        {
            if (lockTaken)
                paymentStatusCheckLock.Release();
        }
    }

    private void PaymeWebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Url))
            return;

        Debug.WriteLine($"PAYME NAVIGATING => {e.Url}");

        if (e.Url.StartsWith(
                "ninimum://payment-result",
                StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;

            if (paymentReturnHandled ||
                Volatile.Read(ref paymentFinished) == 1)
            {
                return;
            }

            paymentReturnHandled = true;

            _ = CheckPaymentResultAsync();

            return;
        }

        ShowLoading(true);
    }

    private void PaymeWebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        ShowLoading(false);

        Debug.WriteLine(
            $"PAYME NAVIGATED => {e.Url}, result={e.Result}");
    }

    private async Task CheckPaymentResultAsync()
    {
        try
        {
            ShowLoading(true);

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                Debug.WriteLine(
                    $"PAYMENT RETURN CHECK => attempt={attempt}");

                bool finished = await CheckPaymentStatusOnceAsync(
                    CancellationToken.None);

                if (finished)
                    return;

                if (attempt < 3)
                    await Task.Delay(1000);
            }

            Debug.WriteLine(
                "PAYMENT RETURN CHECK => status is still pending");

            /*
             * Do not show an error here.
             *
             * The background payment monitor continues checking
             * the backend until Payme callback updates the status.
             */
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[ERROR] CheckPaymentResultAsync => {ex}");
        }
        finally
        {
            ShowLoading(false);
        }
    }

    private async Task FinishPaymentAsync(string paymentStatus)
    {
        if (Interlocked.Exchange(ref paymentFinished, 1) == 1)
            return;

        paymentStatusCts?.Cancel();

        Debug.WriteLine(
            $"PAYMENT FINISHED => type={paymentType}, orderId={orderId}, subscriptionId={subscriptionId}, status={paymentStatus}");

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            // Reset the Shell stack before leaving Payme. Otherwise the old
            // PaymentPage remains under the destination and Back can reopen
            // the Payme WebView after the payment has already completed.
            await AppNavigatorService.NavigateHome(false);

            if (paymentType == "TARIFF")
            {
                // Show the activated tariff as the completion screen. Home is
                // directly underneath it, so Back returns to MainPage.
                await AppNavigatorService.NavigateTo(nameof(MyTariffPage));
            }
        });
    }

    private async Task HandleBackAsync()
    {
        if (Interlocked.Exchange(ref backFlowRunning, 1) == 1)
            return;

        try
        {
            paymentStatusCts?.Cancel();

            if (Volatile.Read(ref paymentFinished) == 1)
                return;

            if (paymentType != "ORDER" || orderId <= 0)
            {
                await AppNavigatorService.NavigateTo("..");
                return;
            }

            string currentStatus = await GetCurrentOrderPaymentStatusAsync();

            if (currentStatus.Equals("PAID", StringComparison.OrdinalIgnoreCase))
            {
                await FinishPaymentAsync("PAID");
                return;
            }

            if (currentStatus.Equals("FAILED", StringComparison.OrdinalIgnoreCase) ||
                currentStatus.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                Interlocked.Exchange(ref paymentFinished, 1);
                await AppNavigatorService.NavigateTo("..");
                return;
            }

            bool shouldLeave = await Shell.Current.DisplayAlert(
                AppResource.PaymentNotCompleted,
                AppResource.IfYouGoBackThisUnpaidOrderWill,
                AppResource.Yes,
                AppResource.NoAscii);

            if (!shouldLeave)
            {
                StartPaymentStatusChecking();
                return;
            }

            Response cancelResponse = await apiService.CancelUnpaidOrder(new CancelOrderRequest
            {
                orderId = orderId,
                userId = appControl.CurrentUserId,
                reason = AppResource.PaymentPageWasClosed
            });

            if (cancelResponse.resultCode == ApiResult.SUCCESS.GetCodeToString())
            {
                Interlocked.Exchange(ref paymentFinished, 1);
                PageDataRefreshState.MarkDirty(PageDataRefreshState.Orders);
                await AppNavigatorService.NavigateTo("..");
                return;
            }

            // The payment callback may have completed while the user was confirming.
            currentStatus = await GetCurrentOrderPaymentStatusAsync();

            if (currentStatus.Equals("PAID", StringComparison.OrdinalIgnoreCase))
            {
                await FinishPaymentAsync("PAID");
                return;
            }

            await AlertService.ShowAlertAsync(
                AppResource.Error,
                cancelResponse.resultMsg ?? AppResource.CouldNotCancelTheUnpaidOrder);

            StartPaymentStatusChecking();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] HandleBackAsync => {ex}");
            await AlertService.ShowAlertAsync(AppResource.Error, AppResource.CouldNotCheckThePaymentStatus);
            StartPaymentStatusChecking();
        }
        finally
        {
            Interlocked.Exchange(ref backFlowRunning, 0);
        }
    }

    private async Task<string> GetCurrentOrderPaymentStatusAsync()
    {
        var response = await apiService.GetOrderPaymentStatus(new OrderStatusRequest
        {
            orderId = orderId,
            userId = appControl.CurrentUserId
        });

        if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            return string.Empty;

        return response.resultData?.paymentStatus ?? string.Empty;
    }

    private void ShowLoading(bool show)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LoadingLayout.IsVisible = show;
            ActivityIndicator.IsRunning = show;
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        paymentStatusCts?.Cancel();
    }

    protected override bool OnBackButtonPressed()
    {
        _ = HandleBackAsync();
        return true;
    }
}