namespace AguasNico_Api.Models.DTO.Clients;

public class UpdateClientAbonosRequest
{
    public long ClientId { get; set; }
    public List<long> AbonoIds { get; set; } = [];
}



