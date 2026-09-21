namespace NinimumDelivery.Models;
public class DeliveryWorker
{
    public long id { get; set; }
    public string workerId { get; set; } = string.Empty;
    public string fullName { get; set; } = string.Empty;
    public string phoneNumber { get; set; } = string.Empty;
    public string status { get; set; } = string.Empty;
    public bool online { get; set; }
    public string? vehicleType { get; set; }
    public string? vehicleNumber { get; set; }
}
