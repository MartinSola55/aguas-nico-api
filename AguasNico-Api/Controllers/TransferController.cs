using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Transfers;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AguasNico_Api.Controllers;

[Authorize(Policy = Policies.Admin)]
public class TransferController(TransferService transferService) : BaseController
{
    private readonly TransferService _transferService = transferService;

    [HttpPost]
    public async Task<BaseResponse<GetTransfersResponse>> GetAll([FromBody] GetTransfersRequest rq) => await _transferService.GetAll(rq);

    [HttpPost]
    public async Task<BaseResponse<CreateTransferResponse>> Create([FromBody] CreateTransferRequest rq) => await _transferService.Create(rq);

    [HttpPost]
    public async Task<BaseResponse<UpdateTransferResponse>> Update([FromBody] UpdateTransferRequest rq) => await _transferService.Update(rq);

    [HttpPost]
    public async Task<BaseResponse> Delete([FromBody] DeleteTransferRequest rq) => await _transferService.Delete(rq);

    [HttpGet]
    public async Task<BaseResponse<GetTransfersByDateResponse>> GetByDate([FromQuery] GetTransfersByDateRequest rq) => await _transferService.GetByDate(rq);
}
