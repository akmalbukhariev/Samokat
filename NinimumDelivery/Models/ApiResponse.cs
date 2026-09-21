namespace NinimumDelivery.Models;
public class ApiResponse<T>
{
    public string? resultCode { get; set; }
    public string? resultMsg { get; set; }
    public T? resultData { get; set; }
}
public class ApiResponse
{
    public string? resultCode { get; set; }
    public string? resultMsg { get; set; }
}
