using Ninimum.Models.Dto;

namespace Models.Responses;

public class ReviewEligibilityData
{
    public bool can_review { get; set; }
    public bool has_purchased { get; set; }
    public bool already_reviewed { get; set; }
    public long? order_id { get; set; }
    public ReviewDto? existing_review { get; set; }
}

public class ReviewEligibilityResponse : Response<ReviewEligibilityData>
{
}
