namespace Models.Requests;

public class CreateOrderRequest
{
    public long? orderId { get; set; }

    public long userId { get; set; }

    public long addressId { get; set; }

    public int totalPrice { get; set; }

    public List<CreateOrderProductRequest> products { get; set; } = new();
}

public class CreateOrderProductRequest
{
    public long? orderId { get; set; }

    public long productId { get; set; }

    public int price { get; set; }

    public int quantity { get; set; }
}