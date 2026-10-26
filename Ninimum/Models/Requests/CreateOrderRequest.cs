namespace Models.Requests;

public class CreateOrderRequest
{
    public long? orderId { get; set; }

    public long userId { get; set; }

    public long? addressId { get; set; }

    public string deliveryAddress { get; set; } = string.Empty;
    public double? deliveryLatitude { get; set; }
    public double? deliveryLongitude { get; set; }

    public int totalPrice { get; set; }
    public int? quotedTotal {get;set;}

    public long? tariffSubscriptionId { get; set; }
    // "COURIER" (default) or "TAXI" (fast delivery; the customer pays the taxi driver).
    public string deliveryMethod { get; set; } = "COURIER";

    public List<CreateOrderProductRequest> products { get; set; } = new();
}

public class CreateOrderProductRequest
{
    public long? orderId { get; set; }

    public long productId { get; set; }

    public int price { get; set; }

    public int quantity { get; set; }
}