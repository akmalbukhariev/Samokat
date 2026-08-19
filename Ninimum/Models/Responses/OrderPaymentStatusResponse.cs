namespace Models.Responses;

public class OrderPaymentStatusResponse : Response<OrderPaymentStatusData>
{
     
}

public class OrderPaymentStatusData
{
    public long orderId { get; set; }
    public string? orderNumber { get; set; }
    public string? paymentStatus { get; set; }
}