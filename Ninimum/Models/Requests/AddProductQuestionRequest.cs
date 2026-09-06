namespace Models.Requests;

public class AddProductQuestionRequest
{
    public long product_id { get; set; }
    public string question { get; set; } = string.Empty;
}
