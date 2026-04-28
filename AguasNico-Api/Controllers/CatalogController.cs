using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Catalog;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AguasNico_Api.Controllers;

public class CatalogController(CatalogService catalogService) : BaseController
{
    private readonly CatalogService _catalogService = catalogService;

    [HttpGet]
    public async Task<BaseResponse<GetCatalogResponse>> GetAll() => await _catalogService.GetAll();
}
