namespace AguasNico_Api.Models.DTO.Auth;

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; }
    public DateTime SessionExpiration { get; set; }
    public UserItem User { get; set; }

    public class UserItem
    {
        public string Id { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int TruckNumber { get; set; }
    }
}


