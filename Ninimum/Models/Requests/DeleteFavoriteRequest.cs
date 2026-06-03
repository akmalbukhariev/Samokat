namespace Models.Requests;

public class DeleteFavoriteRequest
{
    public int user_id { get; set; }
    public int product_id { get; set; }
}