namespace Models.Responses;

public class ReferralData
{
    public bool promo_mode { get; set; }
    public int friends_needed { get; set; }
    public string? code { get; set; }
    public int friends_count { get; set; }
    public bool rewarded { get; set; }
}

public class ReferralResponse : Response<ReferralData> { }
