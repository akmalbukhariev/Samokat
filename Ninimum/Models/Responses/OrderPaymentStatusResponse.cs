namespace Models.Responses;

public class OrderPaymentStatusResponse : Response<OrderPaymentStatusData>
{
     
}

public class OrderPaymentStatusData
{
    public long orderId { get; set; }
    public string? orderNumber { get; set; }
    public string? paymentStatus { get; set; }
    // Payme's cancel reason when the payment failed (1 receiver, 2 card/bank, 3 processing, 4 timeout, 5 refund).
    public int? paymentReason { get; set; }
}