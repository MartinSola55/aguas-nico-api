using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Invoices;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AguasNico_Api.Controllers;

[Authorize(Policy = Policies.Admin)]
public class InvoiceController(InvoiceService invoiceService) : BaseController
{
    private readonly InvoiceService _invoiceService = invoiceService;

    [HttpGet]
    public async Task<BaseResponse<GetInvoicesResponse>> GetInvoices([FromQuery] GetInvoicesRequest rq) => await _invoiceService.GetInvoices(rq);

    [HttpGet]
    public async Task<BaseResponse<GetInvoicesCsvResponse>> GetCsvRows([FromQuery] GetInvoicesCsvRequest rq) => await _invoiceService.GetCsvRows(rq);

    [HttpGet]
    public async Task<IActionResult> DownloadCsv([FromQuery] GetInvoicesCsvRequest rq)
    {
        var bytes = await _invoiceService.BuildCsv(rq);
        return File(bytes, "text/csv", $"facturas_{rq.StartDate:dd-MM-yyy}_{rq.EndDate:dd-MM-yyy}.csv");
    }
}
