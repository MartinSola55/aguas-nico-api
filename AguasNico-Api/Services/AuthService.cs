using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Auth;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AguasNico_Api.Services;

public class AuthService(TokenService tokenService, UserManager<ApplicationUser> userManager)
{
    private readonly TokenService _tokenService = tokenService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<BaseResponse<LoginResponse>> Login(LoginRequest rq)
    {
        var rs = new BaseResponse<LoginResponse>();

        if (string.IsNullOrEmpty(rq.Email) || string.IsNullOrEmpty(rq.Password))
            return rs.SetError(Messages.Error.FieldsRequired(["Email", "Password"]));

        if (!new EmailAddressAttribute().IsValid(rq.Email))
            return rs.SetError(Messages.Error.InvalidEmail());

        var user = await _userManager.FindByEmailAsync(rq.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, rq.Password))
            return rs.SetError(Messages.Error.InvalidLogin());

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
        if (role == null)
            return rs.SetError(Messages.Error.UserWithoutRole());

        var expiration = DateTime.Now.AddDays(30);
        var token = _tokenService.GenerateToken(user, role, expiration);
        if (string.IsNullOrEmpty(token))
            return rs.SetError(Messages.Error.TokenCreation());

        rs.Data = new LoginResponse
        {
            Token = token,
            SessionExpiration = expiration,
            User = new LoginResponse.UserItem
            {
                Id = user.Id,
                Role = role,
                Name = user.Name,
                Email = user.Email ?? "",
                TruckNumber = user.TruckNumber ?? 0
            }
        };

        return rs;
    }

    public BaseResponse Logout()
    {
        try
        {
            _tokenService.GetToken();
            return new BaseResponse();
        }
        catch
        {
            return new BaseResponse().SetError(Messages.Error.ExpiredToken());
        }
    }

    public bool IsAdmin() => _tokenService.GetToken().Role == Roles.Admin;
    public bool IsDealer() => _tokenService.GetToken().Role == Roles.Dealer;
}


