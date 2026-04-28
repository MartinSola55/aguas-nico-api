namespace AguasNico_Api.Models.DTO.Routes;

public class UpdateRouteClientsRequest
{
    public long RouteId { get; set; }
    public List<long> ClientIds { get; set; } = [];
}



