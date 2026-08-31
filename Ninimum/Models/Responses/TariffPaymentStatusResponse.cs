namespace Models.Responses;

public class TariffPaymentStatusResponse : Response<TariffPaymentStatusData>
{
}

public class TariffPaymentStatusData
{
    public long subscriptionId { get; set; }
    public string paymentStatus { get; set; } = string.Empty;
    public string subscriptionStatus { get; set; } = string.Empty;
}
