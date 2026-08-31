namespace Models.Requests;

public class ChangePasswordRequest
{
    public long userId { get; set; }
    public string currentPassword { get; set; } = string.Empty;
    public string newPassword { get; set; } = string.Empty;
}
