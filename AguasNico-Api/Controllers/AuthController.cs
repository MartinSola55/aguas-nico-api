using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Auth;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AguasNico_Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController(AuthService authService) : ControllerBase
{
    private readonly AuthService _authService = authService;

    [HttpPost]
    public async Task<BaseResponse<LoginResponse>> Login([FromBody] LoginRequest rq)
    {
        return await _authService.Login(rq);
    }

    [HttpPost]
    public BaseResponse Logout()
    {
        return _authService.Logout();
    }
}
