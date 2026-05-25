using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Abonos;
using AguasNico_Api.Models.DTO.Common;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Services;

public class AbonoService(APIContext context)
{
    private readonly APIContext _db = context;

    public async Task<BaseResponse<GetAllAbonosResponse>> GetAll()
    {
        return new BaseResponse<GetAllAbonosResponse>
        {
            Data = new GetAllAbonosResponse
            {
                Items = await _db.Abonos
                    .AsNoTracking()
                    .Select(x => new AbonoItem
                    {
                        Id = x.ID,
                        Name = x.Name,
                        Price = x.Price,
                        Products = x.Products.Select(p => new AbonoProductItem
                        {
                            Type = p.Type,
                            TypeName = p.Type.GetDisplayName(),
                            Quantity = p.Quantity
                        }).ToList()
                    })
                    .OrderBy(x => x.Name)
                    .ThenBy(x => x.Price)
                    .ToListAsync()
            }
        };
    }

    public async Task<BaseResponse<CreateAbonoResponse>> Create(CreateAbonoRequest rq)
    {
        var rs = new BaseResponse<CreateAbonoResponse>();
        if (!ValidateAbono(rq.Name, rq.Price, out var error))
            return rs.SetError(error);
        if (rq.Products.GroupBy(x => x.Type).Any(x => x.Count() > 1))
            return rs.SetError("No se pueden agregar dos productos del mismo tipo");
        if (rq.Products.Any(x => x.Quantity < 1 || x.Quantity > 100))
            return rs.SetError(Messages.Error.InvalidField("cantidad"));

        var abono = new Abono
        {
            Name = rq.Name!.Trim(),
            Price = rq.Price,
            Products = rq.Products.Select(x => new AbonoProduct { Type = x.Type, Quantity = x.Quantity }).ToList()
        };

        _db.Abonos.Add(abono);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            return rs.SetError(Messages.Error.Exception());
        }

