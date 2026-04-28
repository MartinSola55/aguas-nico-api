using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Routes;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AguasNico_Api.Controllers;

public class RouteController(RouteService routeService) : BaseController
{
    private readonly RouteService _routeService = routeService;

    [HttpGet]
    public async Task<BaseResponse<GetRoutesResponse>> GetAll([FromQuery] GetRoutesRequest rq) => await _routeService.GetAll(rq);

    [HttpGet]
    public async Task<BaseResponse<GetRouteResponse>> GetOne([FromQuery] GetRouteRequest rq) => await _routeService.GetOne(rq);

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse<CreateRouteResponse>> Create([FromBody] CreateRouteRequest rq) => await _routeService.Create(rq);

    [HttpPost]
    public async Task<BaseResponse<CreateRouteByDealerResponse>> CreateByDealer([FromBody] CreateRouteByDealerRequest rq) => await _routeService.CreateByDealer(rq);

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse> UpdateClients([FromBody] UpdateRouteClientsRequest rq) => await _routeService.UpdateClients(rq);

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse> Delete([FromBody] DeleteRouteRequest rq) => await _routeService.Delete(rq);

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse> Close([FromBody] CloseRouteRequest rq) => await _routeService.Close(rq);

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse<GetRoutesResponse>> SearchByDate([FromQuery] SearchRoutesByDateRequest rq) => await _routeService.SearchByDate(rq);

    [HttpGet]
    public async Task<BaseResponse<GetRoutesResponse>> SearchByDay([FromQuery] SearchRoutesByDayRequest rq) => await _routeService.SearchByDay(rq);

    [HttpGet]
    public async Task<BaseResponse<SearchSoldProductsResponse>> SearchSoldProducts([FromQuery] SearchSoldProductsRequest rq) => await _routeService.SearchSoldProducts(rq);

    [HttpGet]
    public async Task<BaseResponse<ClientsNotInRouteResponse>> ClientsByIDNotInRoute([FromQuery] ClientsByIdNotInRouteRequest rq) => await _routeService.ClientsByIDNotInRoute(rq);

    [HttpGet]
    public async Task<BaseResponse<ClientsNotInRouteResponse>> ClientsByNameNotInRoute([FromQuery] ClientsByNameNotInRouteRequest rq) => await _routeService.ClientsByNameNotInRoute(rq);

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse<GetDispatchedResponse>> GetDispatched([FromQuery] GetDispatchedRequest rq) => await _routeService.GetDispatched(rq);

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse> UpdateDispatched([FromBody] UpdateDispatchedRequest rq) => await _routeService.UpdateDispatched(rq);

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse> SetDispenserPrice([FromBody] SetDispenserPriceRequest rq) => await _routeService.SetDispenserPrice(rq);

    [HttpGet]
    public async Task<BaseResponse<ManualCartDataResponse>> GetManualCartData([FromQuery] ManualCartDataRequest rq) => await _routeService.GetManualCartData(rq);
}
