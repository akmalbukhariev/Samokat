namespace Models.Requests;

public class LoginUserRequest
{
    public string phone_number { get; set; }
    public string password { get; set; }
}