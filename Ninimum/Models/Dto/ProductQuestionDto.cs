namespace Ninimum.Models.Dto;

public class ProductQuestionDto
{
    public long? id { get; set; }
    public long? product_id { get; set; }
    public long? user_id { get; set; }
    public string? customer_name { get; set; }
    public string? question { get; set; }
    public string? answer { get; set; }
    public string? status { get; set; }
    public bool? is_active { get; set; }
    public DateTime? created_at { get; set; }
    public DateTime? answered_at { get; set; }
    public DateTime? updated_at { get; set; }
}
