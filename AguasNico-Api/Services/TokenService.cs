using AguasNico_Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AguasNico_Api.Services;

public class TokenService(IHttpContextAccessor httpContextAccessor, IConfiguration config)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IConfiguration _config = config;

    public Token GetToken()
    {
        if (_httpContextAccessor.HttpContext == null)
            throw new Exception("No se ha podido encontrar el token");

        var claims = _httpContextAccessor.HttpContext.User.Claims;

        return new Token
        {
            UserId = claims.First(x => x.Type == "userId").Value,
            Email = claims.First(x => x.Type == ClaimTypes.Email).Value,
            Name = claims.First(x => x.Type == ClaimTypes.Name).Value,
            Role = claims.First(x => x.Type == ClaimTypes.Role).Value,
        };
    }

    public string? GenerateToken(ApplicationUser user, string role, DateTime expiration)
    {
        var jwtKey = _config["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
            return null;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("userId", user.Id),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims,
            expires: expiration,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

