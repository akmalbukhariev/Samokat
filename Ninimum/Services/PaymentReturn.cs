namespace Ninimum.Services;

/// <summary>
/// Raised when the customer comes back from the Payme app through the return link (ninimum://payment-result),
/// so the open payment page re-checks the payment status at once.
/// </summary>
public static class PaymentReturn
{
    public static event Action? Returned;
    public static void Notify() => MainThread.BeginInvokeOnMainThread(() => Returned?.Invoke());
}
