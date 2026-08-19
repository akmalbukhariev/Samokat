using Api.Services;
using Models.Requests;
using Ninimum.Services;
using Ninimum.Views.Main;
using System.Diagnostics;
using Utils;

namespace Ninimum.Views.Payment;

public partial class PaymentPage : BasePage, IQueryAttributable
{
    private readonly UserApiService apiService;
    private readonly AppControl appControl;

    private long orderId;
    private string paymentUrl = string.Empty;

    private bool paymentReturnHandled;
    private int paymentFinished;

    private CancellationTokenSource? paymentStatusCts;
    private readonly SemaphoreSlim paymentStatusCheckLock = new(1, 1);

    public PaymentPage(UserApiService apiService, AppControl appControl)
    {
        InitializeComponent();

        this.apiService = apiService;
        this.appControl = appControl;

        Shell.SetTabBarIsVisible(this, false);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("PaymentUrl", out var paymentUrlValue))
            paymentUrl = paymentUrlValue?.ToString() ?? string.Empty;

        if (query.TryGetValue("OrderId", out var orderIdValue))
            long.TryParse(orderIdValue?.ToString(), out orderId);

        Debug.WriteLine($"PAYMENT PAGE => orderId={orderId}");

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
        if (orderId <= 0)
            return false;

        bool lockTaken = false;

        try
        {
            await paymentStatusCheckLock.WaitAsync(cancellationToken);
            lockTaken = true;

            if (Volatile.Read(ref paymentFinished) == 1)
                return true;

            var response = await apiService.GetOrderPaymentStatus(
                new OrderStatusRequest
                {
                    orderId = orderId,
                    userId = (long)appControl.userDto.id
                });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                Debug.WriteLine(
                    $"PAYMENT STATUS API ERROR => {response.resultMsg}");

                return false;
            }

            string paymentStatus =
                response.resultData?.paymentStatus ?? string.Empty;

            Debug.WriteLine(
                $"PAYMENT STATUS => orderId={orderId}, status={paymentStatus}");

            if (paymentStatus.Equals(
                    "PAID",
                    StringComparison.OrdinalIgnoreCase))
            {
                await FinishPaymentAsync(paymentStatus);
                return true;
            }

            if (paymentStatus.Equals(
                    "FAILED",
                    StringComparison.OrdinalIgnoreCase))
            {
                await FinishPaymentAsync(paymentStatus);
                return true;
            }

            if (paymentStatus.Equals(
                    "CANCELLED",
                    StringComparison.OrdinalIgnoreCase))
            {
                await FinishPaymentAsync(paymentStatus);
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
            Debug.WriteLine(
                $"[ERROR] CheckPaymentStatusOnceAsync => {ex}");

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
            $"PAYMENT FINISHED => orderId={orderId}, status={paymentStatus}");

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await AppNavigatorService.NavigateTo(nameof(MainPage));
        });
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
        paymentStatusCts?.Cancel();

        _ = AppNavigatorService.NavigateTo("..");

        return true;
    }
}