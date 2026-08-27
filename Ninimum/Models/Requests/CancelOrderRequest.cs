namespace Models.Requests;

public class CancelOrderRequest
{
    public long orderId { get; set; }
    public long userId { get; set; }
    public string reason { get; set; } = string.Empty;
}
