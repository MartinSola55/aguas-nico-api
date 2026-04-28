using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Abonos;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AguasNico_Api.Controllers;

[Authorize(Policy = Policies.Admin)]
public class AbonoController(AbonoService abonoService) : BaseController
{
    private readonly AbonoService _abonoService = abonoService;

    [HttpGet]
    public async Task<BaseResponse<GetAllAbonosResponse>> GetAll() => await _abonoService.GetAll();

    [HttpPost]
    public async Task<BaseResponse<CreateAbonoResponse>> Create([FromBody] CreateAbonoRequest rq) => await _abonoService.Create(rq);

    [HttpPost]
    public async Task<BaseResponse<UpdateAbonoResponse>> Update([FromBody] UpdateAbonoRequest rq) => await _abonoService.Update(rq);

    [HttpPost]
    public async Task<BaseResponse> Delete([FromBody] DeleteAbonoRequest rq) => await _abonoService.Delete(rq);

    [HttpPost]
    public async Task<BaseResponse> RenewAll() => await _abonoService.RenewAll();

    [HttpPost]
    public async Task<BaseResponse> RenewByRoute([FromBody] RenewByRouteRequest rq) => await _abonoService.RenewByRoute(rq);

    [HttpGet]
    public async Task<BaseResponse<GetAbonoClientsResponse>> GetClients([FromQuery] GetAbonoClientsRequest rq) => await _abonoService.GetClients(rq);
}
