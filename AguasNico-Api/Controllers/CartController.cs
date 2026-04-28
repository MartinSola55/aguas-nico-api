using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Carts;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AguasNico_Api.Controllers;

public class CartController(CartService cartService) : BaseController
{
    private readonly CartService _cartService = cartService;

    [HttpGet]
    public async Task<BaseResponse<GetCartForEditResponse>> GetForEdit([FromQuery] GetCartForEditRequest rq) => await _cartService.GetForEdit(rq);

    [HttpPost]
    public async Task<BaseResponse> Update([FromBody] UpdateCartRequest rq) => await _cartService.Update(rq);

    [HttpPost]
    public async Task<BaseResponse> Confirm([FromBody] ConfirmCartRequest rq) => await _cartService.Confirm(rq);

    [HttpPost]
    public async Task<BaseResponse<ConfirmManualCartResponse>> ConfirmManual([FromBody] ConfirmManualCartRequest rq) => await _cartService.ConfirmManual(rq);

    [HttpPost]
    public async Task<BaseResponse> SetState([FromBody] SetCartStateRequest rq) => await _cartService.SetState(rq);

    [HttpPost]
    public async Task<BaseResponse> ResetState([FromBody] ResetCartStateRequest rq) => await _cartService.ResetState(rq);

    [HttpGet]
    public async Task<BaseResponse<GetReturnedProductsResponse>> GetReturnedProducts([FromQuery] GetReturnedProductsRequest rq) => await _cartService.GetReturnedProducts(rq);

    [HttpPost]
    [Authorize(Policy = Policies.Dealer)]
    public async Task<BaseResponse> ReturnProducts([FromBody] ReturnProductsRequest rq) => await _cartService.ReturnProducts(rq);

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<BaseResponse> Delete([FromBody] DeleteCartRequest rq) => await _cartService.Delete(rq);
}
