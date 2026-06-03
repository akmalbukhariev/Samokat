namespace Models.Requests;

public class SearchProductParam : PageSizeRequest
{
    public string keyword { get; set; }
    public double? minPrice { get; set; }
    public double? maxPrice { get; set; }
    public string sortType { get; set; } // cheap, expensive, newest, oldest
}