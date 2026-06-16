using AguasNico_Api.Models.Constants;
namespace AguasNico_Api.Models.DTO.Routes;

public class GetRoutesRequest
{
    public int Day { get; set; }
    public string UserId { get; set; }
}

public class GetRoutesResponse
{
    public List<RouteItem> Routes { get; set; } = [];
}



