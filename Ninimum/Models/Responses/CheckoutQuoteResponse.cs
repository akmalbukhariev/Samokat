namespace Models.Responses;
public class CheckoutQuoteData
{
    public int subtotal {get;set;}
    public int discount {get;set;}
    public int total {get;set;}
    public int delivery_price {get;set;}
    // Taxi delivery is offered only inside working hours (server decides).
    public bool taxi_available {get;set;}
    public List<Models.Requests.CreateOrderProductRequest> products {get;set;}=new();
}
