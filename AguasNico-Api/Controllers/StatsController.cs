using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Stats;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AguasNico_Api.Controllers;

[Authorize(Policy = Policies.Admin)]
public class StatsController(StatsService statsService) : BaseController
{
    private readonly StatsService _statsService = statsService;

    [HttpGet]
    public async Task<BaseResponse<GetYearsResponse>> GetYears() => await _statsService.GetYears();

    [HttpGet]
    public async Task<BaseResponse<GetAnnualProfitsResponse>> GetAnnualProfits([FromQuery] GetAnnualProfitsRequest rq) => await _statsService.GetAnnualProfits(rq);

    [HttpGet]
    public async Task<BaseResponse<GetMonthlyProfitsResponse>> GetMonthlyProfits([FromQuery] GetMonthlyProfitsRequest rq) => await _statsService.GetMonthlyProfits(rq);

    [HttpGet]
    public async Task<BaseResponse<GetProductsSoldResponse>> GetProductsSold([FromQuery] GetProductsSoldRequest rq) => await _statsService.GetProductsSold(rq);

    [HttpGet]
    public async Task<BaseResponse<GetProductsSoldByDealerResponse>> GetProductsSoldByDealer([FromQuery] GetProductsSoldByDealerRequest rq) => await _statsService.GetProductsSoldByDealer(rq);

    [HttpGet]
    public async Task<BaseResponse<GetBalanceByDateResponse>> GetBalanceByDate([FromQuery] GetBalanceByDateRequest rq) => await _statsService.GetBalanceByDate(rq);
}
