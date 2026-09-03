namespace Models.Requests;

public class AddReviewRequest
{
    public long product_id { get; set; }
    public long order_id { get; set; }
    public int rating { get; set; }
    public string comment { get; set; } = string.Empty;
}
