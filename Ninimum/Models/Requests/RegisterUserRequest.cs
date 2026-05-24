namespace Models.Requests;

public class RegisterUserRequest
{
    public long? region_id { get; set; }

    public string first_name { get; set; }

    public string last_name { get; set; }

    public string phone_number { get; set; }

    public string password { get; set; }

    //public List<RegisterChildRequest> children { get; set; } = new();
}

public class RegisterChildRequest
{
    public string first_name { get; set; }

    public string last_name { get; set; }

    public string birth_date { get; set; }

    public string gender { get; set; }
}