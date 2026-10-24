using Ninimum.Resources.Languages;
using Api.Services;
using Models.Requests;
using Models.Responses;
using Ninimum.Services;
using Ninimum.Views.Main;
using Ninimum.Views.Formalization;
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
        // PaymentPage is a transient checkout page. Always start a new query
        // with clean state so an old completed Payme session can never be reused.
        paymentStatusCts?.Cancel();
        paymentReturnHandled = false;
        Interlocked.Exchange(ref paymentFinished, 0);
        Interlocked.Exchange(ref backFlowRunning, 0);

        orderId = 0;
        subscriptionId = 0;
        paymentType = "ORDER";
        paymentUrl = string.Empty;

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
                await FinishPaymentAsync(orderPaymentStatus, orderResponse.resultData?.paymentReason);
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

        // "Pay in the Payme app" links (intent://, payme://, Play Store) can't open inside the WebView.
        if (IsExternalPaymentLink(e.Url))
        {
            e.Cancel = true;
            _ = OpenExternalPaymentLinkAsync(e.Url);
            return;
        }

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

    private static bool IsExternalPaymentLink(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url.StartsWith("intent:", StringComparison.OrdinalIgnoreCase);
        var scheme = uri.Scheme.ToLowerInvariant();
        if (scheme is "http" or "https")
            return uri.Host.Equals("play.google.com", StringComparison.OrdinalIgnoreCase);
        return scheme is not ("about" or "data" or "javascript" or "blob" or "file" or "ninimum");
    }

    /// <summary>Opens the installed Payme app; without it the customer simply keeps paying on this page.</summary>
    private async Task OpenExternalPaymentLinkAsync(string url)
    {
        ShowLoading(false);
        try
        {
#if ANDROID
            if (url.StartsWith("intent:", StringComparison.OrdinalIgnoreCase))
            {
                var intent = Android.Content.Intent.ParseUri(url, Android.Content.IntentUriType.Scheme);
                // Never let a web page start an arbitrary component of another app.
                intent.AddCategory(Android.Content.Intent.CategoryBrowsable);
                intent.SetComponent(null);
                intent.Selector = null;
                intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                try
                {
                    Platform.CurrentActivity!.StartActivity(intent);
                    return;
                }
                catch (Android.Content.ActivityNotFoundException)
                {
                    var fallback = intent.GetStringExtra("browser_fallback_url");
                    if (!string.IsNullOrWhiteSpace(fallback) && fallback.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                        && !fallback.Contains("play.google.com", StringComparison.OrdinalIgnoreCase))
                    {
                        PaymeWebView.Source = fallback;
                        return;
                    }
                    await ShowPaymeAppMissingAsync();
                    return;
                }
            }
#endif
            await Launcher.OpenAsync(new Uri(url));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PAYME APP] could not open {url} => {ex.Message}");
            await ShowPaymeAppMissingAsync();
        }
    }

    private Task ShowPaymeAppMissingAsync() => MainThread.InvokeOnMainThreadAsync(() => DisplayAlert(
        AppResource.Error,
        AppResource.ResourceManager.GetString("PaymeAppNotFound", AppResource.Culture) ?? "Payme ilovasi topilmadi. To‘lovni shu sahifada davom ettiring.",
        AppResource.Ok));

    /// <summary>Back from the Payme app (return link or the app switcher): check the payment at once and keep watching.</summary>
    private void OnReturnedToApp()
    {
        if (Volatile.Read(ref paymentFinished) == 1 || string.IsNullOrWhiteSpace(paymentUrl))
            return;
        paymentReturnHandled = false;
        _ = CheckPaymentResultAsync();
        if (paymentStatusCts == null || paymentStatusCts.IsCancellationRequested)
            StartPaymentStatusChecking();
    }

    private void OnWindowResumed(object? sender, EventArgs e) => OnReturnedToApp();

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

            // Payme may have shown its own error (card declined, bank not answering). Say plainly what to do.
            if (Volatile.Read(ref paymentFinished) == 0)
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    PendingHintLabel.Text = Text("PaymentPendingHint",
                        "To‘lov hali tasdiqlanmadi. Agar Payme’da xatolik chiqqan bo‘lsa (masalan, karta ishlamasa yoki bank javob bermasa), shu sahifada boshqa karta bilan qayta urinib ko‘ring. Pul yechilgan bo‘lsa, tasdiq bir necha daqiqada keladi.");
                    PendingHint.IsVisible = true;
                });

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

    private static string Text(string key, string uz) =>
        AppResource.ResourceManager.GetString(key, AppResource.Culture) ?? uz;

    /// <summary>Why a payment failed, in words the customer understands (Payme's cancel reason).</summary>
    private static string FailureMessage(string status, int? reason)
    {
        if (status.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase) && reason == null)
            return Text("PaymentCancelledUnpaid", "Buyurtma to‘lanmagani uchun bekor qilindi. Pul yechilmadi. Yangi buyurtma berishingiz mumkin.");
        return reason switch
        {
            1 => Text("PaymentFailedReason1", "To‘lov qabul qilinmadi: do‘kon hisobida muammo. Pul yechilmadi. Birozdan keyin qayta urinib ko‘ring."),
            2 => Text("PaymentFailedReason2", "Karta yoki bank (HUMO, UZCARD va boshqalar) to‘lovni o‘tkazmadi. Kartadagi mablag‘ni va karta holatini tekshiring yoki boshqa karta bilan qayta urinib ko‘ring."),
            3 => Text("PaymentFailedReason3", "To‘lov tizimida xatolik yuz berdi. Pul yechilgan bo‘lsa, u avtomatik qaytariladi. Qayta urinib ko‘ring."),
            4 => Text("PaymentFailedReason4", "To‘lov o‘z vaqtida tasdiqlanmadi va bekor qilindi. Pul yechilmadi. Qayta urinib ko‘ring."),
            5 => Text("PaymentFailedReason5", "To‘lov bekor qilindi va pul kartangizga qaytarildi."),
            _ => Text("PaymentFailedUnknown", "To‘lov amalga oshmadi. Pul yechilmadi. Qayta urinib ko‘ring yoki boshqa karta tanlang."),
        };
    }

    private async Task FinishPaymentAsync(string paymentStatus, int? paymentReason = null, bool explain = true)
    {
        if (Interlocked.Exchange(ref paymentFinished, 1) == 1)
            return;

        // A failed payment must never end silently: tell the customer what happened before leaving the page.
        if (explain && !paymentStatus.Equals("PAID", StringComparison.OrdinalIgnoreCase))
        {
            var message = FailureMessage(paymentStatus, paymentReason);
            await MainThread.InvokeOnMainThreadAsync(() =>
                DisplayAlert(Text("PaymentFailedTitle", "To‘lov amalga oshmadi"), message, AppResource.Ok));
        }

        paymentStatusCts?.Cancel();

        Debug.WriteLine(
            $"PAYMENT FINISHED => type={paymentType}, orderId={orderId}, subscriptionId={subscriptionId}, status={paymentStatus}");

        // The checkout changed both Orders and Cart state. Force those tabs to
        // refresh the next time the user opens them.
        PageDataRefreshState.MarkDirty(PageDataRefreshState.Orders);
        PageDataRefreshState.MarkDirty(PageDataRefreshState.Cart);

        if (paymentType == "ORDER" && paymentStatus.Equals("PAID", StringComparison.OrdinalIgnoreCase))
        {
            // A successful order changes product stock. Refresh product surfaces
            // so sold-out products disappear and remaining quantities are current.
            PageDataRefreshState.MarkDirty(PageDataRefreshState.Main);
            PageDataRefreshState.MarkDirty(PageDataRefreshState.Favorites);
        }
        FormalizationNavigationStore.Clear();

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            // IMPORTANT: Shell tabs keep their own navigation stacks. Merely
            // switching to Home leaves PaymentPage inside the Cart tab stack,
            // which is why opening Savatcha later showed the old Payme page.
            // Pop the current checkout stack back to its tab root first.
            PaymeWebView.Source = null;

            try
            {
                await Shell.Current.Navigation.PopToRootAsync(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WARN] Could not pop checkout stack to root => {ex.Message}");
            }

            await AppNavigatorService.NavigateHome(false);

            if (paymentType == "TARIFF")
            {
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
            {
                // Safety net for an already-completed PaymentPage that somehow
                // remained in a Shell tab stack from an older app session.
                await CloseFinishedPaymentPageAsync();
                return;
            }

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
                await FinishPaymentAsync(currentStatus);
                return;
            }

            bool shouldLeave = await Shell.Current.DisplayAlert(
                AppResource.PaymentNotCompleted,
                System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
                {
                    "uz" => "To‘lov oynasidan chiqasizmi? To‘lanmagan buyurtmani bekor qilishga urinamiz. Payme to‘lovni qayta ishlayotgan bo‘lsa, buyurtma holatini tarixdan tekshiring.",
                    "ru" => "Выйти из окна оплаты? Мы попробуем отменить неоплаченный заказ. Если Payme обрабатывает платёж, проверьте статус заказа в истории.",
                    _ => "Leave the payment page? We will try to cancel the unpaid order. If Payme is processing payment, check the order status in history."
                },
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
            }).WaitAsync(TimeSpan.FromSeconds(15));

            if (cancelResponse.resultCode == ApiResult.SUCCESS.GetCodeToString())
            {
                await FinishPaymentAsync("CANCELLED", explain: false);
                return;
            }

            // The payment callback may have completed while the user was confirming.
            currentStatus = await GetCurrentOrderPaymentStatusAsync();

            if (currentStatus.Equals("PAID", StringComparison.OrdinalIgnoreCase) ||
                currentStatus.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                await FinishPaymentAsync(currentStatus, explain: false);
                return;
            }

            await LeaveUnconfirmedPaymentAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] HandleBackAsync => {ex}");
            await LeaveUnconfirmedPaymentAsync();
        }
        finally
        {
            Interlocked.Exchange(ref backFlowRunning, 0);
        }
    }

    private async Task LeaveUnconfirmedPaymentAsync()
    {
        var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var message = language switch
        {
            "uz" => "To‘lov yoki bekor qilish hali tasdiqlanmadi. To‘lov oynasi yopiladi. Buyurtma tarixda saqlanadi; qayta to‘lashdan oldin uning holatini tekshiring.",
            "ru" => "Оплата или отмена ещё не подтверждена. Окно оплаты будет закрыто. Заказ сохранён в истории; проверьте его статус перед повторной оплатой.",
            _ => "Payment or cancellation is not confirmed yet. The payment page will close. Your order remains in history; check its status before paying again."
        };
        await AlertService.ShowAlertAsync(AppResource.PaymentNotCompleted, message);
        // Close the UI without reporting a cancellation that the server has not confirmed.
        Interlocked.Exchange(ref paymentFinished, 1);
        await CloseFinishedPaymentPageAsync();
    }

    private async Task CloseFinishedPaymentPageAsync()
    {
        paymentStatusCts?.Cancel();
        FormalizationNavigationStore.Clear();
        PageDataRefreshState.MarkDirty(PageDataRefreshState.Cart);
        PageDataRefreshState.MarkDirty(PageDataRefreshState.Orders);

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            PaymeWebView.Source = null;

            try
            {
                await Shell.Current.Navigation.PopToRootAsync(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WARN] Could not close finished PaymentPage => {ex.Message}");
            }

            await AppNavigatorService.NavigateHome(false);
        });
    }

    private async Task<string> GetCurrentOrderPaymentStatusAsync()
    {
        var response = await apiService.GetOrderPaymentStatus(new OrderStatusRequest
        {
            orderId = orderId,
            userId = appControl.CurrentUserId
        }).WaitAsync(TimeSpan.FromSeconds(15));

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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        PaymentReturn.Returned -= OnReturnedToApp;
        PaymentReturn.Returned += OnReturnedToApp;
        if (Window != null)
        {
            Window.Resumed -= OnWindowResumed;
            Window.Resumed += OnWindowResumed;
        }

        // A finished checkout must never become visible again when the user
        // taps the Cart tab. This also repairs stacks created by older builds.
        if (Volatile.Read(ref paymentFinished) == 1)
            _ = CloseFinishedPaymentPageAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        PaymentReturn.Returned -= OnReturnedToApp;
        if (Window != null)
            Window.Resumed -= OnWindowResumed;
        paymentStatusCts?.Cancel();
    }

    protected override bool OnBackButtonPressed()
    {
        _ = HandleBackAsync();
        return true;
    }
}
