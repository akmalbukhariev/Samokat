namespace Models.Requests;

public class DeletePaymentCardRequest
{ 
    public int card_id { get; set; }
    public int user_id { get; set; }
}