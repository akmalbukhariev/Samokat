namespace Models.Requests;

public class DeleteCartRequest
{
    public int cart_id { get; set; }
    public int user_id { get; set; }
}