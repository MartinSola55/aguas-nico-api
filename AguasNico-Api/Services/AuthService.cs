using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Auth;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AguasNico_Api.Services;

public class AuthService(TokenService tokenService, APIContext context)
{
    private readonly TokenService _tokenService = tokenService;
    private readonly APIContext _db = context;

    public async Task<BaseResponse<LoginResponse>> Login(LoginRequest rq)
    {
        var rs = new BaseResponse<LoginResponse>();

        if (string.IsNullOrEmpty(rq.Email) || string.IsNullOrEmpty(rq.Password))
            return rs.SetError(Messages.Error.FieldsRequired(["Email", "Password"]));

        if (!new EmailAddressAttribute().IsValid(rq.Email))
            return rs.SetError(Messages.Error.InvalidEmail());

        var email = rq.Email.ToLower();
        var user = await _db
            .User
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user == null || !ValidateHashedPassword(rq.Password, user.PasswordHash))
            return rs.SetError(Messages.Error.InvalidLogin());

        if (user.Role == null)
            return rs.SetError(Messages.Error.UserWithoutRole());

        var expiration = DateTime.UtcNow.AddDays(30);
        var token = _tokenService.GenerateToken(user, user.Role.Name, expiration);
        if (string.IsNullOrEmpty(token))
            return rs.SetError(Messages.Error.TokenCreation());

        rs.Data = new LoginResponse
        {
            Token = token,
            SessionExpiration = expiration,
            User = new LoginResponse.UserItem
            {
                Id = user.Id,
                Role = user.Role.Name,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                TruckNumber = user.TruckNumber ?? 0
            }
        };

        return rs;
    }

    public BaseResponse Logout()
    {
        if (_tokenService.TryGetToken() == null)
            return new BaseResponse().SetError(Messages.Error.ExpiredToken());

        return new BaseResponse();
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool ValidatePassword(string password)
    {
        return new bool[]
        {
            password.Length >= 8,
            password.Any(char.IsUpper),
            password.Any(char.IsLower),
            password.Any(char.IsDigit)
        }.All(x => x);
    }

    public bool IsAdmin() => _tokenService.GetToken().Role == Roles.Admin;
    public bool IsDealer() => _tokenService.GetToken().Role == Roles.Dealer;

    private static bool ValidateHashedPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
