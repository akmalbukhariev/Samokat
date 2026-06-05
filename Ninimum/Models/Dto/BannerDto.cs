namespace Models.Dto
{
    public class BannerDto
    {
        public long? id { get; set; }
        public long? product_id { get; set; }
        public int? sort_order { get; set; }
        public bool? is_active { get; set; }

        public string name { get; set; }
        public string short_description { get; set; }
        public string description { get; set; }
        public string brand { get; set; }

        public double? price { get; set; }
        public double? subscription_price { get; set; }

        public string image_url { get; set; }
    }
}