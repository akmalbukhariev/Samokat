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
}
