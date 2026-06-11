
namespace Ninimum.Models.Dto
{

    public class ReviewDto
    {
        public long? id { get; set; }
        public long? user_id { get; set; }
        public long? product_id { get; set; }
        public long? order_id { get; set; }

        public int? rating { get; set; }
        public string? comment { get; set; }

        public bool? is_active { get; set; }

        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }

        public List<ReviewImageDto> images { get; set; } = new();
    }

    public class ReviewImageDto
    {
        public long? id { get; set; }
        public long? review_id { get; set; }

        public string? image_url { get; set; }

        public DateTime? created_at { get; set; }
    }
}