using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Transfers;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Services;

public class TransferService(APIContext context)
{
    private readonly APIContext _db = context;

    public async Task<BaseResponse<GetTransfersResponse>> GetAll(GetTransfersRequest rq)
    {
        var query = _db.Transfers.AsNoTracking().AsQueryable();
        if (rq.DateFrom != default)
            query = query.Where(x => x.Date.Date >= rq.DateFrom.Date);
        if (rq.DateTo != default)
            query = query.Where(x => x.Date.Date <= rq.DateTo.Date);
        if (!string.IsNullOrEmpty(rq.UserId))
            query = query.Where(x => x.UserID == rq.UserId);

        return new BaseResponse<GetTransfersResponse>
        {
            Data = new GetTransfersResponse
            {
                Items = await query
                    .Select(x => new TransferItem
                    {
                        Id = x.ID,
                        ClientId = x.ClientID,
                        ClientName = x.Client.Name,
                        UserId = x.UserID,
                        DealerName = x.User.Name,
                        Amount = x.Amount,
                        Date = x.Date,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync()
            }
        };
    }

    public async Task<BaseResponse<CreateTransferResponse>> Create(CreateTransferRequest rq)
    {
        var rs = new BaseResponse<CreateTransferResponse>();
        if (!ValidateTransfer(rq.Amount, rq.Date, out var error))
            return rs.SetError(error);

        var client = await _db.Clients.FirstOrDefaultAsync(x => x.ID == rq.ClientId);
        if (client == null)
            return rs.SetError(Messages.Error.EntityNotFound("Cliente"));

        client.Debt -= rq.Amount;

        var transfer = new Transfer
        {
            ClientID = rq.ClientId,
            UserID = client.DealerID ?? rq.UserId ?? "",
            Amount = rq.Amount,
            Date = rq.Date,
        };

        if (string.IsNullOrEmpty(transfer.UserID))
            return rs.SetError(Messages.Error.EntityNotFound("Repartidor"));

        _db.Transfers.Add(transfer);

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

        rs.Data = new CreateTransferResponse { Id = transfer.ID };
        rs.Message = Messages.CRUD.EntityCreated("Transferencia", true);
        return rs;
    }

    public async Task<BaseResponse<UpdateTransferResponse>> Update(UpdateTransferRequest rq)
    {
        var rs = new BaseResponse<UpdateTransferResponse>();
        if (!ValidateTransfer(rq.Amount, rq.Date, out var error))
            return rs.SetError(error);

        var transfer = await _db.Transfers.FirstOrDefaultAsync(x => x.ID == rq.Id);
        if (transfer == null)
            return rs.SetError(Messages.Error.EntityNotFound("Transferencia", true));

        var client = await _db.Clients.FirstOrDefaultAsync(x => x.ID == transfer.ClientID);
        if (client == null)
            return rs.SetError(Messages.Error.EntityNotFound("Cliente"));

        client.Debt += transfer.Amount;
        client.Debt -= rq.Amount;

        if (client.DealerID != null)
            transfer.UserID = client.DealerID;

        transfer.Amount = rq.Amount;
        if (rq.UpdateDate)
            transfer.Date = rq.Date;
        transfer.UpdatedAt = LocalClock.Now;

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

        rs.Data = new UpdateTransferResponse { Id = transfer.ID };
        rs.Message = Messages.CRUD.EntityUpdated("Transferencia", true);
        return rs;
    }

    public async Task<BaseResponse> Delete(DeleteTransferRequest rq)
    {
        var rs = new BaseResponse();
        var transfer = await _db.Transfers.FirstOrDefaultAsync(x => x.ID == rq.Id);
        if (transfer == null)
            return rs.SetError(Messages.Error.EntityNotFound("Transferencia", true));

        var client = await _db.Clients.FirstOrDefaultAsync(x => x.ID == transfer.ClientID);
        if (client == null)
            return rs.SetError(Messages.Error.EntityNotFound("Cliente"));

        client.Debt += transfer.Amount;
        transfer.DeletedAt = LocalClock.Now;

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

        rs.Message = Messages.CRUD.EntityDeleted("Transferencia", true);
        return rs;
    }

    public async Task<BaseResponse<GetTransfersByDateResponse>> GetByDate(GetTransfersByDateRequest rq)
    {
        return new BaseResponse<GetTransfersByDateResponse>
        {
            Data = new GetTransfersByDateResponse
            {
                Items = await _db.Transfers
                    .AsNoTracking()
                    .Where(x => x.Date.Date == rq.Date.Date)
                    .Select(x => new TransferItem
                    {
                        Id = x.ID,
                        ClientId = x.ClientID,
                        ClientName = x.Client.Name,
                        UserId = x.UserID,
                        DealerName = x.User.Name,
                        Amount = x.Amount,
                        Date = x.Date,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync()
            }
        };
    }

    private static bool ValidateTransfer(decimal amount, DateTime date, out string error)
    {
        error = "";
        if (amount <= 0)
            error = "El monto de la transferencia no puede ser menor o igual a 0";
        else if (date.Date > LocalClock.Today)
            error = "La fecha de la transferencia no puede ser mayor a la fecha actual";
        return string.IsNullOrEmpty(error);
    }

    private async Task RollbackIfNeeded()
    {
        if (_db.Database.CurrentTransaction != null)
            await _db.Database.RollbackTransactionAsync();
    }
}

