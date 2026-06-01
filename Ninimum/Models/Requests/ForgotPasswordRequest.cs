namespace Models.Requests;

public class ForgotPasswordParam {
    public string phoneNumber{ get; set; }
    public string tempPassword { get; set; }
}