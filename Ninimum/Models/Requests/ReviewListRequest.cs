namespace Models.Requests;

public class ReviewListRequest : PageSizeRequest {
    public int product_id { get; set; }
}