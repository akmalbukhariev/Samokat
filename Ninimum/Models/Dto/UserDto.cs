namespace Models.Dto
{
    public class UserDto
    {
        public long? id { get; set; }
        public long? region_id { get; set; }

        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone_number { get; set; }

        public double? location_latitude { get; set; }
        public double? location_longitude { get; set; }
        public string address { get; set; }

        public string email { get; set; }
        public string password { get; set; }
        public string profile_image_url { get; set; }

        public DateTime? birth_date { get; set; }
        public string gender { get; set; }

        public bool? is_phone_verified { get; set; }
        public bool? is_active { get; set; }

        public long? created_at { get; set; }
        public long? updated_at { get; set; }
        public long? blocked_until { get; set; }

        public string token_mb { get; set; }
        public string status { get; set; }
        
        public DateTime? CreatedAtDate => ConvertUnixTimestamp(created_at);
        public DateTime? UpdatedAtDate => ConvertUnixTimestamp(updated_at);
        public DateTime? BlockedUntilDate => ConvertUnixTimestamp(blocked_until);

        private static DateTime? ConvertUnixTimestamp(long? timestamp)
        {
            if (!timestamp.HasValue)
                return null;

            return DateTimeOffset
                .FromUnixTimeMilliseconds(timestamp.Value)
                .LocalDateTime;
        }
    }
}