using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Clients;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AguasNico_Api.Controllers;

public class ClientController(ClientService clientService) : BaseController
{
    private readonly ClientService _clientService = clientService;

    [HttpPost]
    public async Task<BaseResponse<GetAllClientsResponse>> GetAll([FromBody] GetAllClientsRequest rq) => await _clientService.GetAll(rq);

    [HttpGet]
    public async Task<BaseResponse<GetClientResponse>> GetOne([FromQuery] GetClientRequest rq) => await _clientService.GetOne(rq);

    [HttpPost]
    public async Task<BaseResponse<CreateClientResponse>> Create([FromBody] CreateClientRequest rq) => await _clientService.Create(rq);

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse<UpdateClientResponse>> Update([FromBody] UpdateClientRequest rq) => await _clientService.Update(rq);

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse> UpdateInvoiceData([FromBody] UpdateInvoiceDataRequest rq) => await _clientService.UpdateInvoiceData(rq);

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse> UpdateProducts([FromBody] UpdateClientProductsRequest rq) => await _clientService.UpdateProducts(rq);

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse> UpdateAbonos([FromBody] UpdateClientAbonosRequest rq) => await _clientService.UpdateAbonos(rq);

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse> Delete([FromBody] DeleteClientRequest rq) => await _clientService.Delete(rq);

    [HttpGet]
    public async Task<BaseResponse<GetProductsAndAbonoResponse>> GetProductsAndAbono([FromQuery] GetProductsAndAbonoRequest rq) => await _clientService.GetProductsAndAbono(rq);

    [HttpGet]
    public async Task<BaseResponse<GetProductsHistoryResponse>> GetProductsHistory([FromQuery] GetProductsHistoryRequest rq) => await _clientService.GetProductsHistory(rq);

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse<GetUnassignedClientsResponse>> GetUnassigned() => await _clientService.GetUnassigned();
}
