using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Users;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AguasNico_Api.Controllers;

public class UserController(UserService userService) : BaseController
{
    private readonly UserService _userService = userService;

    [HttpGet]
    public async Task<BaseResponse<GetProfileResponse>> GetProfile([FromQuery] GetProfileRequest rq) => await _userService.GetProfile(rq);

    [HttpPost]
    public async Task<BaseResponse> UpdateTruckNumber([FromBody] UpdateTruckNumberRequest rq) => await _userService.UpdateTruckNumber(rq);

    [HttpPost]
    public async Task<BaseResponse> UpdatePassword([FromBody] UpdatePasswordRequest rq) => await _userService.UpdatePassword(rq);
}
