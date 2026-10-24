namespace NinimumDelivery.Models;
public class ApiResponse<T>
{
    private string? message;
    public string? resultCode { get; set; }
    // Shown to the courier as is, so always translated text (see ServerMessage).
    public string? resultMsg { get => NinimumDelivery.Services.ServerMessage.Show(resultCode, message); set => message = value; }
    public T? resultData { get; set; }
}
public class ApiResponse
{
    private string? message;
    public string? resultCode { get; set; }
    // Shown to the courier as is, so always translated text (see ServerMessage).
    public string? resultMsg { get => NinimumDelivery.Services.ServerMessage.Show(resultCode, message); set => message = value; }
}
