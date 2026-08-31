namespace Models.Requests;

public class ChangePhoneNumberRequest
{
    public long userId { get; set; }
    public string phoneNumber { get; set; } = string.Empty;
}
