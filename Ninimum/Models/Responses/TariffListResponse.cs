namespace Models.Responses;

public class TariffListResponse : Response<List<TariffData>>
{
}

public class TariffData
{
    public long tariffId { get; set; }
    public string tariffName { get; set; } = string.Empty;
    public int price { get; set; }
    public int durationMonth { get; set; }
    public string? description { get; set; }
    public bool useProductRules {get;set;}
    public bool freeDelivery {get;set;}
    public int? recurringOrderDays {get;set;}
    public List<TariffProductBenefit> productBenefits {get;set;}=new();
}

public class TariffProductBenefit {public string product_name {get;set;}="";public int discount_percent {get;set;}}
