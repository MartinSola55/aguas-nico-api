namespace AguasNico_Api.Models.DTO.Clients;

public class UpdateClientRequest : CreateClientRequest
{
    public long Id { get; set; }
}

public class UpdateClientResponse
{
    public long Id { get; set; }
}



