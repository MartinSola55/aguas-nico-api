using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Terceros;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Services;

public class TerceroService(APIContext context)
{
    private readonly APIContext _db = context;

    public async Task<BaseResponse<GetTercerosResponse>> GetByDate(GetTercerosRequest rq)
    {
        return new BaseResponse<GetTercerosResponse>
        {
            Data = new GetTercerosResponse
            {
                Items = await _db.Terceros
                    .AsNoTracking()
                    .Where(x => x.Date.Date == rq.Date.Date)
                    .Select(x => new TerceroItem
                    {
                        Id = x.ID,
                        Date = x.Date,
                        Name = x.Name,
                        SodaQuantity = x.SodaQuantity,
                        SodaAmount = x.SodaAmount,
                        B12LQuantity = x.B12LQuantity,
                        B12LAmount = x.B12LAmount,
                        B20LQuantity = x.B20LQuantity,
                        B20LAmount = x.B20LAmount
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync()
            }
        };
    }

    public async Task<BaseResponse<CreateTerceroResponse>> Create(CreateTerceroRequest rq)
    {
        var rs = new BaseResponse<CreateTerceroResponse>();
        if (!rs.Attach(Validate<CreateTerceroResponse>(rq.Name, rq.SodaQuantity, rq.SodaAmount, rq.B12LQuantity, rq.B12LAmount, rq.B20LQuantity, rq.B20LAmount)).Success)
            return rs;

        var tercero = new Tercero
        {
            Date = rq.Date == default ? LocalClock.Today : rq.Date.Date,
            Name = rq.Name.Trim(),
            SodaQuantity = rq.SodaQuantity,
            SodaAmount = rq.SodaAmount,
            B12LQuantity = rq.B12LQuantity,
            B12LAmount = rq.B12LAmount,
            B20LQuantity = rq.B20LQuantity,
            B20LAmount = rq.B20LAmount
        };

        _db.Terceros.Add(tercero);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            return rs.SetError(Messages.Error.Exception());
        }

        rs.Data = new CreateTerceroResponse { Id = tercero.ID };
        rs.Message = Messages.CRUD.EntityCreated("Tercero");
        return rs;
    }

    public async Task<BaseResponse<UpdateTerceroResponse>> Update(UpdateTerceroRequest rq)
    {
        var rs = new BaseResponse<UpdateTerceroResponse>();
        var tercero = await _db.Terceros.FirstOrDefaultAsync(x => x.ID == rq.Id);
        if (tercero == null)
            return rs.SetError(Messages.Error.EntityNotFound("Tercero"));

        if (!rs.Attach(Validate<UpdateTerceroResponse>(rq.Name, rq.SodaQuantity, rq.SodaAmount, rq.B12LQuantity, rq.B12LAmount, rq.B20LQuantity, rq.B20LAmount)).Success)
            return rs;

        tercero.Name = rq.Name.Trim();
        tercero.SodaQuantity = rq.SodaQuantity;
        tercero.SodaAmount = rq.SodaAmount;
        tercero.B12LQuantity = rq.B12LQuantity;
        tercero.B12LAmount = rq.B12LAmount;
        tercero.B20LQuantity = rq.B20LQuantity;
        tercero.B20LAmount = rq.B20LAmount;
        tercero.UpdatedAt = LocalClock.Now;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            return rs.SetError(Messages.Error.Exception());
        }

        rs.Data = new UpdateTerceroResponse { Id = tercero.ID };
        rs.Message = Messages.CRUD.EntityUpdated("Tercero");
        return rs;
    }

    public async Task<BaseResponse> Delete(DeleteTerceroRequest rq)
    {
        var rs = new BaseResponse();
        var tercero = await _db.Terceros.FirstOrDefaultAsync(x => x.ID == rq.Id);
        if (tercero == null)
            return rs.SetError(Messages.Error.EntityNotFound("Tercero"));

        tercero.DeletedAt = LocalClock.Now;
        await _db.SaveChangesAsync();
        rs.Message = Messages.CRUD.EntityDeleted("Tercero");
        return rs;
    }

    private static BaseResponse<T> Validate<T>(string name, int sodaQty, decimal sodaAmount, int b12Qty, decimal b12Amount, int b20Qty, decimal b20Amount)
    {
        var rs = new BaseResponse<T>();
        if (string.IsNullOrWhiteSpace(name))
            return rs.SetError(Messages.Error.FieldRequired("nombre"));
        if (name.Length > 200)
            return rs.SetError(Messages.Error.InvalidField("nombre"));
        if (sodaQty < 0 || b12Qty < 0 || b20Qty < 0)
            return rs.SetError(Messages.Error.InvalidField("cantidad"));
        if (sodaAmount < 0 || b12Amount < 0 || b20Amount < 0)
            return rs.SetError(Messages.Error.InvalidField("importe"));
        return rs;
    }
}
