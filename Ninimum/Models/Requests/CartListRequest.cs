namespace Models.Requests;

public class CartListRequest : PageSizeRequest
{
    public int user_id { get; set; }
}