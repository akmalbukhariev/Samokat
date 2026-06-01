using System;
using System.Collections.Generic;

namespace Ninimum.Models.Dto
{
    public class ProductDto
    {
        public long? id { get; set; }
        public long? category_id { get; set; }

        public string name { get; set; }
        public string short_description { get; set; }
        public string description { get; set; }

        public string sku { get; set; }
        public string barcode { get; set; }
        public string brand { get; set; }

        public double? price { get; set; }
        public double? subscription_price { get; set; }

        public int? stock_quantity { get; set; }
        public double? weight_gram { get; set; }

        public bool? is_active { get; set; }
        public bool? is_featured { get; set; }

        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }

        public List<ProductImageDto> images { get; set; } = new();
    }

    public class ProductImageDto
    {
        public long? id { get; set; }
        public long? product_id { get; set; }

        public string image_url { get; set; }

        public int? sort_order { get; set; }
    }
}