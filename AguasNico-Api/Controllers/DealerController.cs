using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Dealers;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AguasNico_Api.Controllers;

[Authorize(Policy = Policies.Admin)]
public class DealerController(DealerService dealerService) : BaseController
{
    private readonly DealerService _dealerService = dealerService;

    [HttpGet]
    public async Task<BaseResponse<GetDealersResponse>> GetAll() => await _dealerService.GetAll();

    [HttpGet]
    public async Task<BaseResponse<GetDealerResponse>> GetOne([FromQuery] GetDealerRequest rq) => await _dealerService.GetOne(rq);

    [HttpGet]
    public async Task<BaseResponse<GetDealerSheetsResponse>> GetSheets([FromQuery] GetDealerSheetsRequest rq) => await _dealerService.GetSheets(rq);

    [HttpGet]
    public async Task<BaseResponse<GetClientsByDayResponse>> GetClientsByDay([FromQuery] GetClientsByDayRequest rq) => await _dealerService.GetClientsByDay(rq);

    [HttpGet]
    public async Task<BaseResponse<GetClientsNotVisitedResponse>> GetClientsNotVisited([FromQuery] GetClientsNotVisitedRequest rq) => await _dealerService.GetClientsNotVisited(rq);

    [HttpGet]
    public async Task<BaseResponse<GetDealerSoldProductsResponse>> GetSoldProducts([FromQuery] GetDealerSoldProductsRequest rq) => await _dealerService.GetSoldProducts(rq);
}
