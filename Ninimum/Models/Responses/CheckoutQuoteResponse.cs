namespace Models.Responses;
public class CheckoutQuoteData
{
    public int subtotal {get;set;}
    public int discount {get;set;}
    public int total {get;set;}
    public int delivery_price {get;set;}
    public List<Models.Requests.CreateOrderProductRequest> products {get;set;}=new();
}