        rs.Data = new CreateAbonoResponse { Id = abono.ID };
        rs.Message = Messages.CRUD.EntityCreated("Abono");
        return rs;
    }

    public async Task<BaseResponse<UpdateAbonoResponse>> Update(UpdateAbonoRequest rq)
    {
        var rs = new BaseResponse<UpdateAbonoResponse>();
        var abono = await _db.Abonos.FirstOrDefaultAsync(x => x.ID == rq.Id);
        if (abono == null)
            return rs.SetError(Messages.Error.EntityNotFound("Abono"));
        if (!ValidateAbono(rq.Name, rq.Price, out var error))
            return rs.SetError(error);

        abono.Name = rq.Name!.Trim();
        abono.Price = rq.Price;
        abono.UpdatedAt = LocalClock.Now;

        await _db.SaveChangesAsync();
        rs.Data = new UpdateAbonoResponse { Id = abono.ID };
        rs.Message = Messages.CRUD.EntityUpdated("Abono");
        return rs;
    }

    public async Task<BaseResponse> Delete(DeleteAbonoRequest rq)
    {
        var rs = new BaseResponse();
        var abono = await _db.Abonos.FirstOrDefaultAsync(x => x.ID == rq.Id);
        if (abono == null)
            return rs.SetError(Messages.Error.EntityNotFound("Abono"));

        abono.DeletedAt = LocalClock.Now;
        var clientAbonos = await _db.ClientAbonos.Where(x => x.AbonoID == rq.Id).ToListAsync();
        var abonoProducts = await _db.AbonoProducts.Where(x => x.AbonoID == rq.Id).ToListAsync();
        foreach (var clientAbono in clientAbonos)
            clientAbono.DeletedAt = LocalClock.Now;
        foreach (var abonoProduct in abonoProducts)
            abonoProduct.DeletedAt = LocalClock.Now;

        try
        {
            await _db.Database.BeginTransactionAsync();
            await _db.SaveChangesAsync();
            await _db.Database.CommitTransactionAsync();
        }
        catch
        {
            await RollbackIfNeeded();
            return rs.SetError(Messages.Error.Exception());
        }

        rs.Message = Messages.CRUD.EntityDeleted("Abono");
        return rs;
    }

    public async Task<BaseResponse> RenewAll()
    {
        var rs = new BaseResponse();
        var today = LocalClock.Now;
        var clientAbonos = await _db.ClientAbonos
            .Include(x => x.Abono)
            .ThenInclude(x => x.Products)
            .Include(x => x.Client)
            .ToListAsync();

        foreach (var clientAbono in clientAbonos)
        {
            var exists = await _db.AbonoRenewals.AnyAsync(x => x.AbonoID == clientAbono.AbonoID && x.ClientID == clientAbono.ClientID && x.CreatedAt.Month == today.Month && x.CreatedAt.Year == today.Year);
            if (exists)
                continue;

            var renewal = new AbonoRenewal
            {
                AbonoID = clientAbono.AbonoID,
                ClientID = clientAbono.ClientID,
                SettedPrice = clientAbono.Abono.Price
            };
            _db.AbonoRenewals.Add(renewal);

            foreach (var product in clientAbono.Abono.Products)
            {
                _db.AbonoRenewalProducts.Add(new AbonoRenewalProduct
                {
                    AbonoRenewal = renewal,
                    Type = product.Type,
                    Available = product.Quantity
                });
            }

            clientAbono.Client.Debt += clientAbono.Abono.Price;
        }

        try
        {
            await _db.Database.BeginTransactionAsync();
            await _db.SaveChangesAsync();
            await _db.Database.CommitTransactionAsync();
        }
        catch
        {
            await RollbackIfNeeded();
            return rs.SetError(Messages.Error.Exception());
        }

        rs.Message = "Todos los abonos se renovaron correctamente";
        return rs;
    }

    public async Task<BaseResponse> RenewByRoute(RenewByRouteRequest rq)
    {
        var rs = new BaseResponse();
        var today = LocalClock.Now;
        var clientIds = await _db.Routes
            .Where(x => x.ID == rq.RouteId)
            .SelectMany(x => x.Carts.Select(c => c.ClientID))
            .Distinct()
            .ToListAsync();

        var clientAbonos = await _db.ClientAbonos
            .Where(x => clientIds.Contains(x.ClientID))
            .Include(x => x.Abono)
            .ThenInclude(x => x.Products)
            .Include(x => x.Client)
            .ToListAsync();

        foreach (var clientAbono in clientAbonos)
        {
            var exists = await _db.AbonoRenewals.AnyAsync(x => x.AbonoID == clientAbono.AbonoID && x.ClientID == clientAbono.ClientID && x.CreatedAt.Month == today.Month && x.CreatedAt.Year == today.Year);
            if (exists)
                continue;

            var renewal = new AbonoRenewal
            {
                AbonoID = clientAbono.AbonoID,
                ClientID = clientAbono.ClientID,
                SettedPrice = clientAbono.Abono.Price,
                ProductsAvailables = clientAbono.Abono.Products.Select(x => new AbonoRenewalProduct
                {
                    Type = x.Type,
                    Available = x.Quantity
                }).ToList()
            };
            _db.AbonoRenewals.Add(renewal);
            clientAbono.Client.Debt += clientAbono.Abono.Price;
        }

        try
        {
            await _db.Database.BeginTransactionAsync();
            await _db.SaveChangesAsync();
            await _db.Database.CommitTransactionAsync();
        }
        catch
        {
            await RollbackIfNeeded();
            return rs.SetError(Messages.Error.Exception());
        }

        rs.Message = "Los abonos se renovaron correctamente";
        return rs;
    }

    public async Task<BaseResponse<GetAbonoClientsResponse>> GetClients(GetAbonoClientsRequest rq)
    {
        return new BaseResponse<GetAbonoClientsResponse>
        {
            Data = new GetAbonoClientsResponse
            {
                Items = await _db.ClientAbonos
                    .AsNoTracking()
                    .Where(x => x.AbonoID == rq.AbonoId && x.Client.IsActive)
                    .Select(x => new ClientSummaryItem
                    {
                        Id = x.Client.ID,
                        Name = x.Client.Name,
                        Address = x.Client.Address,
                        Phone = x.Client.Phone,
                        Email = x.Client.Email ?? "",
                        DealerId = x.Client.DealerID,
                        DealerName = x.Client.Dealer != null ? x.Client.Dealer.Name : "",
                        DeliveryDay = x.Client.DeliveryDay ?? 0,
                        Debt = x.Client.Debt,
                        HasInvoice = x.Client.HasInvoice,
                        OnlyAbonos = x.Client.OnlyAbonos,
                        IsActive = x.Client.IsActive
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync()
            }
        };
    }

    private static bool ValidateAbono(string? name, decimal price, out string error)
    {
        error = "";
        if (string.IsNullOrWhiteSpace(name))
            error = Messages.Error.FieldRequired("nombre");
        else if (price < 0)
            error = Messages.Error.InvalidField("precio");
        return string.IsNullOrEmpty(error);
    }

    private async Task RollbackIfNeeded()
    {
        if (_db.Database.CurrentTransaction != null)
            await _db.Database.RollbackTransactionAsync();
    }
}

