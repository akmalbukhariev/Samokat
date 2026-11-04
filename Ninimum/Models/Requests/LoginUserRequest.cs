namespace Models.Requests;

public class LoginUserRequest
{
    public string phone_number { get; set; }
    public string password { get; set; }

    // SMS sign-in: the typed code, or this phone's key from an earlier confirmed sign-in.
    public string? sms_code { get; set; }
    public string? device_token { get; set; }
}