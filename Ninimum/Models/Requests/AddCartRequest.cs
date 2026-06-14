namespace Models.Requests;

public class AddCartRequest
{
    public int user_id { get; set; }
    public int product_id { get; set; }
    public int quantity { get; set; }
}