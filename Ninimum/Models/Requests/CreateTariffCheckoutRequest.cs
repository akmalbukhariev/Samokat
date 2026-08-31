namespace Models.Requests;

public class CreateTariffCheckoutRequest
{
    public long userId { get; set; }
    public long tariffId { get; set; }
}
