namespace Models.Responses;

public class OrderListResponse : Response<List<OrderListData>>
{
     
}

public class OrderListData
{
    public string? partialReturnStatus { get; set; }
    public string? deliveryMethod { get; set; }
    public string? taxiCompany { get; set; }
    public string? taxiDriverName { get; set; }
    public string? taxiDriverPhone { get; set; }
    public string? taxiCarNumber { get; set; }
    public decimal? taxiFare { get; set; }
    public decimal? partialRefundAmount { get; set; }
    public long orderId { get; set; }
    public long userId { get; set; }
    public string orderNumber { get; set; } = string.Empty;
    public string status { get; set; } = string.Empty;
    public string paymentStatus { get; set; } = string.Empty;
    public decimal totalPrice { get; set; }
    public int productCount { get; set; }
    public string orderedAt { get; set; } = string.Empty;
}