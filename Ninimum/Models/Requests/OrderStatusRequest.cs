namespace Models.Requests;

public class OrderStatusRequest
{
    public long orderId { get; set; }
    public long userId { get; set; }
}