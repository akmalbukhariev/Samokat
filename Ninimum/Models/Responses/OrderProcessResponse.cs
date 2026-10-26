namespace Models.Responses;

public class OrderProcessResponse : Response<OrderProcessData>
{
    
}

public class OrderProcessData
{
    public string? partialReturnStatus { get; set; }
    public string? deliveryMethod { get; set; }
    public string? taxiCompany { get; set; }
    public string? taxiDriverName { get; set; }
    public string? taxiDriverPhone { get; set; }
    public string? taxiCarNumber { get; set; }
    public decimal? taxiFare { get; set; }
    public long orderId { get; set; }
    public string? orderNumber { get; set; }
    public string? status { get; set; }
    public string? orderedAt { get; set; }
    public string? deliveredAt { get; set; }
}