using AguasNico_Api.Models.Constants;
namespace AguasNico_Api.Models.DTO.Routes;

public class CreateRouteRequest
{
    public string UserId { get; set; }
    public Day DayOfWeek { get; set; }
}

public class CreateRouteResponse
{
    public long Id { get; set; }
}



