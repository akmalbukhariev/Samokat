namespace Models.Requests;

public class UpdateReviewRequest
{
    public long id { get; set; }
    public int rating { get; set; }
    public string comment { get; set; } = string.Empty;
    public List<long> keep_image_ids { get; set; } = new();
}
