using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Home;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AguasNico_Api.Controllers;

public class HomeController(HomeService homeService) : BaseController
{
    private readonly HomeService _homeService = homeService;

    [HttpGet]
    public async Task<BaseResponse<GetDashboardResponse>> GetDashboard([FromQuery] GetDashboardRequest rq) => await _homeService.GetDashboard(rq);
}
