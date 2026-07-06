using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Users;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Services;

public class UserService(APIContext context, TokenService tokenService, AuthService authService)
{
    private readonly APIContext _db = context;
    private readonly Token _token = tokenService.GetToken();
    private readonly AuthService _authService = authService;

    public async Task<BaseResponse<GetProfileResponse>> GetProfile(GetProfileRequest rq)
    {
        var rs = new BaseResponse<GetProfileResponse>();
        var targetId = string.IsNullOrEmpty(rq.Id) ? _token.UserId : rq.Id;

        if (targetId != _token.UserId && _token.Role != Roles.Admin)
            return rs.SetError(Messages.Error.Unauthorized(), 403);

        var profile = await _db.User
            .AsNoTracking()
            .Include(x => x.Role)
            .Where(x => x.Id == targetId)
            .Select(x => new GetProfileResponse
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Role = x.Role.Name,
                TruckNumber = x.TruckNumber ?? 0
            })
            .FirstOrDefaultAsync();

        if (profile == null)
            return rs.SetError(Messages.Error.EntityNotFound("Usuario"));

        rs.Data = profile;
        return rs;
    }

    public async Task<BaseResponse> UpdateTruckNumber(UpdateTruckNumberRequest rq)
    {
        var rs = new BaseResponse();

        if (_token.Role != Roles.Admin)
            return rs.SetError(Messages.Error.Unauthorized(), 403);

        var user = await _db.User.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == rq.Id);
        if (user == null)
            return rs.SetError(Messages.Error.EntityNotFound("Repartidor"));

        if (user.Role == null || user.Role.Name != Roles.Dealer)
            return rs.SetError(Messages.Error.InvalidField("número de camión"));

        if (rq.TruckNumber <= 0)
            return rs.SetError(Messages.Error.FieldGraterThanZero("número de camión"));

        user.TruckNumber = rq.TruckNumber;
        user.UpdatedAt = LocalClock.Now;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            return rs.SetError(Messages.Error.Exception());
        }

        rs.Message = Messages.CRUD.EntityUpdated("Repartidor");
        return rs;
    }

    public async Task<BaseResponse> UpdatePassword(UpdatePasswordRequest rq)
    {
        var rs = new BaseResponse();
        var targetId = string.IsNullOrEmpty(rq.Id) ? _token.UserId : rq.Id;

        if (targetId != _token.UserId && _token.Role != Roles.Admin)
            return rs.SetError(Messages.Error.Unauthorized(), 403);

        if (string.IsNullOrEmpty(rq.Password) || !_authService.ValidatePassword(rq.Password))
            return rs.SetError(Messages.Error.InvalidPassword());

        var user = await _db.User.FirstOrDefaultAsync(x => x.Id == targetId);
        if (user == null)
            return rs.SetError(Messages.Error.EntityNotFound("Usuario"));

        user.PasswordHash = _authService.HashPassword(rq.Password);
        user.UpdatedAt = LocalClock.Now;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            return rs.SetError(Messages.Error.Exception());
        }

        rs.Message = Messages.CRUD.EntityUpdated("Contraseña", femine: true);
        return rs;
    }
}
