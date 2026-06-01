namespace Models.Requests;

public class ProductListRequest
{
    public int categoryId{ get; set; }
    public int pageSize{ get; set; }
    public int offset{ get; set; }
}