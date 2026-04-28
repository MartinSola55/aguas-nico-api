using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Common;
using AguasNico_Api.Models.DTO.Expenses;
using AguasNico_Api.Models.DTO.Home;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Services;

public class HomeService(APIContext context, TokenService tokenService, RouteService routeService, DealerService dealerService)
{
    private readonly APIContext _db = context;
    private readonly TokenService _tokenService = tokenService;
    private readonly RouteService _routeService = routeService;
    private readonly DealerService _dealerService = dealerService;

    public async Task<BaseResponse<GetDashboardResponse>> GetDashboard()
    {
        var token = _tokenService.GetToken();
        var today = LocalClock.Today;
        var response = new GetDashboardResponse { Role = token.Role };

        if (token.Role == Roles.Admin)
        {
            var dealers = await _dealerService.GetAll();
            var transfers = await _db.Transfers
                .AsNoTracking()
                .Where(x => x.Date.Date == today)
                .Select(x => new Models.DTO.Routes.TransferItem
                {
                    Id = x.ID,
                    ClientId = x.ClientID,
                    ClientName = x.Client.Name,
                    Amount = x.Amount,
                    Date = x.Date
                })
                .ToListAsync();

            response.Dealers = dealers.Data?.Items ?? [];
            response.SoldProducts = await _routeService.GetSoldProductsByDate(today);
            response.Expenses = await _db.Expenses
                .AsNoTracking()
                .Where(x => x.CreatedAt.Date == today)
                .Select(x => new ExpenseItem
                {
                    Id = x.ID,
                    UserId = x.UserID,
                    DealerName = x.User.Name,
                    Amount = x.Amount,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
            response.Payments = await GetTotalCollected(today);
            response.Transfers = transfers;
            response.Dispensers = await _db.Routes.Where(x => x.CreatedAt.Date == today).SumAsync(x => x.DispenserPrice);
            response.TotalSold = response.Payments.Sum(x => x.Amount) + response.Transfers.Sum(x => x.Amount) + response.Dispensers;
        }
        else
        {
            var todayDay = (Day)(int)LocalClock.Now.DayOfWeek;
            response.DealerRoutes = await _db.Routes
                .AsNoTracking()
                .Where(x => x.UserID == token.UserId && !x.IsStatic && x.DayOfWeek == todayDay)
                .Select(x => new Models.DTO.Routes.RouteItem
                {
                    Id = x.ID,
                    UserId = x.UserID,
                    DealerName = x.User.Name,
                    TruckNumber = x.User.TruckNumber ?? 0,
                    DayOfWeek = x.DayOfWeek,
                    IsStatic = x.IsStatic,
                    IsClosed = x.IsClosed,
                    DispenserPrice = x.DispenserPrice,
                    CreatedAt = x.CreatedAt,
                    TotalCarts = x.Carts.Count,
                    CompletedCarts = x.Carts.Count(y => y.State != State.Pending),
                    PendingCarts = x.Carts.Count(y => y.State == State.Pending)
                })
                .ToListAsync();
        }

        return new BaseResponse<GetDashboardResponse> { Data = response };
    }

    private async Task<List<PaymentAmountItem>> GetTotalCollected(DateTime date)
    {
        var methods = await _db.PaymentMethods.AsNoTracking().Where(x => x.Name == "Efectivo").ToListAsync();
        var result = new List<PaymentAmountItem>();
        foreach (var method in methods)
        {
            result.Add(new PaymentAmountItem
            {
                PaymentMethodId = method.ID,
                PaymentMethodName = method.Name,
                Amount = await _db.CartPaymentMethods.Where(x => x.Cart.CreatedAt.Date == date.Date && x.PaymentMethodID == method.ID).SumAsync(x => x.Amount)
            });
        }
        return result;
    }
}

