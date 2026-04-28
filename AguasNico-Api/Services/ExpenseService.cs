using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Expenses;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Services;

public class ExpenseService(APIContext context)
{
    private readonly APIContext _db = context;

    public async Task<BaseResponse<GetExpensesResponse>> GetAll(GetExpensesRequest rq)
    {
        var query = _db.Expenses.AsNoTracking().AsQueryable();
        if (rq.DateFrom != default)
            query = query.Where(x => x.CreatedAt.Date >= rq.DateFrom.Date);
        if (rq.DateTo != default)
            query = query.Where(x => x.CreatedAt.Date <= rq.DateTo.Date);
        if (!string.IsNullOrEmpty(rq.UserId))
            query = query.Where(x => x.UserID == rq.UserId);

        return new BaseResponse<GetExpensesResponse>
        {
            Data = new GetExpensesResponse
            {
                Items = await query
                    .Select(x => new ExpenseItem
                    {
                        Id = x.ID,
                        UserId = x.UserID,
                        DealerName = x.User.Name,
                        Amount = x.Amount,
                        Description = x.Description,
                        CreatedAt = x.CreatedAt
                    })
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync()
            }
        };
    }

    public async Task<BaseResponse<GetExpenseResponse>> GetOne(GetExpenseRequest rq)
    {
        var rs = new BaseResponse<GetExpenseResponse>();
        var expense = await _db.Expenses
            .AsNoTracking()
            .Where(x => x.ID == rq.Id)
            .Select(x => new GetExpenseResponse
            {
                Id = x.ID,
                UserId = x.UserID,
                DealerName = x.User.Name,
                Amount = x.Amount,
                Description = x.Description,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (expense == null)
            return rs.SetError(Messages.Error.EntityNotFound("Gasto"));

        rs.Data = expense;
        return rs;
    }

    public async Task<BaseResponse<CreateExpenseResponse>> Create(CreateExpenseRequest rq)
    {
        var rs = new BaseResponse<CreateExpenseResponse>();
        if (!rs.Attach(await ValidateExpense<CreateExpenseResponse>(rq.UserId, rq.Amount, rq.Description)).Success)
            return rs;

        var expense = new Expense
        {
            UserID = rq.UserId,
            Amount = rq.Amount,
            Description = rq.Description!.Trim()
        };

        _db.Expenses.Add(expense);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            return rs.SetError(Messages.Error.Exception());
        }

        rs.Data = new CreateExpenseResponse { Id = expense.ID };
        rs.Message = Messages.CRUD.EntityCreated("Gasto");
        return rs;
    }

    public async Task<BaseResponse<UpdateExpenseResponse>> Update(UpdateExpenseRequest rq)
    {
        var rs = new BaseResponse<UpdateExpenseResponse>();
        var expense = await _db.Expenses.FirstOrDefaultAsync(x => x.ID == rq.Id);
        if (expense == null)
            return rs.SetError(Messages.Error.EntityNotFound("Gasto"));

        if (!rs.Attach(await ValidateExpense<UpdateExpenseResponse>(rq.UserId, rq.Amount, rq.Description)).Success)
            return rs;

        expense.UserID = rq.UserId;
        expense.Amount = rq.Amount;
        // Legacy behavior updates only dealer and amount, not description.
        expense.UpdatedAt = LocalClock.Now;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            return rs.SetError(Messages.Error.Exception());
        }

        rs.Data = new UpdateExpenseResponse { Id = expense.ID };
        rs.Message = Messages.CRUD.EntityUpdated("Gasto");
        return rs;
    }

    public async Task<BaseResponse> Delete(DeleteExpenseRequest rq)
    {
        var rs = new BaseResponse();
        var expense = await _db.Expenses.FirstOrDefaultAsync(x => x.ID == rq.Id);
        if (expense == null)
            return rs.SetError(Messages.Error.EntityNotFound("Gasto"));

        expense.DeletedAt = LocalClock.Now;
        await _db.SaveChangesAsync();
        rs.Message = Messages.CRUD.EntityDeleted("Gasto");
        return rs;
    }

    public async Task<BaseResponse<SearchExpenseByDateResponse>> SearchByDate(SearchExpenseByDateRequest rq)
    {
        return new BaseResponse<SearchExpenseByDateResponse>
        {
            Data = new SearchExpenseByDateResponse
            {
                Items = await _db.Expenses
                    .AsNoTracking()
                    .Where(x => x.CreatedAt.Date == rq.Date.Date)
                    .Select(x => new SearchExpenseByDateResponse.ExpenseByDateItem
                    {
                        Dealer = x.User.Name,
                        Description = x.Description,
                        Amount = x.Amount
                    })
                    .OrderBy(x => x.Description)
                    .ToListAsync()
            }
        };
    }

    private async Task<BaseResponse<T>> ValidateExpense<T>(string userId, decimal amount, string? description)
    {
        var rs = new BaseResponse<T>();
        if (string.IsNullOrEmpty(userId) || !await _db.User.AnyAsync(x => x.Id == userId))
            return rs.SetError(Messages.Error.EntityNotFound("Repartidor"));
        if (amount < 0 || amount > 10000000)
            return rs.SetError(Messages.Error.InvalidField("monto"));
        if (string.IsNullOrWhiteSpace(description))
            return rs.SetError(Messages.Error.FieldRequired("descripción"));
        if (description.Length > 200)
            return rs.SetError(Messages.Error.InvalidField("descripción"));
        return rs;
    }
}

