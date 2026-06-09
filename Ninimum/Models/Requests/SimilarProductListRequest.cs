namespace Models.Requests;

public class SimilarProductListRequest : PageSizeRequest
{
    public int user_id { get; set; }
    public int product_id { get; set; }
}