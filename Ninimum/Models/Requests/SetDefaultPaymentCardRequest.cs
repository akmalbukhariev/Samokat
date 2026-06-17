namespace Models.Requests;

public class SetDefaultPaymentCardRequest
{ 
    public int card_id { get; set; }
    public int user_id { get; set; }
}