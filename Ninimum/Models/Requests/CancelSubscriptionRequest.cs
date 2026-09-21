namespace Models.Requests;

public class CancelSubscriptionRequest
{
    public long userId { get; set; }
    public long subscriptionId { get; set; }
}
