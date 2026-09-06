namespace Models.Requests;

public class ProductQuestionListRequest : PageSizeRequest
{
    public long product_id { get; set; }
}
