namespace Models.Responses;

public class OrderDetailResponse : Response
{
    public List<OrderDetailData> resultData { get; set; } = new();
}

public class OrderDetailData
{
    public long orderItemId { get; set; }
    public long orderId { get; set; }
    public long productId { get; set; }
    public string productName { get; set; } = string.Empty;
    public string productImageUrl { get; set; } = string.Empty;
    public decimal unitPrice { get; set; }
    public int quantity { get; set; }
    public decimal totalPrice { get; set; }
}