namespace Models.Requests;

public class AddFavoriteRequest
{
    public int user_id { get; set; }
    public int product_id { get; set; }
}