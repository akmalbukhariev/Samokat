namespace Models.Responses;

public class DeviceTokenData
{
    public string? device_token { get; set; }
}

// Registration returns this phone's key, so the sign-in right after it needs no second SMS.
public class RegisterUserResponse : Response<DeviceTokenData> { }
