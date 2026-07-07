using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO.Routes;

namespace AguasNico_Api.Models.DTO.Home;

public class GetDashboardRequest
{
    public Day Day { get; set; }
}

public class GetDashboardResponse
{
    public List<RouteItem> DealerRoutes { get; set; } = [];
}
