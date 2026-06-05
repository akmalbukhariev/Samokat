namespace Models.Requests;

public class FavoriteListRequest : PageSizeRequest
{
    public int user_id{ get; set; }
}