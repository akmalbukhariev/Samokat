namespace Models.Dto
{
    public class UserDto
    {
        public long? id { get; set; }

        public long? region_id { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string phone_number { get; set; }

        public string email { get; set; }

        public string password { get; set; }

        public string profile_image_url { get; set; }

        public DateTime? birth_date { get; set; }

        public string gender { get; set; }

        public bool? is_phone_verified { get; set; }

        public bool? is_active { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }
    }
}