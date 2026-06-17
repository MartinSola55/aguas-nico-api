using AguasNico_Api.Models.Constants;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AguasNico_Api.Controllers;

[Authorize(Policy = Policies.Admin)]
public class CajaController(CajaService cajaService) : BaseController
{
    private readonly CajaService _cajaService = cajaService;

    [HttpGet]
    public async Task<IActionResult> DownloadDailyClose([FromQuery] DateTime date)
    {
        var bytes = await _cajaService.GenerateDailyClose(date);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"caja_diaria_{date:dd-MM-yyyy}.xlsx");
    }
}
