namespace Models.Requests;

public class CreatePaymentCardRequest
{
    public long user_id { get; set; }

    public string card_number { get; set; }

    public string card_brand { get; set; }
    public string card_hash { get; set; }
    public string last_four_digits { get; set; }
    public string payment_token { get; set; }

    public string card_holder_name { get; set; }
    public int expiry_month { get; set; }
    public int expiry_year { get; set; }

    public string cvv { get; set; }

    public bool is_default { get; set; }
}