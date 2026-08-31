namespace Models.Responses;

public class CreateTariffCheckoutResponse : Response<CreateTariffCheckoutData>
{
}

public class CreateTariffCheckoutData
{
    public long subscriptionId { get; set; }
    public string paymentUrl { get; set; } = string.Empty;
}
