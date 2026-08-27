namespace Ninimum.Models.Dto;

public class SubscriptionDto
{
    public long subscriptionId { get; set; }
    public long userId { get; set; }
    public long tariffId { get; set; }
    public string tariffName { get; set; } = string.Empty;
    public int price { get; set; }
    public string startDate { get; set; } = string.Empty;
    public string endDate { get; set; } = string.Empty;
    public string subscriptionStatus { get; set; } = string.Empty;
}
