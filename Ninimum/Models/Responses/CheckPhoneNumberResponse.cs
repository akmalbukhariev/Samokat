using Models.Dto;

namespace Models.Responses;

public class CheckPhoneNumberResponse : Response<CheckPhoneNumberResult>
{
    
}

public class CheckPhoneNumberResult
{ 
    public string existsYn { get; set; } // Y or N
}