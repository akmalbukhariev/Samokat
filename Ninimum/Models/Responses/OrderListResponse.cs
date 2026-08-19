namespace Models.Responses;

public class OrderListResponse : Response<List<OrderListData>>
{
     
}

public class OrderListData
{
    public long orderId { get; set; }
    public long userId { get; set; }
    public string orderNumber { get; set; } = string.Empty;
    public string status { get; set; } = string.Empty;
    public string paymentStatus { get; set; } = string.Empty;
    public decimal totalPrice { get; set; }
    public int productCount { get; set; }
    public string orderedAt { get; set; } = string.Empty;
}