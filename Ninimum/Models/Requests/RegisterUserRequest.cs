namespace Models.Requests;

public class RegisterUserRequest
{
    public long? region_id { get; set; }

    public string first_name { get; set; }

    public string last_name { get; set; }
    public double location_latitude { get; set; }
    public double location_longitude { get; set; }
    public string address { get; set; }
    public string phone_number { get; set; }

    public string password { get; set; }
}

public class RegisterChildRequest
{
    public string first_name { get; set; }

    public string last_name { get; set; }

    public string birth_date { get; set; }

    public string gender { get; set; }
}