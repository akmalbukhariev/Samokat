namespace Models.Requests;

public class TariffPaymentStatusRequest
{
    public long userId { get; set; }
    public long subscriptionId { get; set; }
}
