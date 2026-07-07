namespace AguasNico_Api.Models.DTO.Users;

public class GetProfileRequest
{
    public string Id { get; set; }
}

public class GetProfileResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public int TruckNumber { get; set; }
}
