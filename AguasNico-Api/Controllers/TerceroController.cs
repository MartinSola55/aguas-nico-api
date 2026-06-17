using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Terceros;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AguasNico_Api.Controllers;

[Authorize(Policy = Policies.Admin)]
public class TerceroController(TerceroService terceroService) : BaseController
{
    private readonly TerceroService _terceroService = terceroService;

    [HttpGet]
    public async Task<BaseResponse<GetTercerosResponse>> GetByDate([FromQuery] GetTercerosRequest rq) => await _terceroService.GetByDate(rq);

    [HttpPost]
    public async Task<BaseResponse<CreateTerceroResponse>> Create([FromBody] CreateTerceroRequest rq) => await _terceroService.Create(rq);

    [HttpPost]
    public async Task<BaseResponse<UpdateTerceroResponse>> Update([FromBody] UpdateTerceroRequest rq) => await _terceroService.Update(rq);

    [HttpPost]
    public async Task<BaseResponse> Delete([FromBody] DeleteTerceroRequest rq) => await _terceroService.Delete(rq);
}
