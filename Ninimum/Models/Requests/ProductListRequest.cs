namespace Models.Requests;

public class ProductListRequest : PageSizeRequest
{
    public int user_id{ get; set; }
    public int category_id { get; set; }
}