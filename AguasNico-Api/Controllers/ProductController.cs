using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Products;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AguasNico_Api.Controllers;

public class ProductController(ProductService productService) : BaseController
{
    private readonly ProductService _productService = productService;

    [HttpPost]
    public async Task<BaseResponse<GetAllProductsResponse>> GetAll([FromBody] GetAllProductsRequest rq) => await _productService.GetAll(rq);

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse<GetProductResponse>> GetOne([FromQuery] GetProductRequest rq) => await _productService.GetOne(rq);

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse<CreateProductResponse>> Create([FromBody] CreateProductRequest rq) => await _productService.Create(rq);

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse<UpdateProductResponse>> Update([FromBody] UpdateProductRequest rq) => await _productService.Update(rq);

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse> Delete([FromBody] DeleteProductRequest rq) => await _productService.Delete(rq);

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse<GetProductClientsResponse>> GetClients([FromQuery] GetProductClientsRequest rq) => await _productService.GetClients(rq);

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse<GetProductStatsResponse>> GetStats([FromQuery] GetProductStatsRequest rq) => await _productService.GetStats(rq);
}
