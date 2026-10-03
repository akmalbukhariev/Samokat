namespace Models.Responses;

public class CreateOrderResponse : Response
{
    public int? availableQuantity { get; set; }
    public long? unavailableProductId { get; set; }
    public long? resultData { get; set; }
}