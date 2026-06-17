namespace Ninimum.Models.Dto;

public class PaymentCardDto
{
    public long id { get; set; }
    public long user_id { get; set; }

    public string card_holder_name { get; set; }
    public string card_brand { get; set; }
    public string last_four_digits { get; set; }

    public int expiry_month { get; set; }
    public int expiry_year { get; set; }

    public string payment_token { get; set; }

    public bool is_default { get; set; }

    public string created_at { get; set; }
    public string updated_at { get; set; }
}