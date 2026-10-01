namespace Models.Requests;

public class ProductListRequest : PageSizeRequest
{
    public int user_id{ get; set; }
    public int category_id { get; set; }
    public bool include_subcategories { get; set; }
}