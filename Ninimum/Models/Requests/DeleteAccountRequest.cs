namespace Models.Requests;

public class DeleteAccountRequest
{
    public long userId { get; set; }
    public string? reasons { get; set; }
}
